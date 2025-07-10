using System;
using System.Linq;
using Enterprise.ZArchitecture.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Enterprise.BufferManagement.Business
{
	public class ExperimentalSettingSerializer : JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var obj = value as ExperimentalSetting;
			writer.WriteStartObject();
			writer.WritePropertyName((NoResString)"Key");  // The only place where it is used
			serializer.Serialize(writer, obj.Key);
			writer.WritePropertyName((NoResString)"Value");  // The only place where it is used
			serializer.Serialize(writer, obj.Value);
			writer.WriteEndObject();
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			JObject jsonObject = JObject.Load(reader);
			var properties = jsonObject.Properties().ToList();
			return new ExperimentalSetting
			{
				Key = (string)properties[0].Value,
				Value = (string)properties[1].Value
			};
		}

		public override bool CanConvert(Type objectType)
		{
			return typeof(ExperimentalSetting).IsAssignableFrom(objectType);
		}
	}
}
