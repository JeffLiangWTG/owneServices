#if NET48
using System;
using System.Diagnostics;
using System.Threading;
using CargoWise.Data;
using CargoWise.Definitions;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.Main.Startup.Tasks
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer")]
	[WTG.StaticAnalysis.Annotation.CodeAlive("SQL Over Http Connection")]
	class StartupSqlProxyServerTask : AbstractApplicationStartupTask
	{
		protected override bool GetShouldExecute(CommandLineArguments arguments) => IsGlowLoaderServiceEnabled;

		protected override bool DoExecute(CommandLineArguments arguments)
		{
			try
			{
				var startupTimespan = TimeSpan.FromMinutes(3);
				using var cancellationTokenSource = new CancellationTokenSource(startupTimespan);
				_ = SqlProxyClientProvider.StartSqlProxyServerProcess(Db.ServerName, Db.DatabaseName, cancellationTokenSource.Token);
			}
			catch (Exception ex)
			{
				SafeEventLogExtensions.SafeWriteEntryToApplicationLog($"{nameof(StartupSqlProxyServerTask)} error: {ex}", EventLogEntryType.Error);
			}

			return true;
		}

		public override string TaskDescription => (NoResString)"Start sql proxy server process";

		public override int FailureExitCode => ExitCodes.Success;

		public static bool IsGlowLoaderServiceEnabled => false; // use Windows Registry to enable
	}
}
#endif
