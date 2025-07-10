using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.GUI.Scanning
{
	[ToolboxItem(false)]
	public partial class ScanMessageUserControl : ZUserControl
	{
		public ScanMessageUserControl()
		{
			InitializeComponent();
			CaptionRenderingEnabled = true;
			Dock = DockStyle.Bottom;
			Visible = false;
		}

		#region Show / Hide Message

		public void ShowMessage(ZString message, NotificationTypes notifyType, bool showOkButton = true)
		{
			if (notifyType == NotificationTypes.Error || notifyType == NotificationTypes.MessageError)
			{
				MsgLabel.ForeColor = Color.Red;
				MsgLabel.BackColor = Color.MistyRose;
				MsgLabelInnerPanel.BackColor = Color.MistyRose;
				MsgLabelOuterPanel.BackColor = Color.Red;

				ScanningManager.PlayScanErrorSound();
			}
			else
			{
				var darkBlue = Color.FromArgb(30, 30, 160);
				var lightBlue = Color.FromArgb(235, 235, 255);
				MsgLabel.ForeColor = darkBlue;
				MsgLabel.BackColor = lightBlue;
				MsgLabelInnerPanel.BackColor = lightBlue;
				MsgLabelOuterPanel.BackColor = darkBlue;
			}

			HideMessageButton.Visible = showOkButton;
			MsgLabel.Text = message;
			Visible = true;

			if (showOkButton)
			{
				HideMessageButton.Focus();
			}
		}

		public void HideMessage()
		{
			Visible = false;
		}

		void HideMessageButton_Click(object sender, EventArgs e)
		{
			HideMessage();
		}

		#endregion
	}
}
