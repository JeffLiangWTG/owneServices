using Microsoft.SqlServer.Types;
using Newtonsoft.Json;

namespace CargoWise.Data.SqlProxy.Interface.Converters;

public class SqlHierarchyIdConverter : JsonConverter<SqlHierarchyId>
{
	public override void WriteJson(JsonWriter writer, SqlHierarchyId value, JsonSerializer serializer)
	{
		writer.WriteValue(value.IsNull ? null : value.ToString());
	}

	public override SqlHierarchyId ReadJson(JsonReader reader, Type objectType, SqlHierarchyId existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		var path = reader.Value as string;
		return string.IsNullOrWhiteSpace(path)
			? SqlHierarchyId.Null
			: SqlHierarchyId.Parse(path);
	}
}
