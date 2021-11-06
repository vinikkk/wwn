using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public struct CommandData
{
	public bool hasParameter;
	public Action callback;

	public CommandData(Action cb, bool hp = false)
	{
		callback = cb;
		hasParameter = hp;
	}
}

public class HON_Parser
{
	//Grammar
	static Dictionary<string, CommandData> commandCallbackDictionary = new Dictionary<string, CommandData>(); //Store all the callbacks of the expected commands in the parser

	static Stack<GameObject> objectStack = new Stack<GameObject>();

	static string currentCommand;
	static string currentParameter;
	static GameObject currentObject;
	/*
	space {
		camera {
			wpos 0,0,-10
		}
		obj {
			light directional
			rotation 45,0,0
		}
		obj {
			mesh sphere
			wpos 0,1,0
		}
		obj {
			mesh plane
			scale 10,1,10
		}
	}
	*/

	string vec3Regex = "/\\d+,\\d+,\\d+/gm";
	string wordOrString = "/'\\w+'|\\w+/gm";

	string testData = "space { camera { wpos 0,1,-10 } obj { light directional rotation 45,45,0 } obj { mesh sphere wpos 0,1,0 } obj { mesh plane scale 10,1,10 } }";

	private void Start()
	{
		Initialize();

		Parse(testData.Split(' '));
	}

	public static void Initialize()
	{
		//Objects
		commandCallbackDictionary.Add("space", new CommandData(NewObject));
		commandCallbackDictionary.Add("obj", new CommandData(NewObject));
		commandCallbackDictionary.Add("camera", new CommandData(CameraObject));

		//Components
		commandCallbackDictionary.Add("mesh", new CommandData(AddMesh, true));
		commandCallbackDictionary.Add("light", new CommandData(AddLight, true));

		//Parameters
		commandCallbackDictionary.Add("rotation", new CommandData(SetRotation, true));
		commandCallbackDictionary.Add("wpos", new CommandData(SetWPos, true));
		commandCallbackDictionary.Add("scale", new CommandData(SetScale, true));

		//Stack handlers
		//Add to stack last object created
		commandCallbackDictionary.Add("{", new CommandData(StackObject));
		//Remove from stack and Instantiate last object created
		commandCallbackDictionary.Add("}", new CommandData(UnstackObject));
	}

	public static void Parse(string[] data)
	{
		for (int i = 0; i < data.Length; i++)
		{
			if (!commandCallbackDictionary.ContainsKey(data[i]))
			{
				Debug.Log($"The command '{data[i]}' does not exist in the grammar.");
				continue;
			}

			currentCommand = data[i];
			if (commandCallbackDictionary[currentCommand].hasParameter)
			{
				currentParameter = data[++i];
			}
			commandCallbackDictionary[currentCommand].callback.Invoke();
		}
	}

	public static void NewObject()
	{
		currentObject = new GameObject(currentCommand);
	}

	public static void CameraObject()
	{
		currentObject = Camera.main.gameObject;
	}

	public static void StackObject()
	{
		objectStack.Push(currentObject);
	}

	public static void UnstackObject()
	{
		objectStack.Pop();
	}

	public static void AddMesh()
	{
		MeshFilter mf = currentObject.AddComponent(typeof(MeshFilter)) as MeshFilter;
		MeshRenderer mr = currentObject.AddComponent(typeof(MeshRenderer)) as MeshRenderer;

		mr.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));

		switch (currentParameter)
		{
			case "cube":
				mf.sharedMesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
				break;
			case "sphere":
				mf.sharedMesh = Resources.GetBuiltinResource<Mesh>("Sphere.fbx");
				break;
			case "plane":
				mf.sharedMesh = Resources.GetBuiltinResource<Mesh>("Plane.fbx");
				break;
		}
	}

	public static void AddLight()
	{
		Light l = currentObject.AddComponent(typeof(Light)) as Light;

		l.shadows = LightShadows.Soft;

		switch (currentParameter)
		{
			case "directional":
				l.type = LightType.Directional;
				break;
			case "point":
				l.type = LightType.Point;
				break;
			case "spot":
				l.type = LightType.Spot;
				break;
		}
	}

	public static void SetRotation()
	{
		currentObject.transform.eulerAngles = StringToVector3(currentParameter);
	}

	public static void SetWPos()
	{
		currentObject.transform.position = StringToVector3(currentParameter);
	}

	public static void SetScale()
	{
		currentObject.transform.localScale = StringToVector3(currentParameter);
	}



	public static Vector3 StringToVector3(string str)
	{
		string[] parameter = str.Split(',');

		return new Vector3(Int32.Parse(parameter[0]), Int32.Parse(parameter[1]), Int32.Parse(parameter[2]));
	}

}
