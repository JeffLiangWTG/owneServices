#if DEBUG
using NUnit.Framework;

namespace CargoWise.ComponentModel.Testing
{
	public class NotificationTests : TestCase
	{
		public void TestEquals()
		{
			Notification notification1 = new Notification(NotificationType.Error, "Message");
			Notification notification2 = new Notification(NotificationType.Error, "Message");
			Notification notification3 = new Notification(NotificationType.Warning, "Message");
			Notification notification4 = new Notification(NotificationType.Error, "Error");

			AssertEquals("Equals", notification1, notification2);
			AssertNotEquals("Not equals", notification2, notification3);
			AssertNotEquals("Not equals", notification3, notification4);
		}
	}
}
#endif
