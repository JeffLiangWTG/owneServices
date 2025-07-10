using System.Collections.Generic;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class DummyNotifications : CargoWise.ComponentModel.INotifications
	{
		public IReadOnlyCollection<CargoWise.ComponentModel.INotification> Notifications => notifications;
		readonly List<CargoWise.ComponentModel.INotification> notifications = new List<CargoWise.ComponentModel.INotification>();

		public void Add(CargoWise.ComponentModel.INotification notification)
		{
			notifications.Add(notification);
		}
	}
}