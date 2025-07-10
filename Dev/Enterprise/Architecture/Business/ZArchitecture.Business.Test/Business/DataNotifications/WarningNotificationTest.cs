using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class WarningNotificationTest : TestCase
	{
		public void TestDisplayMessage()
		{
			WarningNotification notification = new WarningNotification("Cow goes Moo Moo");
			AssertEquals(WarningType.Warning, notification.Type);
			AssertEquals("Warning: Cow goes Moo Moo", notification.Message);
		}

		#region Inherited Tests

		[TestedType(typeof(WarningNotification))]
		sealed class InheritedTest1 : NotificationTest<WarningNotification>
		{
			protected override WarningNotification NewTestNotification()
			{
				return new WarningNotification("AdditionalInfo");
			}
		}

		[TestedType(typeof(WarningNotification))]
		sealed class InheritedTest2 : NotificationTest<WarningNotification>
		{
			protected override WarningNotification NewTestNotification()
			{
				return new WarningNotification(WarningType.MaxLengthExceeded, "AdditionalInfo");
			}
		}

		#endregion
	}
}
