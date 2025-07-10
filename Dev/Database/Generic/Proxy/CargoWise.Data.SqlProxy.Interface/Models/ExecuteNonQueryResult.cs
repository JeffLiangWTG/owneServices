using Newtonsoft.Json;

namespace CargoWise.Data.SqlProxy.Interface.Models;

[method: JsonConstructor]
public class ExecuteNonQueryResult(int rowsAffected)
{
	[JsonRequired]
	public int RowsAffected { get; set; } = rowsAffected;

	public SqlParameterDTO[]? Parameters { get; set; }
}
