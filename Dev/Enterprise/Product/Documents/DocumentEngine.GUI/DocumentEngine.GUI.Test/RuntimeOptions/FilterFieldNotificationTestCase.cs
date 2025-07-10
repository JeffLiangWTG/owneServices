using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.ZArchitecture.GUI.Balloons;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions.Testing
{
	sealed partial class FilterFieldNotificationTestCase : TestCaseWithFactory
	{
		public void TestBalloon()
		{
			FilterField filterField = new DummyFilterField(Factory);

			using (FilterFieldNotification notification = new FilterFieldNotification(filterField))
			{
				notification.Show();
				Application.DoEvents();

				notification.PerformMouseEnterForTesting();
				BalloonDescriptor descriptor = Balloon.Instance.BalloonWindowExposedForTesting.Descriptor;
				AssertNull("Pre-condition: Balloon should not be showing.", descriptor);
				notification.PerformMouseLeaveForTesting();
				descriptor = Balloon.Instance.BalloonWindowExposedForTesting.Descriptor;
				AssertNull("Pre-condition: Balloon should not be showing.", descriptor);

				filterField.AddRowError("Test Error");
				notification.PerformMouseEnterForTesting();
				descriptor = Balloon.Instance.BalloonWindowExposedForTesting.Descriptor;
				AssertNotNull("Balloon should be showing.", descriptor);
				AssertEquals("notifications: errors count", 1, descriptor.Notifications.GetErrors().Count());
				AssertEquals("notifications: warnings count", 0, descriptor.Notifications.GetWarnings().Count());
				AssertEquals("notifications: error message", "Error - : Test Error", descriptor.Notifications.GetErrors().GetFirst().Message);
				notification.PerformMouseLeaveForTesting();
				descriptor = Balloon.Instance.BalloonWindowExposedForTesting.Descriptor;
				AssertNull("Balloon should not be showing.", descriptor);

				filterField.AddRowWarning("Test Warning");
				notification.PerformMouseEnterForTesting();
				descriptor = Balloon.Instance.BalloonWindowExposedForTesting.Descriptor;
				AssertNotNull("Balloon should be showing.", descriptor);
				AssertEquals("notifications: errors count", 1, descriptor.Notifications.GetErrors().Count());
				AssertEquals("notifications: warnings count", 1, descriptor.Notifications.GetWarnings().Count());
				AssertEquals("notifications: error message", "Error - : Test Error", descriptor.Notifications.GetErrors().GetFirst().Message);
				AssertEquals("notifications: warning message", "Warning - : Test Warning", descriptor.Notifications.GetWarnings().GetFirst().Message);
				notification.PerformMouseLeaveForTesting();
				descriptor = Balloon.Instance.BalloonWindowExposedForTesting.Descriptor;
				AssertNull("Balloon should not be showing.", descriptor);

				filterField.ClearAllNotifications();
				notification.PerformMouseEnterForTesting();
				descriptor = Balloon.Instance.BalloonWindowExposedForTesting.Descriptor;
				AssertNull("Balloon should not be showing.", descriptor);
			}
		}

		#region Implementation

		protected override void TearDown()
		{
			Balloon.Instance.Hide();
			base.TearDown();
		}

		#endregion
	}
}
