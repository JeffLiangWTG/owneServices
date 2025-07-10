using System.Data;
using Newtonsoft.Json;

namespace CargoWise.Data.SqlProxy.Interface.Models;

public class SqlProxyRequest
{
	public SqlProxyRequest()
	{
	}

	public SqlProxyRequest(IDbConnectionInfo databaseConnection)
	{
		Connection = new SqlProxyDatabaseDetails(databaseConnection);
	}

	public void SetDatabaseConnection(IDbConnectionInfo databaseConnection)
	{
		Connection = new SqlProxyDatabaseDetails(databaseConnection);
	}

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	public Guid? TransactionId { get; set; }

	[JsonRequired]
	public SqlProxyDatabaseDetails? Connection { get; set; }

	public string? SqlStatement { get; set; }

	public CommandType CommandType { get; set; } = CommandType.Text;

	public CommandBehavior CommandBehavior { get; set; } = CommandBehavior.Default;

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	public int CommandTimeout { get; set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	public SqlParameterDTO[]? Parameters { get; set; }

	public IsolationLevel IsolationLevel { get; set; } = IsolationLevel.ReadCommitted;

	public override string ToString()
	{
		return $@"SqlStatement: {SqlStatement}
CommandType: {CommandType}
Parameters: {(Parameters == null ? string.Empty : string.Join("|", Parameters!.Select(x => x.ToString())))}";
	}
}
