using System.Collections.Generic;
using Enterprise.Integration;

namespace Enterprise.BufferManagement.Business
{
	sealed class DelayedLogger
	{
		internal DelayedLogger(ILogger realLogger)
		{
			this.realLogger = realLogger;
		}

		readonly ILogger realLogger;
		readonly List<Log> logMessages = new List<Log>();

		internal void LogIfSaveSuccessful(LogType type, string message)
		{
			logMessages.Add(new Log { LogType = type, Message = message, LogOnlyIfSaveSucceeded = true });
		}

		internal void LogAlways(LogType type, string message)
		{
			logMessages.Add(new Log { LogType = type, Message = message, LogOnlyIfSaveSucceeded = false });
		}

		internal void FlushLogs(bool saveSucceeded)
		{
			foreach (var message in logMessages)
			{
				if (!message.LogOnlyIfSaveSucceeded || saveSucceeded)
				{
					realLogger.Log(message.LogType, message.Message);
				}
			}
		}

		class Log
		{
			internal LogType LogType { get; set; }
			internal string Message { get; set; }
			internal bool LogOnlyIfSaveSucceeded { get; set; }
		}
	}
}
