using System;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace CargoWise.eHub.MessageEvent.KafkaProducerService.Logging
{
	public static class eHubLoggingExtension
	{
		public static void eHubLog(this ILogger logger, LogLevel level, string messageId, string activityId, string source, string? message, params object?[] args)
		{
			LogMessage(logger, level, messageId, activityId, source, () => logger.Log(level, message, args));
		}

		public static void eHubLog(this ILogger logger, LogLevel level, string messageId, string activityId, string source, Exception? exception, string? message, params object?[] args)
		{
			LogMessage(logger, level, messageId, activityId, source, () => logger.Log(level, exception, message, args));
		}

		private static void LogMessage(ILogger logger, LogLevel level, string messageId, string activityId, string source, Action doLog)
		{
			if (logger.IsEnabled(level))
			{
				using (PushNoneEmpty("MessageID", messageId))
				using (PushNoneEmpty("ActivityID", activityId))
				using (PushNoneEmpty("CustomizedSourceContext", source))
				{
					doLog();
				}
			}
		}

		private static IDisposable? PushNoneEmpty(string key, string val)
		{
			if (string.IsNullOrEmpty(val))
			{
				return null;
			}
			return LogContext.PushProperty(key, val);
		}
	}
}
