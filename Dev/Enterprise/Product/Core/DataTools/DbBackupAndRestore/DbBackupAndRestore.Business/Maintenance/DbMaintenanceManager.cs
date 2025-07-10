using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;

namespace Enterprise.DataTools.DbBackupAndRestore.Business
{
	public class DbMaintenanceManager : DbTaskManager
	{
		public DbMaintenanceManager()
		{
			CompletedDropDBList = new List<string>();
			AlwaysOnHelper = new AlwaysOnHelper();
			DatabaseWaiter = new DatabaseWaiter();
		}

		public List<string> CompletedDropDBList;
		public IAlwaysOnHelper AlwaysOnHelper;
		public IDatabaseWaiter DatabaseWaiter;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public void DropDatabases(string dbServer, string mainDbName, bool includeOperationalDbs, bool includeRefFilesDbs, bool includeBiDbs)
		{
			try
			{
				CheckReleaseKeyAndDropDatabases(dbServer, mainDbName, includeOperationalDbs, includeRefFilesDbs, includeBiDbs);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				var message = string.Empty;
				if (ex is DbBackupAndRestoreException)
				{
					message = ex.Message;
				}
				else
				{
					if (ex.Message.Contains("Value cannot be null."))
					{
						message = "Database drop failed, please refresh Dbs\r\nThe database you are trying to delete no longer exists";
					}
					else
					{
						message = "Database drop failed, please refresh Dbs\r\n" + ex.Message;
					}
				}

				FireOnTaskFailed(message);
			}
		}

		void CheckReleaseKeyAndDropDatabases(string dbServer, string mainDbName, bool includeOperationalDbs, bool includeRefFilesDbs, bool includeBiDbs)
		{
			var canOverWritten = CanDbBeOverwritten(dbServer, mainDbName);

			if (canOverWritten)
			{
				CheckAndDropDatabases(dbServer, mainDbName, includeOperationalDbs, includeRefFilesDbs, includeBiDbs);
			}
			else
			{
				throw new DbBackupAndRestoreException("Database already exists. A valid release key is required to overwrite it.\r\n");
			}
		}

		bool CanDbBeOverwritten(string dbServer, string databaseName)
		{
			using (Db.DisableSchemaVersionCheck())
			using (var connection = Db.NewAdminConnection(dbServer, Db.SqlMasterDb))
			{
				var dbState = connection.DatabaseStateDescription(databaseName);

				if (string.Equals(dbState, "ONLINE", StringComparison.OrdinalIgnoreCase) && Utilities.IsProductionDb(connection, databaseName))
				{
					var sessionInfo = SessionInfoUserKeyEntry.GetSessionInfo(connection, dbServer, databaseName);
					PromptForReleaseKey(sessionInfo);
					return sessionInfo.ShouldRelease();
				}

				return true;
			}
		}

