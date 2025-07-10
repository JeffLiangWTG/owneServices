using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.DocumentVisualizer.Core;

namespace Enterprise.DocumentVisualizer.Business
{
	public sealed class BindableNotifications : NonPersistentBusinessObject, IObsoleteValidation
	{
		public BindableNotifications(IEnumerable<INotification> notifications)
		{
			this.notifications = notifications ?? Enumerable.Empty<INotification>();
		}

		readonly IEnumerable<INotification> notifications;

		public new BindableNotificationsCollection Notifications
		{
			get
			{
				if (bindableNotifications == null)
				{
					bindableNotifications = GetBindableNotifications();
				}
				return bindableNotifications;
			}
		}

		BindableNotificationsCollection bindableNotifications;

		BindableNotificationsCollection GetBindableNotifications()
		{
			var result = new BindableNotificationsCollection();

			foreach (var notification in notifications)
			{
				var docummentNotification = new BindableNotification(notification);
				result.Add(docummentNotification);
			}

			return result;
		}
	}
}
