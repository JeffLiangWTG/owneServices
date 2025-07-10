using System;
using System.Text;
using System.Windows.Forms;

namespace Enterprise.LogShipping.Setup.GUI
{
	public partial class FinalSetupScreenControl : UserAreaControl
	{
		public FinalSetupScreenControl()
		{
			InitializeComponent();
		}

		public override object Value
		{
			get { return string.Empty; }
		}

		#region Controls to Enable/Disable

		protected override Control[] ControlsToEnableDisable
		{
			get
			{
				return controlsToEnableDisable = controlsToEnableDisable ?? new Control[] { setupButton, dropDbCheckBox };
			}
		}

		Control[] controlsToEnableDisable;

		#endregion

		public void DisableSetup()
		{
			setupButton.Enabled = false;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		void FinalSetupScreenControl_VisibleChanged(object sender, EventArgs e)
		{
			LogShippingSetupForm parentForm = this.ParentForm as LogShippingSetupForm;
			if (this.Visible && parentForm != null && parentForm.NextStep)
			{
				LogShippingInfo info = parentForm.Navigator.SetupInfo;

				StringBuilder builder = new StringBuilder();
				builder.AppendFormat("  Primary server - {0}\r\n", (info.PrimaryServer != null ? info.PrimaryServer.FullInstanceName : string.Empty));
				builder.AppendFormat("  Primary database - {0}\r\n", (info.MainDatabase != null ? info.MainDatabase.DatabaseName : string.Empty));
				builder.AppendFormat("  Secondary server - {0}\r\n", (info.SecondaryServer != null ? info.SecondaryServer.FullInstanceName : string.Empty));
				builder.AppendFormat("  Secondary database - {0}\r\n", (info.MainDatabase != null ? info.MainDatabase.SecondaryDatabaseName : string.Empty));
				builder.AppendFormat("  Source directory - {0}\r\n", info.BackupSourceDirectory);
				builder.AppendFormat("  Local copy directory - {0}", info.BackupLocalCopyDirectory);
				settingsTextBox.Text = builder.ToString();
				setupButton.Text = info.SetupAction.ToString();
				dropDbCheckBox.Visible = info.SetupAction == SetupAction.Remove;

				if (info.SetupAction == SetupAction.Change && !info.ConfigurationChanged)
				{
					setupButton.Enabled = false;
					ShowMessage("You did not change configuration settings, you can not process with the same settings");
				}
				else
				{
					setupButton.Enabled = true;
				}

				label.Text = string.Format("Review Log Shipping settings and click on {0} button to ", info.SetupAction.ToString().ToLower());

				switch (info.SetupAction)
				{
					case SetupAction.Setup:
						label.Text += "start configuration.";
						break;
					case SetupAction.Change:
						label.Text += "reconfigure it.";
						break;
					case SetupAction.Remove:
						label.Text += "start process.";
						break;
				}
			}
		}

		void setupButton_Click(object sender, EventArgs e)
		{
			var parentForm = ParentForm as LogShippingSetupForm;
			if (parentForm != null)
			{
				parentForm.Navigator.SetupInfo.ShouldDropDatabaseAfterLSRemoving = dropDbCheckBox.Checked;
				parentForm.Setup();
			}
		}
	}
}
