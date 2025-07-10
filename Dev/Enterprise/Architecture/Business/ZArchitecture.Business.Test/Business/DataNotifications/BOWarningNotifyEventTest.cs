using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(BOWarningNotification))]
	sealed class BOWarningNotifyEventTest : NotificationTest<BOWarningNotification>
	{
		public void TestNotify()
		{
			BOWarningNotification testEvent = new BOWarningNotification("Dogs Eat Grass");
			AssertEquals("Warning: Dogs Eat Grass", testEvent.Message);
			AssertEquals(WarningType.Warning, testEvent.Type);
		}

		protected override BOWarningNotification NewTestNotification()
		{
			return new BOWarningNotification("Message");
		}
	}
}
