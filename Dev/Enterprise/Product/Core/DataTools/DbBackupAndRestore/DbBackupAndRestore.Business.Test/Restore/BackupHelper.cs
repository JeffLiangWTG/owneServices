using System;
using System.IO;
using System.Reflection;
using CargoWise.Data;

namespace Enterprise.DataTools.DbBackupAndRestore.Business;

public static class BackupHelper
{
	public static void CreateDatabaseBackupFromScript(string backupPath, string sqlScriptResourceName, string dbName)
	{
		using var connection = Db.NewAdminConnection(Db.SqlMasterDb);
		using var dropDb = AdoTestUtils.CreateDbDropExistingDisposable(connection, dbName);
		using var usingDb = ((ICurrentDbControl)connection).UseDatabase(dbName);

		var sql = ReadEmbeddedResourceSql(sqlScriptResourceName);
		connection.ExecuteNonQuery(sql);

		Backup(connection, backupPath, dbName);
	}

	static void Backup(DbConnection connection, string path, string databaseName)
	{
		connection.ExecuteNonQuery($"BACKUP DATABASE [{databaseName}] TO DISK = N'{path}' WITH INIT, COMPRESSION");
	}

	static string ReadEmbeddedResourceSql(string resourceName)
	{
		const string resourcePrefix = "Enterprise.DataTools.DbBackupAndRestore.Business.Testing.Restore.TestFiles.";
		var fullResourceName = resourcePrefix + resourceName;
		var assembly = Assembly.GetExecutingAssembly();
		using var stream = assembly.GetManifestResourceStream(fullResourceName) ?? throw new InvalidOperationException($"Resource '{fullResourceName}' not found.");
		using var reader = new StreamReader(stream);
		return reader.ReadToEnd();
	}
}
