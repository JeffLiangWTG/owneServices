using Microsoft.SqlServer.Types;
using Newtonsoft.Json;

namespace CargoWise.Data.SqlProxy.Interface.Converters;

public class SqlGeometryConverter : JsonConverter<SqlGeometry>
{
	public override void WriteJson(JsonWriter writer, SqlGeometry? value, JsonSerializer serializer)
	{
		writer.WriteValue(value?.STAsText()?.ToSqlString().Value);
	}

	public override SqlGeometry ReadJson(JsonReader reader, Type objectType, SqlGeometry? existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		var wkt = reader.Value as string;
		return string.IsNullOrWhiteSpace(wkt)
			? SqlGeometry.Null
			: SqlGeometry.STGeomFromText(new System.Data.SqlTypes.SqlChars(wkt), 0);
	}
}
