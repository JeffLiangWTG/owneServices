using System;
using System.CommandLine;
using System.Linq;
using Enterprise.DataTools.DbBackupAndRestore.Business;
using Enterprise.DataTools.DbBackupAndRestore.GUI;

namespace Enterprise.DataTools.DbBackupAndRestore.Commands;

public class RestoreCommandBuilder
{
	public Command Create(RestoreDbConsole restorer)
	{
		var backupPathOption = new Option<string>(
			"--BackupPath",
			"Backup path (full path of the main database .bak/.dbk file)") { IsRequired = true };

		var dataFilePathOption = new Option<string>(
			"--DataFilePath",
			"Data File Path");

		var logFilePathOption = new Option<string>(
			"--LogFilePath",
			"Log File Path");

		var restoreTypeOption = new Option<string>(
			"--RestoreType",
			"Restore operation to perform") { IsRequired = true };

		var allowedRestoreTypeValues = new[] { "RestoreWithRecovery", "RestoreWithNoRecovery", "CopyProdToTest" };
		restoreTypeOption.FromAmong(allowedRestoreTypeValues);

		var restoreDifferentialBackupOption = new Option<bool>(
			"--RestoreDifferentialBackup",
			"Restore Differential Backup (true|false)");

		var restoreTransactionLogBackupOption = new Option<bool>(
			"--RestoreTransactionLogBackup",
			"Restore Transaction Log Backup (true|false)");

		var excludeAuditDbOption = new Option<bool>(
			"--ExcludeAuditDb",
			"Restore database without Audit DB (true|false)");

		var auditServerOption = new Option<string>(
			"--AuditServer",
			"Destination SQL instance for audit database");

		var auditBackupOption = new Option<string>(
			"--AuditBackup",
			"Audit backup path (full path of the audit database .bak/.dbk file)");

		var auditDataFilePathOption = new Option<string>(
			"--AuditDataFilePath",
			"Audit Data File Path");

		var auditLogFilePathOption = new Option<string>(
			"--AuditLogFilePath",
			"Audit Log File Path");

		var edwServerOption = new Option<string>(
			"--EDWServer",
			"Destination SQL instance for EDW database");

		var edwBackupOption = new Option<string>(
			"--EDWBackup",
			"EDW backup path (full path of the EDW database .bak/.dbk file)");

		var edwDataFilePathOption = new Option<string>(
			"--EDWDataFilePath",
			"EDW Data File Path");

		var edwLogFilePathOption = new Option<string>(
			"--EDWLogFilePath",
			"EDW Log File Path");

		var restoreCommand = new Command("restore", "Restore database");
		restoreCommand.AddOption(SharedCommandOptions.ServerOption);
		restoreCommand.AddOption(SharedCommandOptions.DatabaseOption);
		restoreCommand.AddOption(backupPathOption);
		restoreCommand.AddOption(dataFilePathOption);
		restoreCommand.AddOption(logFilePathOption);
		restoreCommand.AddOption(restoreTypeOption);
		restoreCommand.AddOption(restoreDifferentialBackupOption);
		restoreCommand.AddOption(restoreTransactionLogBackupOption);
		restoreCommand.AddOption(excludeAuditDbOption);
		restoreCommand.AddOption(auditServerOption);
		restoreCommand.AddOption(auditBackupOption);
		restoreCommand.AddOption(auditDataFilePathOption);
		restoreCommand.AddOption(auditLogFilePathOption);
		restoreCommand.AddOption(edwServerOption);
		restoreCommand.AddOption(edwBackupOption);
		restoreCommand.AddOption(edwDataFilePathOption);
		restoreCommand.AddOption(edwLogFilePathOption);

		restoreCommand.SetHandler(context =>
		{
			var result = context.ParseResult;
			var dbServer = result.GetValueForOption(SharedCommandOptions.ServerOption);
			var dbName = result.GetValueForOption(SharedCommandOptions.DatabaseOption);
			Defaults.Instance.ServerName = dbServer;
			Defaults.Instance.DatabaseName = dbName;
			Defaults.Instance.BackupFileName = result.GetValueForOption(backupPathOption);
			Defaults.Instance.DataFilePath = result.GetValueForOption(dataFilePathOption);
			Defaults.Instance.LogFilePath = result.GetValueForOption(logFilePathOption);

			var restoreType = result.GetValueForOption(restoreTypeOption);
			Defaults.Instance.RestoreOption = Enum.TryParse<DbRestoreOption>(restoreType, ignoreCase: true, out var parsedRestoreOption)
				? parsedRestoreOption
				: throw new ArgumentException($"Invalid restore type: {restoreType}");

			Defaults.Instance.RestoreDifferentialBackups = result.GetValueForOption(restoreDifferentialBackupOption);
			Defaults.Instance.RestoreTransactionLogBackups = result.GetValueForOption(restoreTransactionLogBackupOption);
			Defaults.Instance.IsAuditDBExcludedFromRestore = result.GetValueForOption(excludeAuditDbOption);
			Defaults.Instance.AuditServerName = result.GetValueForOption(auditServerOption);
			Defaults.Instance.AuditBackupFileName = result.GetValueForOption(auditBackupOption);
			Defaults.Instance.AuditDataFilePath = result.GetValueForOption(auditDataFilePathOption);
			Defaults.Instance.AuditLogFilePath = result.GetValueForOption(auditLogFilePathOption);
			Defaults.Instance.DataWarehouseServerName = result.GetValueForOption(edwServerOption);
			Defaults.Instance.EdwBackupFileName = result.GetValueForOption(edwBackupOption);
			Defaults.Instance.EdwDataFilePath = result.GetValueForOption(edwDataFilePathOption);
			Defaults.Instance.EdwLogFilePath = result.GetValueForOption(edwLogFilePathOption);

			string availabilityGroup = null;

			var restoreManager = restorer.restoreManager;

			if (restoreManager.IsDbPartOfAlwaysOn(dbServer, dbName))
			{
				availabilityGroup = restoreManager.GetAvailabilityGroupName(dbServer, dbName);
			}
			else
			{
				var availabilityGroups = restoreManager.GetAvailabilityGroupsList(dbServer)
					.Where(groupName => restoreManager.IsPrimaryReplicaOnAvailabilityGroup(dbServer, groupName))
					.ToList();
				if (availabilityGroups.Count != 0)
				{
					availabilityGroup = availabilityGroups[0];
				}
			}

			if (!string.IsNullOrEmpty(availabilityGroup))
			{
				restoreManager.LogMessage($"Using Availability Group: {availabilityGroup}");
			}

			Defaults.Instance.AddDbToAvailabilityGroup = !string.IsNullOrEmpty(availabilityGroup);
			Defaults.Instance.AvailabilityGroup = availabilityGroup;
			if (ValidateAvailabilityGroup(restoreManager) && ValidateTargetDatabaseStatus(restoreManager))
			{
				restoreManager.LogMessage("Running DB Restore");
				restorer.Start();
				context.ExitCode = restorer.ExitCode;
			}
			else
			{
				context.ExitCode = -1;
			}
		});

		return restoreCommand;
	}

