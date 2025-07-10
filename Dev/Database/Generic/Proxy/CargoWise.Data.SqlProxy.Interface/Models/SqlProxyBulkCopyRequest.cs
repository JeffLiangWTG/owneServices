using System.Diagnostics;
using Newtonsoft.Json;

namespace CargoWise.Data.SqlProxy.Interface.Models;

[DebuggerDisplay("Statement = {SqlStatement}")]
public class SqlProxyBulkCopyRequest : SqlProxyRequest
{
	public SqlProxyBulkCopyRequest()
	{
	}

	public SqlProxyBulkCopyRequest(IDbConnectionInfo databaseConnection)
		: base(databaseConnection)
	{
	}

	[JsonRequired]
	public byte[]? Payload { get; set; }

	[JsonRequired]
	public string? TableSchema { get; set; }

	[JsonRequired]
	public string? DestinationTableName { get; set; }

	public int BulkCopyTimeout { get; set; }

	public SqlBulkCopyOptions Options { get; set; }

	public int BatchSize { get; set; }

	public int NotifyAfter { get; set; }

	[JsonRequired]
	public Dictionary<string, string>? ColumnMappings { get; set; }
}
