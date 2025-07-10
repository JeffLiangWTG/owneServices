using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using CargoWise.Data;
using CargoWise.Data.SqlServer;
using CargoWise.DbUpgrader.Foundation;
using Enterprise.ChangeDataCapture.Common;
using Enterprise.DbUpgrader.Shared;
using Enterprise.ZArchitecture.AlwaysOnHelper;

namespace Enterprise.DbUpgrader.Startup
{
	class ChangeDataCaptureManager
	{
		public ChangeDataCaptureManager(AdminConnection mainDbConnection, AdminConnection auditConnection, AdminConnection dataWarehouseConnection, IUpgradeTaskWorkflowLogger logger)
		{
			this.mainDbConnection = mainDbConnection;
			this.auditConnection = auditConnection;
			this.dataWarehouseConnection = dataWarehouseConnection;
			this.logger = logger;
		}
		readonly AdminConnection mainDbConnection;
		readonly AdminConnection auditConnection;
		readonly AdminConnection dataWarehouseConnection;
		readonly IUpgradeTaskWorkflowLogger logger;

		protected virtual string MainDbName => Db.DatabaseName;
		protected virtual string AuditDbName => MainDbName + Db.AuditDatabaseSuffix;
		protected virtual string EdwDbName => MainDbName + Db.EdwDatabaseSuffix;

		public void DisableCdcAndCleanupBiDatabases()
		{
			if (DbRegistry.BiResetChangeDataCapture.LoadValue(mainDbConnection))
			{
				AlterDbAuthorisation(mainDbConnection, MainDbName);
				var isDbStateRestoring = false;

				if (CdcDatabase.IsEnabled(mainDbConnection, MainDbName))
				{
					logger.ShowInfoMessage("Disabling Change Data Capture for main database");
					CdcDatabase.DisableCaptureInstances(mainDbConnection);
					CdcDatabase.Disable(mainDbConnection, MainDbName);
				}

				if (auditConnection != null)
				{
					logger.ShowInfoMessage("Audit database cleanup");
					AlterDbAuthorisation(auditConnection, AuditDbName);
					var isPartOfAlwaysOn = AlwaysOn.IsDbPartOfAlwaysOn(auditConnection, AuditDbName);
					if (isPartOfAlwaysOn)
					{
						// Removing database from the AlwaysOn setup
						var alwaysOnReplicas = GetAlwaysOnSecondaryReplicaNamesList(AuditDbName);
						var replicaCount = alwaysOnReplicas.Count;
						if (RemoveDatabaseFromAlwaysOnSetup(AuditDbName))
						{
							for (int replicaInd = 0; replicaInd < alwaysOnReplicas.Count; replicaInd++)
							{
								var replicaName = alwaysOnReplicas[replicaInd];

								// Dropping database on AlwaysOn replica
								try
								{
									using (var replicaConnection = GetAdminConnectionToReplica(replicaName))
									{
										var dbStateDesc = string.Empty;
										dbStateDesc = GetDatabaseStateDescription(replicaConnection, AuditDbName);

										if (dbStateDesc.Equals("RESTORING"))
										{
											DropDb(replicaConnection, AuditDbName);
											isDbStateRestoring = true;
											logger.StartSubtask(String.Format("Database [{0}] has been dropped on server [{1}] ({2}/{3}).", AuditDbName, replicaName, replicaInd + 1, replicaCount));
										}
										else
										{
											logger.StartSubtask(String.Format("Database [{0}] cannot be dropped on server [{1}] ({2}/{3}). The database state is '{4}'", AuditDbName, replicaName, replicaInd + 1, replicaCount, dbStateDesc));
										}
									}
								}
								catch (SqlException ex)
								{
									var exceptionType = new DbErrorMatch(ex).ExceptionType;
									switch (exceptionType)
									{
										case DbErrorType.SevereError:
										case DbErrorType.ServerDoesNotExist:
										case DbErrorType.GeneralNetworkError:
											logger.StartSubtask(String.Format("Database [{0}] cannot be dropped on server [{1}] ({2}/{3}). The server is unavailable", AuditDbName, replicaName, replicaInd + 1, replicaCount));
											break;
										default:
											throw;
									}
								}
							}
						}
						else
						{
							logger.StartSubtask(String.Format("Database [{0}] cannot be removed from AlwaysOn setup.", AuditDbName));
						}
					}
					if (!isPartOfAlwaysOn || isDbStateRestoring)
					{
						DropBiDatabase(auditConnection, AuditDbName);
					}

					if (!auditConnection.ServerNameReportedByDatabase.Equals(mainDbConnection.ServerNameReportedByDatabase))
					{
						DropApplicationLogins(auditConnection);
						DropAllStaffAssignedLogins(auditConnection);
					}
				}

				if (dataWarehouseConnection != null)
				{
					logger.ShowInfoMessage("EDW database cleanup");
					AlterDbAuthorisation(dataWarehouseConnection, EdwDbName);
					DropBiDatabase(dataWarehouseConnection, EdwDbName);

					if (!dataWarehouseConnection.ServerNameReportedByDatabase.Equals(mainDbConnection.ServerNameReportedByDatabase))
					{
						DropApplicationLogins(dataWarehouseConnection);
					}
				}

				DbRegistry.BiResetChangeDataCapture.SaveValue(false, mainDbConnection);
			}
		}

