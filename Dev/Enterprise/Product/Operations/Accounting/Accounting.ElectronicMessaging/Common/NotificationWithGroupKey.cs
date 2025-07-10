using System;
using CargoWise.ComponentModel;

namespace Enterprise.Accounting.ElectronicMessaging.Common
{
	[Serializable]
	public class NotificationWithGroupKey : INotificationWithGroupKey
	{
		public NotificationWithGroupKey(string groupKey, INotification innerNotification)
		{
			GroupKey = groupKey;
			InnerNotification = innerNotification;
		}

		public INotification ReplaceMessage(string message)
		{
			InnerNotification = InnerNotification.ReplaceMessage(message);
			return this;
		}

		public string GroupKey { get; }

		public INotificationType Type => InnerNotification?.Type;

		public string Message => InnerNotification?.Message;

		INotification InnerNotification { get; set; }
	}
}
