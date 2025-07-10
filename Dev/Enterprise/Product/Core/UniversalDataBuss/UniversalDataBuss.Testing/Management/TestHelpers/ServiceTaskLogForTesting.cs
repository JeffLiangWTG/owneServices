using System.Collections.Generic;
using Enterprise.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.Management.Testing
{
	public class ServiceTaskLogForTesting : ILoggingInformation, ISimpleLogger
	{
		readonly List<string> logs = new List<string>();
		readonly List<SimpleLog> simpleLogs = new List<SimpleLog>();

		public IEnumerable<ISimpleLog> Logs => simpleLogs;

		public override string ToString()
		{
			return string.Join("\r\n", logs.ToArray());
		}

		public void ClearLogs()
		{
			logs.Clear();
			simpleLogs.Clear();
		}

		public void DebugLog(string logMsg)
		{
			AddLogText("Debug - " + logMsg);
			simpleLogs.Add(new SimpleLog(LogType.Debug, logMsg));
		}

		public void Log(string logMsg)
		{
			AddLogText(logMsg);
			simpleLogs.Add(new SimpleLog(LogType.Information, logMsg));
		}

		public void LogError(string logMsg)
		{
			AddLogText("ERROR - " + logMsg);
			simpleLogs.Add(new SimpleLog(LogType.Error, logMsg));
		}

		public void LogWarning(string logMsg)
		{
			AddLogText("Warning - " + logMsg);
			simpleLogs.Add(new SimpleLog(LogType.Warning, logMsg));
		}

		protected virtual void AddLogText(string text)
		{
			logs.Add(text);
		}

		public void Log(LogType type, string message)
		{
			switch (type)
			{
				case LogType.Error:
					LogError(message);
					break;
				case LogType.Warning:
					LogWarning(message);
					break;
				case LogType.Information:
					Log(message);
					break;
				case LogType.Debug:
				default:
					DebugLog(message);
					break;
			}
		}
	}
}
