using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using CargoWise.Common;
using Enterprise.Dat.Implementation;

namespace CargoWise.Data.SqlProxy.Interface.Test.EndToEndTest;

static class TestHelper
{
	public static IDisposable UseDatabaseDisposable(this SqlConnection sqlConnection, string databaseName)
	{
		var originalDatabaseName = sqlConnection.Database;
		sqlConnection.ChangeDatabase(databaseName);
		return new DisposableAction(() => sqlConnection.ChangeDatabase(originalDatabaseName));
	}

	public static SqlDataReader ExecuteReader(this SqlConnection sqlConnection, string sql, Action<SqlCommand>? commandAction = null)
	{
		using var command = sqlConnection.CreateCommand();
		command.CommandText = sql;
		command.CommandType = CommandType.Text;
		commandAction?.Invoke(command);
		return command.ExecuteReader();
	}

	public static void ExecuteNonQuery(this SqlConnection sqlConnection, string sql, Action<SqlCommand>? commandAction = null)
	{
		using var command = sqlConnection.CreateCommand();
		command.CommandText = sql;
		command.CommandType = CommandType.Text;

		commandAction?.Invoke(command);
		command.ExecuteNonQuery();
	}

	public static void CreateDatabaseDropExisting(this SqlConnection sqlConnection, string databaseName)
	{
		sqlConnection.KillOtherConnections(databaseName);
		sqlConnection.DropDatabaseIfExists(databaseName);

		var sqlText = $@"
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = @databaseName)
BEGIN
	CREATE DATABASE [{databaseName}];
END";
		sqlConnection.ExecuteNonQuery(sqlText, command =>
		{
			var sqlParameter = new SqlParameter("@databaseName", SqlDbType.NVarChar, 128) { Value = databaseName };
			command.Parameters.Add(sqlParameter);
		});

		sqlConnection.ChangeDatabase(databaseName);
	}

	public static void DropDatabaseIfExists(this SqlConnection sqlConnection, string databaseName)
	{
		var sqlText = $"IF EXISTS (SELECT name FROM sys.databases WHERE name = @databaseName) DROP DATABASE [{databaseName}]";
		sqlConnection.ExecuteNonQuery(sqlText, command =>
		{
			var sqlParameter = new SqlParameter("@databaseName", SqlDbType.NVarChar, 128) { Value = databaseName };
			command.Parameters.Add(sqlParameter);
		});
	}

	public static void KillOtherConnections(this SqlConnection sqlConnection, string databaseName)
	{
		var query = @"
SELECT spid
FROM sys.sysprocesses
WHERE dbid = DB_ID(@DatabaseName) AND spid != @@SPID
";

		using var command = new SqlCommand(query, sqlConnection);
		command.Parameters.AddWithValue("@DatabaseName", databaseName);

		using var reader = command.ExecuteReader();

		while (reader.Read())
		{
			var spid = reader.GetInt16(0);

			try
			{
				using var killCommand = new SqlCommand($"KILL {spid}", sqlConnection);
				killCommand.ExecuteNonQuery();
			}
			catch (SqlException)
			{
				// ignore errors
			}
		}
	}

	public static SqlConnection OpenLocalSqlConnection()
	{
		var connection = new SqlConnection(LazyConnectionString.Value);
		connection.Open();
		return connection;
	}

	static readonly Lazy<string> LazyConnectionString = new(() =>
	{
		var builder = new SqlConnectionStringBuilder()
		{
			DataSource = LocalDBConnection.GetServerName(),
			InitialCatalog = Db.SqlMasterDb,
			ConnectTimeout = 30,
			IntegratedSecurity = true,
			Encrypt = false,
			TrustServerCertificate = true,
			ApplicationName = Assembly.GetCallingAssembly().GetName().ToString(),
			PersistSecurityInfo = false,
		}.ToString();

		return builder;
	});
}
