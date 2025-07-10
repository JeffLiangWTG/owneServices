using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Integration;

namespace Enterprise.Accounting.Utility.Testing
{
	[Serializable]
	public class LoggerForTesting : ILogger
	{
		void ILogger.Log(LogType logType, string message)
		{
			notifiedEventListWithType.Add((logType, message));
		}
		readonly List<(LogType Type, string Message)> notifiedEventListWithType = new List<(LogType Type, string Message)>();

		void ILogger.Log(LogType logType, string message, Exception ex)
		{
			NotifiedEventList.Add(message);
		}

		public override string ToString() => string.Join("\r\n", NotifiedEventList.ToArray());

		public List<string> NotifiedEventList => notifiedEventListWithType.Select(ne => ne.Message).ToList();

		public List<string> ErrorEventList => notifiedEventListWithType.Where(ne => ne.Type == LogType.Error).Select(ne => ne.Message).ToList();

		public List<string> DebugEventList => notifiedEventListWithType.Where(ne => ne.Type == LogType.Debug).Select(ne => ne.Message).ToList();

		public List<string> InformationEventList => notifiedEventListWithType.Where(ne => ne.Type == LogType.Information).Select(ne => ne.Message).ToList();
	}
}
