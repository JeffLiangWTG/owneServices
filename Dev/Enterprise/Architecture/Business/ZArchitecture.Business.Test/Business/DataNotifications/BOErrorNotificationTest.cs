using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(BOErrorNotification))]
	sealed class BOErrorNotificationTest : NotificationTest<BOErrorNotification>
	{
		public void TestNotify()
		{
			BOErrorNotification testEvent = new BOErrorNotification("Dogs Eat Grass");
			AssertEquals("Error: Dogs Eat Grass", testEvent.Message);
			AssertEquals(ErrorType.Error, testEvent.Type);
		}

		protected override BOErrorNotification NewTestNotification()
		{
			return new BOErrorNotification("Message");
		}
	}
}
