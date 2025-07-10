using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class NotificationForCoreFunctionalityTest : TestCase
	{
		public void TestType()
		{
			var info = new NotificationSubscriberNotificationForTest(NotificationSubscriberType.VerboseInfo, "test123");
			AssertEquals(NotificationSubscriberType.VerboseInfo, info.Type);
			AssertEquals("test123", info.Message);
			AssertEquals(info.Message, info.Message);

			info = new NotificationSubscriberNotificationForTest(NotificationSubscriberType.Info, "test12345");
			AssertEquals(NotificationSubscriberType.Info, info.Type);
			AssertEquals("test12345", info.Message);
		}

		public void TestDefaultDisplayMessage()
		{
			var info = new NotificationSubscriberNotificationForTest(NotificationSubscriberType.VerboseInfo, "AdditionalInfo");
			AssertEquals("Default DisplayMessage should come from AdditionalInfo", "AdditionalInfo", info.Message);
		}

		public void TestMultiLineDisplayMessage()
		{
			var info = new NotificationSubscriberWithDisplayMessageForTest(NotificationSubscriberType.VerboseInfo, "AdditionalInfo");
			AssertEquals("MultiLineDisplayMessage should come from the DisplayMessage", "OverriddenDisplayMessage", info.MultiLineDisplayMessage);
		}

		public void TestBlankMessageDoesNotThrowsSilentException()
		{
			var notification = new NotificationSubscriberWithBlankDisplayMessageForTest();
			AssertEquals("Should be no exception initially for the test", 0, Enterprise.ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance.Count);

			notification.PublicAllowBlankDisplayMessage = false;
			string notUsed = notification.Message;
			AssertEquals("Should be 0 exception as empty display messages are always allowed", 0, Enterprise.ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance.Count);
			Enterprise.ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance.Clear();

			notUsed = notification.Message;
			notification.PublicAllowBlankDisplayMessage = true;
			AssertEquals("Should be no exception now as blank is allowed for this notification type", 0, Enterprise.ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance.Count);
		}
	}
}
