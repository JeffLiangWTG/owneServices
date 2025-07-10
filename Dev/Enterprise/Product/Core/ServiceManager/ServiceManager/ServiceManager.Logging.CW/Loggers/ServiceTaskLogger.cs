using System;
using CargoWise.Common;
using CargoWise.Data;
using Microsoft.Extensions.Logging;
using ServiceManager.Logging.Abstractions;
using ServiceManager.Runner.Abstractions;
using ILoggerFactory = ServiceManager.Integration.ServiceTasks.CW.ILoggerFactory;

namespace ServiceManager.Logging.CW
{
	class ServiceTaskLogger : IServiceTaskLogger
	{
		public ServiceTaskLogger(IRunnerLogger runnerLogger, ILoggerFactory loggerFactory)
		{
			this.runnerLogger = runnerLogger ?? throw new ArgumentNullException(nameof(this.runnerLogger));
			this.loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
		}

		public IDisposable SetTaskLoggerCode(string code)
		{
			if (TaskLoggerIsActive)
			{
				throw new InvalidOperationException("Attempt to start new service task logger without disposing previous.");
			}

			serviceTaskLogger = loggerFactory.NewServiceTaskLogger(Db.ServerName, Db.DatabaseName, code);
			return new DisposableAction(() => serviceTaskLogger = null);
		}

		public bool TaskLoggerIsActive => serviceTaskLogger != null;

		public void Log(LogLevel type, string message)
		{
			var logger = serviceTaskLogger;
			if (logger != null)
			{
				logger.Log(type.ToLogType(), message);
			}
			else
			{
				runnerLogger.Log(type, message);
				runnerLogger.Log(LogLevel.Error, "Attempt to log to the service task logger without initialization.");
			}
		}

		public void Log(LogLevel type, string message, Exception? ex)
		{
			var logger = serviceTaskLogger;
			if (logger != null)
			{
				logger.Log(type.ToLogType(), message, ex);
			}
			else
			{
				runnerLogger.Log(type, message, ex);
				runnerLogger.Log(LogLevel.Error, "Attempt to log to the service task logger without initialization.");
			}
		}

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
		{
			Log(logLevel, formatter(state, exception), exception);
		}

		public bool IsEnabled(LogLevel logLevel)
		{
			return true;
		}

		public IDisposable? BeginScope<TState>(TState state) where TState : notnull
		{
			return LoggerNullScope.Instance;
		}

		readonly IRunnerLogger runnerLogger;
		readonly ILoggerFactory loggerFactory;
		Enterprise.Integration.ILogger? serviceTaskLogger;
	}
}
