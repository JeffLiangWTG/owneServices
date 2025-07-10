using System;
using System.Threading;
using NLog;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Logging.Abstractions;

namespace ServiceManager.Logging.CW
{
	public class LoggerFactory : ILoggerFactory, ILoggerFinalizer
	{
		public Enterprise.Integration.ILogger NewServiceTaskLogger(string dbServer, string dbName, string programCode, string logDirectoryPath)
		{
			return new LoggerNLogWrapper(dbServer, dbName, programCode, logDirectoryPath).ToCW1Logger();
		}

		public void ShutDownLog()
		{
			LogManager.Shutdown();
		}

		static void OnProcessExit(object? sender, EventArgs e)
		{
			AppDomain.CurrentDomain.ProcessExit -= OnProcessExit;
			LogManager.Shutdown();
		}

		public Enterprise.Integration.ILogger NewScheduledUpgradeLogger(string dbServer, string dbName)
		{
			_ = lazyProcessExitHandlerRegistered.Value;
			return new LoggerNLogWrapper(dbServer, dbName, DbUpgraderServiceTaskCode, trackServiceTaskErrors: false).ToCW1Logger();
		}

		static Lazy<bool> lazyProcessExitHandlerRegistered { get; } =
			new Lazy<bool>(() =>
				{
					AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
					return true;
				},
				LazyThreadSafetyMode.ExecutionAndPublication);

		const string DbUpgraderServiceTaskCode = "UPG";
	}
}
