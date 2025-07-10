using System.Collections.Generic;
using CargoWise.ComponentModel;

namespace CargoWise.EntityFramework
{
	internal class ZCollectionNotificationProviderHelper : INotificationProvider
	{
		public ZCollectionNotificationProviderHelper(IBusinessObjectCollection collection)
		{
			this.collection = collection;
		}
		public IEnumerable<INotification> Notifications
		{
			get
			{
				foreach (BusinessObject child in collection)
				{
					if (!child.IsTopLevel)
					{
						foreach (INotification notification in child.NotificationsIncludingChildren)
						{
							yield return notification;
						}
					}
				}
			}
		}

		public bool HasNotifications()
		{
			foreach (BusinessObject child in collection)
			{
				if (!child.IsTopLevel && child.HasNotifications())
				{
					return true;
				}
			}
			return false;
		}

		public bool HasNotifications(INotificationType type)
		{
			foreach (BusinessObject child in collection)
			{
				if (!child.IsTopLevel && child.HasNotifications(type))
				{
					return true;
				}
			}
			return false;
		}

		public INotificationType GetHighestSeverityNotificationType()
		{
			return GetHighestSeverityNotificationTypes().GetHighestSeverityNotificationType();
		}

		IEnumerable<INotificationType> GetHighestSeverityNotificationTypes()
		{
			foreach (BusinessObject child in collection)
			{
				if (!child.IsTopLevel)
				{
					yield return child.GetHighestSeverityNotificationType();
				}
			}
		}

		readonly IBusinessObjectCollection collection;
	}
}
