using System;
using System.Reflection;
using System.Threading;
using Enterprise.Upgrades;
using Microsoft.Extensions.Logging;
using WTG.ApplicationLogging.Abstractions;
using WTG.ApplicationLogging.Builder;

[assembly: AssemblyTitle("CargoWise One Native Image Installer")]

namespace CargoWise.NGenInstallerProgram
{
	class Program
	{
		public static string ProgName { get; } = typeof(Program).Assembly.GetName().Name;

		static void Main(string[] args)
		{
			Environment.ExitCode = -1;

			using var loggerFactory = ApplicationLoggingBuilder.Build(o => o.Configure(Product.CargoWise));

			var logger = loggerFactory.CreateLogger("NGenInstaller");

			Run(logger, args);

			static void Run(ILogger logger, string[] args)
			{
				try
				{
					logger.LogInformation($"Starting {ProgName}.exe");

					var parameters = CommandLineParameters.Parse(args);
					InstallOrUninstall(logger, parameters);

					logger.LogInformation($"Finished {ProgName}.exe");

					Environment.ExitCode = 0;
				}
				catch (Exception ex)
				{
					logger.LogError(ex, message: ex.Message);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1078:Do not use Process.Start to open a file or url, use WebUrlLauncher or FileOpener for proper integration with Remote Desktop Services. False alarm if you are running a process for a reason other than to open a file or url.", Justification = "Uses ProcessStartInfo")]
		static void InstallOrUninstall(ILogger logger, CommandLineParameters parameters)
		{
			logger.LogInformation("Performing {action} task", parameters.Action);

			if (parameters.DelayTimeForTest > TimeSpan.Zero)
			{
				Thread.Sleep(parameters.DelayTimeForTest);
			}

			logger.LogInformation("Using 64 bit version");

			const string mutexName = "Global\\CargoWiseOneNGen";
			using (var mutex = new UpgraderMutex(mutexName))
			{
				logger.LogInformation("Waiting for mutex {mutexName}...", mutexName);
				if (mutex.WaitOne())
				{
					logger.LogInformation("Mutex {mutexName} obtained", mutexName);

					var nGenCaller = new NGenCaller(logger, parameters.GetNGenExecutablePath(false), parameters.ExecutionTimeout);

					nGenCaller.QueuePause();
					try
					{
						nGenCaller.InstallOrUninstall(
							parameters.Action,
							parameters.RootFilePath);
					}
					finally
					{
						nGenCaller.QueueContinue();
					}
				}
			}
		}
	}
}

