using CargoWise.Data.SqlProxy.Interface.Converters;
using Newtonsoft.Json;

namespace CargoWise.Data.SqlProxy.Interface.Models;

[method: JsonConstructor]
public class ExecuteScalarResult(string type, string? value)
{
	[JsonRequired]
	public string Type { get; set; } = type;

	[JsonProperty(NullValueHandling = NullValueHandling.Include)]
	public string? Value { get; set; } = value;

	[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
	public SqlParameterDTO[]? Parameters { get; set; }

	public object AsSqlValue()
	{
		return SqlValueConverter.FromJson(Value, Type);
	}

	public object? AsValue()
	{
		var csValue = SqlValueConverter.SqlValueToCsValue(AsSqlValue());
		return csValue == DBNull.Value ? null : csValue;
	}
}
