using System;
using System.Data;
using System.Globalization;
using CargoWise.Bi.Common;
using CargoWise.Bi.Maintenance;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.DbUpgrader.Foundation;
using Enterprise.DbUpgrader.Shared;
using Enterprise.Environment;

namespace Enterprise.DbUpgrader.Startup
{
	abstract class BusinessIntelligenceUpgradeConclusion : UpgradeConclusion
	{
		public BusinessIntelligenceUpgradeConclusion(AdminConnection mainDbConnection, AdminConnection biConnection)
			: base(mainDbConnection)
		{
			this.biConnection = biConnection ?? throw new ArgumentNullException(nameof(biConnection));
		}
		protected readonly AdminConnection biConnection;

		protected abstract string BiDatabaseType { get; }
		protected abstract string BiDatabaseName { get; }

		public override void RunAfterUpgradeSteps(IUpgradeContext upgradeContext, IUpgradeTaskWorkflowLogger logger)
		{
			ShrinkLogFile(logger);
			EnsureLoginsMappedToBiDatabase();
			CreateLinkedServer(logger);
			BackupBiDatabase(logger);
			RunCleanupTasks();
		}

		void CreateLinkedServer(IUpgradeTaskWorkflowLogger logger)
		{
			logger.StartTask("Creating linked server");
			new LinkedServerCreator(biConnection, mainDbConnection.ServerName, logger).CreateLinkedServer();
		}

		#region Backup BI database

		protected void BackupBiDatabase(IUpgradeTaskWorkflowLogger logger)
		{
			var shouldCreateBackup = DataUtils.LoadDbExtendedProperty(biConnection, "ShouldCreateBackup", BiDatabaseName);
			if (!string.IsNullOrEmpty(shouldCreateBackup))
			{
				DataUtils.DropDbExtendedProperty(biConnection, "ShouldCreateBackup", BiDatabaseName);
				var backupFilePath = DbRegistry.BackupFilePath.LoadValue(mainDbConnection);
				if (!string.IsNullOrEmpty(backupFilePath) && (biConnection.ServerNameReportedByDatabase.Equals(mainDbConnection.ServerNameReportedByDatabase, StringComparison.OrdinalIgnoreCase) || IsBackupDirectoryUncPath(backupFilePath)))
				{
					using (((ICurrentDbControl)biConnection).UseDatabase(BiDatabaseName))
					{
						logger.StartTask(string.Format(CultureInfo.InvariantCulture, "Creating backup for database [{0}].", BiDatabaseName));
						BackupDatabase(logger, biConnection, backupFilePath);
					}
				}
				else if (string.IsNullOrEmpty(backupFilePath))
				{
					logger.ShowInfoMessage($"Backup file path registry is empty. Skipping full backup for '{BiDatabaseName}'.");
				}
				else if (!IsBackupDirectoryUncPath(backupFilePath))
				{
					logger.ShowInfoMessage($"Backup directory is not a UNC path. Skipping full backup for '{BiDatabaseName}'.");
				}
			}
		}

		void BackupDatabase(IUpgradeTaskWorkflowLogger logger, AdminConnection biConnection, string backupFilePath)
		{
			try
			{
				CallBackupDbProcedure(biConnection, backupFilePath);
				DeleteBackupHistory(logger, biConnection);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				logger.ShowInfoMessage(string.Format(CultureInfo.InvariantCulture, "Failed to create a backup for database - {0}\r\n{1}", BiDatabaseName, ex));
			}
		}

