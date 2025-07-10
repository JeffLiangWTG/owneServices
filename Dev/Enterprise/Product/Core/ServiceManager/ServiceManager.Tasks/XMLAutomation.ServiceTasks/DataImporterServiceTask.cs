using System;
using System.Threading;
using CargoWise.ComponentModel;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ServiceManager.Tasks.XMLAutomation.ServiceTasks;
using Enterprise.ZArchitecture;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	DataImporterServiceTask.Code,
	DataImporterServiceTask.ServiceTaskDescription,
	"BP",
	typeof(DataImporterServiceTask),
	AllowsMultipleInstances = false,
	IsMandatory = false,
	MinimumPeriod = "1minute",
	DefaultScheduleRunEvery = "1minute"
	)
]

namespace Enterprise.ServiceManager.Tasks.XMLAutomation.ServiceTasks
{
	public class DataImporterServiceTask : ServiceProviderImpl, INotifications
	{
		public const string Code = "XMI";
		public const string ServiceTaskDescription = "Data Importer Task";

		public override void RunTask(CancellationToken token)
		{
			ServiceLogger.Log(LogType.Information, "Starting XMI service task. Current Branch - " + (Env.CurrentBranch != null ? Env.CurrentBranch.Code : "(null)"));

			DataImportDirector.Run(token);
			DataImportDirector.ResetNotifications();

			ServiceLogger.Log(LogType.Information, "Finished XMI service task. Current Branch - " + (Env.CurrentBranch != null ? Env.CurrentBranch.Code : "(null)"));
		}

		[HostedServiceRequirement]
		public static string ShouldRun() => eHubMessagingRegistry.Instance.HasInterfaceConnector ? string.Empty : FormattableString.Invariant($"You need the Interface Connector license to activate the {ServiceTaskDescription}");

		#region INotifications Members

		void INotifications.Add(INotification notification)
		{
			Notify(notification);
		}

		void Notify(INotification notification)
		{
			var message = GetNotificationMessage(notification);
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

			if (string.IsNullOrEmpty(message))
			{
				var notificationType = notification.Type as INotificationSubscriberType;
				if (notificationType != null)
				{
					message = notificationType.Message;
				}
			}

			return message;
		}

		#endregion

		#region DataImportDirector

		protected BatchImportDirector DataImportDirector
		{
			get { return dataImportDirector ?? (dataImportDirector = GetDataImportDirector()); }
		}
		BatchImportDirector dataImportDirector;

#if DEBUG
		protected virtual
#endif
		BatchImportDirector GetDataImportDirector()
		{
			return new BatchImportDirector(this);
		}

		#endregion
	}
}
