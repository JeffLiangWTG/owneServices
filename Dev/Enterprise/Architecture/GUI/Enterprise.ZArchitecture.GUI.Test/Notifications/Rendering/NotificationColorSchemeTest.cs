using System;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Notifications.Testing
{
	sealed class NotificationColorSchemeTest : TestCase
	{
		public void TestGetColorThrowsAnExceptionForUnmappedNotificationType()
		{
			AssertExceptionThrown(typeof(ArgumentOutOfRangeException), delegate { NotificationColorScheme.GetColor(null); });
		}

		public void TestGetColorReturnMappedColorOfNotificationType()
		{
			Assert(NotificationColorScheme.GetColor(CargoWise.EntityFramework.NotificationType.Warning) == NotificationColorScheme.WarningColor);
			Assert(NotificationColorScheme.GetColor(CargoWise.EntityFramework.NotificationType.MessageError) == NotificationColorScheme.MessageErrorColor);
			Assert(NotificationColorScheme.GetColor(CargoWise.EntityFramework.NotificationType.Error) == NotificationColorScheme.ErrorColor);
		}

		public void TestGetFontColorReturnMappedColorOfNotificationType()
		{
			Assert(NotificationColorScheme.GetFontColor(CargoWise.EntityFramework.NotificationType.Warning) == NotificationColorScheme.WarningFontColor);
			Assert(NotificationColorScheme.GetFontColor(CargoWise.EntityFramework.NotificationType.MessageError) == NotificationColorScheme.MessageErrorFontColor);
			Assert(NotificationColorScheme.GetFontColor(CargoWise.EntityFramework.NotificationType.Error) == NotificationColorScheme.ErrorFontColor);
		}
	}
}
