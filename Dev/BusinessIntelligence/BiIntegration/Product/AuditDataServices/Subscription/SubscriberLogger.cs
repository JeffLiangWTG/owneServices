using System;
using Enterprise.Integration;

namespace Enterprise.AuditDataServices.Subscription
{
	public class SubscriberLogger : ILogger
	{
		public SubscriberLogger(ILogger innerLogger, string logPrefix)
		{
			this.innerLogger = innerLogger;
			this.logPrefix = logPrefix;
		}

		public void Log(LogType type, string message)
		{
			LogWithPrefix(type, message, ex: null);
		}

		public void Log(LogType type, string message, Exception ex)
		{
			LogWithPrefix(type, message, ex);
		}

		void LogWithPrefix(LogType type, string message, Exception ex)
		{
			innerLogger.Log(type, "[" + logPrefix + "] " + message, ex);
		}

		readonly ILogger innerLogger;
		readonly string logPrefix;
	}
}
