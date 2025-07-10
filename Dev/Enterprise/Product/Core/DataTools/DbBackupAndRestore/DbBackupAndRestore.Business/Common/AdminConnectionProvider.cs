using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Data;

namespace Enterprise.DataTools.DbBackupAndRestore.Business;

sealed class AdminConnectionProvider : IDisposable
{
	static AdminConnectionProvider()
	{
		DbConnection.ApplicationName = "Db Backup And Restore Tool";
	}

	public AdminConnectionProvider(string dbServer, string auditServer, string edwServer, IErrorReporter errorReporter)
	{
		this.dbServer = dbServer;
		this.auditServer = auditServer;
		this.edwServer = edwServer;
		this.errorReporter = errorReporter;
		connections = new Dictionary<string, DbBackupAndRestoreToolConnection>(StringComparer.OrdinalIgnoreCase);
	}

	public DbBackupAndRestoreToolConnection GetConnection(string dbType) => GetServerConnection(GetServerName(dbType));
	public DbBackupAndRestoreToolConnection GetMainDbConnection() => GetServerConnection(dbServer);
	public DbBackupAndRestoreToolConnection GetAuditDbConnection() => GetServerConnection(auditServer);
	public DbBackupAndRestoreToolConnection GetEdwDbConnection() => GetServerConnection(edwServer);

	DbBackupAndRestoreToolConnection GetServerConnection(string serverName)
	{
		if (!connections.TryGetValue(serverName, out var connection))
		{
			connection = new DbBackupAndRestoreToolConnection(serverName, Db.SqlMasterDb, errorReporter);
			connections.Add(serverName, connection);
		}

		return connection;
	}

	string GetServerName(string dbType)
	{
		switch (dbType)
		{
			case DbFileInfo.DbTypeAuditDB:
				return auditServer;
			case DbFileInfo.DbTypeEdwDB:
				return edwServer;
			default:
				return dbServer;
		}
	}

	public void Dispose()
	{
		foreach (var connection in connections.Values)
		{
			connection.Dispose();
		}
	}

	readonly Dictionary<string, DbBackupAndRestoreToolConnection> connections;
	readonly string dbServer;
	readonly string auditServer;
	readonly string edwServer;
	readonly IErrorReporter errorReporter;
}
