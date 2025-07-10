using CargoWise.Common;
using CargoWise.ComponentModel;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public class PreValidateTraderNotifications : INotifications
	{
		public PreValidateTraderNotifications(MessageSendingNotificationCollection notificationCollection)
		{
			this.notificationCollection = Argument.NotNull(notificationCollection, nameof(MessageSendingNotificationCollection));
		}

		public void Add(INotification notification)
		{
			var severity = notification.Type.Severity;
			if (severity < NotificationType.Warning.Severity)
			{
				notificationCollection.AddError(notification.Message);
			}
			else if (severity >= NotificationType.Warning.Severity && severity < NotificationType.Information.Severity)
			{
				notificationCollection.AddWarning(notification.Message);
			}
			else
			{
				notificationCollection.AddInformation(notification.Message);
			}
		}

		readonly MessageSendingNotificationCollection notificationCollection;
	}
}
