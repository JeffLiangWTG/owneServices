using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AppDomainWrappers.Net;
using CargoWise.Application;
using CargoWise.Bi.Common;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.DataTools.DbBackupAndRestore.Business.Common;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.DbUpgrader.Shared;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using static System.FormattableString;
using DbConnection = CargoWise.Data.DbConnection;

[assembly: WTG.StaticAnalysis.Annotation.UsesConstants(typeof(BiConstants))]

namespace Enterprise.DataTools.DbBackupAndRestore.Business
{
	#region Interfaces

	public interface IDbRestoreManager
	{
		void RefreshExtendedProperties(string dbServer);
		void LogMessage(string msg);
		DbFileInfoCollection GetBackupDbFileInfoCollection(string dbServer, string backupFilePath, string auditServer, string auditBackupFilePath, string dwServer, string edwBackupFilePath);
		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", Justification = "Out parameter required by design")]
		DbFileInfoCollection ApplyExtendedProperties(DbFileInfoCollection dbFiles, out bool applyForAllFiles);
		void RestoreDatabases(DbServerConfiguration mainServerConfig, AuditDbServerConfiguration auditServerConfig, EdwDbServerConfiguration edwServerConfig, DbRestoreSettings dbRestoreSettings);
		DatabaseStatus GetDatabaseStatus(string serverName, string targetDbName);
		string GetAuditServer(string serverName, string targetDbName);
		string GetDataWarehouseServer(string serverName, string targetDbName);
		List<string> GetAvailabilityGroupsList(string dbServer);
		string GetAvailabilityGroupName(string dbServer, string dbName);
		string GetPrimaryReplicaOnAvailabilityGroup(string dbServer, string groupName);
		bool IsServerAvailabilityGroupListener(string dbServer);
		bool IsPrimaryReplicaOnAvailabilityGroup(string dbServer, string groupName);
		bool IsDbPartOfAlwaysOn(string dbServer, string dbName);
	}

	#endregion

	#region Factory

	public static class DbRestoreManagerFactory
	{
		public static IDbRestoreManager Create(
			ISynchronizeInvoke sync,
			InformationEvent onTaskStarted,
			ProgressEvent onSubtaskStarted,
			InformationEvent onTaskCompleted,
			InformationEvent onTaskFailed,
			InformationEvent onShowInfoMessage,
			ConfirmationPromptDelegate confirmationPromptDelegate,
			ReleaseKeyDelegate releaseKeyDelegate
			)
		{
			var dbrm = new DbRestoreManager(new DbHeaderOnlyReader()) { SyncInvoke = sync };
			dbrm.OnTaskStarted += onTaskStarted;
			dbrm.OnSubtaskStarted += onSubtaskStarted;
			dbrm.OnTaskCompleted += onTaskCompleted;
			dbrm.OnTaskFailed += onTaskFailed;
			dbrm.OnShowInfoMessage += onShowInfoMessage;
			dbrm.OnConfirmationPrompt = confirmationPromptDelegate;
			dbrm.OnPromtForReleaseKey = releaseKeyDelegate;

			return dbrm;
		}

		public static IDbRestoreManager Create(
			InformationEvent onTaskStarted,
			ProgressEvent onSubtaskStarted,
			InformationEvent onTaskCompleted,
			InformationEvent onTaskFailed,
			InformationEvent onShowInfoMessage,
			ConfirmationPromptDelegate confirmationPromptDelegate
		)
		{
			var dbrm = new DbRestoreManager(new DbHeaderOnlyReader());
			dbrm.OnTaskStarted += onTaskStarted;
			dbrm.OnSubtaskStarted += onSubtaskStarted;
			dbrm.OnTaskCompleted += onTaskCompleted;
			dbrm.OnTaskFailed += onTaskFailed;
			dbrm.OnShowInfoMessage += onShowInfoMessage;
			dbrm.OnConfirmationPrompt = confirmationPromptDelegate;

			return dbrm;
		}
	}

	#endregion

	#region Configuration Classes

	#region Server Configuration

	public class DbServerConfiguration
	{
		public DbServerConfiguration(string serverName, string backupFilePath, string dataFilePath = null, string logFilePath = null)
		{
			ServerName = serverName;
			BackupFilePath = backupFilePath;
			DataFilePath = dataFilePath;
			LogFilePath = logFilePath;
		}

		public static DbServerConfiguration Empty => new DbServerConfiguration(string.Empty, string.Empty);

		public string ServerName { get; }
		public string BackupFilePath { get; }
		public string DataFilePath { get; }
		public string LogFilePath { get; }
	}

	public class AuditDbServerConfiguration : DbServerConfiguration
	{
		public AuditDbServerConfiguration(string serverName, string backupFilePath, string dataFilePath = null, string logFilePath = null) : base(serverName, backupFilePath, dataFilePath, logFilePath)
		{
		}

		public new static AuditDbServerConfiguration Empty => new AuditDbServerConfiguration(string.Empty, string.Empty);
	}

	public class EdwDbServerConfiguration : DbServerConfiguration
	{
		public EdwDbServerConfiguration(string serverName, string backupFilePath, string dataFilePath = null, string logFilePath = null) : base(serverName, backupFilePath, dataFilePath, logFilePath)
		{
		}

		public new static EdwDbServerConfiguration Empty => new EdwDbServerConfiguration(string.Empty, string.Empty);
	}

	#endregion

	#region Restore Settings

	public class DbRestoreSettings
	{
		public DbRestoreSettings(string targetDbName, DbFileInfoCollection restoreDbFiles, DbRestoreOption restoreOption, bool restoreOperationalDatabases = true, bool restoreDifferentialBackups = false, bool restoreTransactionLogBackups = false, bool addDbToAvailabilityGroup = false, string availabilityGroup = null)
		{
			if (addDbToAvailabilityGroup && string.IsNullOrEmpty(availabilityGroup))
			{
				throw new ArgumentException("Availability Group name must be specified when adding the database to an Availability Group.");
			}

			TargetDbName = targetDbName;
			RestoreDbFiles = restoreDbFiles;
			RestoreOption = restoreOption;
			RestoreOperationalDatabases = restoreOperationalDatabases;
			RestoreDifferentialBackups = restoreDifferentialBackups;
			RestoreTransactionLogBackups = restoreTransactionLogBackups;
			AddDbToAvailabilityGroup = addDbToAvailabilityGroup;
			AvailabilityGroup = availabilityGroup;
		}

		public string TargetDbName { get; set; }
		public DbFileInfoCollection RestoreDbFiles { get; set; }
		public DbRestoreOption RestoreOption { get; set; }
		public bool RestoreOperationalDatabases { get; set; }
		public bool RestoreDifferentialBackups { get; set; }
		public bool RestoreTransactionLogBackups { get; set; }
		public bool AddDbToAvailabilityGroup { get; set; }
		public string AvailabilityGroup { get; set; }
	}

	#endregion

	#region Database Details

	public class DatabaseDetails
	{
		public string DatabaseName { get; }
		public string DatabaseType { get; }
		public DbServerConfiguration ServerConfig { get; }
		public string AvailabilityGroupName { get; }
		public ReadOnlyCollection<string> SecondaryReplicaNamesList { get; }
		public DbFileInfoCollection DbFiles { get; }

		public DatabaseDetails(string databaseName, string dbType, DbServerConfiguration serverConfig, DbFileInfoCollection dbFiles, string availabilityGroupName, List<string> secondaryReplicaNamesList)
		{
			DatabaseName = databaseName;
			DatabaseType = dbType;
			ServerConfig = serverConfig;
			DbFiles = dbFiles;
			AvailabilityGroupName = availabilityGroupName;
			SecondaryReplicaNamesList = secondaryReplicaNamesList.AsReadOnly() ?? new ReadOnlyCollection<string>(new List<string>());
		}
	}

	#endregion

	#endregion

	public class DbRestoreManager : DbTaskManager, IDbRestoreManager
	{
		#region Constructor

#if DEBUG
		public
#else
	internal
#endif
		DbRestoreManager(IDbHeaderOnlyReader dbHeaderOnlyReader) : this(dbHeaderOnlyReader, new DatabaseWaiter())
		{
		}

		internal DbRestoreManager(IDbHeaderOnlyReader dbHeaderOnlyReader, IDatabaseWaiter databaseWaiter)
		{
			this.dbHeaderOnlyReader = dbHeaderOnlyReader ?? throw new ArgumentNullException(nameof(dbHeaderOnlyReader));
			this.databaseWaiter = databaseWaiter;
		}

		#endregion

		public void RestoreDatabases(DbServerConfiguration mainServerConfig, AuditDbServerConfiguration auditServerConfig, EdwDbServerConfiguration edwServerConfig, DbRestoreSettings dbRestoreSettings)
		{
			EnsureNotNull(mainServerConfig, nameof(mainServerConfig));
			EnsureNotNull(auditServerConfig, nameof(auditServerConfig));
			EnsureNotNull(edwServerConfig, nameof(edwServerConfig));
			EnsureNotNull(dbRestoreSettings, nameof(dbRestoreSettings));

			SchemaVersionBefore = CheckDbSchemaVersion(mainServerConfig.ServerName, dbRestoreSettings.TargetDbName);

			if (!string.IsNullOrEmpty(auditServerConfig.ServerName))
			{
				FireOnTaskStarted("Verifying Audit Server connection");
				CheckBiServer(auditServerConfig.ServerName, dbRestoreSettings.RestoreDbFiles);
			}
			if (!string.IsNullOrEmpty(edwServerConfig.ServerName) && (!string.Equals(auditServerConfig.ServerName, edwServerConfig.ServerName, StringComparison.OrdinalIgnoreCase)))
			{
				FireOnTaskStarted("Verifying Data Warehouse Server connection");
				CheckBiServer(edwServerConfig.ServerName, dbRestoreSettings.RestoreDbFiles);
			}

			var success = RestoreDatabasesFromRestoreType(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings);
			var dbStatus = GetDatabaseStatus(mainServerConfig.ServerName, dbRestoreSettings.TargetDbName);

			if (success)
			{
				if (dbStatus == DatabaseStatus.Online)
				{
					SchemaVersionAfter = CheckDbSchemaVersion(mainServerConfig.ServerName, dbRestoreSettings.TargetDbName);
					RecordDbRestoreDetails(mainServerConfig.ServerName, auditServerConfig.ServerName, edwServerConfig.ServerName, dbRestoreSettings);
					DisableCDCWhenAuditAndEdwDBNotAvailable(mainServerConfig.ServerName, auditServerConfig.ServerName, edwServerConfig.ServerName, dbRestoreSettings);
					RunDbRestoreSubscribers(mainServerConfig.ServerName, dbRestoreSettings.TargetDbName);
					AdjustRecoveryModels(mainServerConfig.ServerName, auditServerConfig.ServerName, edwServerConfig.ServerName, dbRestoreSettings);
				}

				FireOnTaskCompleted(string.Format(CultureInfo.CurrentCulture, "Database restore with restore option {0} completed with success", dbRestoreSettings.RestoreOption));
			}

			LogMessage(string.Format(CultureInfo.CurrentCulture, "Current database status: {0}", dbStatus));
		}

		void EnsureNotNull(object arg, string argName)
		{
			if (arg == null)
			{
				throw new ArgumentNullException(argName);
			}
		}

