using System;

namespace Enterprise.DocumentEngine
{
	interface IJsonSerializable
	{
		object GetJsonData();
	}

	interface IJsonConverter
	{
		Type JsonDataType { get; }
		object GetObjectData(object value);
	}
}
