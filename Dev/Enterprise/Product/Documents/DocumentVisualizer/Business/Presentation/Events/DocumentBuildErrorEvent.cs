using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.DocumentVisualizer.Core;

namespace Enterprise.DocumentVisualizer.Presentation
{
	sealed class DocumentBuildErrorEvent : INotificationProvider
	{
		public DocumentBuildErrorEvent(IEnumerable<INotification> notifications)
		{
			Argument.NotNull(notifications, nameof(notifications));

			this.notifications = notifications;
		}

		readonly IEnumerable<INotification> notifications;

		public IEnumerable<INotification> Notifications => notifications;
	}
}
