using System;

namespace Enterprise.DocumentVisualizer.Presentation
{
	public interface INotificationViewBuildService
	{
		INotificationView CreateNotificationView(string id, string message, Core.NotificationType notificationType, Action onClick);
	}
}