using CargoWise.Database.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace CargoWise.Data.SqlProxy.Interface.Models;

public class SqlProxyDatabaseDetails : IDbConnectionInfo
{
	public SqlProxyDatabaseDetails()
	{
	}

	public SqlProxyDatabaseDetails(IDbConnectionInfo databaseConnection)
	{
		ServerName = databaseConnection.ServerName;
		Database = databaseConnection.Database;
		UserName = databaseConnection.UserName;
		Password = databaseConnection.Password;
		SuffixedApplicationName = databaseConnection.SuffixedApplicationName;
		ConnectTimeout = databaseConnection.ConnectTimeout;

		var versions = GlobalServiceProvider.Instance.GetRequiredService<IDatabaseAspectVersions>();

		SchemaVersionMajor = versions.SchemaVersion?.Major;
		SchemaVersionMinor = versions.SchemaVersion?.Minor;
		ScriptVersionMajor = versions.ScriptVersion?.Major;
		ScriptVersionMinor = versions.ScriptVersion?.Minor;
		TransformationVersionMajor = versions.TransformationVersion?.Major;
		TransformationVersionMinor = versions.TransformationVersion?.Minor;
	}

	[JsonRequired]
	public string? ServerName { get; set; }

	[JsonRequired]
	public string? Database { get; set; }

	[JsonRequired]
	public string? UserName { get; set; }

	[JsonRequired]
	public string? Password { get; set; }

	[JsonRequired]
	public string? SuffixedApplicationName { get; set; } = string.Empty;

	[JsonRequired]
	public int ConnectTimeout { get; set; } = 30;

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	public int? SchemaVersionMajor { get; set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	public int? SchemaVersionMinor { get; set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	public int? ScriptVersionMajor { get; set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	public int? ScriptVersionMinor { get; set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	public int? TransformationVersionMajor { get; set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	public int? TransformationVersionMinor { get; set; }
}
