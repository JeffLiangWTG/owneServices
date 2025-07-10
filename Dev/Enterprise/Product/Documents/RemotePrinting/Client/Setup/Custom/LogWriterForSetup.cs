using System;
using System.Globalization;
using System.IO;
using Enterprise.RemotePrinting.Client.CustomAction;

namespace Enterprise.RemotePrinting.Client.Setup.Custom
{
	public class LogWriterForSetup
	{
		public LogWriterForSetup(string logName)
		{
			LogFilePath = GetLogFilePath(logName);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "logFilePath")]
		protected string GetLogFilePath(string logName)
		{
			if (string.IsNullOrEmpty(logName))
			{
				logName = "EMPTY";
			}

			var logDirectory = Constants.LogsFolder;
			var logFilePath = Path.Combine(logDirectory, string.Format(CultureInfo.InvariantCulture, "InstallLog{0:_yyyyMMdd_HHmmss}_{1}.txt", GetCurrentTime(), logName));
			return logFilePath;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1060:DoNotUseDateTimeNow", Justification = "Need local time for log file name")]
		protected virtual DateTime GetCurrentTime() => DateTime.Now;

		public void WriteLog(string message)
		{
			if (string.IsNullOrEmpty(LogFilePath))
			{
				return;
			}

			WriteLogCore(message);
		}

		protected virtual void WriteLogCore(string message)
		{
			try
			{
				File.AppendAllText(LogFilePath, message);
			}
			catch (Exception)
			{
				// Error while saving to log
			}
		}

		protected readonly string LogFilePath;
	}
}
