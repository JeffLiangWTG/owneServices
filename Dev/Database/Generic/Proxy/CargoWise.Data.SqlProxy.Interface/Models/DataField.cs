using Newtonsoft.Json;

namespace CargoWise.Data.SqlProxy.Interface.Models;

[method: JsonConstructor]
public class DataField(string name, string type)
{
	[JsonRequired]
	public string Name { get; set; } = name;

	[JsonRequired]
	public string Type { get; set; } = type;
}
