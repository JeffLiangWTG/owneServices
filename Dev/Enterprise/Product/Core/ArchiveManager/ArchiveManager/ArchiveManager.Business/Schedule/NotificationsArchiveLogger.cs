using System;
using CargoWise.Common;
using CargoWise.ComponentModel;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture;

namespace Enterprise.ArchiveManager.Business.Schedule
{
	public class NotificationsArchiveLogger : IArchiveLogger
	{
		public NotificationsArchiveLogger(INotifications notifications)
			=> this.notifications = notifications;

		public void LogError(string code, string message)
			=> LogError(LogMessage(code, message));

		public void LogError(string message)
			=> notifications.AddError(message);

		public void LogInfo(string code, string message)
			=> LogInfo(LogMessage(code, message));

		public void LogInfo(string message)
			=> notifications.Notify(new InfoNotification(message));

		public void LogWarning(string code, string message)
			=> LogWarning(LogMessage(code, message));

		public void LogWarning(string message)
			=> notifications.AddWarning(message);

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

		readonly INotifications notifications;

		string LogMessage(string code, string message)
			=> string.IsNullOrEmpty(code) ? message : $"{code}|{message}";

		string EnsureKeyIsPrefixed(string key)
			=> key.StartsWith(ErrorReportingPrefix) ? key : ErrorReportingPrefix + key;

		const string ErrorReportingPrefix = "ArchiveManager.";
	}
}
