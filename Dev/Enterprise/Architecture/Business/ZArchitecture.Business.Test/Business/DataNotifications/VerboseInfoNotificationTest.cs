using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(VerboseInfoNotification))]
	sealed class VerboseInfoNotificationTest : NotificationTest<VerboseInfoNotification>
	{
		public void TestInitialise()
		{
			VerboseInfoNotification @event = new VerboseInfoNotification("test");
			AssertEquals("test", @event.Message);
			AssertEquals(NotificationSubscriberType.VerboseInfo, @event.Type);
		}

		#region Implementation

		protected override VerboseInfoNotification NewTestNotification()
		{
			return new VerboseInfoNotification("AdditionalInfo");
		}

		#endregion
	}
}
