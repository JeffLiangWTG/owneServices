using System;
using System.Collections.Generic;
using Enterprise.DocumentVisualizer.Core;

namespace Enterprise.DocumentVisualizer.Business
{
	/// <summary>
	/// Captures all logs including Debug
	/// </summary>
	sealed class VerboseLogger : ILogger
	{
		public IReadOnlyCollection<string> Logs => logs;
		readonly List<string> logs = new List<string>();

		public IReadOnlyCollection<string> ReportableLogs => reportableLogs;
		readonly List<string> reportableLogs = new List<string>();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1060:DoNotUseDateTimeNow", Justification = "Baseline")]
		public void Log(LogMessageType messageType, params object[] parameters)
		{
			if (parameters == null
				|| parameters.Length == 0)
			{
				return;
			}

			var parametersAsString = string.Join(", ", parameters);
			var message = $"{DateTime.Now:o} - {parametersAsString}";
			logs.Add(message);

			if (messageType == LogMessageType.ErrorReport)
			{
				reportableLogs.Add(message);
			}
		}
	}
}
