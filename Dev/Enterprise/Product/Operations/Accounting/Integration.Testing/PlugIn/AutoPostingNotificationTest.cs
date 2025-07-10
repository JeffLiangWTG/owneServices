using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Accounting.Integration.Testing
{
	class AutoPostingNotificationTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			var notification = new AutoPostingNotification(System.Array.Empty<ZGuid>(), false);
			AssertEquals(0, notification.EmailRecipients.Length);
			AssertEquals(false, notification.SuppressUnpostARNotificationWhenAPPosted);

			notification = new AutoPostingNotification(new ZGuid[] { ZGuid.NewZGuid(), ZGuid.NewZGuid() }, true);
			AssertEquals(2, notification.EmailRecipients.Length);
			AssertEquals(true, notification.SuppressUnpostARNotificationWhenAPPosted);
		}
	}
}
