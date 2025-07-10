using Newtonsoft.Json;

namespace CargoWise.Data.SqlProxy.Interface.Models;

[method: JsonConstructor]
public class ResultSet(DataField[] fieldNames)
{
	[JsonRequired]
	public DataField[] FieldNames { get; set; } = fieldNames;

	[JsonRequired]
	public List<object[]> Rows { get; set; } = new();
}
