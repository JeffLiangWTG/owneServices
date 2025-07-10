using System;
using System.Collections.Generic;
using Enterprise.Integration;
using Microsoft.Extensions.Logging;
using ILogger = Enterprise.Integration.ILogger;
using MicrosoftILogger = Microsoft.Extensions.Logging.ILogger;

namespace Enterprise.ZArchitecture.Core
{
	public class TokenValidationLogger : MicrosoftILogger
	{
		readonly ILogger wtgLogger;
		readonly Dictionary<LogLevel, LogType> logLevelMapping = new Dictionary<LogLevel, LogType>();

		public TokenValidationLogger(ILogger logger)
		{
			wtgLogger = logger;

			logLevelMapping.Add(LogLevel.Debug, LogType.Debug);
			logLevelMapping.Add(LogLevel.Information, LogType.Information);
			logLevelMapping.Add(LogLevel.Warning, LogType.Warning);
			logLevelMapping.Add(LogLevel.Error, LogType.Error);
			logLevelMapping.Add(LogLevel.Critical, LogType.Error);
		}

		public IDisposable BeginScope<TState>(TState state)
		{
			// This logger doesn't do anything complex so we don't need any real Disposable logic.
			return NullDisposable.Instance;
		}

		public bool IsEnabled(LogLevel logLevel)
		{
			return logLevelMapping.ContainsKey(logLevel);
		}

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
		{
			if (!IsEnabled(logLevel))
			{
				return;
			}

			var logType = logLevelMapping[logLevel];
			var message = formatter(state, exception);

			if (exception != null)
			{
				wtgLogger?.Log(logType, message, exception);
			}
			else
			{
				wtgLogger?.Log(logType, message);
			}
		}
	}
}
