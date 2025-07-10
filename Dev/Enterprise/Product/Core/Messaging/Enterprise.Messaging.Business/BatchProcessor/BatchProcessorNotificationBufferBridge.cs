using CargoWise.ComponentModel;
using Enterprise.BatchProcessor;
using Enterprise.ZArchitecture;

namespace Enterprise.DataTransfer
{
	public class BatchProcessorNotificationBufferBridge : NotificationBuffer
	{
		public BatchProcessorNotificationBufferBridge(LoggingInformation logger) : base(new NotificationBuffer())
		{
			this.Logger = logger;
		}

		protected BatchProcessorNotificationBufferBridge(INotifications inner) : base(inner)
		{
		}

		public override void Notify(INotification notification)
		{
			base.Notify(notification);
			if (notification is BatchNotification || notification is ErrorNotification)
			{
				INotificationSubscriberNotification subscriberNotification = notification as INotificationSubscriberNotification;
				Logger.Log(subscriberNotification != null ? subscriberNotification.MultiLineDisplayMessage : (notification.Message + "\n"));
			}
		}

		readonly LoggingInformation Logger;
	}
}
