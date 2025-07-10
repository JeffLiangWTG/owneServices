using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CargoWise.Data.SqlProxy.Interface.Models;

// TODO: This is temporary complexity before .net 8
// Can be entirely deleted once this is using System.Text.Json
public class SqlProxyReaderResponseItemConverter : JsonConverter
{
	public override bool CanConvert(Type objectType)
	{
		return typeof(SqlProxyReaderResponseItem).IsAssignableFrom(objectType);
	}

	public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
	{
		if (reader.TokenType == JsonToken.Null)
		{
			return null!;
		}

		var jsonObject = JObject.Load(reader);
		var type = jsonObject[GlowReaderResponseJsonProperty.Type]?.ToString();

		SqlProxyReaderResponseItem result = type switch
		{
			GlowReaderResponseItemType.Header => new SqlReaderResponseHeader(),
			GlowReaderResponseItemType.Row => new SqlReaderResponseRow(),
			_ => throw new JsonSerializationException($"Unknown type: {type}")
		};

		serializer.Populate(jsonObject.CreateReader(), result);
		return result;
	}

	public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}

		writer.WriteStartObject();

		switch (value)
		{
			case SqlReaderResponseRow row:
				writer.WritePropertyName(GlowReaderResponseJsonProperty.Type);
				writer.WriteValue(row.Type);
				writer.WritePropertyName(GlowReaderResponseJsonProperty.Values);
				serializer.Serialize(writer, row.Values);
				writer.WritePropertyName(GlowReaderResponseJsonProperty.Depth);
				writer.WriteValue(row.Depth);
				break;

			case SqlReaderResponseHeader header:
				writer.WritePropertyName(GlowReaderResponseJsonProperty.Type);
				writer.WriteValue(header.Type);
				writer.WritePropertyName(GlowReaderResponseJsonProperty.Columns);
				serializer.Serialize(writer, header.Columns);
				writer.WritePropertyName(GlowReaderResponseJsonProperty.RecordsAffected);
				writer.WriteValue(header.RecordsAffected);
				writer.WritePropertyName(GlowReaderResponseJsonProperty.HasRows);
				writer.WriteValue(header.HasRows);
				break;

			default:
				throw new JsonSerializationException($"Unknown type: {value.GetType()}");
		}

		writer.WriteEndObject();
	}
}
