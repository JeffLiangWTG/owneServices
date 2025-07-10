using Enterprise.Customs.Business.MessageManagers;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class UserNotificationWrapperTest : TestCase
	{
		public void TestIUserNotificationMembers()
		{
			IUserNotification notificationWrapper = new UserNotificationWrapper(new NotificationBuffer());
			AssertEquals(true, notificationWrapper.ShowConfirmation("", ""));
			AssertEquals(true, notificationWrapper.ShowConfirmation("", "", "", ""));
		}
	}
}
