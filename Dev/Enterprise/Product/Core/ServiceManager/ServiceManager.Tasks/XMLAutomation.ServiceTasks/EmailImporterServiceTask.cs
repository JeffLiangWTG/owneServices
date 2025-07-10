using System;
using System.Threading;
using CargoWise.ComponentModel;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ServiceManager.Tasks.XMLAutomation.ServiceTasks;
using Enterprise.ZArchitecture;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	EmailImporterServiceTask.Code,
	EmailImporterServiceTask.ServiceTaskDescription,
	"BP",
	typeof(EmailImporterServiceTask),
	AllowsMultipleInstances = true,
	IsMandatory = false,
	MinimumPeriod = "1minute",
	DefaultScheduleRunEvery = "1minute"
	)
]

namespace Enterprise.ServiceManager.Tasks.XMLAutomation.ServiceTasks
{
	public class EmailImporterServiceTask : ServiceProviderImpl, INotifications
	{
		public const string Code = "EMI";
		public const string ServiceTaskDescription = "Email Importer Task";

		[HostedServiceRequirement]
		public static string ShouldRun() => eHubMessagingRegistry.Instance.HasInterfaceConnector ? string.Empty : FormattableString.Invariant($"You need the Interface Connector license to activate the {ServiceTaskDescription}");

		public override void RunTask(CancellationToken token)
		{
			EmailImportDirector.Run(token);
			EmailImportDirector.ResetNotifications();
		}

		#region EmailImportDirector

		protected EmailImportBatchDirector EmailImportDirector
		{
			get { return emailImportDirector ?? (emailImportDirector = GetEmailImportBatchDirector()); }
		}
		EmailImportBatchDirector emailImportDirector;

#if DEBUG
		protected virtual
#endif
 EmailImportBatchDirector GetEmailImportBatchDirector()
		{
			return new EmailImportBatchDirector(this);
		}

		#endregion

		#region INotifications Members

		void INotifications.Add(INotification notification)
		{
			Notify(notification);
		}

		void Notify(INotification notification)
		{
			string message = GetNotificationMessage(notification);
			if (notification is ErrorNotification || Equals(notification.Type, NotificationType.Error))
			{
				ServiceLogger.Log(LogType.Error, message);
			}
			else if (notification is WarningNotification || Equals(notification.Type, NotificationType.Warning))
			{
				ServiceLogger.Log(LogType.Warning, message);
			}
			else
			{
				ServiceLogger.Log(LogType.Information, message);
			}
		}

		string GetNotificationMessage(INotification notification)
		{
			var message = string.Empty;

			var subscriberNotification = notification as INotificationSubscriberNotification;
			if (subscriberNotification != null)
			{
				message = subscriberNotification.MultiLineDisplayMessage;
			}

			if (string.IsNullOrEmpty(message))
			{
				message = notification.Message;
			}

			if (!string.IsNullOrEmpty(message))
			{
				return message;
			}

			var notificationType = notification.Type as INotificationSubscriberType;
			if (notificationType != null)
			{
				message = notificationType.Message;
			}

			return message;
		}

		#endregion
	}
}
