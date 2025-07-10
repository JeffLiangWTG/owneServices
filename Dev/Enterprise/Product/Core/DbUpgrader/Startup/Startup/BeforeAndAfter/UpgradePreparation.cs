using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using CargoWise.Bi.Common;
using CargoWise.Data;
using CargoWise.DbUpgrader.Foundation;
using Enterprise.ChangeDataCapture.Common;
using Enterprise.DbUpgrader.Shared;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DbUpgrader.Startup
{
	public interface IUpgradePreparation
	{
		void ApplyServerConfigurationSettings();
		void SetAutoStatsOnModelDatabase();
		void AlterStorageDocDatabaseAutoCreateStatistics();
		void TurnOffMainDbStatistics();
		void EnsureMainDatabaseSettings(IUpgradeTaskWorkflowLogger logger);
		void AdjustAllDatabasesRecoveryModels(IUpgradeTaskWorkflowLogger logger);
		void EnsureCargoWiseDatabaseSettings(IEnumerable<string> allDatabases);
		void EnableCdc(IUpgradeTaskWorkflowLogger logger);
		void ManageFilegroups();
	}

	class UpgradePreparation : IUpgradePreparation
	{
		public UpgradePreparation(AdminConnection connection)
		{
			this.connection = connection;
			mainDbName = ((ICurrentDbControl)connection).InitialDatabase;
		}

		readonly AdminConnection connection;
		readonly string mainDbName;

		public void ApplyServerConfigurationSettings()
		{
			DataUtils.SetServerConfigOption(connection, "clr enabled", "1");
			DataUtils.SetServerConfigOption(connection, "max text repl size (B)", "-1");
			DataUtils.SetServerConfigOption(connection, "default language", "0");
			DataUtils.SetServerConfigOption(connection, "remote access", "1");
		}
		
		public void SetAutoStatsOnModelDatabase()
		{
			var stats = StatisticsSwitch.Instance.GetStatisticsSettings(connection, "model");
			if (!stats.Current.AutoCreate || !stats.Current.AutoUpdate || !stats.Current.AutoUpdateAsync)
			{
				StatisticsSwitch.Instance.SetDbStatisticsSettings(connection, "model", true, true, true);
			}
		}

		#region CDC and Filegroups

		public virtual bool ShouldEnableCdc()
		{
			var isCdcNotDisabledByRegistry = !IsCdcDisabledOnRegistry();
			return
#if DEBUG
			!Globals.IsTest &&
			!NUnit.Framework.TestingState.IsRunningOnDAT &&
#endif
			isCdcNotDisabledByRegistry && IsEdwOrAuditServerSet;
		}

		bool IsEdwOrAuditServerSet => !string.IsNullOrEmpty(BiServers.LoadAuditServerUsingCacheIfPossible(connection)) ||
								!string.IsNullOrEmpty(BiServers.LoadDataWarehouseServerUsingCacheIfPossible(connection));

		public virtual bool IsCdcDisabledOnRegistry()
		{
			return DbRegistry.BiDisableChangeDataCapture.LoadValue(connection);
		}

		public void EnableCdc(IUpgradeTaskWorkflowLogger logger)
		{
			if (ShouldEnableCdc())
			{
				if (!CdcDatabase.IsEnabled(connection, mainDbName))
				{
					logger.ShowInfoMessage("Enabling CDC on database");
					CdcDatabase.Enable(connection, mainDbName);
				}
				else
				{
					try
					{
						CdcDatabase.DisableAndStopJobsAndTrigger(connection, mainDbName);
					}
					catch (SqlException ex)
					{
						logger.ShowInfoMessage(String.Format(CultureInfo.InvariantCulture, "Encountered the following error while attempting to disable CDC jobs or triggers. Proceeding with database upgrade.\r\n{0}", ex.Message));
					}
				}
			}
			else if (CdcDatabase.IsEnabled(connection, mainDbName))
			{
				logger.ShowInfoMessage("Disabling CDC on database");
				DbRegistry.BiDisableChangeDataCapture.SaveValue(false, connection);
				CdcDatabase.DisableCaptureInstances(connection);
				CdcDatabase.Disable(connection, mainDbName);
			}
		}

		public void ManageFilegroups()
		{
			var fileGroupCreator = new DatabaseFileGroupCreator(connection, mainDbName);

			CreateSecondaryFilegroupAndFileIfNotExists_DebugOnly(fileGroupCreator);

			if (CdcDatabase.IsEnabled(connection, mainDbName) || (Globals.IsDebugMode && !Globals.IsTest))
			{
				var cdcFileGroupName = CdcDatabase.FileGroup;
				CreateFileGroupAndSetFileGrowth(fileGroupCreator, cdcFileGroupName, "DataCdc", 1,
					CdcFilegroupInitialSizeInGb, CdcFilegroupGrowthSize, CdcFilegroupGrowthSizeUnit);
			}

			var reportFileGroup = "REPORTGROUP";
			CreateFileGroupAndSetFileGrowth(fileGroupCreator, reportFileGroup, "DataReport", 4,
				ReportFilegroupInitialSizeInGb, ReportFilegroupGrowthSize, ReportFilegroupGrowthSizeUnit);
		}

		void CreateFileGroupAndSetFileGrowth(DatabaseFileGroupCreator fileGroupCreator, string fileGroupName, string fileSuffix, int numberOfFiles,
			int? initialSizeInGb, int? fileGroupGrowthSize, string fileGroupGrowthSizeUnit)
		{
			var fileGroupInfo = fileGroupCreator.GetFileGroupInfo(fileGroupName);
			if (!fileGroupInfo.Any())
			{
				fileGroupCreator.Create(fileGroupName, fileSuffix, numberOfFiles, initialSizeInGb, fileGroupGrowthSize, fileGroupGrowthSizeUnit);
			}
			else
			{
				if (fileGroupInfo.Any(x => x.IsFileGrowthSizeLessThanMinimum(fileGroupGrowthSize.Value, fileGroupGrowthSizeUnit)) || (Globals.IsDebugMode && !Globals.IsTest))
				{
					fileGroupCreator.ChangeFileGrowthSize(fileGroupName, fileGroupGrowthSize.Value, fileGroupGrowthSizeUnit);
				}
			}
		}

		int? CdcFilegroupInitialSizeInGb
		{
			get
			{
#if DEBUG
				return null;
#else
				return 1;
#endif
			}
		}

		protected virtual int? CdcFilegroupGrowthSize
		{
			get { return 1; }
		}

		protected virtual string CdcFilegroupGrowthSizeUnit
		{
#if DEBUG
			get { return "MB"; }
#else
			get { return "GB"; }
#endif
		}

		int? ReportFilegroupInitialSizeInGb
		{
			get
			{
#if DEBUG
				return null;
#else
				return 1;
#endif
			}
		}

		protected virtual int? ReportFilegroupGrowthSize
		{
			get { return 1; }
		}

		protected virtual string ReportFilegroupGrowthSizeUnit
		{
#if DEBUG
			get { return "MB"; }
#else
			get { return "GB"; }
#endif
		}

		[Conditional("DEBUG")]
		void CreateSecondaryFilegroupAndFileIfNotExists_DebugOnly(DatabaseFileGroupCreator fileGroupCreator)
		{
			fileGroupCreator.Create("SECONDARY", "Data02");
		}

		protected virtual int? InitialFileSizeInGb
		{
			get { return 1; }
		}

		#endregion

		#region StorageDocs Stats

		public void AlterStorageDocDatabaseAutoCreateStatistics()
		{
			var eDocs = connection.GetDatabases(DatabaseType.SD);
			foreach (var dbName in eDocs)
			{
				var stats = StatisticsSwitch.Instance.GetStatisticsSettings(connection, dbName);
				if (stats.Current.AutoCreate || !stats.Current.AutoUpdate || !stats.Current.AutoUpdateAsync)
				{
					var isReadOnly = !connection.IsDbWriteable(dbName);

					try
					{
						// set read-only db as writeable
						if (isReadOnly)
						{
							connection.AlterDbWriteableState(dbName, writeable: true);
						}

						// set auto create stats to OFF
						StatisticsSwitch.Instance.SetDbStatisticsSettings(connection, dbName, false, true, true);

						// remove all auto created stats
						RemoveAutoCreatedStats(dbName);
					}
					finally
					{
						// set db back as read-only if applicable
						if (isReadOnly)
						{
							connection.AlterDbWriteableState(dbName, writeable: false);
						}
					}
				}
			}
		}

		void RemoveAutoCreatedStats(string dbName)
		{
			var sql = @"
DECLARE @stmt nvarchar(max) = '';
SELECT
	@stmt += 'DROP STATISTICS [' + sch.name + '].[' + o.name + '].[' + s.name + '];'
FROM
	sys.objects      AS o
	JOIN sys.schemas AS sch ON sch.schema_id = o.schema_id
	JOIN sys.stats   AS s   ON s.object_id = o.object_id
WHERE
	o.is_ms_shipped = 0
	AND s.auto_created = 1;

if (LEN(@stmt) > 0) EXEC(@stmt);
";

			using (((ICurrentDbControl)connection).UseDatabase(dbName))
			using (var cmd = connection.Command(sql))
			{
				cmd.ExecuteNonQuery();
			}
		}

		#endregion

		#region Main DB Settings

		public void TurnOffMainDbStatistics()
		{
			StatisticsSwitch.Instance.CheckAndTurnStatistics(mainDbName, isON: false);
		}

		public void EnsureMainDatabaseSettings(IUpgradeTaskWorkflowLogger logger)
		{
			DataUtils.EnsureClrEnabledAndTrustworthyOn(connection, mainDbName);
		}

		#endregion

		#region Recovery Models

		public void AdjustAllDatabasesRecoveryModels(IUpgradeTaskWorkflowLogger logger)
		{
			logger.ShowInfoMessage("Adjusting database recovery models");
			var appliedChangesDescription = DbRecoveryModelManager.AdjustAllDatabases(connection, mainDbName);

			if (appliedChangesDescription.Length == 0)
			{
				logger.ShowInfoMessage("No database needed recovery model adjustment");
				return;
			}

			logger.ShowInfoMessage($"{appliedChangesDescription.Length} database(s) needed recovery model adjustment");
			foreach (var message in appliedChangesDescription)
			{
				logger.ShowInfoMessage("\t" + message);
			}
		}

		#endregion

		#region All Databases (of a given client)

		public void EnsureCargoWiseDatabaseSettings(IEnumerable<string> allDbs)
		{
			var snapshotIsolation = new SnapshotIsolationManager();

			foreach (string db in allDbs)
			{
				AlterDbAuthorisation(connection, db);
				SetPageVerifyChecksum(db);
				SetDbAutoCloseOff(db);
				DataUtils.SetCompatibilityLevelBasedOnServerVersion(connection, db);
				snapshotIsolation.EnableSnapshotIsolationForDatabase(connection, db, checkAlreadyEnabled: true);
				EnableArithAbort(db);
			}
		}

		public static void AlterDbAuthorisation(AdminConnection connection, string dbName)
		{
			try
			{
				DataUtils.AlterDbAuthorisation(connection, dbName);
			}
			catch (OperationCanceledException ex)
			{
				throw new UpgradeBlockedException(ex.Message, ex);
			}
		}

		void SetPageVerifyChecksum(string dbName)
		{
			DataUtils.SetDbPropertyImmediately(connection, dbName, "page_verify_option != 2", "PAGE_VERIFY CHECKSUM");
		}

		void SetDbAutoCloseOff(string dbName)
		{
			DataUtils.SetDbPropertyImmediately(connection, dbName, "is_auto_close_on != 0", "AUTO_CLOSE OFF");
		}

		void EnableArithAbort(string dbName)
		{
			DataUtils.SetDbPropertyImmediately(connection, dbName, "is_arithabort_on != 1", "ARITHABORT ON");
		}

		#endregion
	}
}
