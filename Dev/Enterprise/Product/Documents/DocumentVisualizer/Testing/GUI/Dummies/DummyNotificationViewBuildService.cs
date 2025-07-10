using System;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Presentation;

namespace Enterprise.DocumentVisualizer.Testing.GUI
{
	sealed class DummyNotificationViewBuildService : INotificationViewBuildService
	{
		public INotificationView CreateNotificationView(string id, string message, NotificationType notificationType, Action onClick)
		{
			return new DummyNotificationView
			{
				Id = id,
				Message = message,
				Visible = true
			};
		}
	}
}