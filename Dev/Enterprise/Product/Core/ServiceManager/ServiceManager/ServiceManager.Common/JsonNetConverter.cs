using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ServiceManager.Common.Abstractions;

namespace ServiceManager.Common;

public class JsonNetConverter : IJsonConverter
{
	public string Serialize(object o)
	{
		return JsonConvert.SerializeObject(o, SerializerSettings);
	}

	public static JsonSerializerSettings SerializerSettings { get; } = new()
	{
		ContractResolver = new AlphabeticOrderJsonContractResolver(),
		DateFormatHandling = DateFormatHandling.MicrosoftDateFormat,
		Converters = new JsonConverter[] { new TimespanConverter() }
	};

	class AlphabeticOrderJsonContractResolver : DefaultContractResolver
	{
		protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
		{
			return base.CreateProperties(type, memberSerialization)
				.OrderBy(p => p.Order ?? int.MaxValue)
				.ThenBy(p => p.PropertyName, StringComparer.Ordinal)
				.ToList();
		}
	}

	class TimespanConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(TimeSpan);
		}

		public override bool CanRead => true;
		public override bool CanWrite => true;

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			if (objectType != typeof(TimeSpan))
			{
				throw new ArgumentException(null, nameof(objectType));
			}

			var spanString = reader.Value as string;
			if (spanString == null)
			{
				return null;
			}
			return XmlConvert.ToTimeSpan(spanString);
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var duration = (TimeSpan)value;
			writer.WriteValue(XmlConvert.ToString(duration));
		}
	}
}
