using System;
using Enterprise.Integration;
using Microsoft.Extensions.Logging;

namespace ServiceManager.Logging.CW
{
	class LoggerAdapter : Enterprise.Integration.ILogger
	{
		public LoggerAdapter(Microsoft.Extensions.Logging.ILogger logger)
		{
			this.logger = logger;
		}

		public void Log(LogType type, string message) => 
			logger.Log(ConvertToLogLevel(type), message);

		public void Log(LogType type, string message, Exception ex) =>
			logger.Log(ConvertToLogLevel(type), ex, message);

		static LogLevel ConvertToLogLevel(LogType type) =>
			type switch
			{
				LogType.Error => LogLevel.Error,
				LogType.Information => LogLevel.Information,
				LogType.Warning => LogLevel.Warning,
				_ => LogLevel.Debug
			};

		internal readonly Microsoft.Extensions.Logging.ILogger logger;
	}
}