		void AlterDbAuthorisation(AdminConnection connection, string dbName)
		{
			if (connection.DatabaseExists(dbName))
			{
				DataUtils.AlterDbAuthorisation(connection, dbName);
			}
		}

		protected virtual List<string> GetAlwaysOnSecondaryReplicaNamesList(string dbName)
		{
			return new List<string>(AlwaysOnHelper.GetAlwaysOnSecondaryReplicaNames(dbName, useCache: false));
		}

		protected virtual bool RemoveDatabaseFromAlwaysOnSetup(string dbName)
		{
			return AlwaysOn.RemoveDatabaseFromAlwaysOnSetup(dbName);
		}

		protected virtual AdminConnection GetAdminConnectionToReplica(string replicaName)
		{
			return Db.NewAdminConnection(replicaName, Db.SqlMasterDb);
		}

		protected virtual string GetDatabaseStateDescription(AdminConnection replicaConnection, string dbName)
		{
			var tryCount = 0;
			var dbStateDesc = string.Empty;

			while (tryCount < TryCount && !(dbStateDesc = replicaConnection.DatabaseStateDescription(dbName)).Equals("RESTORING"))
			{
				Thread.Sleep(++tryCount < TryCount ? 3000 : 0);
			}
			return dbStateDesc;
		}

		protected virtual void DropDb(AdminConnection replicaConnection, string dbName)
		{
			new DbRemover(dbName).Drop(replicaConnection);
		}

		public void DropBiDatabase(DbConnection biConnection, string biDatabaseName)
		{
			logger.ShowInfoMessage($"Deleting DB backup history: {biDatabaseName}");
			biConnection.ExecuteNonQuery($"EXEC msdb.dbo.sp_delete_database_backuphistory @database_name = '{biDatabaseName}'");

			if (biConnection.DatabaseExists(biDatabaseName))
			{
				logger.ShowInfoMessage(string.Format(CultureInfo.InvariantCulture, "Dropping database: {0}", biDatabaseName));
				biConnection.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture, DropBiDatabaseCmdText, biDatabaseName));
			}
			else
			{
				logger.ShowInfoMessage(string.Format(CultureInfo.InvariantCulture, "Cannot drop database [{0}] because it does not exist.", biDatabaseName));
			}
		}

		internal const string DropBiDatabaseCmdText = @"ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{0}];";

		void DropApplicationLogins(DbConnection biConnection)
		{
			var dropLoginSql = @"IF EXISTS (SELECT name FROM sys.server_principals WHERE name='{0}') DROP LOGIN [{0}]";
			var loginList = Db.GetAllLoginNames(MainDbName);

			foreach (var login in loginList)
			{
				logger.ShowInfoMessage(string.Format(CultureInfo.InvariantCulture, "Dropping login if it exists: {0}", login));
				biConnection.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture, dropLoginSql, login));
			}
		}

		void DropAllStaffAssignedLogins(DbConnection biConnection)
		{
			logger.ShowInfoMessage("Dropping staff assigned logins if they exist.");
			biConnection.ExecuteNonQuery(GetDropAllStaffAssignedLoginsQuery(MainDbName));
		}

		string GetDropAllStaffAssignedLoginsQuery(string dbName)
		{
			return $@"
DECLARE @sqlCmd VARCHAR(MAX) = '';
SELECT @sqlCmd = @sqlCmd + 'DROP LOGIN ' + quotename(name) + '; '
FROM 
	(
		SELECT name
		FROM sys.server_principals 
		WHERE name LIKE '{DataUtils.ReplaceSqlLikeWildcard(DbUserRepository.GetStaffDbLoginFullPrefix(dbName))}%'
		UNION
		SELECT name
		FROM sys.server_principals 
		WHERE default_database_name = '{dbName}'
	) tbl
IF (@sqlCmd != '') EXEC (@sqlCmd);";
		}

		internal int TryCount = 20;
	}
}
