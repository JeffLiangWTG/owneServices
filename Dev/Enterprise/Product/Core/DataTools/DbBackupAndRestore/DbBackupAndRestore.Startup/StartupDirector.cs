using System;
using System.CommandLine.Parsing;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using CargoWise.Data;
using Enterprise.DataTools.DbBackupAndRestore.Business;
using Enterprise.DataTools.DbBackupAndRestore.Commands;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DataTools.DbBackupAndRestore
{
	public static class StartupDirector
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		internal static int Main(string[] args)
		{
			Db.DisableSchemaVersionCheckPermanently();
			HookUnhandledExceptions();
			Logger.Instance.LogMessage("Starting Instance with Arguments: \"" + string.Join("\", \"", args).Replace("\" ", "\\\", \"").Replace("\r\n", "") + "\"");

			EnterpriseApplicationConfiguration.ConfigureObjectFactory();

			var commandLineBuilder = new RootCommandBuilder().Create(args);
			var parser = commandLineBuilder.Build();
			return parser.InvokeAsync(args).Result;
		}

		static void HookUnhandledExceptions()
		{
			AppDomain.CurrentDomain.UnhandledException += (s, e) => LogException(s, e.ExceptionObject);
			TaskScheduler.UnobservedTaskException += (s, e) => LogException(s, e.Exception);

			static void LogException(object sender, object exception)
			{
				try
				{
					Logger.Instance.LogErrorMessage($"Unhandled Exception is detected. \r\n {exception}");
				}
				catch
				{
					// ignore any exception
				}
			}
		}
	}
}
