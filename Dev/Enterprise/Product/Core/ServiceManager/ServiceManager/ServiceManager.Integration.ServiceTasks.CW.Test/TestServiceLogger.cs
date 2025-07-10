using System;
using System.Collections.Generic;
using System.Text;
using Enterprise.Integration;

namespace ServiceManager.Integration.ServiceTasks.CW.Test
{
	public class TestServiceLogger : ILogger
	{
		public void Log(LogType type, string message)
		{
			Log(type, message, null);
		}

		public void Log(LogType type, string message, Exception ex)
		{
			logLines.Add($"{type}|{message}{(ex != null ? $"|{ex}" : string.Empty)}");
		}

		public override string ToString()
		{
			var builder = new StringBuilder();
			logLines.ForEach(delegate (string line) { builder.AppendLine(line); });
			return builder.ToString();
		}

		public void ClearLog()
		{
			logLines.Clear();
		}

		public string this[int index] => logLines[index];

		public int Count => logLines.Count;

		readonly List<string> logLines = new();
	}
}
