using System;
using System.Drawing;

using CargoWise.ComponentModel;

namespace Enterprise.ZArchitecture.GUI.Notifications
{
	public static class NotificationColorScheme
	{
		public static readonly Color WarningColor = Color.FromArgb(255, 210, 166);
		public static readonly Color MessageErrorColor = Color.FromArgb(176, 216, 255);
		public static readonly Color ErrorColor = Color.FromArgb(255, 215, 215);

		public static readonly Color WarningFontColor = Color.Olive;
		public static readonly Color MessageErrorFontColor = Color.Blue;
		public static readonly Color ErrorFontColor = Color.Red;

		public static Color GetColor(INotificationType type)
		{
			if (type == CargoWise.EntityFramework.NotificationType.Warning)
			{
				return WarningColor;
			}

			if (type == CargoWise.EntityFramework.NotificationType.MessageError)
			{
				return MessageErrorColor;
			}

			if (type == CargoWise.EntityFramework.NotificationType.Error)
			{
				return ErrorColor;
			}

			var notificationColorProvider = type as INotificationColorProvider;
			if (notificationColorProvider != null)
			{
				return notificationColorProvider.GetColor();
			}

			throw new ArgumentOutOfRangeException(nameof(type), "Can't find corresponding color for notification type - " + type);
		}

		public static Color GetFontColor(INotificationType type)
		{
			if (type == CargoWise.EntityFramework.NotificationType.Warning)
			{
				return WarningFontColor;
			}

			if (type == CargoWise.EntityFramework.NotificationType.MessageError)
			{
				return MessageErrorFontColor;
			}

			if (type == CargoWise.EntityFramework.NotificationType.Error)
			{
				return ErrorFontColor;
			}

			var notificationColorProvider = type as INotificationColorProvider;
			if (notificationColorProvider != null)
			{
				return notificationColorProvider.GetFontColor();
			}

			throw new ArgumentOutOfRangeException(nameof(type), "Can't find corresponding color for notification type - " + type);
		}
	}
}
