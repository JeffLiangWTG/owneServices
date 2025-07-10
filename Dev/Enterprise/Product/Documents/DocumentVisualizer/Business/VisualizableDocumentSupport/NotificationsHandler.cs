using System.Collections.Generic;
using CargoWise.ComponentModel;

namespace Enterprise.DocumentVisualizer.Business
{
	public sealed class NotificationsHandler : INotifications
	{
		public ICollection<INotification> Notifications
		{
			get { return notifications ?? (notifications = new List<INotification>()); }
		}

		List<INotification> notifications;

		void INotifications.Add(INotification notification)
		{
			Notifications.Add(notification);
		}
	}
}
