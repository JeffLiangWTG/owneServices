using System.CommandLine;
using Enterprise.DataTools.DbBackupAndRestore.Business;
using Enterprise.DataTools.DbBackupAndRestore.GUI;

namespace Enterprise.DataTools.DbBackupAndRestore.Commands;

public class DropCommandBuilder
{
	public Command Create(MaintenanceDbConsole maintenanceConsole)
	{
		var dropOperationalDatabasesOption = new Option<bool>(
			"--DropOperationalDatabases",
			"Drop operational databases (true|false)");

		var dropReferenceDatabasesOption = new Option<bool>(
			"--DropReferenceDatabases",
			"Drop reference databases (true|false)");

		var dropBiDatabasesOption = new Option<bool>(
			"--DropBIDatabases",
			"Drop BI databases (true|false)");

		var dropCommand = new Command("drop", "Drop database");
		dropCommand.AddOption(SharedCommandOptions.ServerOption);
		dropCommand.AddOption(SharedCommandOptions.DatabaseOption);
		dropCommand.AddOption(dropOperationalDatabasesOption);
		dropCommand.AddOption(dropReferenceDatabasesOption);
		dropCommand.AddOption(dropBiDatabasesOption);

		dropCommand.SetHandler(context =>
		{
			var result = context.ParseResult;

			Defaults.Instance.ServerName = result.GetValueForOption(SharedCommandOptions.ServerOption);
			Defaults.Instance.DatabaseName = result.GetValueForOption(SharedCommandOptions.DatabaseOption);
			Defaults.Instance.IncludeOperationalDbs = result.GetValueForOption(dropOperationalDatabasesOption);
			Defaults.Instance.IncludeReferenceDbs = result.GetValueForOption(dropReferenceDatabasesOption);
			Defaults.Instance.IncludeBiDbs = result.GetValueForOption(dropBiDatabasesOption);

			maintenanceConsole.Start();
			context.ExitCode = maintenanceConsole.ExitCode;
		});

		return dropCommand;
	}
}
