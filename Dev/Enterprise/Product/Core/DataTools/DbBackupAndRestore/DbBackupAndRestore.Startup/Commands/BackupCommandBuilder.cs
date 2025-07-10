using System.CommandLine;
using Enterprise.DataTools.DbBackupAndRestore.Business;
using Enterprise.DataTools.DbBackupAndRestore.GUI;

namespace Enterprise.DataTools.DbBackupAndRestore.Commands;

public class BackupCommandBuilder
{
	public Command Create(BackupDbConsole backupConsole)
	{
		var backupFolderOption = new Option<string>(
			"--BackupFolder",
			"Backup folder for main database") { IsRequired = true };

		var auditBackupFolderOption = new Option<string>(
			"--AuditBackupFolder",
			"Backup folder for audit database");

		var edwBackupFolderOption = new Option<string>(
			"--EDWBackupFolder",
			"Backup folder for EDW database");

		var backupOperationalDatabasesOption = new Option<bool>(
			"--BackupOperationalDatabases",
			"Backup operational databases (true|false)");

		var backupReferenceDatabasesOption = new Option<bool>(
			"--BackupReferenceDatabases",
			"Backup reference databases (true|false)");

		var backupBiDatabasesOption = new Option<bool>(
			"--BackupBiDatabases",
			"Backup BI databases (true|false)");

		var backupCommand = new Command("backup", "Backup database");
		backupCommand.AddOption(SharedCommandOptions.ServerOption);
		backupCommand.AddOption(SharedCommandOptions.DatabaseOption);
		backupCommand.AddOption(backupFolderOption);
		backupCommand.AddOption(auditBackupFolderOption);
		backupCommand.AddOption(edwBackupFolderOption);
		backupCommand.AddOption(backupOperationalDatabasesOption);
		backupCommand.AddOption(backupReferenceDatabasesOption);
		backupCommand.AddOption(backupBiDatabasesOption);

		backupCommand.SetHandler(context =>
		{
			var result = context.ParseResult;

			Defaults.Instance.ServerName = result.GetValueForOption(SharedCommandOptions.ServerOption);
			Defaults.Instance.DatabaseName = result.GetValueForOption(SharedCommandOptions.DatabaseOption);
			Defaults.Instance.BackupFolder = result.GetValueForOption(backupFolderOption);
			Defaults.Instance.AuditBackupFolder = result.GetValueForOption(auditBackupFolderOption);
			Defaults.Instance.EdwBackupFolder = result.GetValueForOption(edwBackupFolderOption);
			Defaults.Instance.IncludeOperationalDbs = result.GetValueForOption(backupOperationalDatabasesOption);
			Defaults.Instance.IncludeReferenceDbs = result.GetValueForOption(backupReferenceDatabasesOption);
			Defaults.Instance.IncludeBiDbs = result.GetValueForOption(backupBiDatabasesOption);

			backupConsole.Start();
			context.ExitCode = backupConsole.ExitCode;
		});
		return backupCommand;
	}
}
