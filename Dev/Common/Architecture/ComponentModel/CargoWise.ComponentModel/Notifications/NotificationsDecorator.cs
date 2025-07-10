using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;

namespace CargoWise.ComponentModel
{
	public class NotificationsDecorator : INotifications
	{
		public NotificationsDecorator(INotifications notifications)
		{
			InnerNotifications = Argument.NotNull(notifications, nameof(notifications));
		}

		INotifications InnerNotifications { get; }

		List<INotification> DecoratedEvents => decoratedEvents ?? (decoratedEvents = new List<INotification>());
		List<INotification> decoratedEvents;

		public bool ReportedFatalError { get; private set; }

		public void Add(INotification notification)
		{
			if (notification.Type.IsFatal)
			{
				ReportedFatalError = true;
			}

			DecoratedEvents.Add(notification);

			InnerNotifications.Add(notification);
		}

		public string MessagesAsString => string.Join("\r\n", DecoratedEvents.Select(e => e.Message));
	}
}
