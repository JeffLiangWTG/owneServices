using System.Data.SqlTypes;
using Microsoft.SqlServer.Types;
using Newtonsoft.Json;

namespace CargoWise.Data.SqlProxy.Interface.Converters;

public class SqlGeographyConverter : JsonConverter<SqlGeography>
{
	public override void WriteJson(JsonWriter writer, SqlGeography? value, JsonSerializer serializer)
	{
		var wkt = value?.STAsText()?.ToSqlString().Value;
		writer.WriteValue(wkt);
	}

	public override SqlGeography ReadJson(JsonReader reader, Type objectType, SqlGeography? existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		var wkt = reader.Value as string;
		return string.IsNullOrWhiteSpace(wkt)
			? SqlGeography.Null
			: SqlGeography.STGeomFromText(new SqlChars(wkt), 4326);
	}
}
