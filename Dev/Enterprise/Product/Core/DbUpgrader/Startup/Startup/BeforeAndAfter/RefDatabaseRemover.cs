using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.SqlServer;
using CargoWise.DbUpgrader.Foundation;
using Enterprise.DbUpgrader.Shared;
using Enterprise.ZArchitecture.AlwaysOnHelper;

namespace Enterprise.DbUpgrader.Startup
{
	class RefDatabaseRemover
	{
		public void DropUnusedDatabases(IUpgradeContext upgradeContext, AdminConnection mainConnection, IUpgradeTaskWorkflowLogger logger)
		{
			try
			{
				var dropList = GetDatabasesToDrop(upgradeContext, mainConnection, logger);
				var refDbCount = dropList.Count;
				logger.StartTask(CleaningStartLog(refDbCount));

				if (refDbCount > 0)
				{
					for (int refDbInd = 0; refDbInd < refDbCount; refDbInd++)
					{
						var refDbName = dropList[refDbInd];
						if (IsDbPartOfAlwaysOn(mainConnection, refDbName))
						{
							// Removing database from the AlwaysOn setup
							var alwaysOnReplicas = GetAlwaysOnSecondaryReplicaNamesList(refDbName);
							var replicaCount = alwaysOnReplicas.Count;
							if (RemoveDatabaseFromAlwaysOnSetup(refDbName))
							{
								for (int replicaInd = 0; replicaInd < alwaysOnReplicas.Count; replicaInd++)
								{
									var replicaName = alwaysOnReplicas[replicaInd];

									// Dropping database on AlwaysOn replica
									try
									{
										using (var replicaConnection = GetAdminConnectionToReplica(replicaName))
										{
											var tryCount = 0;
											var dbStateDesc = "";
											while (tryCount < TryCount && !(dbStateDesc = GetDatabaseStateDescription(replicaConnection, refDbName)).Equals("RESTORING"))
											{
												Thread.Sleep(++tryCount < TryCount ? 3000 : 0);
											}

											if (dbStateDesc.Equals("RESTORING"))
											{
												DropRefDb(replicaConnection, refDbName);
												logger.StartSubtask(String.Format("Database [{0}] ({2}/{3}) has been dropped on server [{1}] ({4}/{5}).", refDbName, replicaName, refDbInd + 1, refDbCount, replicaInd + 1, replicaCount));
											}
											else
											{
												logger.StartSubtask(String.Format("Database [{0}] ({2}/{3}) cannot be dropped on server [{1}] ({4}/{5}). The database state is '{6}'", refDbName, replicaName, refDbInd + 1, refDbCount, replicaInd + 1, replicaCount, dbStateDesc));
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
												logger.StartSubtask(String.Format("Database [{0}] ({2}/{3}) cannot be dropped on server [{1}] ({4}/{5}). The server is unavailable", refDbName, replicaName, refDbInd + 1, refDbCount, replicaInd + 1, replicaCount));
												break;
											default:
												throw;
										}
									}
								}
							}
							else
							{
								logger.StartSubtask(String.Format("Database [{0}] ({1}/{2}) cannot be removed from AlwaysOn setup.", refDbName, refDbInd + 1, refDbCount));
								continue;
							}
						}

						DropRefDb(mainConnection, refDbName);
						logger.StartSubtask(String.Format("Database [{0}] ({2}/{3}) has been dropped on server [{1}]", refDbName, mainConnection.ServerName, refDbInd + 1, refDbCount));
					}
				}
				else
				{
					logger.StartSubtask("No databases to drop detected.");
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				var message = ex.Message;
				if (ex.InnerException != null)
				{
					message += System.Environment.NewLine + ex.InnerException.Message;
				}

				logger.StartSubtask(message);
			}
		}

		protected virtual List<string> GetDatabasesToDrop(IUpgradeContext upgradeContext, AdminConnection mainConnection, IUpgradeTaskWorkflowLogger logger)
		{
			var dropList = new List<string>();

			// synonym name  : RefDbEntUS_USCACCase
			// synonym object: [Test_RefDb_Ent_US].[dbo].[USCACCase]
			// synonym object: [CW-RefDb-Ent-US-000130].[dbo].[USCACCase]
			// synonym object: [CW-AG-RefDb-ORDWP4-CP1AS1-Ent-US-000130].[dbo].[USCACCase]
			var sql = @"
;WITH
	MigratedToShared AS (
			SELECT DISTINCT
				IndividualRefDbName = CONCAT(DB_NAME(), '_RefDb_', SUBSTRING(name, 6, 3), '_', SUBSTRING(name, 9, 2)) COLLATE database_default
			FROM
				sys.synonyms
			WHERE 1=1
				AND
				(
					base_object_name LIKE N'%CW-RefDb%'
					OR base_object_name LIKE N'%CW-AG-RefDb%'
				)
				AND base_object_name NOT LIKE N'%CW-RefDb-Xxx-XX-0%'
		)
SELECT
	s.IndividualRefDBName
FROM
	MigratedToShared   AS s
	JOIN sys.databases AS d ON d.name = s.IndividualRefDBName
ORDER BY
	s.IndividualRefDBName
";

			mainConnection.ExecuteReader(sql, (reader) =>
			{
				dropList.Add((string)reader["IndividualRefDBName"]);
			});

			return dropList;
		}

		protected virtual string CleaningStartLog(int refDbCount)
		{
			return FormattableString.Invariant($"Removing unused exclusive reference databases due to migrating them to shared reference databases ({refDbCount})");
		}

		protected virtual bool IsDbPartOfAlwaysOn(DbConnection connection, string refDbName)
		{
			return AlwaysOn.IsDbPartOfAlwaysOn(connection, refDbName);
		}

		protected virtual List<string> GetAlwaysOnSecondaryReplicaNamesList(string refDbName)
		{
			return new List<string>(AlwaysOnHelper.GetAlwaysOnSecondaryReplicaNames(refDbName, useCache: false));
		}

		protected virtual bool RemoveDatabaseFromAlwaysOnSetup(string refDbName)
		{
			return AlwaysOn.RemoveDatabaseFromAlwaysOnSetup(refDbName);
		}

		protected virtual void DropRefDb(AdminConnection replicaConnection, string refDbName)
		{
			new DbRemover(refDbName).Drop(replicaConnection);
		}

		protected virtual AdminConnection GetAdminConnectionToReplica(string replicaName)
		{
			return Db.NewAdminConnection(replicaName, Db.SqlMasterDb);
		}

		protected virtual string GetDatabaseStateDescription(AdminConnection replicaConnection, string refDbName)
		{
			return replicaConnection.DatabaseStateDescription(refDbName);
		}

		internal int TryCount = 20;
	}
}
