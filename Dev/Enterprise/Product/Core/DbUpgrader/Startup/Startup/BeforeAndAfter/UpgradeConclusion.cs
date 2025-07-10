using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.DbUpgrader.Foundation;
using Enterprise.DbUpgrader.Shared;
using Enterprise.Environment;
using Enterprise.ServiceManager.Tasks.DbMaintenance;
using Enterprise.ServiceManager.Tasks.DbSecurityAdmin;
using Enterprise.ZArchitecture.Environment;
using static System.FormattableString;

namespace Enterprise.DbUpgrader.Startup
{
	partial class UpgradeConclusion
	{
		public UpgradeConclusion(AdminConnection mainDbConnection)
		{
			this.mainDbConnection = mainDbConnection;
		}

		protected readonly AdminConnection mainDbConnection;
		protected readonly string mainDbName = Db.DatabaseName;

		public virtual void RunAfterUpgradeSteps(IUpgradeContext upgradeContext, IUpgradeTaskWorkflowLogger logger)
		{
			DropDuplicatedStatistics(logger);
			ShrinkLogFile(logger);
			PerformLoginMaintenanceTasks(logger);
			PerformDatabaseTuningTasks();
			RunCleanupTasks(upgradeContext, logger);
		}

		void DropDuplicatedStatistics(IUpgradeTaskWorkflowLogger logger)
		{
			logger.ShowInfoMessage(".");
			logger.StartTask($"--- Drop Duplicated Statistics - START ---");
			var sql = string.Format(CultureInfo.InvariantCulture, @"
;WITH
	duplicated AS (
			SELECT
				sch_name   = sch.name,
				obj_name   = obj.name,
				stats_name = s.name
				, s.object_id, s.stats_id
				, s.auto_created, s.user_created
				, s.has_filter, s.filter_definition
				, sts_def.definition
			FROM
				sys.schemas AS sch
				JOIN sys.objects AS obj ON obj.schema_id = sch.schema_id
				JOIN sys.stats AS s ON s.object_id = obj.object_id
					AND (s.auto_created = 1 OR s.user_created = 1)

				CROSS APPLY
				(
					SELECT TOP (1)
						definition = {0}.dbo.CLRCssvAgg(sc.column_id) OVER (PARTITION BY sc.object_id, sc.stats_id) + ','
					FROM
						sys.stats_columns AS sc
					WHERE 1=1
						AND sc.object_id = s.object_id AND sc.stats_id = s.stats_id
					ORDER BY
						sc.stats_column_id
				) AS sts_def
			WHERE 1=1
				AND obj.is_ms_shipped = 0
				AND obj.type in ('U', 'V')
		)
	, covering AS (
			SELECT
				s.object_id, s.stats_id
				, s.has_filter
				, s.filter_definition
				, definition = ISNULL(ind_def.definition, sts_def.definition)
			FROM
				sys.stats AS s
				OUTER APPLY
				(
					SELECT TOP (1)
						definition = {0}.dbo.CLRCssvAgg(sc.column_id) OVER (PARTITION BY sc.object_id, sc.stats_id) + ','
					FROM
						sys.stats_columns AS sc
					WHERE 1=1
						AND s.user_created = 1
						AND sc.object_id = s.object_id AND sc.stats_id = s.stats_id
					ORDER BY
						sc.stats_column_id
				) AS sts_def

				OUTER APPLY
				(
					SELECT TOP (1)
						definition = {0}.dbo.CLRCssvAgg(sc.column_id) OVER (PARTITION BY sc.object_id, sc.index_id) + ','
					FROM
						sys.index_columns AS sc
					WHERE 1=1
						AND s.user_created = 0
						AND sc.object_id = s.object_id AND sc.index_id = s.stats_id
						AND sc.is_included_column = 0
					ORDER BY
						sc.key_ordinal
				) AS ind_def
			WHERE 1=1
				AND s.auto_created = 0
		)
SELECT
	stmt = N'DROP STATISTICS ' + QUOTENAME(sch_name) + N'.' + QUOTENAME(obj_name) + N'.' + QUOTENAME(stats_name) + N';'
FROM
	duplicated
WHERE
	EXISTS
	(
		SELECT NULL
		FROM
			covering
		WHERE 1=1
			AND covering.object_id = duplicated.object_id
			AND covering.stats_id <> duplicated.stats_id
			AND covering.definition LIKE duplicated.definition + '%'
			AND
			(
				duplicated.has_filter = 0 AND covering.has_filter = 0
				OR duplicated.has_filter = 1 AND covering.filter_definition = duplicated.filter_definition
			)
	)

"
				, ((ICurrentDbControl)mainDbConnection).InitialDatabase.QuoteName() // 0
				);

			foreach (var dbName in mainDbConnection.GetDatabases(DatabaseType.All & ~DatabaseType.EDW))
			{
				using (((ICurrentDbControl)mainDbConnection).UseDatabase(dbName))
				{
					logger.StartSubtask($"{dbName}...");

					var stmts = new List<string>();
					mainDbConnection.ExecuteReader(sql, (reader) => stmts.Add((string)reader["stmt"]));

					foreach (var stmt in stmts)
					{
						try
						{
							mainDbConnection.ExecuteNonQuery(stmt);
						}
						catch (SqlException ex) when (!ex.IsCriticalException())
						{
							logger.ShowInfoMessage(string.Format(CultureInfo.CurrentCulture, "Error dropping duplicated stats {0} ({1})\r\n", stmt, ex.Message));
						}
					}

					logger.ShowInfoMessage(Invariant($"Dropped {stmts.Count} duplicated statistics"));
				}
			}

			logger.ShowInfoMessage(".");
			logger.StartTask("--- Drop Duplicated Statistics - END ---");
			logger.ShowInfoMessage(".");
		}

