using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public abstract class SetVisibleOnControlUpdatesNotificationsTestCase : TestCaseWithDummy
	{
		protected abstract Control GetNewControl();

		public void TestNotificationOfVisibilityChange()
		{
			using (var parentControl = new ListenForNotificationsBroadcasterTest.DummyControl())
			using (var control = GetNewControl())
			{
				parentControl.Controls.Add(control);
				parentControl.LastNotifiedVisibleChangeControl = null;
				control.Visible = false;
				AssertEquals(control, parentControl.LastNotifiedVisibleChangeControl);
				parentControl.LastNotifiedVisibleChangeControl = null;
				control.Visible = true;
				AssertEquals(control, parentControl.LastNotifiedVisibleChangeControl);
			}
		}
	}
}