		/// <summary>
		/// Drop a Database and its dependants DocManager + Reference Files databases as requested.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		protected virtual void CheckAndDropDatabases(string dbServer, string mainDbName, bool includeOperationalDbs, bool includeRefFilesDbs, bool includeBiDbs)
		{
			try
			{
				FireOnTaskStarted(string.Format(CultureInfo.InvariantCulture, "Database drop started\r\n\r\nServer: {0}\r\nMain Database: {1}", dbServer, mainDbName));

				CompletedDropDBList.Clear();

				var dbList =  GetDatabaseList(dbServer, mainDbName, includeOperationalDbs, includeRefFilesDbs, includeBiDbs);

				FireOnTaskStarted(string.Format(CultureInfo.InvariantCulture, "Databases to drop:\r\n{0}", string.Join("\r\n", dbList.Select(_ => _.databaseName))));

				foreach (var (databaseName, serverName) in dbList)
				{
					using (Db.DisableSchemaVersionCheck())
					using (var primaryConnection = Db.NewAdminConnection(serverName, Db.SqlMasterDb))
					{
						FireOnTaskStarted(string.Format(CultureInfo.InvariantCulture, "Dropping database [{0}]\r\n", databaseName));

						if (AlwaysOnHelper.IsDbPartOfAlwaysOn(primaryConnection, databaseName))
						{
							var replicaServers = AlwaysOnHelper.GetSecondaryReplicaNamesList(primaryConnection, databaseName);

							FireOnSubtaskStarted(string.Format(CultureInfo.InvariantCulture, "Database participates in AlwaysOn group with the following secondary replicas:\r\n{0}", string.Join("\r\n", replicaServers)), 0);

							FireOnSubtaskStarted(string.Format(CultureInfo.InvariantCulture, "Removing database {0} from AlwaysOn Group\r\n", databaseName), 0);
							AlwaysOnHelper.RemoveDatabaseFromAvailabilityGroup(primaryConnection, databaseName, AlwaysOnHelper.GetAvailabilityGroupName(primaryConnection, databaseName));

							FireOnSubtaskStarted(string.Format(CultureInfo.InvariantCulture, "Dropping database on primary: {0}\r\n", serverName), 0);
							DropDatabase(primaryConnection, mainDbName, databaseName);

							foreach (var replicaServerName in replicaServers)
							{
								using (var secondaryConnection = Db.NewAdminConnection(replicaServerName, Db.SqlMasterDb))
								using (var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(2)))
								{
									try
									{
										FireOnSubtaskStarted(string.Format(CultureInfo.InvariantCulture, "Dropping database on secondary: {0}\r\n", replicaServerName), 0);
										DatabaseWaiter.WaitUntilDatabaseReadyToRestore(secondaryConnection, databaseName, Logger.Instance, cancellationTokenSource.Token);
										DropDatabase(secondaryConnection, mainDbName, databaseName);
									}
									catch (Exception)
									{
										FireOnTaskFailed($"Failed to drop database: {databaseName} from secondary replica: {replicaServerName}, please drop this database manually");
									}
								}
							}
						}
						else
						{
							DropDatabase(primaryConnection, mainDbName, databaseName);
						}

						FireOnTaskCompleted(string.Format(CultureInfo.InvariantCulture, "Successfully dropped database: [{0}]", databaseName));
					}
				}

				FireOnTaskCompleted("Database drop completed with success");
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				FireOnTaskFailed("Database drop failed, please refresh DBs\r\n" + ex.Message);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
		protected virtual void DropDatabase(AdminConnection connection, string mainDbName, string databaseName)
		{
			var deleteBackupHistorySql = "EXEC msdb.dbo.sp_delete_database_backuphistory @database_name = '{0}'";

			var rawDropCmdText = "IF EXISTS(SELECT null FROM sys.databases WHERE name='{0}') DROP DATABASE [{0}]";

			var dropLoginSql = @"IF EXISTS (SELECT name FROM sys.server_principals WHERE name='{0}') DROP LOGIN [{0}]";

			var i = 0;
			var commandTimeout = 0;

			FireOnSubtaskStarted(string.Format(CultureInfo.InvariantCulture, "Removing DB Backup History: {0} from Server: {1}\r\n", databaseName, connection.ServerName), ++i);

			var commandText = string.Format(CultureInfo.InvariantCulture, deleteBackupHistorySql, databaseName);
			connection.ExecuteNonQuery(commandText, commandTimeout);

			var retries = 3;

			while (retries > 0)
			{
				try
				{
					FireOnSubtaskStarted(string.Format(CultureInfo.InvariantCulture, "Dropping Database: {0} from Server: {1}\r\n", databaseName, connection.ServerName), ++i);
					commandText = string.Format(CultureInfo.InvariantCulture, rawDropCmdText, databaseName);
					connection.ExecuteNonQuery(commandText, commandTimeout);

					break;
				}
				catch (Exception)
				{
					retries--;
					if (retries == 0)
					{
						throw;
					}

					FireOnSubtaskStarted(string.Format(CultureInfo.InvariantCulture, "Killing other connections\r\n"), ++i);
					DbConnectionKiller.KillOtherConnections(connection, databaseName);
				}
			}

			CompletedDropDBList.Add(databaseName);

			if (databaseName == mainDbName)
			{
				var loginList = Db.GetAllLoginNames(mainDbName);

				foreach (var login in loginList)
				{
					FireOnSubtaskStarted(string.Format(CultureInfo.InvariantCulture, "Drop Login if it exists: {0}\r\n", login), ++i);
					commandText = string.Format(CultureInfo.InvariantCulture, dropLoginSql, login);
					connection.ExecuteNonQuery(commandText, commandTimeout);
				}

				FireOnSubtaskStarted("Drop staff assigned logins if they exist.\r\n", ++i);
				commandText = GetDropAllStaffAssignedLoginsFromDbSql(mainDbName);
				connection.ExecuteNonQuery(commandText, commandTimeout);
			}
		}

		List<(string databaseName, string serverName)> GetDatabaseList(string dbServer, string mainDbName, bool includeOperationalDbs, bool includeRefFilesDbs, bool includeBiDbs)
		{
			var databaseList = new List<(string, string)>();

			using (Db.DisableSchemaVersionCheck())
			using (var mainServerConnection = Db.NewAdminConnection(dbServer, Db.SqlMasterDb))
			{
				var dbDropList = GetDbDropList(mainServerConnection, mainDbName, includeOperationalDbs, includeRefFilesDbs);
				databaseList.AddRange(dbDropList.Select(db => (db, dbServer)));

				if (includeBiDbs)
				{
					var dwServer = Utilities.GetDataWarehouseServer(mainServerConnection, mainDbName);
					var auditServer = Utilities.GetAuditServer(mainServerConnection, mainDbName);

					if (string.IsNullOrEmpty(auditServer) && mainServerConnection.DatabaseExists(mainDbName + Db.AuditDatabaseSuffix))
					{
						auditServer = dbServer;
					}
					if (!string.IsNullOrEmpty(auditServer))
					{
						databaseList.Add((mainDbName + Db.AuditDatabaseSuffix, auditServer));
					}

					if (string.IsNullOrEmpty(dwServer) && mainServerConnection.DatabaseExists(mainDbName + Db.EdwDatabaseSuffix))
					{
						dwServer =  dbServer;
					}
					if (!string.IsNullOrEmpty(dwServer))
					{
						databaseList.Add((mainDbName + Db.EdwDatabaseSuffix, dwServer));
					}
				}
			}
			return databaseList;
		}

		#region Delegate

		void PromptForReleaseKey(SessionInfo sessionInfo)
		{
			OnPromtForReleaseKey?.Invoke(sessionInfo);
		}

		public ReleaseKeyDelegate OnPromtForReleaseKey;

		#endregion

		public string[] GetServerDbList(string dbServer, bool requirePrint)
		{
			var result = Array.Empty<string>();

			if (string.IsNullOrEmpty(dbServer))
			{
				return result;
			}

			try
			{
				if (requirePrint)
				{
					FireOnTaskStarted(string.Format(CultureInfo.InvariantCulture, "Getting List of Databases\r\n\r\nServer: {0}", dbServer));
				}
				var dbList = new StringCollection();

				var sqlText = @"
						SELECT
							name
						FROM
							sys.databases
						WHERE
							name not in ('master', 'tempdb', 'model', 'msdb')
							and name not like 'DBUPG[_]%'
						ORDER BY
							name ASC";

				using (Db.DisableSchemaVersionCheck())
				using (DbConnection conn = Db.NewAdminConnection(dbServer, Db.SqlMasterDb))
				using (var cmd = conn.Command(sqlText))
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						dbList.Add(reader.GetString(0));
					}
				}

				result = new string[dbList.Count];
				dbList.CopyTo(result, 0);
				if (requirePrint)
				{
					FireOnTaskCompleted("Finished getting list of databases");
				}
			}
			catch (SqlException sqlEx) when (new DbErrorMatch(sqlEx).IsInfrastructureDbError)
			{
				FireOnTaskFailed($"Failed to connect to Sql Server. It might be triggered by incorrect Server value. If problem persists, please contact your administrator.\r\n\r\nDetail Message: {sqlEx.Message}");
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				if (requirePrint)
				{
					FireOnTaskFailed("Error getting list of databases\r\n\r\n" + ex.Message);
				}
				//throw;
			}

