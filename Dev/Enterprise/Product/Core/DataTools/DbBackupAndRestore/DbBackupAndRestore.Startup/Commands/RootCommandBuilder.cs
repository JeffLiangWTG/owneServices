using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Enterprise.DataTools.DbBackupAndRestore.Business;
using Enterprise.DataTools.DbBackupAndRestore.GUI;

namespace Enterprise.DataTools.DbBackupAndRestore.Commands;

public class RootCommandBuilder
{
	public virtual CommandLineBuilder Create(string[] args)
	{
		var rootCommand = new RootCommand("Database Backup and Restore");

		var restoreCommand = new RestoreCommandBuilder().Create(new RestoreDbConsole());
		var backupCommand = new BackupCommandBuilder().Create(new BackupDbConsole());
		var dropCommand = new DropCommandBuilder().Create(new MaintenanceDbConsole());

		rootCommand.AddCommand(restoreCommand);
		rootCommand.AddCommand(backupCommand);
		rootCommand.AddCommand(dropCommand);

		var commandLineBuilder = new CommandLineBuilder(rootCommand);

		commandLineBuilder.AddMiddleware(async (context, next) =>
		{
			if (context.ParseResult.CommandResult.Command.Name != restoreCommand.Name &&
				context.ParseResult.CommandResult.Command.Name != backupCommand.Name &&
				context.ParseResult.CommandResult.Command.Name != dropCommand.Name)
			{
				context.ExitCode = ParseLegacyCLI(args);
			}
			else
			{
				await next(context);
			}
		});

		commandLineBuilder.UseDefaults();
		return commandLineBuilder;
	}

	public virtual int ParseLegacyCLI(string[] args)
	{
		var exitCode = 0;
		var cmd = new CommandLineSet();
		if (cmd.Parse(args))
		{
			if (Defaults.Instance.DisplayHelpText)
			{
				cmd.DisplayHelpText();
			}
			else if (Defaults.Instance.RunSalesDbRestore)
			{
				NativeMethods.HideConsoleWindow();
				Application.Run(new SalesRestoreDbForm(Defaults.Instance.SalesDbRestore));
			}
			else if (Defaults.Instance.RunDbRestore)
			{
				Logger.Instance.LogMessage("Running DB Restore");
				var restorer = new RestoreDbConsole();
				restorer.Start();
				exitCode = restorer.ExitCode;
			}
			else if (!args.Any(a => string.Equals(a, "-h", StringComparison.OrdinalIgnoreCase) || string.Equals(a, "-help", StringComparison.OrdinalIgnoreCase)))
			{
				NativeMethods.HideConsoleWindow();
				var mainForm = new MainForm();
				if (!string.IsNullOrEmpty(Defaults.Instance.ServerName))
				{
					mainForm.ShowRestoreTabPage();
				}

				Application.Run(mainForm);
			}
		}
		else
		{
			Logger.Instance.LogErrorMessage("Invalid argument. See -help for options.");
		}

		return exitCode;
	}

	static class NativeMethods
	{
		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

		public static void HideConsoleWindow()
		{
			IntPtr hWnd = Process.GetCurrentProcess().MainWindowHandle;

			if (hWnd != IntPtr.Zero)
			{
				ShowWindow(hWnd, 0);
			}
		}
	}
}