		#region Shrink Log File

		void ShrinkLogFile(IUpgradeTaskWorkflowLogger logger)
		{
			if (
#if DEBUG
			NUnit.Framework.TestingState.IsRunningOnDAT ||
#endif
			Env.Registry.SkipLogShrinkingAndBackupInUpgrade)
			{
				return;
			}

			try
			{
				logger.ShowInfoMessage(".");
				logger.StartTask("--- Shrink log file - START ---");

				ShrinkLogFileOnDatabase(logger, mainDbConnection, Db.DatabaseName);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				logger.ShowInfoMessage(ex.Message);
			}
			finally
			{
				logger.ShowInfoMessage(".");
				logger.StartTask("--- Shrink log file - END   ---");
				logger.ShowInfoMessage(".");
			}
		}

		string BackupFilePath
		{
			get
			{
				if (backupFilePath == null)
				{
					using (((ICurrentDbControl)mainDbConnection).UseDatabase(Db.DatabaseName))
					{
						backupFilePath = DbRegistry.BackupFilePath.LoadValue(mainDbConnection);
					}
				}
				return backupFilePath;
			}
		}
		string backupFilePath;

		protected virtual int TargetLogSize
		{
			get
			{
				return 0;
			}
		}

		protected void ShrinkLogFileOnDatabase(IUpgradeTaskWorkflowLogger logger, DbConnection conn, string dbName)
		{
			if (conn != null && conn.DatabaseExists(dbName))
			{
				try
				{
					object message;
					var maximumAllowedVLFsCount = DbRegistry.MaximumAllowedVLFsCount.LoadValue(conn);
					var maximumLogToTwoWeekBackupPercentage = DbRegistry.MaximumLogToTwoWeekBackupPercentage.LoadValue(conn);

					using (((ICurrentDbControl)conn).UseDatabase(dbName))
					using (DbCommand shrinkCommand = conn.Command("ep_ShrinkLogFile"))
					{
						shrinkCommand.CommandTimeout = 0;
						shrinkCommand.CommandType = CommandType.StoredProcedure;
						shrinkCommand.AddParameter("@BKPath", SqlDbType.NVarChar, BackupFilePath);
						shrinkCommand.AddParameter("@MAXVLFs", SqlDbType.Int, maximumAllowedVLFsCount);
						shrinkCommand.AddParameter("@maximumLogToTwoWeekBackupPercentage", SqlDbType.Int, maximumLogToTwoWeekBackupPercentage);

						if (TargetLogSize > 0)
						{
							shrinkCommand.AddParameter("@TargetLog_MB", SqlDbType.Int, TargetLogSize);
						}

#if DEBUG
						shrinkCommand.AddParameter("@TargetLogGrowth_MB", SqlDbType.Int, 1);
#endif

						shrinkCommand.AddOutputParameter("@MSG", SqlDbType.NVarChar, 65535, 0, 0, 0);
						shrinkCommand.ExecuteNonQuery();
						message = shrinkCommand.GetParameterValue("@MSG");
					}

					var shrinkMsg = (message == DBNull.Value) ? string.Empty : message.ToString();
					foreach (string process in shrinkMsg.Split('|'))
					{
						logger.ShowInfoMessage(process);
					}
				}
				catch (SqlException e)
				{
					if (e.Number == 50000)
					{
						logger.ShowInfoMessage("Could not shrink log. " + e.Message);
					}
					else
					{
						throw;
					}
				}
			}
		}