		void CallBackupDbProcedure(AdminConnection biConnection, string backupFilePath)
		{
			var backupTimeStamp = GetBackupTimeStamp(biConnection);
			var fullServerName = biConnection.ServerNameReportedByDatabase.Trim().Replace('\\', '-');
			var backupFileName = BiDatabaseName + ".bak";
			var description = "T-" + backupTimeStamp + "_S-" + fullServerName;
			bool useDbBackupCompression;
			bool backupIncludesCheckSum;

			using (((ICurrentDbControl)mainDbConnection).UseDatabase(mainDbName))
			{
				useDbBackupCompression = Env.Registry.UseDbBackupCompression;
				backupIncludesCheckSum = Env.Registry.BackupIncludesCheckSum;
			}

			using (var backupCmd = biConnection.Command("ep_BackupDb"))
			{
				backupCmd.CommandTimeout = 0; // No timeout
				backupCmd.CommandType = CommandType.StoredProcedure;
				backupCmd.AddParameter("@DbName", SqlDbType.VarChar, 128, BiDatabaseName);
				backupCmd.AddParameter("@FolderPath", SqlDbType.VarChar, 800, backupFilePath);
				backupCmd.AddParameter("@FileName", SqlDbType.VarChar, 200, backupFileName);
				backupCmd.AddParameter("@BkpType", SqlDbType.VarChar, 20, "FULL");
				backupCmd.AddParameter("@BackupDescription", SqlDbType.NVarChar, 255, description);
				backupCmd.AddParameter("@IsCompressed", SqlDbType.Bit, useDbBackupCompression);
				backupCmd.AddParameter("@UseCheckSum", SqlDbType.Bit, backupIncludesCheckSum);

				backupCmd.ExecuteNonQuery();
			}
		}

		string GetBackupTimeStamp(AdminConnection biConnection)
		{
			var backupTime = biConnection.ExecuteScalar<DateTime>("SELECT GETDATE()");
			return backupTime.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
		}

		bool IsBackupDirectoryUncPath(string backupFilePath)
		{
			return backupFilePath.StartsWith("\\", StringComparison.OrdinalIgnoreCase);
		}

		void DeleteBackupHistory(IUpgradeTaskWorkflowLogger logger, AdminConnection biConnection)
		{
			try
			{
				using (DbCommand deleteCmd = biConnection.Command("ep_DeleteBackupHistory"))
				{
					deleteCmd.CommandTimeout = 0; // No timeout
					deleteCmd.CommandType = CommandType.StoredProcedure;
					deleteCmd.AddParameter("@oldest_date", SqlDbType.DateTime, Env.Time.CurrentUtcDate.AddYears(-2));
					deleteCmd.ExecuteNonQuery();
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				logger.ShowInfoMessage(string.Format(CultureInfo.InvariantCulture, "Failed to delete backup history of database - {0}\r\n{1}", BiDatabaseName, ex));
			}
		}

		#endregion

		#region  Shrink Log File

		void ShrinkLogFile(IUpgradeTaskWorkflowLogger logger)
		{
			if (Env.Registry.SkipLogShrinkingAndBackupInUpgrade)
			{
				return;
			}

			try
			{
				logger.ShowInfoMessage(".");
				logger.StartTask($"--- Shrink {BiDatabaseType} log file - START ---");

				ShrinkLogFileOnDatabase(logger, biConnection, BiDatabaseName);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				logger.ShowInfoMessage(ex.Message);
			}
			finally
			{
				logger.ShowInfoMessage(".");
				logger.StartTask($"--- Shrink {BiDatabaseType} log file - END   ---");
				logger.ShowInfoMessage(".");
			}
		}

		#endregion

		protected void EnsureLoginsMappedToBiDatabase()
		{
			if (biConnection != null && !String.Equals(mainDbConnection.ServerName, biConnection.ServerName, StringComparison.OrdinalIgnoreCase))
			{
				// Logins (server level)
				// Connection initial and current database is master => Db.DatabaseName used to determine login names.
				((IDbLoginRepair)biConnection).EnableApplicationDbLogins();

				// Database user mapping (DB level)
				var ensureLoginsMappedToCurrentDbAction = new Action<string>(
					(db) =>
					{
						if (!String.IsNullOrWhiteSpace(db) && biConnection.DatabaseExists(db))
						{
							using (((ICurrentDbControl)biConnection).UseDatabase(db))
							{
								((IDbLoginRepair)biConnection).EnsureDbLoginsHaveRightsToCurrentDatabase();
							}
						}
					}
				);

				ensureLoginsMappedToCurrentDbAction(BiDatabaseName);
			}
		}

		void RunCleanupTasks()
		{
			if (biConnection != null)
			{
				UpgUtils.CleanupAuxDatabases(biConnection, Db.DatabaseName);
			}
		}

		#region Shrink Log Files

		protected override int TargetLogSize
		{
			get
			{
				if (!targetLogSize.HasValue)
				{
					var recommendedSize = Math.Max(MainDbSizeMb * 0.05m, BiDbSizeMb * 0.1m);
					if (recommendedSize >= 4096)
					{
						recommendedSize = 4096;
					}
					else
					{
						recommendedSize = Math.Ceiling(recommendedSize / 64) * 64;
					}
					targetLogSize = Convert.ToInt32(recommendedSize);
				}
				return targetLogSize.Value;
			}
		}
		int? targetLogSize;

		decimal MainDbSizeMb
		{
			get
			{
				return GetDatabaseSizeMb(mainDbConnection, Db.DatabaseName);
			}
		}

		decimal BiDbSizeMb
		{
			get
			{
				return GetDatabaseSizeMb(biConnection, BiDatabaseName);
			}
		}

		decimal GetDatabaseSizeMb(DbConnection connection, string dbName)
		{
			using (var cmd = connection.Command("dbo.GetDBSizeWithFileType"))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameter("@DbName", SqlDbType.NVarChar, 128, dbName);
				cmd.AddParameter("@FileTypeDesc", SqlDbType.NVarChar, 60, "ROWS");
				cmd.AddOutputParameter("@SizeMb", SqlDbType.Decimal, 32, 0, 2, null);
				cmd.ExecuteNonQuery();

				var totalSize = Convert.ToDecimal(cmd.GetParameterValue("@SizeMb"), CultureInfo.InvariantCulture);
				return totalSize;
			}
		}

