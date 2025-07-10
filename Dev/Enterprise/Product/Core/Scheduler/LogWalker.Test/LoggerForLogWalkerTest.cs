using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Integration;

namespace Enterprise.LogWalker.Test
{
	[Serializable]
	public class LoggerForLogWalkerTest : ILogger
	{
		public List<LogEntryForTest> LogEntries { get; } = new List<LogEntryForTest>();

		public void Log(LogType logType, string message)
		{
			LogEntries.Add(new LogEntryForTest(logType, message));
		}

		public void Log(LogType logType, string message, Exception ex)
		{
			LogEntries.Add(new LogEntryForTest(logType, message));
		}

		public void ClearLog()
		{
			LogEntries.Clear();
		}

		public override string ToString()
		{
			return string.Join("\r\n", LogEntries);
		}
	}

	public class LogEntryForTest
	{
		public LogEntryForTest(LogType logType, ZString message)
		{
			LogType = logType;
			Message = message;
		}

		public LogType LogType { get; set; }
		public ZString Message { get; set; }

		public override string ToString()
		{
			return $"{LogType}: {Message}";
		}
	}
}
