using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(ProgressNotification))]
	sealed class ProgressNotificationTest : NotificationTest<ProgressNotification>
	{
		public void TestProgressNotification()
		{
			ProgressNotification progressNotification = new ProgressNotification(10);
			AssertEquals(NotificationSubscriberType.Progress, progressNotification.Type);
			AssertEquals(10, progressNotification.PercentageComplete);
		}

		#region Implementation

		protected override void AssertNotificationsEqual(string message, ProgressNotification lhs, ProgressNotification rhs)
		{
			base.AssertNotificationsEqual(message, lhs, rhs);
			AssertEquals(message + "; PercentageComplete", lhs.PercentageComplete, rhs.PercentageComplete);
		}

		protected override ProgressNotification NewTestNotification()
		{
			return new ProgressNotification(3);
		}

		#endregion
	}
}