			return result;
		}

		IEnumerable<string> GetDbDropList(DbConnection conn, string mainDatabaseName, bool includeOperationalDbs, bool includeRefFilesDbs)
		{
			var result = new List<string>();

			result.Add(mainDatabaseName);

			foreach (var dbName in GetRelatedDatabases(conn, mainDatabaseName))
			{
				var dbType = GetDbTypeFromDbName(mainDatabaseName, dbName);

				if (includeOperationalDbs && (dbType == DbType.eDocs || dbType == DbType.UserRepository))
				{
					result.Add(dbName);
				}
				else if (includeRefFilesDbs && dbType == DbType.ReferenceDatabase)
				{
					result.Add(dbName);
				}
			}

			return result;
		}

		enum DbType { eDocs, ReferenceDatabase, UserRepository, Other }

		DbType GetDbTypeFromDbName(string mainDatabaseName, string dbName)
		{
			if (RefDbTableNameResolver.IsExclusiveDatabase(mainDatabaseName, dbName))
			{
				return DbType.ReferenceDatabase;
			}

			if (dbName.StartsWith(string.Format(CultureInfo.CurrentCulture, "{0}{1}", mainDatabaseName, "_SD"), StringComparison.OrdinalIgnoreCase))
			{
				return DbType.eDocs;
			}

			if (string.Equals(dbName, mainDatabaseName + "_UserRepository", StringComparison.OrdinalIgnoreCase))
			{
				return DbType.UserRepository;
			}

			return DbType.Other;
		}

		IEnumerable<string> GetRelatedDatabases(DbConnection connection, string mainDatabaseName)
		{
			var sqlText = string.Format(CultureInfo.CurrentCulture, @"
SELECT name
FROM sys.databases
WHERE name like '{0}[_]SD[0-9][0-9][0-9]'
   OR name like '{0}[_]RefDb[_]___[_]__'
   OR name like '{0}[_]UserRepository'
",
				mainDatabaseName);
			return DataUtils.GetListOfValuesFromQuery(connection, sqlText);
		}
	}
}
