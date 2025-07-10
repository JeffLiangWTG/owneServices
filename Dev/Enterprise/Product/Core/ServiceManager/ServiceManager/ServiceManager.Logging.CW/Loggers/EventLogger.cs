using System;
using Enterprise.ZArchitecture.Core;
using Microsoft.Extensions.Logging;
using NLog;
using ServiceManager.Host.Abstractions;
using ServiceManager.Logging.Abstractions;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace ServiceManager.Logging.CW
{
	class EventLogger : IEventLogger
	{
		public EventLogger(IServiceManagerHostOptions hostOptions)
		{
			this.dbServer = hostOptions.ServerName;
			this.dbName = hostOptions.DatabaseName;
			applicationPath = AppDomain.CurrentDomain.BaseDirectory;
			processControllerVersion = ReleaseInfo.Instance.VersionNumber.ToString();

			Initialize();

			logger = LogManager.GetLogger(NLogEventLogTargetFactory.TargetName);
		}

		static void Initialize()
		{
			LoggerHelper.ConfigureTargetAndRule(new NLogEventLogTargetFactory(), NLogEventLogTargetFactory.TargetName, NLog.LogLevel.Info);

			LogManager.ReconfigExistingLoggers();
		}

		public void Log(LogLevel logLevel, string message)
		{
			Log(logLevel, message, null);
		}

		public void Log(LogLevel logLevel, string message, Exception? ex)
		{
			var logEventInfo = new LogEventInfo
			{
				Level = LoggerHelper.ConvertLogLevel(logLevel),
				Message = message,
				Exception = ex,
				Properties =
				{
					["database"] = dbName,
					["database_host"] = dbServer,
					["applicationPath"] = applicationPath,
					["processcontroller"] = new
					{
						version = processControllerVersion,
					},
				},
			};
			logger.Log(logEventInfo);
		}

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
		{
			Log(logLevel, formatter(state, exception), exception);
		}

		public bool IsEnabled(LogLevel logLevel)
		{
			return LoggerHelper.IsEnabled(logger, logLevel);
		}

		public IDisposable? BeginScope<TState>(TState state) where TState : notnull
		{
			return LoggerNullScope.Instance;
		}

		readonly string dbServer;
		readonly string dbName;
		readonly NLog.ILogger logger;
		readonly string processControllerVersion;
		readonly string applicationPath;
	}
}
