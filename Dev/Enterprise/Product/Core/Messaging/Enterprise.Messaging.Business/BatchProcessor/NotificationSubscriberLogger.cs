using CargoWise.ComponentModel;
using Enterprise.ZArchitecture;

namespace Enterprise.BatchProcessor
{
	public class NotificationSubscriberLogger : LoggingInformation
	{
		public NotificationSubscriberLogger(INotifications notifications)
			: base()
		{
			this.notifications = notifications;
		}

		public override void Log(string logMsg)
		{
			notifications.Notify(new InfoNotification(logMsg));
		}

		public override void AddBlankLine()
		{
		}

		public override void ClearLogs()
		{
		}

		readonly INotifications notifications;
	}
}
