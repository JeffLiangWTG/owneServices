using System.Drawing;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI.Scanning;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Test.Forms.Scanning
{
	public class ScanMessageUserControlTest : TestCase
	{
		#region TestConstruction

		public void TestConstruction()
		{
			using (var form = new ZForm())
			{
				var control = new ScanMessageUserControl();
				form.Controls.Add(control);

				form.Show();
				AssertEquals(false, control.Visible);
				AssertEquals(DockStyle.Bottom, control.Dock);
			}
		}

		#endregion

		#region TestShowMessage

		public void TestShowMessage()
		{
			AssertShowMessage(NotificationTypes.Error, Color.Red, Color.MistyRose);
			AssertShowMessage(NotificationTypes.None, Color.FromArgb(30, 30, 160), Color.FromArgb(235, 235, 255));
		}

		void AssertShowMessage(NotificationTypes notifyType, Color expectedTextColor, Color expectedBackColor)
		{
			using (var form = new ZFormWithScanMsgControl())
			{
				var control = form.Control;

				form.Show();
				AssertEquals("Precondition", false, control.Visible);

				control.ShowMessage("Hefty Smurf", notifyType);
				AssertEquals(true, control.Visible);
				AssertEquals("Hefty Smurf", control.MsgLabel.Text);
				AssertEquals(true, control.HideMessageButton.Visible);
				AssertEquals(true, control.HideMessageButton.Focused);
				// cannot easily test that the sound was played

				AssertEquals("Text colour was incorrect.", expectedTextColor, control.MsgLabel.ForeColor);
				AssertEquals("Back colour was incorrect.", expectedBackColor, control.MsgLabel.BackColor);
				AssertEquals("Button back colour was incorrect.", expectedBackColor, control.MsgLabelInnerPanel.BackColor);
				AssertEquals("Border colour was incorrect.", expectedTextColor, control.MsgLabelOuterPanel.BackColor);

				control.ShowMessage("Hefty Smurf", notifyType, showOkButton: false);
				AssertEquals("The button should be hidden.", false, control.HideMessageButton.Visible);
			}
		}

		#endregion

		#region TestHideMessage

		public void TestHideMessage()
		{
			using (var form = new ZFormWithScanMsgControl())
			{
				var control = form.Control;

				form.Show();
				control.ShowMessage("Hefty Smurf", NotificationTypes.Error);
				AssertEquals("Precondition", true, control.Visible);

				control.HideMessage();
				AssertEquals(false, control.Visible);
			}
		}

		#endregion

		#region TestHideMsgButtonClick

		public void TestHideMsgButtonClick()
		{
			using (var form = new ZFormWithScanMsgControl())
			{
				var control = form.Control;

				form.Show();
				control.ShowMessage("Hefty Smurf", NotificationTypes.Error);
				AssertEquals("Precondition", true, control.Visible);

				control.HideMessageButton.PerformClick();
				AssertEquals(false, control.Visible);
			}
		}

		#endregion

		#region Implementation

		public class ZFormWithScanMsgControl : ZForm
		{
			public ZFormWithScanMsgControl()
			{
				Control = new ScanMessageUserControlForTest();
				Controls.Add(Control);
			}

			public readonly ScanMessageUserControlForTest Control;
		}

		public class ScanMessageUserControlForTest : ScanMessageUserControl
		{
			public new ZButton HideMessageButton
			{
				get { return base.HideMessageButton; }
			}

			public new ZLabel MsgLabel
			{
				get { return base.MsgLabel; }
			}

			public new ZPanel MsgLabelOuterPanel
			{
				get { return base.MsgLabelOuterPanel; }
			}

			public new ZPanel MsgLabelInnerPanel
			{
				get { return base.MsgLabelInnerPanel; }
			}
		}

		#endregion
	}
}
