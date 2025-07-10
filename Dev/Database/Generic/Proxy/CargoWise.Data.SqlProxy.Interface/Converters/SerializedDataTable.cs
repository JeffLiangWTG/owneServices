namespace CargoWise.Data.SqlProxy.Interface.Converters;

public class SerializedDataTable
{
	public string TableName { get; set; } = string.Empty;

	public string? PrimaryKey { get; set; }

	public List<SerializedDataColumn> Columns { get; set; } = [];

	public List<List<string?>> Rows { get; set; } = [];
}
