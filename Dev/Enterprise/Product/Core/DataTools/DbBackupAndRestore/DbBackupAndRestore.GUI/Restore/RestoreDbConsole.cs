using System;
using System.Diagnostics.CodeAnalysis;
using Enterprise.DataTools.DbBackupAndRestore.Business;

namespace Enterprise.DataTools.DbBackupAndRestore.GUI
{
	public class RestoreDbConsole : DbConsole
	{
		public RestoreDbConsole(IDbRestoreManager dbRestoreManager = null)
		{
			Initialise(dbRestoreManager);
		}

		void Initialise(IDbRestoreManager dbRestoreManager)
		{
			if (dbRestoreManager != null)
			{
				restoreManager = dbRestoreManager;
			}
			else
			{
				restoreManager = DbRestoreManagerFactory.Create(
					DbTools_OnTaskStarted,
					DbTools_OnSubtaskStarted,
					DbTools_OnTaskCompleted,
					DbTools_OnTaskFailed,
					DbTools_OnShowInfoMessage,
					DbTools_OnConfirmationPrompt
				);
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Top level handling. Catches exception and displays an error to user instead of letting it become an unhandled exception.")]
		protected override void OnDatabaseTaskStart()
		{
			if (string.IsNullOrEmpty(Defaults.Instance.BackupFileName))
			{
				throw new InvalidOperationException("There is no backup file.\r\nPlease use correct parameters or use GUI version instead.");
			}

			restoreManager.RefreshExtendedProperties(Defaults.Instance.ServerName);
			restoreDbFiles = restoreManager.GetBackupDbFileInfoCollection(
				Defaults.Instance.ServerName, Defaults.Instance.BackupFileName,
				Defaults.Instance.AuditServerName, Defaults.Instance.AuditBackupFileName,
				Defaults.Instance.DataWarehouseServerName, Defaults.Instance.EdwBackupFileName);
			restoreDbFiles = restoreManager.ApplyExtendedProperties(restoreDbFiles, out _);

			var mainServerConfiguration = new DbServerConfiguration(
				Defaults.Instance.ServerName,
				Defaults.Instance.BackupFileName,
				Defaults.Instance.DataFilePath,
				Defaults.Instance.LogFilePath
			);
			var auditServerConfiguration = new AuditDbServerConfiguration(
				Defaults.Instance.AuditServerName,
				Defaults.Instance.AuditBackupFileName,
				Defaults.Instance.AuditDataFilePath,
				Defaults.Instance.AuditLogFilePath
			);
			var edwServerConfiguration = new EdwDbServerConfiguration(
				Defaults.Instance.DataWarehouseServerName,
				Defaults.Instance.EdwBackupFileName,
				Defaults.Instance.EdwDataFilePath,
				Defaults.Instance.EdwLogFilePath
			);
			var dbRestoreSettings = new DbRestoreSettings(
				Defaults.Instance.DatabaseName,
				restoreDbFiles,
				Defaults.Instance.RestoreOption,
				Defaults.Instance.RestoreOperationalDatabases,
				Defaults.Instance.RestoreDifferentialBackups,
				Defaults.Instance.RestoreTransactionLogBackups,
				Defaults.Instance.AddDbToAvailabilityGroup,
				Defaults.Instance.AvailabilityGroup
			);

			restoreManager.RestoreDatabases(mainServerConfiguration, auditServerConfiguration, edwServerConfiguration, dbRestoreSettings);
		}

		public override void DbTools_OnConfirmationPrompt(ConfirmationPromptArgs promptArgs)
		{
			switch (promptArgs.PromptType)
			{
				case PromptType.ConfirmAuditDBExcluded:
					if (Defaults.Instance.IsAuditDBExcludedFromRestore)
					{
						promptArgs.Result = ConfirmationPromptResult.Yes;
					}
					else
					{
						promptArgs.Result = ConfirmationPromptResult.No;
						PrintOutput("The Audit DB was not included in the restore. You must specify -excludeauditdb if you do not intend to restore the Audit DB");
					}
					break;
				default:
					PrintOutput(promptArgs.PromptTitle);
					PrintOutput(promptArgs.PromptMessage);
					PrintOutput("Y/N?");
					PrintOutput($"User confirmation prompt skipped in CLI mode with default response. value='{promptArgs.Result.ToString()}'");
					break;
			}
		}

		public IDbRestoreManager restoreManager;
		DbFileInfoCollection restoreDbFiles;
	}
}
