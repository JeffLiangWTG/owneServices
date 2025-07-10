using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(InfoNotification))]
	sealed class InfoNotificationTest : NotificationTest<InfoNotification>
	{
		public void TestInitialise()
		{
			InfoNotification notification = new InfoNotification("Test");
			AssertEquals(NotificationSubscriberType.Info, notification.Type);
			AssertEquals("Test", notification.Message);
		}

		public void TestSilentExceptionReportedWhenMessageBlank()
		{
			AssertEquals("Should be no exception initially for the test", 0, Enterprise.ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance.Count);
			new InfoNotification("");
			AssertEquals("Should be 1 exception now due to empty display message", 1, Enterprise.ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance.Count);
			Enterprise.ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance.Clear();
		}

		protected override InfoNotification NewTestNotification()
		{
			return new InfoNotification("Message");
		}
	}
}
