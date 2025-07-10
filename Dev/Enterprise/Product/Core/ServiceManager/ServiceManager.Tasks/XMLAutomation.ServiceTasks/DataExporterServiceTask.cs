using System;
using System.Threading;
using CargoWise.ComponentModel;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ServiceManager.Tasks.XMLAutomation.ServiceTasks;
using Enterprise.ZArchitecture;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	DataExporterServiceTask.Code,
	DataExporterServiceTask.ServiceTaskDescription,
	"BP",
	typeof(DataExporterServiceTask),
	AllowsMultipleInstances = true,
	IsMandatory = false,
	MinimumPeriod = "1minute",
	DefaultScheduleRunEvery = "1minute"
	)
]

namespace Enterprise.ServiceManager.Tasks.XMLAutomation.ServiceTasks
{
	public class DataExporterServiceTask : ServiceProviderImpl, INotifications
	{
		public const string Code = "XME";
		public const string ServiceTaskDescription = "Data Exporter Task";

		[HostedServiceRequirement]
		public static string ShouldRun() => eHubMessagingRegistry.Instance.HasInterfaceConnector ? string.Empty : FormattableString.Invariant($"You need the Interface Connector license to activate the {ServiceTaskDescription}");

		public override void RunTask(CancellationToken token)
		{
			DataExportDirector.Run(token);
			DataExportDirector.ResetNotifications();
		}

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

		#region DataExportDirector

		protected BatchExportDirector DataExportDirector
		{
			get { return dataExportDirector ?? (dataExportDirector = GetDataExportDirector()); }
		}
		BatchExportDirector dataExportDirector;

#if DEBUG
		protected virtual
#endif
 BatchExportDirector GetDataExportDirector()
		{
			return new BatchExportDirector(this);
		}

		#endregion
	}
}
