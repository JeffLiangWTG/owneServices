namespace Enterprise.Client.EDI.VersionReporting.BatchProcessor
{
	using System;
	using System.Collections.Generic;
	using CargoWise.Common;
	using CargoWise.ComponentModel;
	using CargoWise.EntityFramework;
	using CargoWise.Integration;
	using CargoWise.Types;
	using Enterprise.eHubMessaging.Business;
	using Enterprise.Integration;
	using Enterprise.MasterFiles.Business;
	using Enterprise.Messaging.Business;
	using Enterprise.ZArchitecture;

	public abstract class ReportMessageAction : IMessageAction
	{
		public ReportMessageAction(BusinessObjectFactoryProvider factoryProvider)
		{
		}

		bool ExecuteActionCore(EDIMessage message, INotifications notifications, out List<ITransactionParticipant> participants)
		{
			participants = new List<ITransactionParticipant>(0);
			using (var reader = SystemMessage.GetXmlReader(message))
			{
				var processor = CreateProcessor(message.Interchange.EI_From, notifications);
				processor.Process(reader.ReadOuterXml());
			}
			return true;
		}

		internal abstract IEmailAttachmentProcessor CreateProcessor(string from, INotifications notifications);

		#region IMessageAction implementation

		bool IMessageAction.ExecuteAction(EDIMessage message, INotifications notifications, out List<ITransactionParticipant> participants)
		{
			try
			{
				return ExecuteActionCore(message, notifications, out participants);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce(this.GetType().Name, "Message: " + message.EM_MessageTextShort, ex);
				participants = new List<ITransactionParticipant>(0);

				// Returning true since we don't want this message or other messages in the batch retried.
				// The processor has its own factories which may have been saved.
				return true;
			}
		}

		void IMessageAction.SendNotificationEmail(ZString subject, ZString body, INotifications notifications, bool onSuccess)
		{
		}

		#endregion
	}

	/// <summary>
	/// Convert ILogger to INotifications.
	/// </summary>
	internal class NotificationLogger : ILogger
	{
		public static ILogger CreateLogger(INotifications notifications)
		{
			if (notifications != null)
			{
				return new NotificationLogger(notifications);
			}
			else
			{
				return null;
			}
		}

		public NotificationLogger(INotifications notifications)
		{
			this.notifications = notifications;
		}

		readonly INotifications notifications;

		public void Log(LogType type, string message, Exception ex)
		{
			if (ex != null)
			{
				message += ex.ToString();
			}

			if (type == LogType.Error)
			{
				notifications.AddError(message);
			}
			else if (type == LogType.Information)
			{
				notifications.Notify(new InfoNotification(message));
			}
			else if (type == LogType.Warning)
			{
				notifications.AddWarning(message);
			}
			else if (type == LogType.Debug)
			{
				notifications.Notify(new VerboseInfoNotification(message));
			}
		}

		public void Log(LogType type, string message)
		{
			Log(type, message, null);
		}
	}
}