		#endregion

		#region Cleanup BI databases

		public void CleanupBiDatabase(IUpgradeTaskWorkflowLogger logger)
		{
			if (biConnection != null)
			{
				DropDatabaseIfExistsAndIsEmpty(logger, biConnection, BiDatabaseName);
			}
		}

		protected void DropDatabaseIfExistsAndIsEmpty(IUpgradeTaskWorkflowLogger logger, DbConnection connection, string dbName)
		{
			try
			{
				if (connection.DatabaseExists(dbName))
				{
					var biDbVersion = DataUtils.LoadDbExtendedProperty(connection, BiConstants.MainDbSchemaVersionExtPtyName, dbName);

					if (String.IsNullOrWhiteSpace(biDbVersion) && !DoesMasterStateExists(connection, dbName))
					{
						logger?.ShowInfoMessage($"Dropping database [{dbName}]");
						DropDatabase(connection, dbName);
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				logger?.ShowInfoMessage(String.Format(CultureInfo.InvariantCulture, "Cannot drop database [{0}]. {1}", dbName, ex.Message));
			}
		}

		bool DoesMasterStateExists(DbConnection connection, string dbName)
		{
			using (((ICurrentDbControl)connection).UseDatabase(dbName))
			{
				var sqlText = $"IF EXISTS (SELECT NULL FROM sys.tables t INNER JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE t.name = 'MasterState' AND s.name = '{BiConstants.BiAdminSchemaName}') SELECT 1 ELSE SELECT 0";
				return Convert.ToBoolean(connection.ExecuteScalar(sqlText), CultureInfo.InvariantCulture);
			}
		}

		void DropDatabase(DbConnection connection, string dbName)
		{
			var query = $"DROP DATABASE [{dbName}]";
			connection.ExecuteNonQuery(query);
		}

		#endregion
	}
}
