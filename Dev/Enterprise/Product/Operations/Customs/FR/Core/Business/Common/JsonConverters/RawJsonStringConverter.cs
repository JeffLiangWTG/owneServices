using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Enterprise.Customs.FR.Business
{
	public class RawJsonStringConverter : JsonConverter<string>
	{
		public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.String)
			{
				return reader.GetString();
			}
			else
			{
				using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
				{
					return doc.RootElement.GetRawText();
				}
			}
		}

		public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
		{
			using (JsonDocument doc = JsonDocument.Parse(value))
			{
				doc.WriteTo(writer);
			}
		}
	}
}
