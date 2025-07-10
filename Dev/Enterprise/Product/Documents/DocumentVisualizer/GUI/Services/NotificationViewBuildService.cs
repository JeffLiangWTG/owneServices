using System;
using Enterprise.DocumentVisualizer.Presentation;

namespace Enterprise.DocumentVisualizer.GUI
{
	sealed class NotificationViewBuildService : INotificationViewBuildService
	{
		public INotificationView CreateNotificationView(string id, string message, Core.NotificationType notificationType, Action onClick)
		{
			var notificationView = new NotificationView(id,
				message,
				notificationType,
				onClick);

			return notificationView;
		}
	}
}
