using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Res = Enterprise.Messaging.Business.Res;

namespace Enterprise.BatchProcessor
{
	public class LoggingInformation : LoggingInformationBase, IErrorReporter, ILoggingInformation, INotifications
	{
		readonly List<ISimpleLog> logs = new List<ISimpleLog>();

		#region Events

		public delegate void LogInfoAdded(string log, LogType logType);
		public event LogInfoAdded OnLogInfoAdded;

		protected void FireLogInfoAddedEvent(string msg, LogType logType)
		{
			if (OnLogInfoAdded != null)
			{
				OnLogInfoAdded(msg, logType);
			}
		}

		#endregion

		public override IEnumerable<ISimpleLog> Logs => logs;

		public LoggingInformation()
			: base()
		{
		}

		public override void ClearLogs()
		{
			UserLogStrings.Clear();
			DebugLogStrings.Clear();
			logs.Clear();
		}

		public override void AddBlankLine()
		{
			FireLogInfoAddedEvent("", LogType.Information);
		}

		public override void Log(string logMsg)
		{
			Log(logMsg, LogType.Information);
		}

		public void Log(string logMsg, LogType logType)
		{
			if (logMsg != null && logMsg.Length > 0)
			{
				string dataString = Gap(1) + logMsg;
				FireLogInfoAddedEvent(dataString, logType);
				UserLogStrings.Add(dataString);
				DebugLogStrings.Add(dataString);
				logs.Add(new SimpleLog(logType, logMsg));
			}
		}

		public override void LogError(string logMsg)
		{
			this.Log(logMsg, LogType.Error);
			logs.Add(new SimpleLog(LogType.Error, logMsg));
		}

		public void LogWarning(string logMsg)
		{
			this.Log(logMsg, LogType.Warning);
			logs.Add(new SimpleLog(LogType.Warning, logMsg));
		}

		public void AddSeparatorLine()
		{
			FireLogInfoAddedEvent("-----------------------------------------------------------------------------------------------------------------------------------------------------", LogType.Information);
		}

		public void ContinueLog(string logMsg)
		{
			if (logMsg != null && logMsg.Length > 0)
			{
				string dataString = Gap(3) + logMsg;
				FireLogInfoAddedEvent(dataString, LogType.Information);
				UserLogStrings.Add(dataString);
				DebugLogStrings.Add(dataString);
				logs.Add(new SimpleLog(LogType.Information, logMsg));
			}
		}

		public void DebugLog(string logMsg)
		{
			if (logMsg != null && logMsg.Length > 0)
			{
				string dataString = " *** " + Gap(1) + ReplaceDebugStars(logMsg);
				FireLogInfoAddedEvent(dataString, LogType.Debug);
				DebugLogStrings.Add(dataString);
				logs.Add(new SimpleLog(LogType.Debug, logMsg));
			}
		}

		public void ContinueDebugLog(string logMsg)
		{
			if (logMsg != null && logMsg.Length > 0)
			{
				string dataString = Gap(2) + " *** " + Gap(1) + ReplaceDebugStars(logMsg);
				FireLogInfoAddedEvent(dataString, LogType.Debug);
				DebugLogStrings.Add(dataString);
				logs.Add(new SimpleLog(LogType.Debug, logMsg));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		public void SendFailureNotification(string errorMsg, bool logAsWellAsSendEmail)
		{
			if (logAsWellAsSendEmail)
			{
				Log(errorMsg);
			}
			string subject = Res.GetString("0d58b3c3-c70c-4075-9d27-fd562e254078", "Error Report from Batch Processor running at {0} on server {1}", GlbCompany.CurrentCompany.GC_Name, System.Environment.MachineName);
			string body = Res.GetString("0dfa8b45-1c9d-4d66-811a-00053e7d998e", "This batch process is connected to the {0} DB on {1}.", Db.DatabaseName, Db.ServerName) + "\r\n";
			body += Res.GetString("2f7f0e8d-9898-4082-9e6f-99598822994d", "The following Error was reported at") + " " + Env.Time.CurrentLocalDateTime.ToString("ddMMMyyyy HH:mm:ss") + System.Environment.NewLine;
			body += System.Environment.NewLine;
			body += errorMsg;
			if (!GlbStaff.CurrentUser.GS_IsDeveloper)
			{
				if (errorMsg.StartsWith("Unexpected error attempting to download - MHUBCommandFailedException", StringComparison.InvariantCulture))
				{
					// send this for monitoring only - generally a temporary internet connection to CrimsonLogic MHUB system, (SG Customs TradeNet), issue
					Env.OutgoingMailManager.CreateAndSaveSimple(subject, body, MessagingConstants.SupportEmail.Singapore);
				}
				else
				{
					Env.OutgoingMailManager.CreateAndSaveSimple(subject, body, Core.Constants.EmailAddresses.ErrorReportAddress);
				}
			}
			else
			{
				Globals.Message.ShowError(body, subject);
			}
			//Env.OutgoingMailManager.SendEmailToCompanyNotificationGroupWithFallBack(Subject, Body);
		}

		public override void Log(LogType type, string message)
		{
			Log(message, type);
		}

		protected string Gap(int tabSpaces)
		{
			string result = "";
			for (int i = 0; i < tabSpaces; i++)
			{
				result += "\t";
			}
			return result;
		}

		protected string ReplaceDebugStars(string input)
		{
			return input.Replace(" *** ", "");
		}

		public void ShowMessage(string message)
		{
			this.Log("Exception Report Message: " + message);
		}

		public void ShowError(string message, string caption)
		{
			this.Log(caption + ": " + message);
		}

		#region IErrorReporter Members

		void IErrorReporter.Clear()
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer only string")]
		void IErrorReporter.Report(string key, string message, Exception exception)
		{
			if (exception != null)
			{
				Log("Exception Report Message: " + message);
			}
			else
			{
				Log(message);
			}
		}

		void IErrorReporter.ReportDeveloperExceptionOrHandleSilently(string key, string message, Exception exception)
		{
		}

		#endregion

		#region INotifications

		void INotifications.Add(INotification notification)
		{
			LogType GetLogType(INotificationType type)
			{
				if (NotificationType.Information == type || NotificationSubscriberType.Info == type as NotificationSubscriberType)
				{
					return LogType.Information;
				}
				else if (NotificationType.Warning == type)
				{
					return LogType.Warning;
				}
				else if (NotificationType.Error == type)
				{
					return LogType.Error;
				}
				else
				{
					ErrorReporter.ReportOnce("UnknownNotificationType", FormattableString.Invariant($"Unknown notification type:{type.ToString()}"));
					return LogType.Information;
				}
			}

			Log(notification.Message, GetLogType(notification.Type));
		}

		#endregion

		class SimpleLog : ISimpleLog
		{
			public LogType Type { get; }
			public string Message { get; }

			public SimpleLog(LogType type, string message)
			{
				Type = type;
				Message = message;
			}

			public override string ToString()
			{
				if (Type == LogType.Information)
				{
					return Message;
				}

				return Type + " - " + Message;
			}
		}
	}
}
