using System;
using System.Globalization;
using CargoWise.ComponentModel;
using Enterprise.Integration;
using Enterprise.ZArchitecture;

namespace ServiceManager.Integration.ServiceTasks.CW
{
	class TaskNotificationSubscriber : INotifications
	{
		public TaskNotificationSubscriber(ILogger logger)
		{
			serviceLogger = logger ?? throw new ArgumentNullException(nameof(logger));
			mutex = new object();
		}

		#region INotifications Members

		void INotifications.Add(INotification notification)
		{
			lock (mutex)
			{
				Notify(notification);
			}
		}

		void Notify(INotification notification)
		{
			var logType = LogType.Information;
			var message = notification.Message;

			if (
				notification.Type.Equals(NotificationType.Error) ||
				notification.Type is ErrorType)
			{
				logType = LogType.Error;
			}
			else if (
				notification.Type.Equals(NotificationType.Warning) ||
				notification.Type is WarningType)
			{
				logType = LogType.Warning;
			}
			else if (notification.Type.Equals(NotificationSubscriberType.VerboseInfo))
			{
				logType = LogType.Debug;
			}
			else if (notification is ProgressNotification)
			{
				logType = LogType.Debug;
				message += " " + ((ProgressNotification)notification).PercentageComplete.ToString(CultureInfo.InvariantCulture) + "%";
			}

			serviceLogger.Log(logType, message);
		}

		#endregion

		readonly object mutex;
		readonly ILogger serviceLogger;
	}
}