		#endregion

		void PerformLoginMaintenanceTasks(IUpgradeTaskWorkflowLogger logger)
		{
			var dbSecurity = new DbSecurity();
			dbSecurity.RefreshDbReaderRolePermissions(mainDbConnection, msg => Log(logger, msg));
			dbSecurity.RefreshSchemaDbRoles(mainDbConnection, msg => Log(logger, msg));

			((IDbLoginRepair)mainDbConnection).DropDbLoginUsersFromDatabase(Db.SqlMasterDb, msg => Log(logger, msg));
			((IDbLoginRepair)mainDbConnection).EnsureDbLoginsCorrectlyMappedToAllDatabases(msg => Log(logger, msg));
		}

		static void Log(IUpgradeTaskWorkflowLogger logger, string msg)
		{
			if (!string.IsNullOrWhiteSpace(msg))
			{
				logger.ShowInfoMessage(msg);
			}
		}

		void PerformDatabaseTuningTasks()
		{
			StatisticsSwitch.Instance.CheckAndTurnStatistics(mainDbName, isON: true);
		}

		#region Run Cleanup Tasks

		void RunCleanupTasks(IUpgradeContext upgradeContext, IUpgradeTaskWorkflowLogger logger)
		{
			UpgUtils.CleanupAuxDatabases(mainDbConnection, mainDbName);

			new RefDatabaseRemover().DropUnusedDatabases(upgradeContext, mainDbConnection, logger);
			new UnsupportedRefDatabaseRemover().DropUnusedDatabases(upgradeContext, mainDbConnection, logger);

			if (EnvProxy.Instance.Registry.UseModernSqlSecuritySystem)
			{
				SynchroniseSqlSecurity(logger);
			}

			PropagateAlwaysOnDatabases(logger);

			RegistryItemDictionary.Instance.PurgeAll();
		}

		void SynchroniseSqlSecurity(IUpgradeTaskWorkflowLogger logger)
		{
			try
			{
				logger.StartTask("Synchronising Sql security");

				var dsaServiceTask = new DbSecurityAdminTask();
				dsaServiceTask.ServiceLogger = new UpgradeManagerLoggerProxy(logger, UpgradeManager.HandleLogOnlyException);
				dsaServiceTask.RunTask(CancellationToken.None);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				const string message = "Failed to synchronise Sql security";
				logger.ShowInfoMessage(message + System.Environment.NewLine + ex.ToString());
				ErrorReporter.ReportOnce(message, new UpgradeManagerException(ex));
			}
		}

		void PropagateAlwaysOnDatabases(IUpgradeTaskWorkflowLogger logger)
		{
			try
			{
				logger.StartTask("Propagating AlwaysOn databases");

				var aomServiceTask = new AlwaysOnManagementServiceTask(new UpgradeManagerLoggerProxy(logger, UpgradeManager.HandleLogOnlyException));
				aomServiceTask.RunTask(CancellationToken.None);
			}
			catch (SqlException ex) when (!ex.IsCriticalException())
			{
				const string message = "Failed to propagate AlwaysOn databases";
				logger.ShowInfoMessage(message + System.Environment.NewLine + ex.ToString());
				ErrorReporter.ReportOnce(message, new UpgradeManagerException(ex));
			}
		}

		#endregion // Run Cleanup Tasks
	}
}
