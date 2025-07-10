using System.Windows.Forms;
using CargoWise.ComponentModel;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ListenForNotificationsBroadcasterTest : TestCase
	{
		public void TestNotifyParents()
		{
			using (var control0 = new DummyControl())
			using (var control1 = new Control())
			using (var control2 = new DummyControl())
			using (var control3 = new Control())
			using (var control4 = new DummyControl())
			{
				control0.Controls.Add(control1);
				control1.Controls.Add(control2);
				control2.Controls.Add(control3);
				control3.Controls.Add(control4);

				NotificationBroadcaster.Instance.BroadcastNotificationsChange(CargoWise.EntityFramework.NotificationType.MessageError, control4);
				AssertEquals(CargoWise.EntityFramework.NotificationType.MessageError, control2.LastNotifiedState);
				AssertEquals(control4, control2.LastNotifiedStateControl);
				AssertEquals(CargoWise.EntityFramework.NotificationType.MessageError, control0.LastNotifiedState);
				AssertEquals(control4, control0.LastNotifiedStateControl);

				NotificationBroadcaster.Instance.BroadcastVisibilityChange(control3);
				AssertEquals(control3, control2.LastNotifiedVisibleChangeControl);
				AssertEquals(control3, control0.LastNotifiedVisibleChangeControl);
			}
		}

		internal class DummyControl : Control, IListenForNotifications
		{
			public Control LastNotifiedStateControl;
			public Control LastNotifiedVisibleChangeControl;
			public INotificationType LastNotifiedState;

			public void NotifyAboutStateOfChildControl(Control control, INotificationType state)
			{
				LastNotifiedStateControl = control;
				LastNotifiedState = state;
			}

			public void NotifyAboutVisibilityChangeOfChildControl(Control control)
			{
				LastNotifiedVisibleChangeControl = control;
			}
		}
	}
}
