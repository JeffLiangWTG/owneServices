using System;
using System.IO;
using System.Linq;
using CargoWise.Common;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace Enterprise.ServiceManager.Shared.Testing.Logging
{
	public static class LoggerTestHelper
	{
		public static IDisposable CleanupCurrentLogFolderThenStopLoggerAndCleanupLeftoversOnDispose(string directoryPath)
		{
			if (Directory.Exists(directoryPath))
			{
				Directory.Delete(directoryPath, true);
			}

			return new DisposableAction(() =>
			{
				LogManager.Shutdown();
				Directory.Delete(directoryPath, true);
			});
		}

		public static IDisposable TemporaryLoggingConfigurationWithTarget(Target target)
		{
			var currentConfiguration = LogManager.Configuration;

			return new DisposableAction(() =>
			{
				var config = new LoggingConfiguration();
				config.AddTarget(target);
				config.LoggingRules.Add(new LoggingRule("*", NLog.LogLevel.Debug, target) { RuleName = nameof(target) });

				LogManager.Configuration = config;
				LogManager.Configuration.Reload();
			},
			() =>
			{
				LogManager.Configuration = currentConfiguration;
				LogManager.Configuration?.Reload();
			});
		}

		public static IDisposable TemporaryLoggingConfiguration()
		{
			var currentConfiguration = LogManager.Configuration;
			return new DisposableAction(() =>
				{
					LogManager.Configuration = new LoggingConfiguration();
					LogManager.Configuration.Reload();
				},
				() =>
				{
					LogManager.Configuration = currentConfiguration;
					LogManager.Configuration?.Reload();
				});
		}

		public static IDisposable RedirectConsoleLogging(TextWriter writer)
		{
			var current = Console.Out;
			return new DisposableAction(() =>
				{
					Console.SetOut(writer);
				},
				() =>
				{
					Console.SetOut(current);
				});
		}

		public static void LogDummyDataToTriggerLogArchive(Microsoft.Extensions.Logging.ILogger logger)
		{
			var fileTargets = LogManager.Configuration.AllTargets
				.OfType<FileTarget>()
				.Where(target => target.ArchiveAboveSize > 0);
			foreach (var fileTarget in fileTargets)
			{
				fileTarget.ArchiveAboveSize = 10000;
			}

			const string logMessage = "This is a test to see if archival of log file is triggered.";
			const int length = 1024;
			for (var i = 0; i < length; i++)
			{
				logger.Log(Microsoft.Extensions.Logging.LogLevel.Information, logMessage);
			}
		}
	}
}
