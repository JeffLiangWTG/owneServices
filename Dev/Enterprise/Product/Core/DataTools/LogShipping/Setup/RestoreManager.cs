using System;
using System.Text;
using CargoWise.Common;

namespace Enterprise.LogShipping.Setup
{
	[Flags]
	public enum DatabaseInitialisationOptions
	{
		None,
		Reinitialise,
		OnlyGenerateScript
	}

	public class RestoreManager
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public bool Restore(LogShippingInfo info)
		{
			Argument.NotNull(info, nameof(info));

			bool result = true;

			foreach (DatabaseInfo dbInfo in info.GetDatabaseInfoListToProcess())
			{
				if (dbInfo != null &&
					dbInfo.ShouldInitialise &&
					info.SecondaryServer != null &&
					!string.IsNullOrEmpty(dbInfo.SecondaryDatabaseName) &&
					!string.IsNullOrEmpty(dbInfo.BackupFullFileName))
				{
					ShowMessage(string.Format("Database restore started.\r\nServer: {0}\r\nDatabase: {1}\r\n", info.SecondaryServer.FullInstanceName, dbInfo.SecondaryDatabaseName));

					try
					{
						using var connection = DbManager.OpenNewConnectionWithOdysseyAdminLogin(info.SecondaryServer.FullInstanceName);
						var restoreScript = dbInfo.SetupInfo.PrimaryServer != null ? GetRestoreScript(dbInfo, connection) : string.Empty;

						if (info.MainDatabase != null && dbInfo.SecondaryDatabaseExists())
						{
							ShowMessage("Getting exclusive database access.");
							DbManager.KillConnections(connection, info.MainDatabase.SecondaryDatabaseName);
							ShowMessage("Success.");

							ShowMessage("Dropping secondary database.");
							DbManager.DropDatabase(connection, info.MainDatabase.SecondaryDatabaseName);
							ShowMessage("Success.\r\n");
						}

						ShowMessage("Restoring...");

						using (var cmd = DbManager.NewSqlCommand(restoreScript, connection))
						{
							cmd.CommandTimeout = TimeoutInSeconds;
							cmd.ExecuteNonQuery();
						}

						ShowMessage("Database restore completed with success.\r\n");
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						result = false;
						ShowMessage("Database restore failed.\r\n" + ex.Message);
						break;
					}
				}
			}

			return result;
		}

		public string GetRestoreScript(DatabaseInfo dbInfo)
		{
			Argument.NotNull(dbInfo, nameof(dbInfo));
			Argument.NotNull(dbInfo.SetupInfo.PrimaryServer, nameof(dbInfo.SetupInfo.PrimaryServer));
			Argument.NotNull(dbInfo.SetupInfo.SecondaryServer, nameof(dbInfo.SetupInfo.SecondaryServer));
			Argument.NotNullOrEmpty(dbInfo.SecondaryDatabaseName, nameof(dbInfo.SecondaryDatabaseName));
			Argument.NotNullOrEmpty(dbInfo.BackupFullFileName, nameof(dbInfo.BackupFullFileName));

			string result;

			using (var connection = DbManager.OpenNewConnectionWithOdysseyAdminLogin(dbInfo.SetupInfo.SecondaryServer.FullInstanceName))
			{
				result = GetRestoreScript(dbInfo, connection);
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		string GetRestoreScript(DatabaseInfo dbInfo, SqlConnection connection)
		{
			Argument.NotNull(dbInfo, nameof(dbInfo));
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNull(dbInfo.SetupInfo.PrimaryServer, nameof(dbInfo.SetupInfo.PrimaryServer));
			Argument.NotNullOrEmpty(dbInfo.SecondaryDatabaseName, nameof(dbInfo.SecondaryDatabaseName));
			Argument.NotNullOrEmpty(dbInfo.BackupFullFileName, nameof(dbInfo.BackupFullFileName));

			ShowMessage(string.Format("Getting list of database files in the backup...\r\nServer  : {0}\r\nBackup Path: {1}", dbInfo.SetupInfo.PrimaryServer.ServerName, dbInfo.BackupFileName));
			BackupFileInfo[] backupFiles = BackupFileInfo.GetBackupFileInfoArray(connection, dbInfo);
			ShowMessage("Finished getting list of database files in the backup.\r\n");

			// Builds before scripts before dropping the secondary database, so it can get the existing DB file locations.
			return GetRestoreStandBySqlScript(dbInfo, backupFiles);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		string GetRestoreStandBySqlScript(DatabaseInfo dbInfo, BackupFileInfo[] backupFiles)
		{
			Argument.NotNull(dbInfo, nameof(dbInfo));
			Argument.NotNull(backupFiles, nameof(backupFiles));
			Argument.NotNullOrEmpty(dbInfo.SecondaryDatabaseName, nameof(dbInfo.SecondaryDatabaseName));
			Argument.NotNullOrEmpty(dbInfo.BackupFullFileName, nameof(dbInfo.BackupFullFileName));

			StringBuilder sqlTextBuilder = new StringBuilder();
			sqlTextBuilder.AppendFormat("RESTORE DATABASE [{0}] FROM DISK= '{1}' WITH REPLACE, ", dbInfo.SecondaryDatabaseName, dbInfo.BackupFullFileName);
			sqlTextBuilder.AppendLine();

			foreach (BackupFileInfo fileInfo in backupFiles)
			{
				sqlTextBuilder.AppendFormat("MOVE '{0}' TO '{1}', ", fileInfo.LogicalName, fileInfo.PhysicalName);
				sqlTextBuilder.AppendLine();
			}

			sqlTextBuilder.AppendFormat(@"STANDBY = '{0}\{1}_{2}.tuf'",
				dbInfo.SetupInfo.BackupLocalCopyDirectory,
				dbInfo.SecondaryDatabaseName,
				DateTime.UtcNow.ToString("yyyyMMddHHmmss"));

			sqlTextBuilder.AppendLine();
			sqlTextBuilder.AppendLine();

			return sqlTextBuilder.ToString();
		}

		#region ShowMessage

		void ShowMessage(string message)
		{
			if (OnShowMessage != null)
			{
				OnShowMessage(message);
			}
		}

		public event NotificationDelegate OnShowMessage;

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public const int TimeoutInSeconds = 18000;
	}
}
