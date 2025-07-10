using System;
using System.Collections.Generic;
using CargoWise.Data;
using Enterprise.ServiceManager.Shared;
using Microsoft.Extensions.Logging;
using ServiceManager.Runner.Abstractions;

namespace ServiceManager.Logging.CW
{
	class RunnerLogger : IRunnerLogger
	{
		public RunnerLogger()
			: this(new Lazy<ILogger>(() => new LoggerNLogWrapper(Db.ServerName, Db.DatabaseName, ServiceManagerHelper.HostLoggerCode, archiveLogFiles: false)))
		{
		}

		internal RunnerLogger(Lazy<ILogger> logger)
		{
			loggerLazy = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		void LogWithProperties(Action logAction)
		{
			using (Logger.BeginScope(new[] { new KeyValuePair<string, object>("exe", "Runner") }))
			{
				logAction();
			}
		}

		public void Log(LogLevel logLevel, string message, ICommandInfo commandInfo)
		{
			_ = commandInfo ?? throw new ArgumentNullException(nameof(commandInfo));
			LogWithProperties(() => Logger.Log(logLevel, $"{commandInfo} {message}"));
		}

		public void Log(LogLevel logLevel, string message)
		{
			LogWithProperties(() => Logger.Log(logLevel, message));
		}

		public void Log(LogLevel logLevel, string message, Exception? ex)
		{
			LogWithProperties(() => Logger.Log(logLevel, ex, message));
		}

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
		{
			Log(logLevel, formatter(state, exception), exception);
		}

		public bool IsEnabled(LogLevel logLevel)
		{
			return Logger.IsEnabled(logLevel);
		}

		public IDisposable? BeginScope<TState>(TState state) where TState : notnull
		{
			return Logger.BeginScope(state);
		}

		readonly Lazy<ILogger> loggerLazy;
		ILogger Logger => loggerLazy.Value;
	}
}
