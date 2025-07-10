using System;
using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.ArchiveManager.Integration;

namespace Enterprise.ArchiveManager.GUI.Test
{
	public class TestArchiveLogger : IArchiveLogger
	{
		public List<string> ListOfMessages { get; set; } = new List<string>();

		public void LogInfo(string code, string message)
			=> ListOfMessages.Add("Information|" + LogMessage(code, message));

		public void LogInfo(string message)
			=> ListOfMessages.Add("Information|" + message);

		public void LogWarning(string code, string message)
			=> ListOfMessages.Add("Warning|" + LogMessage(code, message));

		public void LogWarning(string message)
			=> ListOfMessages.Add("Warning|" + message);

		public void LogError(string code, string message)
			=> ListOfMessages.Add("Error|" + LogMessage(code, message));

		public void LogError(string message)
			=> ListOfMessages.Add("Error|" + message);

		public void LogAndReportError(string key, string systemCode, string message, Exception exception)
		{
			message = LogMessage(systemCode, message);
			key = EnsureKeyIsPrefixed(key);

			if (ErrorReporter.HasBeenReported(key))
			{
				LogError(message + "\n" + exception.ToString());
			}
			else
			{
				ErrorReporter.ReportOnce(key, message, exception);
			}
		}

		string LogMessage(string code, string message)
			=> string.IsNullOrEmpty(code) ? message : $"{code}|{message}";

		string EnsureKeyIsPrefixed(string key)
			=> key.StartsWith(ErrorReportingPrefix) ? key : ErrorReportingPrefix + key;

		const string ErrorReportingPrefix = "ArchiveManager.";
	}
}