	bool ValidateAvailabilityGroup(IDbRestoreManager restoreManager)
	{
		if (Defaults.Instance.AddDbToAvailabilityGroup)
		{
			if (!string.IsNullOrWhiteSpace(Defaults.Instance.AuditServerName))
			{
				if (!restoreManager.IsPrimaryReplicaOnAvailabilityGroup(Defaults.Instance.AuditServerName, Defaults.Instance.AvailabilityGroup))
				{
					restoreManager.LogMessage(
						$"Audit Server {Defaults.Instance.AuditServerName} must be a primary replica on {Defaults.Instance.AvailabilityGroup} to add database to availability group");
					return false;
				}
			}

			if (!string.IsNullOrWhiteSpace(Defaults.Instance.DataWarehouseServerName))
			{
				if (!restoreManager.IsPrimaryReplicaOnAvailabilityGroup(Defaults.Instance.DataWarehouseServerName, Defaults.Instance.AvailabilityGroup))
				{
					restoreManager.LogMessage(
						$"Data Warehouse Server {Defaults.Instance.DataWarehouseServerName} must be a primary replica on {Defaults.Instance.AvailabilityGroup} to add database to availability group");
					return false;
				}
			}

			if (Defaults.Instance.RestoreOption == DbRestoreOption.RestoreWithNoRecovery)
			{
				restoreManager.LogMessage(
					"Restore WITH NORECOVERY is not supported for when adding database to primary replicas");
				return false;
			}
		}
		return true;
	}

	bool ValidateTargetDatabaseStatus(IDbRestoreManager restoreManager)
	{
		var groupName = restoreManager.GetAvailabilityGroupName(Defaults.Instance.ServerName, Defaults.Instance.DatabaseName);
		if (restoreManager.IsDbPartOfAlwaysOn(Defaults.Instance.ServerName, Defaults.Instance.DatabaseName) && !restoreManager.IsPrimaryReplicaOnAvailabilityGroup(Defaults.Instance.ServerName, groupName))
		{
			restoreManager.LogMessage(
				$"The database '{Defaults.Instance.DatabaseName}' is part of an Always On availability group. " +
				"Please ensure that the destination server is currently hosting the primary replica before proceeding."
			);
			return false;
		}

		var dbStatus = restoreManager.GetDatabaseStatus(Defaults.Instance.ServerName, Defaults.Instance.DatabaseName);
		if (!IsDatabaseInRestorableState(dbStatus))
		{
			restoreManager.LogMessage($"The database '{Defaults.Instance.DatabaseName}' is not in a state that allows for restoration: {dbStatus}. Please check its status.");
			return false;
		}
		return true;
	}

	bool IsDatabaseInRestorableState(DatabaseStatus dbStatus) => dbStatus == DatabaseStatus.Online ||
																 dbStatus == DatabaseStatus.Standby ||
																 dbStatus == DatabaseStatus.EmergencyMode ||
																 dbStatus == DatabaseStatus.Restoring ||
																 dbStatus == DatabaseStatus.DoesNotExist;
}
