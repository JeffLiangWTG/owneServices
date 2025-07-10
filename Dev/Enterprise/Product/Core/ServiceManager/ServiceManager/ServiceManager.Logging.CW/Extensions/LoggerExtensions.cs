using System;
using Enterprise.Integration;

namespace ServiceManager.Logging.CW
{
	public static class LoggerExtensions
	{
		public static ILogger ToCW1Logger(this Microsoft.Extensions.Logging.ILogger logger)
		{
			return new LoggerAdapter(logger);
		}

		public static LogType ToLogType(this Microsoft.Extensions.Logging.LogLevel logLevel)
		{
			return logLevel switch
			{
				Microsoft.Extensions.Logging.LogLevel.Debug => LogType.Debug,
				Microsoft.Extensions.Logging.LogLevel.Error => LogType.Error,
				Microsoft.Extensions.Logging.LogLevel.Information => LogType.Information,
				Microsoft.Extensions.Logging.LogLevel.Warning => LogType.Warning,
				_ => throw new NotSupportedException($"{logLevel} is not supported")
			};
		}
	}
}
