using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(NewlineNotification))]
	sealed class NewlineNotificationTest : NotificationTest<NewlineNotification>
	{
		public void TestDisplayMessageBlank()
		{
			NewlineNotification notification = new NewlineNotification();
			AssertEquals("DisplayMessage should be blank so a newline is created", "", notification.Message);
		}

		public void TestMultiLineDisplayMessage_ANewline()
		{
			NewlineNotification notification = new NewlineNotification();
			AssertEquals("MultiLineDisplayMessage should be a newline", "\r\n", notification.MultiLineDisplayMessage);
		}

		protected override NewlineNotification NewTestNotification()
		{
			return new NewlineNotification();
		}
	}
}