		void AdjustRecoveryModels(string dbServer, string auditServer, string edwServer, DbRestoreSettings dbRestoreSettings)
		{
			try
			{
				using var connectionProvider = new AdminConnectionProvider(dbServer, auditServer, edwServer, this);

				foreach (var dbFile in dbRestoreSettings.RestoreDbFiles.Cast<DbFileInfo>().GroupBy(d => d.DbName).Select(g => g.First()))
				{
					var dbName = GetActualDatabaseName(dbRestoreSettings.TargetDbName, dbFile);
					var connection = connectionProvider.GetConnection(dbFile.DbType);
					DbRecoveryModelManager.AdjustDatabase(connection, dbRestoreSettings.TargetDbName, dbName);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				LogDetailedError(ex, taskFailed: true);
			}
		}

		void RunDbRestoreSubscribers(string serverName, string dbName)
		{
			try
			{
				var subscribers = ObjectFactory.Get<IEnumerable>("DbRestoreSubscribers");
				foreach (IDbRestoreSubscriber subscriber in subscribers)
				{
					using (Db.DisableSchemaVersionCheck())
					using (var connection = Db.NewAdminConnection(serverName, dbName))
					{
						if (IsTargetDatabaseMainDb(dbName))
						{
							var errorMsg = subscriber.Run(connection);
							if (!string.IsNullOrEmpty(errorMsg))
							{
								LogMessage(string.Format(CultureInfo.CurrentCulture, "\r\nThere is an error when running database restored task '{0}': {1}\r\n", subscriber.ReadableName, errorMsg));
							}
						}
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				LogDetailedError(ex, taskFailed: true);
			}
		}

		bool RestoreDatabasesFromRestoreType(DbServerConfiguration mainServerConfig, AuditDbServerConfiguration auditServerConfig, EdwDbServerConfiguration edwServerConfig, DbRestoreSettings dbRestoreSettings)
		{
			switch (dbRestoreSettings.RestoreOption)
			{
				case DbRestoreOption.RestoreWithRecovery:
				case DbRestoreOption.RestoreWithNoRecovery:
					return RestoreDatabasesSafe(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings);
				case DbRestoreOption.CopyProdToTest:
					return GetLastDBBackup(mainServerConfig.ServerName, dbRestoreSettings.TargetDbName) &&
						RestoreDatabasesSafe(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings);
				default:
					FireOnTaskFailed("Please select a valid restore option");
					return false;
			}
		}

		void CheckBiServer(string biServer, DbFileInfoCollection restoreDbFiles)
		{
			if (!string.IsNullOrWhiteSpace(biServer) && BiFilesFound(restoreDbFiles))
			{
				try
				{
					using (Db.DisableSchemaVersionCheck())
					using (var biConnection = Db.NewAdminConnection(biServer, Db.SqlMasterDb))
					{
						biConnection.EnsureIsOpen();
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					LogDetailedError(ex, taskFailed: true);
				}
			}
		}

		bool BiFilesFound(DbFileInfoCollection restoreDbFiles)
		{
			var checkBiServer = false;
			foreach (DbFileInfo file in restoreDbFiles)
			{
				if ((file.DbType == DbFileInfo.DbTypeAuditDB || file.DbType == DbFileInfo.DbTypeEdwDB) && file.Visible)
				{
					checkBiServer = true;
					break;
				}
			}

			return checkBiServer;
		}

		public string GetAuditServer(string serverName, string targetDbName)
		{
			var result = string.Empty;
			try
			{
				using (Db.DisableSchemaVersionCheck())
				using (var connection = Db.NewAdminConnection(serverName, Db.SqlMasterDb))
				{
					if (connection.DatabaseExists(targetDbName))
					{
						result = Utilities.GetAuditServer(connection, targetDbName);
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
			}
			return result;
		}

		public string GetDataWarehouseServer(string serverName, string targetDbName)
		{
			string result = null;
			try
			{
				using (Db.DisableSchemaVersionCheck())
				using (var connection = Db.NewAdminConnection(serverName, Db.SqlMasterDb))
				{
					if (connection.DatabaseExists(targetDbName))
					{
						result = Utilities.GetDataWarehouseServer(connection, targetDbName);
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
			}
			return result;
		}

		public DatabaseStatus GetDatabaseStatus(string serverName, string targetDbName)
		{
			var result = DatabaseStatus.DoesNotExist;
			using (Db.DisableSchemaVersionCheck())
			using (var connection = Db.NewAdminConnection(serverName, Db.SqlMasterDb))
			{
				if (connection.DatabaseExists(targetDbName))
				{
					result = Utilities.GetDatabaseStatus(connection, targetDbName);
				}
			}
			return result;
		}

		#region Confirmation Prompts

		bool GetLastDBBackup(string server, string dbname)
		{
			var (recentBackup, msg) = GetLastDBBackupDetails(server, dbname);

			if (recentBackup)
			{
				return true;
			}

			var userConfirmPrompt = new ConfirmationPromptArgs()
			{
				PromptType = PromptType.ConfirmNoBackupOfTestDb,
				PromptTitle = @"Please check if test database has a full backup before continue",
				PromptMessage = msg
			};
			OnConfirmationPrompt?.Invoke(userConfirmPrompt);
			return userConfirmPrompt.Result != ConfirmationPromptResult.No;
		}

#if DEBUG
		public virtual
#endif
		bool RestoreOperationalDatabasesPrompt()
		{
			var userConfirmPrompt = new ConfirmationPromptArgs()
			{
				PromptType = PromptType.ConfirmRestoreWithoutOperationalDatabases,
				PromptTitle = "Restore Without Operational Databases",
				PromptMessage = "Operational Databases have been found. Restoring without them may cause errors. Dropping or updating the databases first is recommended. Are you sure you want to continue without dropping or updating the databases?"
			};
			OnConfirmationPrompt?.Invoke(userConfirmPrompt);
			return userConfirmPrompt.Result != ConfirmationPromptResult.No;
		}

#if DEBUG
		public virtual
#endif
		bool AuditSkipRestorePrompt()
		{
			var userConfirmPrompt = new ConfirmationPromptArgs()
			{
				PromptType = PromptType.ConfirmAuditDBExcluded,
				PromptTitle = "Restore Prompt",
				PromptMessage = "Audit database backup was not selected to synchronise with the main database. Are you sure?"
			};
			OnConfirmationPrompt?.Invoke(userConfirmPrompt);
			return userConfirmPrompt.Result != ConfirmationPromptResult.No;
		}

#if DEBUG
		public virtual
#endif
		bool OverwriteDbInAvailabilityGroupPrompt()
		{
			var userConfirmPrompt = new ConfirmationPromptArgs()
			{
				PromptType = PromptType.ConfirmOverwriteDbInAvailabilityGroup,
				PromptTitle = "Overwrite Database in Availability Group Prompt",
				PromptMessage = "Database will be overwritten on all nodes in the availability group. Are you sure?"
			};
			OnConfirmationPrompt?.Invoke(userConfirmPrompt);
			return userConfirmPrompt.Result != ConfirmationPromptResult.No;
		}

#if DEBUG
		public virtual
#endif
		bool RemoveDbFromAvailabilityGroupPrompt()
		{
			var userConfirmPrompt = new ConfirmationPromptArgs()
			{
				PromptType = PromptType.ConfirmRemoveDbFromAvailabilityGroup,
				PromptTitle = "Remove from Availability Group Prompt",
				PromptMessage = "Database will be removed from all nodes in the availability group. Are you sure?"
			};
			OnConfirmationPrompt?.Invoke(userConfirmPrompt);
			return userConfirmPrompt.Result != ConfirmationPromptResult.No;
		}

		#endregion

		string CheckDbSchemaVersion(string serverName, string dbName)
		{
			var result = "";
			using (Db.DisableSchemaVersionCheck())
			using (var connection = Db.NewAdminConnection(serverName, Db.SqlMasterDb))
			{
				if (connection.DatabaseExists(dbName))
				{
					var schemaVersion = CopyProductionToTestHelper.GetStmData(connection, dbName, CopyProductionToTestHelper.SchemaVersion);
					var minorSchemaVersion = CopyProductionToTestHelper.GetStmData(connection, dbName, CopyProductionToTestHelper.MinorSchemaVersion);
					if (schemaVersion != null)
					{
						result = schemaVersion + "." + minorSchemaVersion;
					}
				}
			}
			return result;
		}

		public string SchemaVersionBefore { get; set; }

		public string SchemaVersionAfter { get; set; }

		protected bool RecordDbRestoreDetails(string serverName, string auditServerName, string dwServerName, DbRestoreSettings dbRestoreSettings)
		{
			var dbStatus = GetDatabaseStatus(serverName, dbRestoreSettings.TargetDbName);
			if (dbStatus != DatabaseStatus.Online)
			{
				LogMessage(string.Format(CultureInfo.CurrentCulture, "Unable to update registry for database - incorrect database state: Required={0} Current={1}", DatabaseStatus.Online, dbStatus));
				return false;
			}

#pragma warning disable CW1061 // Do not use System.DateTime.UtcNow Rule
			var completionDate = DateTime.UtcNow;
#pragma warning restore CW1061 // Do not use System.DateTime.UtcNow Rule

			using (Db.DisableSchemaVersionCheck())
			using (var connection = Db.NewAdminConnection(serverName, dbRestoreSettings.TargetDbName))
			{
				if (!IsTargetDatabaseMainDb(dbRestoreSettings.TargetDbName))
				{
					return true;
				}

				var registry = string.Format(CultureInfo.InvariantCulture, registryValue, dbRestoreSettings.RestoreOption.ToString(), ReleaseInfo.Instance.VersionNumber.ToString(), completionDate.ToString(Core.Constants.LastDatabaseRestoreRegistryConstants.CompletionDateFormat, CultureInfo.InvariantCulture), SchemaVersionBefore, SchemaVersionAfter);
				Utilities.UpdateRegistry(connection, dbRestoreSettings.TargetDbName, "LastDatabaseRestore", registry, false);

				if (!string.IsNullOrWhiteSpace(auditServerName) && BiFilesFound(dbRestoreSettings.RestoreDbFiles))
				{
					LogMessage("Setting Audit server registry for target database");
					Utilities.UpdateRegistry(connection, dbRestoreSettings.TargetDbName, "BiAuditServer", auditServerName, true);
				}
				else if (!string.IsNullOrEmpty(Utilities.GetAuditServer(connection, dbRestoreSettings.TargetDbName)) && dbRestoreSettings.RestoreOption != DbRestoreOption.CopyProdToTest)
				{
					LogMessage("Deleting Audit server registry from target database");
					Utilities.DeleteRegistry(connection, dbRestoreSettings.TargetDbName, "BiAuditServer");
				}

				if (!string.IsNullOrWhiteSpace(dwServerName) && BiFilesFound(dbRestoreSettings.RestoreDbFiles))
				{
					LogMessage("Setting Data Warehouse server registry for target database");
					Utilities.UpdateRegistry(connection, dbRestoreSettings.TargetDbName, "BiDataWarehouseServer", dwServerName, true);
				}
				else if (!string.IsNullOrEmpty(Utilities.GetDataWarehouseServer(connection, dbRestoreSettings.TargetDbName)) && dbRestoreSettings.RestoreOption != DbRestoreOption.CopyProdToTest)
				{
					LogMessage("Deleting Data Warehouse server registry from target database");
					Utilities.DeleteRegistry(connection, dbRestoreSettings.TargetDbName, "BiDataWarehouseServer");
					Utilities.DeleteRegistry(connection, dbRestoreSettings.TargetDbName, "BiAnalysisServer");
					Utilities.DeleteRegistry(connection, dbRestoreSettings.TargetDbName, "BiPowerBiWebPortalUrl");
				}
			}

			return true;
		}

		public void LogMessage(string msg)
		{
			FireOnShowInfoMessage(msg);
		}

		readonly string registryValue = "<?xml version=\"1.0\" encoding=\"utf-16\"?><LastDbRestoreInfo><Operation>{0}</Operation><ToolVersion>{1}</ToolVersion><CompletionDate>{2}</CompletionDate><DbSchemaVersionBefore>{3}</DbSchemaVersionBefore><DbSchemaVersionAfter>{4}</DbSchemaVersionAfter></LastDbRestoreInfo>";

		public DbFileInfoCollection GetBackupDbFileInfoCollection(string dbServer, string backupFilePath, string auditServer, string auditBackupFilePath, string dwServer, string edwBackupFilePath)
		{
			var result = new DbFileInfoCollection();

			try
			{
				using (Db.DisableSchemaVersionCheck())
				using (var conn = Db.NewAdminConnection(dbServer, Db.SqlMasterDb))
				{
					FireOnTaskStarted(string.Format(CultureInfo.InvariantCulture, "Getting list of database files in the backup\r\nServer : {0}\r\nBkp Path: {1}", dbServer, backupFilePath));
					result = GetRelatedDbFileInfoCollection(conn, dbServer, backupFilePath);
				}

				if (!string.IsNullOrEmpty(auditServer) && !string.IsNullOrEmpty(auditBackupFilePath))
				{
					FireOnTaskStarted(string.Format(CultureInfo.InvariantCulture, "\r\nGetting list of Audit database files in the backup\r\nAudit Server : {0}\r\nAudit Bkp Path: {1}", auditServer, auditBackupFilePath));
					GetRelatedBiDbFileInfoCollection(result, auditServer, auditBackupFilePath, DbFileInfo.DbTypeAuditDB);
				}

				if (!string.IsNullOrEmpty(dwServer) && !string.IsNullOrEmpty(edwBackupFilePath))
				{
					FireOnTaskStarted(string.Format(CultureInfo.InvariantCulture, "\r\nGetting list of EDW database files in the backup\r\nData Warehouse Server : {0}\r\nEDW Bkp Path: {1}", dwServer, edwBackupFilePath));
					GetRelatedBiDbFileInfoCollection(result, dwServer, edwBackupFilePath, DbFileInfo.DbTypeEdwDB);
				}

				FireOnTaskCompleted("Finished getting list of database files in the backup");
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				LogDetailedError(ex, "Error getting list of database files in the backup.", taskFailed: true);
				result.Clear();
			}

			return result;
		}

		public static (bool, string) GetLastDBBackupDetails(string dbServer, string dbName)
		{
			var backupFinishTime = "";
			var backupPath = "";
			var result = "";
			var recentBackup = false;

			var sqlText = string.Format(CultureInfo.InvariantCulture, @"SELECT
				b.backup_finish_date as BKFinishTime,
				m.physical_device_name as BKPath
				FROM msdb.dbo.backupset b with(nolock)
				JOIN msdb.dbo.backupmediafamily m with(nolock)  ON b.media_set_id = m.media_set_id
				WHERE b.backup_set_id = (
					select max(backup_set_id) from msdb.dbo.backupset where database_name = '{0}' and type = 'D'
				)", dbName);

			using (Db.DisableSchemaVersionCheck())
			using (var connection = Db.NewAdminConnection(dbServer, Db.SqlMasterDb))
			{
				using (var dbCmd = connection.Command(sqlText))
				using (var reader = dbCmd.ExecuteReader())
				{
					while (reader.Read())
					{
						backupFinishTime = reader["BKFinishTime"].ToString();
						backupPath = reader["BKPath"].ToString();
					}
				}

				if (string.IsNullOrEmpty(backupPath) || string.IsNullOrEmpty(backupFinishTime) || !FileExists(Db.NewAdminConnection(dbServer, Db.SqlMasterDb), backupPath))
				{
					result = string.Format(CultureInfo.CurrentCulture, @"The test database: {0} does not have a backup history or backup file, if the copy to test process fails, test database will be lost, please click on 'No' and go to backup tab to make a backup first.", dbName);
				}
				else
				{
					if (DateTime.TryParse(backupFinishTime, out var backupTime))
					{
						var dbServerTime = Utilities.GetDbServerDateTime(connection);
						if ((dbServerTime - backupTime).Duration() < RecentBackupTimeFrame)
						{
							recentBackup = true;
						}
					}

					if (!recentBackup)
					{
						result = string.Format(CultureInfo.CurrentCulture, @"The test database: {0} has a backup in {1} created at {2}, please consider if the backup is recent enough, if the copy to test process fails, test database will be lost and you will need to restore from the backup. Do you want to continue?", dbName, backupPath, backupFinishTime);
					}
				}
			}

			return (recentBackup, result);
		}

		static TimeSpan RecentBackupTimeFrame => new TimeSpan(1, 0, 0);

		bool CanDbBeOverwritten(DbConnection connection, string dbServer, string databaseName)
		{
			var result = true;

			if (Utilities.IsProductionDb(connection, databaseName))
			{
				var sessionInfo = SessionInfoUserKeyEntry.GetSessionInfo(connection, dbServer, databaseName);
				PromptForReleaseKey(sessionInfo);
				result = sessionInfo.ShouldRelease();
			}
			return result;
		}

		#region Extended properties

		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", Justification = "By design")]
		public DbFileInfoCollection ApplyExtendedProperties(DbFileInfoCollection dbFiles, out bool applyForAllFiles)
		{
			var result = new DbFileInfoCollection();
			applyForAllFiles = true;

			foreach (DbFileInfo file in dbFiles)
			{
				var extendedPath = GetExtendedProperties(file.DbType, file.FileType);

				if (!string.IsNullOrEmpty(extendedPath))
				{
					file.FolderPathView = extendedPath;
				}
				else
				{
					applyForAllFiles = false;
				}

				result.Add(file);
			}

			return result;
		}

		string GetExtendedProperties(string dbType, string fileType)
		{
			var result = "";

			if (DbFileInfo.DbTypeEDocs.Equals(dbType, StringComparison.OrdinalIgnoreCase))
			{
				result = "eDocs";
			}
			else if (DbFileInfo.DbTypeMain.Equals(dbType, StringComparison.OrdinalIgnoreCase))
			{
				result = "MainDb";
			}
			else if (DbFileInfo.DbTypeRefDB.Equals(dbType, StringComparison.OrdinalIgnoreCase))
			{
				result = "Reference";
			}
			else if (DbFileInfo.DbTypeUserRepository.Equals(dbType, StringComparison.OrdinalIgnoreCase))
			{
				result = "UserRepository";
			}

			if (DbFileInfo.FileTypeData.Equals(fileType, StringComparison.OrdinalIgnoreCase))
			{
				result += "DataFile";
			}
			else if (DbFileInfo.FileTypeLog.Equals(fileType, StringComparison.OrdinalIgnoreCase))
			{
				result += "LogFile";
			}

			extendedProperties.TryGetValue(result, out result);

			return result;
		}

		public void RefreshExtendedProperties(string dbServer)
		{
			extendedProperties.Clear();

			var sqlText = "SELECT name, value FROM [master].[sys].[extended_properties] WHERE name in ('eDocsDataFile', 'eDocsLogFile', 'MainDbDataFile', 'MainDbLogFile', 'ReferenceDataFile', 'ReferenceLogFile', 'UserRepositoryDataFile', 'UserRepositoryLogFile')";

			using (Db.DisableSchemaVersionCheck())
			using (var connection = Db.NewAdminConnection(dbServer, Db.SqlMasterDb))
			using (var dbCmd = connection.Command(sqlText))
			using (var reader = dbCmd.ExecuteReader())
			{
				while (reader.Read())
				{
					extendedProperties.Add(reader["name"].ToString(), reader["value"].ToString());
				}
			}
		}

		protected Dictionary<string, string> extendedProperties = new Dictionary<string, string>();

		#endregion Extended properties

		bool RestoreDatabasesSafe(DbServerConfiguration mainServerConfig, AuditDbServerConfiguration auditServerConfig, EdwDbServerConfiguration edwServerConfig, DbRestoreSettings dbRestoreSettings)
		{
			try
			{
				FireOnTaskStarted(string.Format(CultureInfo.InvariantCulture, "Database restore started\r\n\r\nServer: {0}\r\nDatabase: {1}\r\n", mainServerConfig.ServerName, dbRestoreSettings.TargetDbName));
				RestoreDatabasesUnsafe(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings);
				return true;
			}
			catch (DbBackupAndRestoreException ex)
			{
				LogMessage(string.Format(CultureInfo.InvariantCulture, "Operation cancelled. {0}\r\n", ex.Message));
				return false;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				LogDetailedError(ex, "Database restore failed.", taskFailed: true, showStackTrace: true);
				return false;
			}
		}

		internal void RestoreDatabasesUnsafe(DbServerConfiguration mainServerConfig, AuditDbServerConfiguration auditServerConfig, EdwDbServerConfiguration edwServerConfig, DbRestoreSettings dbRestoreSettings)
		{
			if (!dbRestoreSettings.RestoreOperationalDatabases && ContainsOperationalDatabases(dbRestoreSettings.RestoreDbFiles) && !RestoreOperationalDatabasesPrompt())
			{
				throw new DbBackupAndRestoreException("Operational databases not restored - process cancelled.\r\n");
			}

			using (Db.DisableSchemaVersionCheck())
			using (var connection = Db.NewAdminConnection(mainServerConfig.ServerName, Db.SqlMasterDb))
			{
				var isNewDb = !Utilities.DatabaseExists(connection, dbRestoreSettings.TargetDbName);
				if (!isNewDb && dbRestoreSettings.RestoreOption == DbRestoreOption.CopyProdToTest)
				{
					ValidateCopyProdToTestPreRestoreSettings(connection, dbRestoreSettings);
				}

				if (dbRestoreSettings.RestoreOption != DbRestoreOption.CopyProdToTest)
				{
					if (Utilities.GetDatabaseStatus(connection, dbRestoreSettings.TargetDbName) != DatabaseStatus.Restoring && !CanDbBeOverwritten(connection, mainServerConfig.ServerName, dbRestoreSettings.TargetDbName))
					{
						throw new DbBackupAndRestoreException("Database already exists. A valid release key is required to overwrite it.\r\n");
					}
				}

				ValidateAlwaysOnSupportSettings(connection, dbRestoreSettings);

				var vaultAuxDbName = dbRestoreSettings.TargetDbName + "-DataVault";
				try
				{
					// Save test specific data before restoring production backup over it
					PerformCopyProductionToTestPreRestoreSteps(connection, dbRestoreSettings, vaultAuxDbName, isNewDb);
					DoRestore(connection, mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings);
#if DEBUG
					ApplyChangeAfterRestoreDatabase(connection, dbRestoreSettings.TargetDbName);
#endif
					// Re-apply test specific data (from vault DB) and make other test system related adjustments
					PerformCopyProductionToTestPostRestoreSteps(connection, dbRestoreSettings, vaultAuxDbName, isNewDb);
				}
				finally
				{
					if (dbRestoreSettings.RestoreOption == DbRestoreOption.CopyProdToTest)
					{
						DropDataVaultDbIfExists(connection, vaultAuxDbName);
					}
				}
			}
		}

		bool ContainsOperationalDatabases(DbFileInfoCollection dbFiles)
		{
			return dbFiles.Cast<DbFileInfo>().Any(fileInfo => fileInfo.DbType == DbFileInfo.DbTypeEDocs);
		}

		#region Copy Production to Test

		void ValidateCopyProdToTestPreRestoreSettings(AdminConnection connection, DbRestoreSettings dbRestoreSettings)
		{
			if (!Utilities.IsTestOrTrainingDb(connection, dbRestoreSettings.TargetDbName))
			{
				var isNotTestDbMessage = string.Format(CultureInfo.InvariantCulture, "{0} is not a test or training database.", dbRestoreSettings.TargetDbName);
				throw new DbBackupAndRestoreException(isNotTestDbMessage);
			}
			if (!PreserveTestValueFieldExists(connection, dbRestoreSettings.TargetDbName))
			{
				var earlierToolVersionRequired = "Restoring to this database requires Database Backup And Restore tool version 15.10.18.0 or older.";
				throw new DbBackupAndRestoreException(earlierToolVersionRequired);
			}
			CheckMinUpgradableDbMajorSchemaVersion(connection, dbRestoreSettings.TargetDbName);
		}

		void CheckMinUpgradableDbMajorSchemaVersion(AdminConnection connection, string targetDbName)
		{
			var currentSchemaVersion = CopyProductionToTestHelper.GetStmData(connection, targetDbName, CopyProductionToTestHelper.SchemaVersion);
			var minUpgradableDbMajorSchemaVersion = DataRegistry.MinUpgradableDbMajorSchemaVersion;
			if (currentSchemaVersion == null || int.Parse(currentSchemaVersion) < minUpgradableDbMajorSchemaVersion)
			{
				var isNotAcceptableVersionMessage = string.Format(CopyProductionToTestHelper.ExceptionIfVersionIsLessThanMinOrNULL, minUpgradableDbMajorSchemaVersion, currentSchemaVersion ?? "NULL", targetDbName);
				throw new DbBackupAndRestoreException(isNotAcceptableVersionMessage);
			}
			if (int.Parse(currentSchemaVersion) > SchemaVersion.Application.Major)
			{
				var isNotAcceptableVersionMessage = string.Format(CopyProductionToTestHelper.ExceptionIfDbSchemaIsGreaterThanAppVersion, targetDbName);
				throw new DbBackupAndRestoreException(isNotAcceptableVersionMessage);
			}
		}

		void DropDataVaultDbIfExists(AdminConnection connection, string vaultAuxDbName)
		{
			// Drop auxiliary db
			FireOnSubtaskStarted("Dropping Auxiliary Database\r\n", 0);

			DbConnectionKiller.KillOtherConnections(connection, vaultAuxDbName);
			connection.Command(Invariant($"DROP DATABASE IF EXISTS {vaultAuxDbName.QuoteName()}"), CopyProductionToTestTimeoutInSeconds).ExecuteNonQuery();
		}

		bool PreserveTestValueFieldExists(DbConnection connection, string dbName)
		{
			var query = string.Format(CultureInfo.InvariantCulture, @"IF EXISTS (SELECT NULL FROM [{0}].sys.columns c INNER JOIN [{0}].sys.tables t ON c.object_id = t.object_id WHERE c.name = 'SD_PreserveTestValue' AND t.name = 'StmData') SELECT 1 ELSE SELECT 0", dbName);
			return ((int)connection.ExecuteScalar(query) == 1);
		}

#if DEBUG
		public virtual void ApplyChangeAfterRestoreDatabase(DbConnection connection, string dbName)
		{
		}
#endif

		/// <summary>
		/// - Create auxiliary DB (vault)
		/// - Save test specific data to be preserved to the auxiliary database
		/// </summary>
		protected virtual void PerformCopyProductionToTestPreRestoreSteps(AdminConnection connection, DbRestoreSettings dbRestoreSettings, string vaultAuxDbName, bool isNewTestDb)
		{
			if (dbRestoreSettings.RestoreOption != DbRestoreOption.CopyProdToTest)
			{
				return;
			}

			DropDataVaultDbIfExists(connection, vaultAuxDbName);

			if (!isNewTestDb)
			{
				//Creating temporary Db
				FireOnSubtaskStarted("Creating Auxiliary Database\r\n", 0);

				var creatingCommand = CopyProductionToTestScriptManager.GetCreateTemporaryDbScript(vaultAuxDbName);
				connection.ExecuteNonQuery(creatingCommand, CopyProductionToTestTimeoutInSeconds);

				FireOnSubtaskStarted("Copying test data to auxiliary database\r\n", 0);
				var populatingCommand = CopyProductionToTestScriptManager.GetPopulateTemporaryDataScript(vaultAuxDbName, dbRestoreSettings.TargetDbName);
				connection.ExecuteNonQuery(populatingCommand, CopyProductionToTestTimeoutInSeconds);
			}
		}

		/// <summary>
		/// - Create read-only access keys for S3 Doc storage
		/// - Clean up production specific data
		/// - Re-apply test data saved in the vault auxiliary DB
		/// - Adjust licence info to comply with test systems
		/// - Drop auxiliary DB
		/// </summary>
		protected virtual void PerformCopyProductionToTestPostRestoreSteps(AdminConnection connection, DbRestoreSettings dbRestoreSettings, string vaultAuxDbName, bool isNewTestDb)
		{
			if (dbRestoreSettings.RestoreOption != DbRestoreOption.CopyProdToTest)
			{
				return;
			}

			var licence = new LicenceBuilder();

			//
			// Store new read-only access keys for S3 Doc storage
			//
			if (licence.WasRestoredDatabaseHostedWithCargoWise(connection, dbRestoreSettings.TargetDbName) && IsEDocsStorageProviderS3(connection, dbRestoreSettings.TargetDbName))
			{
				if (!isNewTestDb)
				{
					RemoveS3RegistryItemsFromAuxDb(connection, vaultAuxDbName);
				}

				DoNotPreserveTestValueForS3RegistryItems(connection, dbRestoreSettings.TargetDbName);

				LogMessage("Creating new read-only access keys for S3 Doc storage");
				StoreNewReadOnlyAccessKeys(connection, dbRestoreSettings.TargetDbName);
			}

			//
			// Clear production specific info from test system
			//
			if (connection.ExecuteScalar(FormattableString.Invariant($"select object_id('{dbRestoreSettings.TargetDbName}.dbo.StmNums')")) != DBNull.Value)
			{
				var clearSpecificInfoCommand = CopyProductionToTestScriptManager.GetClearDataToBeOverwrittenByTestDataScript(vaultAuxDbName, dbRestoreSettings.TargetDbName);
				connection.ExecuteNonQuery(clearSpecificInfoCommand, CopyProductionToTestTimeoutInSeconds);
			}

			var physicalServerId = isNewTestDb ? "???" : LicenceBuilder.GetPhysicalServerId(connection, "TempStmData", vaultAuxDbName);

			//
			// Adjust company licences in the newly restored test system.
			//
			licence.AdjustCompanyLicencesToFitTestSystem(connection, dbRestoreSettings.TargetDbName, physicalServerId);

			if (!isNewTestDb)
			{
				// Copy data saved in auxiliary database to restored test database
				FireOnSubtaskStarted("Copying test data back to target database\r\n", 0);
				var copyTempDbDataToTestDbCommand = CopyProductionToTestScriptManager.GetCopyTempDbDataToTestDbScript(vaultAuxDbName, dbRestoreSettings.TargetDbName);
				connection.Command(copyTempDbDataToTestDbCommand, CopyProductionToTestTimeoutInSeconds).ExecuteNonQuery();
			}
		}

		public void DisableCDCWhenAuditAndEdwDBNotAvailable(string dbServer, string auditServer, string dwServer, DbRestoreSettings dbRestoreSettings)
		{
			try
			{
				string query = string.Format(@"SELECT is_cdc_enabled FROM sys.databases WHERE name = '{0}'", dbRestoreSettings.TargetDbName);
				bool isBIRegistrySet = true;

				using (Db.DisableSchemaVersionCheck())
				using (var connection = Db.NewAdminConnection(dbServer, dbRestoreSettings.TargetDbName))
				{
					if (Convert.ToInt32(connection.Command(query).ExecuteScalar()) == 1)
					{
						//Check Registry Value
						if (connection.DatabaseExists(dbRestoreSettings.TargetDbName))
						{
							isBIRegistrySet = (!string.IsNullOrEmpty(Utilities.GetAuditServer(connection, dbRestoreSettings.TargetDbName)) || (!string.IsNullOrEmpty(Utilities.GetDataWarehouseServer(connection, dbRestoreSettings.TargetDbName))));
						}

						if (!isBIRegistrySet)
						{
							DisableCDC(connection);
							if (dbRestoreSettings.RestoreOption == DbRestoreOption.CopyProdToTest)
							{
								Utilities.UpdateRegistry(connection, dbRestoreSettings.TargetDbName, "BiResetChangeDataCapture", "True", false);
							}
						}
						else
						{
							var isRestoreBIDatabases = !string.IsNullOrEmpty(auditServer) || !string.IsNullOrEmpty(dwServer);
							if (dbRestoreSettings.RestoreOption == DbRestoreOption.CopyProdToTest && !isRestoreBIDatabases)
							{
								Utilities.UpdateRegistry(connection, dbRestoreSettings.TargetDbName, "BiResetChangeDataCapture", "True", false);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				FireOnTaskCompleted(Invariant($"{nameof(DisableCDCWhenAuditAndEdwDBNotAvailable)} has encountered with errors: {ex}"));
			}
		}

		void DisableCDC(AdminConnection connection)
		{
			var query = @"BEGIN
						EXEC sys.sp_cdc_disable_db;
						END";
			connection.Command(query).ExecuteNonQuery();
		}

		const int CopyProductionToTestTimeoutInSeconds = 3600;

		#endregion

		public IAlwaysOnHelper AlwaysOnHelper { get; set; } = new AlwaysOnHelper();
		public IAWSHelper AWSHelper { get; set; } = new AWSHelper();

		protected virtual void DoRestore(AdminConnection connection, DbServerConfiguration mainServerConfig, AuditDbServerConfiguration auditServerConfig, EdwDbServerConfiguration edwServerConfig, DbRestoreSettings dbRestoreSettings)
		{
			var isPartOfAlwaysOn = AlwaysOnHelper.IsDbPartOfAlwaysOn(connection, dbRestoreSettings.TargetDbName);
			if (isPartOfAlwaysOn && string.IsNullOrEmpty(dbRestoreSettings.AvailabilityGroup))
			{
				dbRestoreSettings.AvailabilityGroup = AlwaysOnHelper.GetAvailabilityGroupName(connection, dbRestoreSettings.TargetDbName);
			}

			var dbDetailsList = GetAllDatabaseDetails(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings);
			var replicasWithDbExcludingDestinationServer = !string.IsNullOrEmpty(dbRestoreSettings.AvailabilityGroup)
				? AlwaysOnHelper.GetReplicaServersWithDatabase(
					connection,
					dbRestoreSettings.TargetDbName,
					dbRestoreSettings.AvailabilityGroup
				).Where(replica => !string.Equals(GetBaseServerName(replica), GetBaseServerName(connection.ServerName), StringComparison.OrdinalIgnoreCase)).ToList()
				: new List<string>();

			PrepareAvailabilityGroupForRestore(connection, dbRestoreSettings, dbDetailsList, replicasWithDbExcludingDestinationServer, isPartOfAlwaysOn);
			PrepareAndExecuteRestoreTasks(connection, dbRestoreSettings, dbDetailsList);
			FinaliseAvailabilityGroupForRestore(dbRestoreSettings, dbDetailsList, replicasWithDbExcludingDestinationServer);
		}

		List<DatabaseDetails> GetAllDatabaseDetails(DbServerConfiguration mainServerConfig, AuditDbServerConfiguration auditServerConfig, EdwDbServerConfiguration edwServerConfig, DbRestoreSettings dbRestoreSettings)
		{
			var dbDetailsList = new List<DatabaseDetails>();
			dbDetailsList.AddRange(GetDatabaseDetails(mainServerConfig, DbFileInfo.DbTypeUserRepository));
			dbDetailsList.AddRange(GetDatabaseDetails(mainServerConfig, DbFileInfo.DbTypeEDocs));
			dbDetailsList.AddRange(GetDatabaseDetails(mainServerConfig, DbFileInfo.DbTypeRefDB));
			dbDetailsList.AddRange(GetDatabaseDetails(edwServerConfig, DbFileInfo.DbTypeEdwDB));

			var auditDbDetails = GetDatabaseDetails(auditServerConfig, DbFileInfo.DbTypeAuditDB);
			if (auditDbDetails.IsNullOrEmpty() && !AuditSkipRestorePrompt())
			{
				throw new DbBackupAndRestoreException("Audit database not included in restore - process cancelled.\r\n");
			}
			dbDetailsList.AddRange(auditDbDetails);

			// Add Main DB at the end of the list to ensure it is restored last
			dbDetailsList.AddRange(GetDatabaseDetails(mainServerConfig, DbFileInfo.DbTypeMain));

			dbDetailsList = dbDetailsList.Where(dbDetails => dbDetails != null).ToList();

			List<DatabaseDetails> GetDatabaseDetails(DbServerConfiguration serverConfig, string dbType)
			{
				var dbDetailsList = new List<DatabaseDetails>();
				if (string.IsNullOrEmpty(serverConfig.ServerName) || string.IsNullOrEmpty(serverConfig.BackupFilePath))
				{
					return dbDetailsList;
				}
				var dbFilesByDbName = GetSpecificDbFiles(dbRestoreSettings.RestoreDbFiles, dbType);
				if (dbFilesByDbName.Count == 0)
				{
					return dbDetailsList;
				}

				using (var connection = Db.NewAdminConnection(serverConfig.ServerName, Db.SqlMasterDb))
				{
					foreach (var dbFiles in dbFilesByDbName)
					{
						var actualDbName = GetActualDatabaseName(dbRestoreSettings.TargetDbName, dbFiles.Value[0]);
						if (!dbDetailsList.Any(details => details.DatabaseName == actualDbName))
						{
							dbDetailsList.Add(new DatabaseDetails(
								actualDbName,
								dbType,
								serverConfig,
								dbFiles.Value,
								dbRestoreSettings.AvailabilityGroup,
								!string.IsNullOrEmpty(dbRestoreSettings.AvailabilityGroup) ? AlwaysOnHelper.GetSecondaryReplicaNamesListForAvailabilityGroup(connection, dbRestoreSettings.AvailabilityGroup) : new List<string> { }));
						}
					}
				}
				return dbDetailsList;
			}

			return dbDetailsList;
		}

		void PrepareAndExecuteRestoreTasks(AdminConnection connection, DbRestoreSettings dbRestoreSettings, List<DatabaseDetails> dbDetailsList)
		{
			try
			{
				var restoreTasks = new List<Task>();

				if (dbRestoreSettings.AddDbToAvailabilityGroup)
				{
					restoreTasks.AddRange(CreateSecondaryRestoreTasks(dbDetailsList, dbRestoreSettings));
					restoreTasks.Add(Task.Run(() => RestorePrimaryReplica(dbDetailsList, dbRestoreSettings)));
				}
				else
				{
					restoreTasks.Add(Task.Run(() => RestorePrimaryReplica(dbDetailsList, dbRestoreSettings)));
				}

				Task.WhenAll(restoreTasks).Wait();
			}
			catch (AggregateException ex) when (ex.InnerExceptions.Any(inner => inner is DbBackupAndRestoreException))
			{
				var dbRestoreException = ex.InnerExceptions.First(inner => inner is DbBackupAndRestoreException);
				throw dbRestoreException;
			}
		}

		IEnumerable<Task> CreateSecondaryRestoreTasks(List<DatabaseDetails> dbDetailsList, DbRestoreSettings dbRestoreSettings)
		{
			var secondaryRestoreTasks = new List<Task>();

			foreach (var dbDetails in dbDetailsList)
			{
				try
				{
					foreach (var replicaName in dbDetails.SecondaryReplicaNamesList)
					{
						LogMessage($"Starting restore database {dbDetails.DatabaseName} on secondary replica {replicaName}");

						using (var replicaConnection = Db.NewAdminConnection(replicaName, Db.SqlMasterDb))
						using (var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(5)))
						{
							databaseWaiter.WaitUntilDatabaseReadyToRestore(replicaConnection, dbDetails.DatabaseName, Logger.Instance, cancellationTokenSource.Token);
						}

						secondaryRestoreTasks.Add(Task.Run(() =>
						{
							try
							{
								using (var replicaConnection = Db.NewAdminConnection(replicaName, Db.SqlMasterDb))
								{
									DoRestoreWithNoRecovery(dbDetails, dbRestoreSettings, replicaConnection);
								}
							}
							catch (Exception ex)
							{
								LogDetailedError(ex, $"Failed to restore database {dbDetails.DatabaseName} secondary replica: {replicaName}. Please run the AOM Service Task to automatically fix the secondary database.");
							}
						}));
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					LogDetailedError(ex, $"Error while starting restore database {dbDetails.DatabaseName} on secondary replica. Primary replica restore cancelled. Check Secondary replicas before trying again.");
					JoinAllPrimaryDbToAvailabilityGroup(dbDetailsList, dbRestoreSettings);
					throw;
				}
			}
			return secondaryRestoreTasks;
		}

		protected virtual void RestorePrimaryReplica(List<DatabaseDetails> dbDetailsList, DbRestoreSettings dbRestoreSettings)
		{
			foreach (var dbDetails in dbDetailsList)
			{
				ExecuteRestoreWithNoRecovery(dbDetails, dbRestoreSettings);
			}

			if (dbRestoreSettings.RestoreOption != DbRestoreOption.RestoreWithNoRecovery)
			{
				foreach (var dbDetails in dbDetailsList)
				{
					ExecuteRestoreWithRecovery(dbDetails, dbRestoreSettings);
					if (dbDetails.DatabaseName == dbRestoreSettings.TargetDbName)
					{
						ConfigureServerForDatabase(dbDetails.ServerConfig.ServerName, dbRestoreSettings);
					}
				}
			}
		}

		#region Availability Group Handlers

		void ValidateAlwaysOnSupportSettings(AdminConnection connection, DbRestoreSettings dbRestoreSettings)
		{
			var isPartOfAlwaysOn = AlwaysOnHelper.IsDbPartOfAlwaysOn(connection, dbRestoreSettings.TargetDbName);
			if (!isPartOfAlwaysOn)
			{
				return;
			}

			if (!AlwaysOnHelper.IsPrimaryReplicaOnAvailabilityGroup(connection, dbRestoreSettings.AvailabilityGroup))
			{
				var isSecondaryReplicaMessage = string.Format(CultureInfo.InvariantCulture,
					"The database {0} is part of an Always On availability group, and the destination server {1} is currently hosting a secondary replica. " +
					"Ensure that the server is hosting the primary replica, then retry the command.",
					dbRestoreSettings.TargetDbName, connection.ServerName);
				throw new DbBackupAndRestoreException(isSecondaryReplicaMessage);
			}
			else if (dbRestoreSettings.RestoreOption == DbRestoreOption.RestoreWithNoRecovery)
			{
				var isPrimaryReplicaMessage = string.Format(CultureInfo.InvariantCulture, "{0} is on an AlwaysOn primary availability replica. Restore WITH NORECOVERY is not supported for primary replicas.", dbRestoreSettings.TargetDbName);
				throw new DbBackupAndRestoreException(isPrimaryReplicaMessage);
			}
		}

		void PrepareAvailabilityGroupForRestore(AdminConnection connection, DbRestoreSettings dbRestoreSettings, List<DatabaseDetails> dbDetailsList, List<string> replicasWithDb, bool isPartOfAlwaysOn)
		{
			if ((replicasWithDb.Count > 0 || isPartOfAlwaysOn) && !dbRestoreSettings.AddDbToAvailabilityGroup && !RemoveDbFromAvailabilityGroupPrompt())
			{
				throw new DbBackupAndRestoreException("Restore to availability group - process cancelled.\r\n");
			}
			else if (dbRestoreSettings.AddDbToAvailabilityGroup && !OverwriteDbInAvailabilityGroupPrompt())
			{
				throw new DbBackupAndRestoreException("Restore to availability group - process cancelled.\r\n");
			}

			RemoveAllPrimaryDbFromAvailabilityGroup(dbDetailsList);
		}

		void FinaliseAvailabilityGroupForRestore(DbRestoreSettings dbRestoreSettings, List<DatabaseDetails> dbDetailsList, List<string> replicasWithDb)
		{
			if (dbRestoreSettings.AddDbToAvailabilityGroup)
			{
				JoinAllPrimaryDbToAvailabilityGroup(dbDetailsList, dbRestoreSettings);
				JoinAllSecondaryDbToAvailabilityGroup(dbDetailsList, dbRestoreSettings);
			}
			else
			{
				DropDbFromOtherReplicasInAvailabilityGroup(dbDetailsList, replicasWithDb);
			}
		}

		void RemoveAllPrimaryDbFromAvailabilityGroup(List<DatabaseDetails> dbDetailsList)
		{
			foreach (var dbDetails in dbDetailsList)
			{
				using (var connection = Db.NewAdminConnection(dbDetails.ServerConfig.ServerName, Db.SqlMasterDb))
				{
					var isPartOfAlwaysOn = AlwaysOnHelper.IsDbPartOfAlwaysOn(connection, dbDetails.DatabaseName);
					if (isPartOfAlwaysOn)
					{
						LogMessage($"Removing {dbDetails.DatabaseName} database from availability group");
						AlwaysOnHelper.RemoveDatabaseFromAvailabilityGroup(connection, dbDetails.DatabaseName, dbDetails.AvailabilityGroupName);
						using (var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(5)))
						{
							databaseWaiter.WaitUntilDatabaseRemovedFromAvailabilityGroup(connection, dbDetails.DatabaseName, Logger.Instance, cancellationTokenSource.Token, AlwaysOnHelper);
						}
					}
				}
			}
		}

#if DEBUG
		public virtual
#endif
		void DropDbFromOtherReplicasInAvailabilityGroup(List<DatabaseDetails> dbDetailsList, List<string> serverNames)
		{
			foreach (var serverName in serverNames)
			{
				using (var connection = Db.NewAdminConnection(serverName, Db.SqlMasterDb))
				{
					foreach (var dbDetails in dbDetailsList)
					{
						LogMessage($"Dropping {dbDetails.DatabaseName} from server {serverName}");
						var dropDbCommand = Invariant($"DROP DATABASE IF EXISTS {dbDetails.DatabaseName.QuoteName()}");
						connection.Command(dropDbCommand).ExecuteNonQuery();
					}
				}
			}
		}

		void JoinAllSecondaryDbToAvailabilityGroup(List<DatabaseDetails> dbDetailsList, DbRestoreSettings dbRestoreSettings)
		{
			foreach (var dbDetails in dbDetailsList)
			{
				foreach (var replicaName in dbDetails.SecondaryReplicaNamesList)
				{
					try
					{
						using (var replicaConnection = RetryAdminConnectionAfterTimeouts(replicaName, Db.SqlMasterDb, 5))
						{
							replicaConnection.DefaultCommandTimeOutInSeconds = 600;
							LogMessage($"Joining database {dbDetails.DatabaseName} on secondary replica {replicaName} to availability group");

							var success = AlwaysOnHelper.JoinSecondaryDatabaseToAvailabilityGroup(replicaConnection, dbDetails.DatabaseName, dbDetails.AvailabilityGroupName);
							if (!success)
							{
								throw new DbBackupAndRestoreException($"Failed to join database '{dbDetails.DatabaseName}' to availability group '{dbDetails.AvailabilityGroupName}'.");
							}

							LogMessage($"Successfully joined database {dbDetails.DatabaseName} on secondary replica {replicaName} to availability group");
						}

						if (dbDetails.DatabaseName == dbRestoreSettings.TargetDbName)
						{
							ConfigureServerForDatabase(replicaName, dbRestoreSettings);
						}
					}
					catch (SqlException ex) when (ex.Number == 41145)
					{
						// Error: 41145, Severity: 16, State: 1. Cannot join database to availability group. The database has already joined the availability group. This is an informational message.
						// If auto seeding is enabled, trying to join a secondary database will fail with error 41145. It is safe to ignore it.
						LogMessage($"{dbDetails.DatabaseName} on secondary replica {replicaName} was already joined to availability group!");
					}
					catch (Exception ex)
					{
						var errorMessage = $"Failed to complete operation for database {dbDetails.DatabaseName} on secondary replica {replicaName} to availability group.\nPlease run the AOM Service Task to automatically fix and rejoin the secondary database.";
						LogDetailedError(ex, errorMessage);
					}
				}
			}
		}

		void JoinAllPrimaryDbToAvailabilityGroup(List<DatabaseDetails> dbDetailsList, DbRestoreSettings dbRestoreSettings)
		{
			foreach (var dbDetails in dbDetailsList)
			{
				if (!string.IsNullOrEmpty(dbDetails.AvailabilityGroupName))
				{
					var serverRole = !string.IsNullOrEmpty(dbRestoreSettings.AvailabilityGroup) ? "primary replica" : "server";
					LogMessage($"Joining database {dbDetails.DatabaseName} on primary replica {dbDetails.ServerConfig.ServerName} to availability group");
					using (var connection = RetryAdminConnectionAfterTimeouts(dbDetails.ServerConfig.ServerName, Db.SqlMasterDb, 5))
					{
						connection.DefaultCommandTimeOutInSeconds = 300;
						AlwaysOnHelper.AddDatabaseToAvailabilityGroup(connection, dbDetails.DatabaseName, dbDetails.AvailabilityGroupName);
					}
					LogMessage($"Successfully joined database {dbDetails.DatabaseName} on primary replica {dbDetails.ServerConfig.ServerName} to availability group");
				}
			}
		}

		#endregion

		protected virtual void ExecuteRestoreWithNoRecovery(DatabaseDetails dbDetails, DbRestoreSettings dbRestoreSettings)
		{
			try
			{
				DoRestoreWithNoRecovery(dbDetails, dbRestoreSettings);
			}
			catch (Exception ex)
			{
				LogDetailedError(ex, $"Restore task failed for {dbDetails.DatabaseName}.");
				throw new DbBackupAndRestoreException($"Restore failed - process cancelled.");
			}
		}

		protected virtual void ExecuteRestoreWithRecovery(DatabaseDetails dbDetails, DbRestoreSettings dbRestoreSettings)
		{
			try
			{
				DoRestoreWithRecovery(dbDetails, dbRestoreSettings);
			}
			catch (Exception ex)
			{
				LogDetailedError(ex, $"Restore task failed for {dbDetails.DatabaseName}.");
				throw new DbBackupAndRestoreException($"Restore failed - process cancelled.");
			}
		}

		protected virtual void DoRestoreWithNoRecovery(DatabaseDetails dbDetails, DbRestoreSettings dbRestoreSettings, AdminConnection connection = null)
		{
			var dbRestoreCount = 0;

			var isConnectionProvided = connection != null;
			if (!isConnectionProvided)
			{
				connection = Db.NewAdminConnection(dbDetails.ServerConfig.ServerName, Db.SqlMasterDb);
			}

			try
			{
				dbRestoreCount = RestoreWithNoRecovery(connection, dbDetails);

				if (dbRestoreSettings.RestoreDifferentialBackups)
				{
					FireOnSubtaskStarted("Restoring differential backups", ++dbRestoreCount);
					RestoreDifferentialBackups(connection, dbDetails);
				}

				if (dbRestoreSettings.RestoreTransactionLogBackups)
				{
					FireOnSubtaskStarted("Restoring transaction log backups", ++dbRestoreCount);
					RestoreTransactionLogBackups(connection, dbDetails);
				}
			}
			finally
			{
				if (!isConnectionProvided)
				{
					connection.Dispose();
				}
			}
		}

		int DoRestoreWithRecovery(DatabaseDetails dbDetails, DbRestoreSettings dbRestoreSettings)
		{
			var dbRestoreCount = dbDetails.DbFiles.Count;
			using (Db.DisableSchemaVersionCheck())
			using (var connection = Db.NewAdminConnection(dbDetails.ServerConfig.ServerName, Db.SqlMasterDb))
			{
				RestoreWithRecovery(connection, dbDetails.DatabaseName);

				using (var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(5)))
				{
					databaseWaiter.WaitUntilDatabaseIsOnline(connection, dbDetails.DatabaseName, Logger.Instance, cancellationTokenSource.Token);
				}

				GrantDatabaseRights(connection, dbDetails, dbRestoreSettings, ref dbRestoreCount);

				if (IsTargetDatabaseMainDb(dbRestoreSettings.TargetDbName))
				{
					SynchroniseSynonyms(dbDetails.ServerConfig.ServerName, dbRestoreSettings.TargetDbName);
				}
			}

			return dbRestoreCount;
		}

		void ConfigureServerForDatabase(string databaseServerName, DbRestoreSettings dbRestoreSettings)
		{
			if (!IsTargetDatabaseMainDb(dbRestoreSettings.TargetDbName))
			{
				return;
			}

			try
			{
				using (Db.DisableSchemaVersionCheck())
				using (var connection = Db.NewAdminConnection(databaseServerName, Db.SqlMasterDb))
				{
					FireOnSubtaskStarted($"Configuring Server {databaseServerName}", 1);
					serverConfigurationUtils.ConfigureServerFromDatabase(connection, dbRestoreSettings.TargetDbName);
					LogMessage($"Successfully configured Server {databaseServerName}");
				}
			}
			catch (Exception ex)
			{
				LogDetailedError(ex, "An error occurred while trying to configure the server.\nThe server needs to be configured manually.");
			}
		}

		void GrantDatabaseRights(AdminConnection connection, DatabaseDetails dbDetails, DbRestoreSettings dbRestoreSettings, ref int dbRestoreCount)
		{
			if (IsTargetDatabaseMainDb(dbRestoreSettings.TargetDbName))
			{
				FireOnSubtaskStarted($"Granting rights on restored database {dbRestoreSettings.TargetDbName} to the application login on {dbDetails.ServerConfig.ServerName}", ++dbRestoreCount);
				EnsureApplicationLoginHasRightsToRestoredDatabases(dbDetails.ServerConfig.ServerName, dbRestoreSettings.TargetDbName);
			}
			else
			{
				FireOnSubtaskStarted($"Granting rights on restored BI database {dbRestoreSettings.TargetDbName} to the application login on {dbDetails.ServerConfig.ServerName}", ++dbRestoreCount);
				var suffix = dbDetails.DatabaseType == DbFileInfo.DbTypeAuditDB ? Db.AuditDatabaseSuffix : Db.EdwDatabaseSuffix;
				EnsureApplicationLoginHasRightsToRestoredBiDatabase(connection, dbRestoreSettings.TargetDbName, dbRestoreSettings.TargetDbName + suffix);
			}
		}

		AdminConnection RetryAdminConnectionAfterTimeouts(string serverName, string dbName, int retries)
		{
			var count = 0;
			while (true)
			{
				count++;
				try
				{
					return Db.NewAdminConnection(serverName, dbName);
				}
				catch (TimeoutException)
				{
					if (count >= retries)
					{
						LogMessage($"Connection unavailable after maximum number of retries {count}.");
						throw;
					}
					else
					{
						LogMessage($"Connection timed out on attempt number {count}. Retrying connection.");
					}
				}
			}
		}

		void RestoreTransactionLogBackups(AdminConnection connection, DatabaseDetails dbDetails)
		{
			var backupDirectory = Path.GetDirectoryName(dbDetails.ServerConfig.BackupFilePath);
			var nameParts = Path.GetFileNameWithoutExtension(dbDetails.ServerConfig.BackupFilePath).Split('-');
			var previousMainDbName = nameParts[nameParts.Length - 1];

			var newOldDatabaseNames = new List<Tuple<string, string>>();

			for (var i = 0; i < dbDetails.DbFiles.Count; i++)
			{
				if (!newOldDatabaseNames.Exists(x => string.Equals(x.Item1, dbDetails.DatabaseName, StringComparison.OrdinalIgnoreCase)))
				{
					var prevDb = GetActualDatabaseName(previousMainDbName, dbDetails.DbFiles[i]);
					newOldDatabaseNames.Add(new Tuple<string, string>(dbDetails.DatabaseName, prevDb));
				}
			}

			var fileList = GetTransactionLogBackupFileListFromDbPath(connection, backupDirectory);

			foreach (var file in fileList)
			{
				var fileMatch = Utilities.TransactionLogBackupFileRegex.Match(file);
				if (fileMatch.Success)
				{
					var dbNameFromFile = fileMatch.Groups[1].Value;
					var dbMatch = newOldDatabaseNames.Find(x => string.Equals(x.Item2, dbNameFromFile, StringComparison.OrdinalIgnoreCase));
					if (dbMatch != null)
					{
						RestoreSingleLogBackupFile(connection, dbMatch.Item1, Path.Combine(backupDirectory, file));
					}
				}
			}
		}

		internal List<string> GetTransactionLogBackupFileListFromDbPath(DbConnection connection, string backupDirectory)
		{
			var files = GetFileListFromDbPath(connection, backupDirectory, Utilities.TransactionLogBackupFileExtension);
			IDbBackupFileInfo ReadHeaderOnly(string fileName)
			{
				IDbBackupFileInfo backUpFileInfo = null;
				try
				{
					backUpFileInfo = dbHeaderOnlyReader.ReadHeaderOnly(connection, Path.Combine(backupDirectory, fileName));
				}
				catch (Exception ex)
				{
					LogDetailedError(ex, $"Will not attempt to restore '{fileName}' as it cannot be read.", connection);
				}
				return backUpFileInfo;
			}

			var result = files
				.Select(fileName => (FileName: fileName, HeaderInfo: ReadHeaderOnly(fileName)))
				.Where(tuple => tuple.HeaderInfo != null)
				.Select(tuple => (tuple.HeaderInfo.DatabaseName, tuple.FileName, tuple.HeaderInfo.LastLSN))
				.OrderBy(tuple => tuple.DatabaseName, StringComparer.OrdinalIgnoreCase)
				.ThenBy(tuple => tuple.LastLSN)
				.ThenBy(tuple => tuple.FileName, StringComparer.OrdinalIgnoreCase)
				.Select(tuple => tuple.FileName)
				.ToList();

			return result;
		}

		void RestoreSingleLogBackupFile(AdminConnection connection, string dbName, string backupFile)
		{
			LogMessage($"Restoring log for database [{dbName}] on {connection.ServerName}. File: {backupFile}");
			var sqlScript = string.Format(CultureInfo.InvariantCulture, @"
If Exists(Select null From sys.databases Where name = '{0}' and state <> 0)
RESTORE LOG [{0}] FROM DISK = N'{1}' WITH NORECOVERY", dbName, backupFile);
			using (var cmd = connection.Command(sqlScript, Utilities.BackupTimeoutInSeconds))
			{
				try
				{
					cmd.ExecuteNonQuery();
				}
				catch (SqlException ex)
				{
					var errorMessage = string.Format(CultureInfo.CurrentCulture, "Error restoring transaction log backup '{0}'.", Path.GetFileName(backupFile));
					LogDetailedError(ex, errorMessage, connection);
				}
			}
		}

		void RestoreDifferentialBackups(AdminConnection connection, DatabaseDetails dbDetails)
		{
			var backupDirectory = Path.GetDirectoryName(dbDetails.ServerConfig.BackupFilePath);
			var nameParts = Path.GetFileNameWithoutExtension(dbDetails.ServerConfig.BackupFilePath).Split('-');
			var previousMainDbName = nameParts[nameParts.Length - 1];

			var processedDbs = new List<string>();

			foreach (DbFileInfo db in dbDetails.DbFiles)
			{
				var newDbName = GetActualDatabaseName(dbDetails.DatabaseName, db);
				if (processedDbs.Contains(newDbName))
				{
					continue;
				}

				var previousDbName = GetActualDatabaseName(previousMainDbName, db);
				var backupFile = Path.Combine(backupDirectory, previousDbName + Utilities.DifferentialBackupFileSuffix + Utilities.DifferentialBackupFileExtension);

				if (FileExists(connection, backupFile))
				{
					LogMessage("Restoring file: " + backupFile);
					RestoreSingleDifferentialBackupFile(connection, newDbName, backupFile);
					processedDbs.Add(newDbName);
				}
			}
		}

		void RestoreSingleDifferentialBackupFile(AdminConnection connection, string dbName, string backupFilePath)
		{
			var sqlScript = string.Format(CultureInfo.InvariantCulture, @"
If Exists(Select null From sys.databases Where name = '{0}' and state <> 0)
RESTORE DATABASE [{0}] FROM DISK = N'{1}' WITH NORECOVERY", dbName, backupFilePath);
			using (var cmd = connection.Command(sqlScript, Utilities.BackupTimeoutInSeconds))
			{
				try
				{
					cmd.ExecuteNonQuery();
				}
				catch (SqlException ex)
				{
					LogDetailedError(ex, "Error restoring differential backup.", connection);
				}
			}
		}

		void RestoreWithRecovery(AdminConnection connection, string databaseName)
		{
			RestoreSingleDatabaseWithRecovery(connection, databaseName);
		}

		void RestoreSingleDatabaseWithRecovery(AdminConnection connection, string db)
		{
			var sqlScript = string.Format(CultureInfo.InvariantCulture, @"
If Exists(Select null From sys.databases Where name = '{0}' and state <> 0)
RESTORE DATABASE [{0}] WITH RECOVERY, KEEP_CDC", db);
			using (var cmd = connection.Command(sqlScript, Utilities.BackupTimeoutInSeconds))
			{
				try
				{
					LogMessage($"Starting RECOVERY restore database {db} on {connection.ServerName}");
					cmd.ExecuteNonQuery();
					LogMessage($"Completed RECOVERY restore database {db} on {connection.ServerName}");
				}
				catch (SqlException ex)
				{
					LogDetailedError(ex, "Error restoring differential backup.", connection);
				}
			}
		}

		int RestoreWithNoRecovery(AdminConnection targetServerConnection, DatabaseDetails dbDetails)
		{
			var firstDbfile = dbDetails.DbFiles[0];

			var originalDbStatus = Utilities.GetDatabaseStatus(targetServerConnection, dbDetails.DatabaseName);
			var restoreSql = BuildRestoreSql();

			try
			{
				LogMessage($"Starting restore database {dbDetails.DatabaseName} on {targetServerConnection.ServerName} from file {firstDbfile.FilePath}");
				targetServerConnection.ExecuteNonQuery(restoreSql, Utilities.BackupTimeoutInSeconds);
				LogMessage($"Completed restore database {dbDetails.DatabaseName} on {targetServerConnection.ServerName} from file {firstDbfile.FilePath}");
			}
			catch (Exception ex)
			{
				LogDetailedError(ex, $"Error restoring database {dbDetails.DatabaseName}.", targetServerConnection);
				HandleRestoreFailure();
				throw;
			}

			string BuildRestoreSql()
			{
				var restoreBaseMask = "RESTORE DATABASE [{0}] FROM DISK='{1}' WITH NORECOVERY, REPLACE ";
				var restoreSql = string.Format(CultureInfo.InvariantCulture, restoreBaseMask, dbDetails.DatabaseName, firstDbfile.FilePath);
				var moveClauseBuilder = new StringBuilder(restoreSql);

				if (originalDbStatus != DatabaseStatus.DoesNotExist && originalDbStatus != DatabaseStatus.Restoring)
				{
					moveClauseBuilder.Insert(0, string.Format(@"ALTER DATABASE [{0}] SET OFFLINE WITH ROLLBACK IMMEDIATE;", dbDetails.DatabaseName));
				}

				foreach (DbFileInfo fileInfo in dbDetails.DbFiles)
				{
					var folderPath = GetFallbackFilePath(fileInfo.FileType, fileInfo.FolderPathView, dbDetails.ServerConfig.DataFilePath, dbDetails.ServerConfig.LogFilePath);
					var filePath = GetDatabaseFilePath(targetServerConnection, folderPath, dbDetails.DatabaseName, fileInfo.FilePathSuffix);
					moveClauseBuilder.Append(string.Format(CultureInfo.InvariantCulture, ", MOVE '{0}' TO '{1}'", fileInfo.LogicalName, filePath));
				}

				return moveClauseBuilder.ToString();
			}

			void HandleRestoreFailure()
			{
				try
				{
					var dbStatus = Utilities.GetDatabaseStatus(targetServerConnection, dbDetails.DatabaseName);
					if (dbStatus == DatabaseStatus.Offline && originalDbStatus == DatabaseStatus.Online)
					{
						targetServerConnection.ExecuteNonQuery(string.Format(@"ALTER DATABASE [{0}] SET ONLINE WITH ROLLBACK IMMEDIATE;", dbDetails.DatabaseName));
					}
				}
				catch (Exception ex)
				{
					LogDetailedError(ex, $"Error reverting database {dbDetails.DatabaseName} to its original state {originalDbStatus}.", targetServerConnection);
				}
			}

			return dbDetails.DbFiles.Count;
		}

		static Exception SynchroniseSynonymsCore(string serverName, string databaseName)
		{
			Db.InitializeDatabaseDetails(serverName, databaseName);

			try
			{
				EnterpriseApplicationConfiguration.ConfigureObjectFactory();
				using (Db.DisableSchemaVersionCheck())
				using (var adminConnection = Db.NewAdminConnection())
				{
					var director = ObjectFactory.New<BaseRefDbSynonymSynchroniser>(new UpgradeContext(), adminConnection);
					director.SynchroniseReferenceDbSynonyms();

					if (!director.IsCancelled)
					{
						new DbUserRepository().SynchroniseSynonyms(adminConnection);
					}
				}
				return null;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				return ex;
			}
		}

#if NETFRAMEWORK
		[SuppressMessage("CargoWiseOne", "CW1157:Do not use System.AppDomain.", Justification = "Pending migration")]   // WI00669071 - Do not use System.AppDomain.
		internal virtual void SynchroniseSynonyms(string dbServer, string targetDbName)
		{
			var appDomain = AppDomain.CreateDomain($"{nameof(DbRestoreManager)}_{nameof(SynchroniseSynonyms)}");
			using (new DisposableAction(() => AppDomain.Unload(appDomain)))
			{
				appDomain.SetData("ServerName", dbServer);
				appDomain.SetData("DatabaseName", targetDbName);
				appDomain.DoCallBack(() =>
				{
					var serverName = (string)AppDomain.CurrentDomain.GetData("ServerName");
					var databaseName = (string)AppDomain.CurrentDomain.GetData("DatabaseName");

					var ex = SynchroniseSynonymsCore(serverName, databaseName);
					AppDomain.CurrentDomain.SetData("Exception", ex);
				});

				var exception = (Exception)appDomain.GetData("Exception");
				if (exception != null)
				{
					LogMessage($"{nameof(SynchroniseSynonyms)} on Server: {dbServer}, Database: {targetDbName}, has encountered error: {exception}");
				}
			}
		}
#else
		internal virtual void SynchroniseSynonyms(string dbServer, string targetDbName)
		{
			var task = Task.Run(() => SynchroniseSynonymsCore(dbServer, targetDbName));

			var exception = task.Result;
			if (exception != null)
			{
				LogMessage($"{nameof(SynchroniseSynonyms)} on Server: {dbServer}, Database: {targetDbName}, has encountered error: {exception}");
			}
		}
#endif

		class UpgradeContext : IUpgradeContext
		{
			public bool IsHosted => EnvProxy.IsHostedWithCargowise;
			public bool? IsInternalSystem => throw new NotImplementedException();
			public bool? IsUATSystem => throw new NotImplementedException();
		}

		internal void EnsureApplicationLoginHasRightsToRestoredDatabases(string dbServer, string targetDbName)
		{
			using var appDomainWrapper = new AppDomainWrapper();
			var configuration = new ProcessConfig
			{
				NamespacePath = "Enterprise.DataTools.DbBackupAndRestore.Business",
				ClassName = nameof(DbRestoreManager),
				MethodName = nameof(EnsureApplicationLoginHasRightsToRestoredDatabasesStatic),
				MethodParameters = [dbServer, targetDbName],
				AssemblyDependancies = ["Microsoft.Extensions.DependencyInjection.dll"]
			};
			configuration.AssemblyFile = Path.Combine(configuration.BinFolder, "Enterprise.DbBackupAndRestore.Business.dll");
			var result = appDomainWrapper.RunMethodInProcess48(configuration);
			if (!string.IsNullOrEmpty(result))
			{
				LogMessage(Invariant($"\tUnable to map application login to all databases ({result}) on server {dbServer}.\r\n"));
			}
		}

		static void EnsureApplicationLoginHasRightsToRestoredDatabasesStatic(string dbServer, string targetDbName)
		{
			Db.InitializeDatabaseDetails(dbServer, targetDbName);

			var sdOrRefDbMatch = Utilities.DocManagerOrRefFileDbRegex.Match(targetDbName);
			var specificLoginDbName = (sdOrRefDbMatch.Success) ? sdOrRefDbMatch.Groups[0].Value : targetDbName;

			using (Db.DisableSchemaVersionCheck())
			using (var securityConnection = Db.NewAdminConnection(dbServer, specificLoginDbName))
			{
				try
				{
					// Ensure application login users are created
					((IDbLoginRepair)securityConnection).EnsureDbLoginsCorrectlyMappedToAllDatabases(msg => { });
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
#pragma warning disable CW1106 // Required to send data back from the AppDomainWrapper
					Console.WriteLine(ex.ToString());
#pragma warning restore CW1106
				}
			}
		}

		static Exception EnsureApplicationLoginHasRightsToRestoredBiDatabaseCore(string serverName, string databaseName)
		{
			Db.InitializeDatabaseDetails(serverName, databaseName);

			using (Db.DisableSchemaVersionCheck())
			using (var securityConnection = Db.NewAdminConnection())
			{
				try
				{
					// Ensure application login users are created
					((IDbLoginRepair)securityConnection).EnableApplicationDbLogins();
					((IDbLoginRepair)securityConnection).EnsureWriterDbLoginHasRightsToCurrentDatabase();
					return null;
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					return ex;
				}
			}
		}

#if NETFRAMEWORK
		[SuppressMessage("CargoWiseOne", "CW1157:Do not use System.AppDomain.", Justification = "Pending migration")]   // WI00669071 - Do not use System.AppDomain.
		void EnsureApplicationLoginHasRightsToRestoredBiDatabase(AdminConnection biConnection, string targetDbName, string biDbName)
		{
			if (biConnection != null && biConnection.DatabaseExists(biDbName))
			{
				using (((ICurrentDbControl)biConnection).UseDatabase(biDbName))
				{
					var appDomain = AppDomain.CreateDomain(nameof(EnsureApplicationLoginHasRightsToRestoredDatabases));
					using (new DisposableAction(() => AppDomain.Unload(appDomain)))
					{
						appDomain.SetData("ServerName", biConnection.ServerName);
						appDomain.SetData("DatabaseName", biDbName);
						appDomain.DoCallBack(() =>
						{
							var serverName = (string)AppDomain.CurrentDomain.GetData("ServerName");
							var databaseName = (string)AppDomain.CurrentDomain.GetData("DatabaseName");

							var ex = EnsureApplicationLoginHasRightsToRestoredBiDatabaseCore(serverName, databaseName);

							AppDomain.CurrentDomain.SetData("Exception", ex);
						});

						var exception = (Exception)appDomain.GetData("Exception");
						if (exception != null)
						{
							LogMessage(Invariant($"\tUnable to map application login to BI database ({exception.Message}).\r\n"));
						}
					}
				}
			}
		}
#else
		void EnsureApplicationLoginHasRightsToRestoredBiDatabase(AdminConnection biConnection, string targetDbName, string biDbName)
		{
			if (biConnection != null && biConnection.DatabaseExists(biDbName))
			{
				using (((ICurrentDbControl)biConnection).UseDatabase(biDbName))
				{
					var task = Task.Run(() => EnsureApplicationLoginHasRightsToRestoredBiDatabaseCore(biConnection.ServerName, biDbName));

					var exception = task.Result;
					if (exception != null)
					{
						LogMessage(Invariant($"\tUnable to map application login to BI database ({exception.Message}).\r\n"));
					}
				}
			}
		}
#endif

		#region Utility Methods

		internal protected virtual bool IsTargetDatabaseMainDb(string targetDbName)	=> DataUtils.IsDbNameAlphaNumeric(targetDbName);
		string GetMainDbName(string dbName) => dbName.Split('_')[0];

		Dictionary<string, DbFileInfoCollection> GetSpecificDbFiles(DbFileInfoCollection dbFiles, string dbType)
		{
			var result = new Dictionary<string, DbFileInfoCollection>();

			foreach (DbFileInfo dbFile in dbFiles)
			{
				if (dbFile.Visible && dbFile.DbType == dbType)
				{
					if (!result.ContainsKey(dbFile.DbName))
					{
						result[dbFile.DbName] = new DbFileInfoCollection();
					}
					result[dbFile.DbName].Add(dbFile);
				}
			}

			return result;
		}

		string GetFallbackFilePath(string fileType, string filePath, string dataFilePath, string logFilePath)
		{
			return (fileType == "D"
					? GetFallbackFilePath(filePath, dataFilePath)
					: GetFallbackFilePath(filePath, logFilePath));
		}

		string GetFallbackFilePath(string filePath, string userPath)
		{
			return !string.IsNullOrEmpty(userPath) ? userPath : filePath;
		}

		internal string GetActualDatabaseName(string targetDbName, DbFileInfo dbFile)
		{
			return GetActualDatabaseName(targetDbName, dbFile.SharedRefDb, dbFile.DbType, dbFile.DbName);
		}

		internal string GetActualDatabaseName(string targetDbName, bool isSharedRefDb, string dbType, string dbName)
		{
			var realDbName = targetDbName;

			if (isSharedRefDb)
			{
				realDbName = dbName;
			}
			else if (dbType != DbFileInfo.DbTypeMain)
			{
				realDbName += dbName;
			}
			return realDbName;
		}

		public static string GetBaseServerName(string dbServer)
		{
			var nameWithoutInstance = dbServer.Split('\\')[0];
			return nameWithoutInstance.Split('.')[0];
		}

		static string GetDatabaseFilePath(AdminConnection connection, string folderPath, string dbName, string fileSuffix)
		{
			var result = Path.Combine(folderPath, dbName + fileSuffix);

			var query = @"
				SELECT mf.physical_name
				FROM sys.master_files mf
				INNER JOIN sys.databases db ON db.database_id = mf.database_id
				WHERE db.name = @DbName
				AND REPLACE(mf.physical_name, '\\', '\') = @PhysicalName";

			using (var command = connection.Command(query))
			{
				command.AddParameter("@DbName", SqlDbType.VarChar, dbName);
				command.AddParameter("@PhysicalName", SqlDbType.VarChar, result);
				var physicalName = (string)command.ExecuteScalar();

				if (physicalName != null)
				{
					result = physicalName;
				}
			}
			return result;
		}

		void LogDetailedError(Exception ex, string operationContext = null, DbConnection connection = null, bool taskFailed = false, bool showStackTrace = false)
		{
			var errorMessage = ex.Message;
			if (ex is SqlException sqlException)
			{
				var errorHandler = new DbErrorHandler(sqlException, connection);
				var userFriendlyMessage = errorHandler.GetDBErrorUserFriendlyMessage();
				errorMessage = string.Format(CultureInfo.CurrentCulture, "{0}\r\n{1}", !string.IsNullOrEmpty(userFriendlyMessage) ? userFriendlyMessage : sqlException.Message, errorHandler.GetExtraDebugInformation());
			}

			var contextMessage = string.IsNullOrEmpty(operationContext)
						 ? string.Empty
						 : operationContext + "\r\n";

			var stackTraceMessage = showStackTrace ? $"\r\nStackTrace: {ex.StackTrace}" : string.Empty;

			if (taskFailed)
			{
				FireOnTaskFailed(string.Format(CultureInfo.CurrentCulture, "{0}Exception: {1} \r\nMessage: {2}{3}", contextMessage, ex.GetType(), errorMessage, stackTraceMessage));
			}
			else
			{
				LogMessage(string.Format(CultureInfo.CurrentCulture, "{0}Exception: {1} \r\nMessage: {2}{3}", contextMessage, ex.GetType(), errorMessage, stackTraceMessage));
			}
		}

		static DbFileInfoCollection GetRelatedDbFileInfoCollection(DbConnection conn, string dbServer, string backupFilePath)
		{
			var backupFile = new FileInfo(backupFilePath);
			var backupFolderPath = backupFile.DirectoryName;
			var fileList = GetFileListFromDbPath(conn, backupFolderPath, Utilities.FullBackupFileExtension);
			fileList.AddRange(GetFileListFromDbPath(conn, backupFolderPath, Utilities.DifferentialBackupFileExtension));

			var fileValidity = Utilities.DetermineFileValidity(backupFilePath);
			var sharedRefFileRegex = RefDbTableNameResolver.SharedDbPrefix + RefDbTableNameResolver.RefDbAffix + @"-\w{3}-\w{2}-[0-9]+"
				+ "|" + RefDbTableNameResolver.SharedAvailabilityGroupRefDbPrefix + @"[\w-0-9]+-[0-9]+";

			var relatedDocManagerFileRegex = GetRelatedDatabaseBackupFileRegex(backupFile.Name, fileValidity, @"_SD[0-9]{3}");
			var relatedUserRepositoryFileRegex = GetRelatedDatabaseBackupFileRegex(backupFile.Name, fileValidity, DbUserRepository.RepositoryDbSuffix);
			var relatedExclusiveRefFileRegex = GetRelatedDatabaseBackupFileRegex(backupFile.Name, fileValidity, @"_RefDb_\w{3}_\w{2}");
			var relatedSharedRefFileRegex = GetRelatedDatabaseBackupFileRegex(backupFile.Name, fileValidity, sharedRefFileRegex, isSharedDb: true);
			var relatedSingleSharedRefFileRegex = GetRelatedDatabaseBackupFileRegex(backupFile.Name, fileValidity, "CW-RefDatabase", isSharedDb: true);

			fileList = GetDatabaseBackupFiles(fileList, conn, backupFilePath);

			var result = MatchDbFiles(
				backupFilePath,
				backupFolderPath,
				fileList,
				dbServer,
				(relatedDocManagerFileRegex, DbFileInfo.DbTypeEDocs, sharedRefDb: false),
				(relatedUserRepositoryFileRegex, DbFileInfo.DbTypeUserRepository, sharedRefDb: false),
				(relatedExclusiveRefFileRegex, DbFileInfo.DbTypeRefDB, sharedRefDb: false),
				(relatedSharedRefFileRegex, DbFileInfo.DbTypeRefDB, sharedRefDb: true),
				(relatedSingleSharedRefFileRegex, DbFileInfo.DbTypeSingleSharedRefDB, sharedRefDb: true));

			if (result.Count == 0)
			{
				throw new FileLoadException("File may not exist or SQL Server may not have permission to access the file.");
			}

			return result;
		}

		static void GetRelatedBiDbFileInfoCollection(DbFileInfoCollection collection, string biServer, string backupFilePath, string biFileType)
		{
			if (biFileType != null)
			{
				string dbName = null;
				if (biFileType == DbFileInfo.DbTypeAuditDB)
				{
					dbName = Db.AuditDatabaseSuffix;
				}
				else if (biFileType == DbFileInfo.DbTypeEdwDB)
				{
					dbName = Db.EdwDatabaseSuffix;
				}

				var dbFiles = DbFileInfoCollection.GetDbFileInfoCollectionFromBackup(biServer, backupFilePath, biFileType, dbName);
				for (var i = 0; i < dbFiles.Count; i++)
				{
					collection.Add(dbFiles[i]);
				}
			}
		}

		static List<string> GetFileListFromDbPath(DbConnection conn, string backupFolderPath, string fileExtension)
		{
			var fileList = new List<string>();
			var sqlText = string.Format(CultureInfo.InvariantCulture, "EXEC xp_dirtree '{0}', 1, 1", backupFolderPath);

			using (var cmd = conn.Command(sqlText))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					var isFileFlag = Convert.ToInt32(reader["file"], CultureInfo.CurrentCulture);

					if (isFileFlag == 1)
					{
						var fileName = reader["subdirectory"].ToString().Trim();
						if (fileName.EndsWith(fileExtension, StringComparison.OrdinalIgnoreCase))
						{
							fileList.Add(fileName);
						}
					}
				}
			}

			return fileList;
		}

		// Given a backup file, this method extracts the DB files and puts them into a collection
		static DbFileInfoCollection MatchDbFiles(
			string backupFilePath,
			string backupFolderPath,
			List<string> fileList,
			string dbServer,
			params (Regex regex, string dbType, bool sharedRefDb)[] matchers)
		{
			var collection = new DbFileInfoCollection();

			foreach (var fileName in fileList)
			{
				var filePath = Path.Combine(backupFolderPath, fileName);
				string dbType = null;
				string dbName = null;
				var sharedRefDb = false;

				if (backupFilePath.Equals(filePath, StringComparison.OrdinalIgnoreCase))
				{
					dbType = DbFileInfo.DbTypeMain;

					dbName =
						  Utilities.StandardBackupFileRegex.IsMatch(fileName)
						? Utilities.StandardBackupFileRegex.Match(fileName).Groups[4].Value
						: fileName;
				}
				else
				{
					foreach (var (regex, type, isShared) in matchers)
					{
						if (regex.IsMatch(fileName))
						{
							dbType = type;
							dbName = regex.Match(fileName).Groups[1].Value;
							sharedRefDb = isShared;
							break;
						}
					}
				}

				if (dbType != null)
				{
					var dbFiles = DbFileInfoCollection.GetDbFileInfoCollectionFromBackup(dbServer, filePath, dbType, dbName);

					for (var i = 0; i < dbFiles.Count; i++)
					{
						if (sharedRefDb)
						{
							dbFiles[i].SharedRefDb = true;
						}
						collection.Add(dbFiles[i]);
					}
				}
			}

			return collection;
		}

		/// <summary>
		/// Odyssey.bak
		/// Odyssey_SD001.bak
		/// Odyssey_UserRepository.bak
		/// Odyssey_RefDb_Ent_US.bak
		/// CW-RefDb-Cmr-AU-000007.bak
		/// CW-AG-RefDb-ORDWP4-CP1AS1-Cmr-AU-000007.bak
		/// </summary>
		/// <param name="fileName">Main Database Backup File Name</param>
		/// <param name="relatedDbRegexPattern">Related Database Additional Pattern</param>
		/// <param name="fileValidity">Type of file</param>
		/// <param name="isSharedDb"></param>
		/// <returns></returns>
#if DEBUG
		protected
#endif
		static Regex GetRelatedDatabaseBackupFileRegex(string fileName, FileValidity fileValidity, string relatedDbRegexPattern, bool isSharedDb = false)
		{
			string resultRegexPattern;

			if (fileValidity == FileValidity.ValidStandard)
			{
				var standardBackupFileMatch = Utilities.StandardBackupFileRegex.Match(fileName);

				resultRegexPattern = string.Format(
					CultureInfo.InvariantCulture,
					"^{0}_T-{1}_S-{2}_D-{3}{4}$",
					standardBackupFileMatch.Groups[1].Value,
					standardBackupFileMatch.Groups[2].Value,
					standardBackupFileMatch.Groups[3].Value,
					(isSharedDb ? "(" : standardBackupFileMatch.Groups[4].Value + "(") + relatedDbRegexPattern + ")",
					Utilities.FullBackupFileExtension.Replace(".", @"\."));
			}
			else
			{
				var extension = new FileInfo(fileName).Extension;
				var dbName = fileName.Replace(extension, "");
				resultRegexPattern = string.Format(
					CultureInfo.InvariantCulture,
					"^{0}{1}$",
					(isSharedDb ? "(" : dbName + "(") + relatedDbRegexPattern + ")",
					extension.Replace(".", @"\."));
			}

			return new Regex(resultRegexPattern, RegexOptions.IgnoreCase);
		}

		public static bool FileExists(DbConnection conn, string filePath)
		{
			var isExists = false;
			var sqlText = string.Format(CultureInfo.InvariantCulture, "EXEC xp_fileexist '{0}'", filePath);
			using (var cmd = conn.Command(sqlText))
			using (var reader = cmd.ExecuteReader())
			{
				if (reader.Read())
				{
					isExists = (Convert.ToInt32(reader["File Exists"], CultureInfo.InvariantCulture) == 1);
				}
			}
			return isExists;
		}

		/// <summary>
		/// Given a list of files, this method returns which files in that list have valid SqlServer metadata
		/// </summary>
		/// <param name="fileList">The list of file names to check for metadata</param>
		/// <param name="conn">The DB connection</param>
		/// <param name="backupFilePath">The path that the files are located in</param>
		/// <returns></returns>
		static List<string> GetDatabaseBackupFiles(List<string> fileList, DbConnection conn, string backupFilePath)
		{
			var backupFile = new FileInfo(backupFilePath);

			return !FileExists(conn, backupFilePath)
				? new List<string>()
				: GetDatabaseBackupFromMetadata(fileList, conn, backupFile);
		}

		static List<string> GetDatabaseBackupFromMetadata(IEnumerable<string> fileList, DbConnection conn, FileInfo backupFile)
		{
			var result = new List<string>();
			bool hasDescription;
			DateTime backupTimeSet;
			string backupSet = null;

			var sqlText = @"RESTORE LABELONLY FROM DISK = '" + backupFile.FullName + "'";

			// Reads metadata from the backup file - https://docs.microsoft.com/en-us/sql/t-sql/statements/restore-statements-labelonly-transact-sql
			using (var cmd = conn.Command(sqlText))
			using (var reader = cmd.ExecuteReader())
			{
				if (reader.Read())
				{
					hasDescription = !reader.IsDBNull(7);
					backupTimeSet = (DateTime)reader["mediadate"];
					if (hasDescription)
					{
						backupSet = reader["mediadescription"].ToString().Trim();
					}
				}
				else
				{
					return result;
				}
			}

			return GetDatabaseBackupFilesFromMetadata(fileList, conn, backupFile.DirectoryName, hasDescription, backupSet, backupTimeSet);
		}

		static List<string> GetDatabaseBackupFilesFromMetadata(IEnumerable<string> fileList, DbConnection conn, string backupFolderPath, bool hasDescription, string backupSet, DateTime backupTimeSet)
		{
			var result = new List<string>();

			foreach (var fileName in fileList)
			{
				var path = Path.Combine(backupFolderPath, fileName);

				if (Utilities.StandardBackupFileRegex.Match(path).Success
					|| Utilities.StandardFBKBackupFileRegex.Match(path).Success
					|| Utilities.StandardDBKBackupFileRegex.Match(path).Success)
				{
					continue;
				}

				var sqlText = @"RESTORE LABELONLY FROM DISK = '" + path + "'";
				using (var cmd = conn.Command(sqlText))
				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						if (hasDescription)
						{
							if (backupSet == reader["mediadescription"].ToString().Trim())
							{
								result.Add(fileName);
							}
						}
						else
						{
							result.Add(fileName);
						}
					}
				}
			}

			return result;
		}

		public List<string> GetAvailabilityGroupsList(string dbServer)
		{
			var result = new List<string>();

			try
			{
				var isListener = IsServerAvailabilityGroupListener(dbServer);
				FireOnTaskStarted(string.Format(CultureInfo.CurrentCulture, "Getting list of availability groups\r\n\r\nServer: {0}", dbServer));
				using (var dbConnection = Db.NewAdminConnection(dbServer, Db.SqlMasterDb))
				{
					result = AlwaysOnHelper.GetAvailabilityGroups(dbConnection);
				}
				FireOnTaskCompleted("Finished getting list of availability groups");
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				LogDetailedError(ex, $"Failed to retrieve the list of availability groups associated with '{dbServer}'.", taskFailed: true);
			}

			return result;
		}

		public string GetAvailabilityGroupName(string dbServer, string dbName)
		{
			var result = string.Empty;
			try
			{
				using (var dbConnection = Db.NewAdminConnection(dbServer, Db.SqlMasterDb))
				{
					if (dbConnection.DatabaseExists(dbName))
					{
						var isPartOfAlwaysOn = AlwaysOnHelper.IsDbPartOfAlwaysOn(dbConnection, dbName);
						if (isPartOfAlwaysOn)
						{
							result = AlwaysOnHelper.GetAvailabilityGroupName(dbConnection, dbName);
							return result;
						}
					}

					var mainDbName = GetMainDbName(dbName);
					if (!string.Equals(dbName, mainDbName, StringComparison.OrdinalIgnoreCase) && dbConnection.DatabaseExists(mainDbName))
					{
						var isMainDbPartOfAlwaysOn = AlwaysOnHelper.IsDbPartOfAlwaysOn(dbConnection, mainDbName);
						if (isMainDbPartOfAlwaysOn)
						{
							result = AlwaysOnHelper.GetAvailabilityGroupName(dbConnection, mainDbName);
						}
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				LogDetailedError(ex, "Failed to retrieve the availability group name.");
			}
			return result;
		}

		public string GetPrimaryReplicaOnAvailabilityGroup(string dbServer, string groupName)
		{
			var result = string.Empty;
			try
			{
				using (var dbConnection = Db.NewAdminConnection(dbServer, Db.SqlMasterDb))
				{
					result = AlwaysOnHelper.GetPrimaryReplicaOnAvailabilityGroup(dbConnection, groupName);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				LogDetailedError(ex, $"Failed to retrieve primary replica for availability group '{groupName}'.");
			}
			return result;
		}

		public bool IsServerAvailabilityGroupListener(string dbServer)
		{
			var result = false;
			try
			{
				using (var dbConnection = Db.NewAdminConnection(dbServer, Db.SqlMasterDb))
				{
					return AlwaysOnHelper.IsServerAvailabilityGroupListener(dbConnection);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				LogDetailedError(ex, $"Failed to verify listener status for '{dbServer}' on availability group.");
			}
			return result;
		}

		public bool IsPrimaryReplicaOnAvailabilityGroup(string dbServer, string groupName)
		{
			var result = false;
			try
			{
				using (var dbConnection = Db.NewAdminConnection(dbServer, Db.SqlMasterDb))
				{
					return AlwaysOnHelper.IsPrimaryReplicaOnAvailabilityGroup(dbConnection, groupName);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				LogDetailedError(ex, $"Failed to check availability mode for '{dbServer}' on availability group.");
			}
			return result;
		}

		public bool IsDbPartOfAlwaysOn(string dbServer, string dbName)
		{
			var result = false;
			try
			{
				using (var dbConnection = Db.NewAdminConnection(dbServer, Db.SqlMasterDb))
				{
					return AlwaysOnHelper.IsDbPartOfAlwaysOn(dbConnection, dbName);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				LogDetailedError(ex, $"Failed to check database participation in availability group.");
			}
			return result;
		}

		void RemoveS3RegistryItemsFromAuxDb(AdminConnection connection, string vaultAuxDbName)
		{
			connection.ExecuteNonQuery($@" DELETE FROM [{vaultAuxDbName}]..TempStmData WHERE SD_Name IN ('EDocsStorageProvider', 'EDocsStorageServiceUrl', 'DocManagerStorageBucketName', 'EDocsStorageAccess')");
		}

		void DoNotPreserveTestValueForS3RegistryItems(AdminConnection connection, string dbName)
		{
			connection.ExecuteNonQuery($@" UPDATE [{dbName}]..StmData SET [SD_PreserveTestValue] = 0 WHERE SD_Name IN ('EDocsStorageProvider', 'EDocsStorageServiceUrl', 'DocManagerStorageBucketName', 'EDocsStorageAccess')");
		}

		static bool IsEDocsStorageProviderS3(AdminConnection connection, string dbName)
		{
			var storageProvider = Utilities.GetRegistryValue(connection, dbName, "EDocsStorageProvider");

			return !string.IsNullOrEmpty(storageProvider) && storageProvider.Equals("S3", StringComparison.OrdinalIgnoreCase);
		}

		void StoreNewReadOnlyAccessKeys(AdminConnection connection, string dbName)
		{
			LogMessage("Getting bucket name");
			var bucketName = Utilities.GetRegistryValue(connection, dbName, "DocManagerStorageBucketName");

			if (TryGetAccessKey(out var accessKey, out var internalEx))
			{
				LogMessage("Updating database with access key");
				Utilities.UpdateRegistry(connection, dbName, "EDocsStorageAccess", accessKey, false);
			}
			else
			{
				LogMessage($"Clearing S3 credentials and setting eDocs Storage Provider to DB");
				Utilities.UpdateRegistry(connection, dbName, "EDocsStorageProvider", "DB", false);
				Utilities.DeleteRegistry(connection, dbName, "EDocsStorageServiceUrl");
				Utilities.DeleteRegistry(connection, dbName, "DocManagerStorageBucketName");
				Utilities.DeleteRegistry(connection, dbName, "EDocsStorageAccess");

				FireOnTaskFailed("Failed to create read-only access keys for S3.");

				ExceptionDispatchInfo.Capture(internalEx).Throw();
			}

			bool TryGetAccessKey(out string accessKeyString, out Exception exception)
			{
				try
				{
					LogMessage("Getting readonly access key");
					accessKeyString = AWSHelper.GetReadOnlyAccessKeysWithAssumeRole(bucketName);
					exception = null;
					return true;
				}
				catch (Exception ex)
				{
					accessKeyString = null;
					exception = ex;
					return false;
				}
			}
		}

		#endregion

		#region Delegate

		void PromptForReleaseKey(SessionInfo sessionInfo)
		{
			OnPromtForReleaseKey?.Invoke(sessionInfo);
		}

		public ReleaseKeyDelegate OnPromtForReleaseKey;

		#endregion

		readonly IDbHeaderOnlyReader dbHeaderOnlyReader;
		readonly IDatabaseWaiter databaseWaiter;
		internal ServerConfigurationUtils serverConfigurationUtils = new ServerConfigurationUtils();
	}

	#region Enums

	public enum DbRestoreOption
	{
		RestoreWithRecovery = 0,
		RestoreWithNoRecovery = 1,
		CopyProdToTest = 2
	}

	#endregion
}
