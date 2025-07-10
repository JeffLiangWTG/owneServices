#define SuppressResourceStringsCheckRegion

using System;
using System.Windows.Forms;

namespace Enterprise.LogShipping.Setup.GUI
{
	public partial class InitializeSecondaryDbControl : UserAreaControl
	{
		public InitializeSecondaryDbControl()
		{
			InitializeComponent();
		}

		public override object Value
		{
			get
			{
				DatabaseInitialisationOptions result = DatabaseInitialisationOptions.None;

				if (ParentForm != null && !ParentForm.Navigator.SetupInfo.SecondaryDatabasesExist() || chooseBackupRadioButton.Checked)
				{
					result |= DatabaseInitialisationOptions.Reinitialise;
				}

				if (GenerateScriptCheckBox.Checked)
				{
					result |= DatabaseInitialisationOptions.OnlyGenerateScript;
				}

				return result;
			}
		}

		#region Controls to Enable/Disable

		protected override Control[] ControlsToEnableDisable
		{
			get
			{
				return controlsToEnableDisable = controlsToEnableDisable ??
					new Control[] { matchBackupsLinkLabel, skipStepRadioButton, chooseBackupRadioButton };
			}
		}

		Control[] controlsToEnableDisable;

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1112:Do Not Bind to Enabled property.", Justification = "LogShippingForm doesn't have TabPages")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "LogShippingForm doesn't have TabPages")]
		void InitializeSecondaryDbControl_VisibleChanged(object sender, EventArgs e)
		{
			if (Visible)
			{
				matchBackupsLinkLabel.DataBindings.Clear();
				matchBackupsLinkLabel.Visible = true;

				if (ParentForm != null)
				{
					if (!ParentForm.Navigator.SetupInfo.SecondaryDatabasesExist())
					{
						dbInstalledPanel.Visible = false;
						dbNotInstalledPanel.Visible = true;
						matchBackupsLinkLabel.Enabled = true;
					}
					else
					{
						dbNotInstalledPanel.Visible = false;
						skipStepRadioButton.Text = string.Format("Skip this step and {0}configure Log Shipping for the existing secondary database.", ParentForm.Navigator.SetupInfo.SetupAction == SetupAction.Change ? "re" : "");
						dbInstalledPanel.Visible = true;
						matchBackupsLinkLabel.DataBindings.Add("Enabled", chooseBackupRadioButton, "Checked");
					}

					if (ParentForm != null && ParentForm.NextStep && ParentForm.Navigator.SetupInfo.MainDatabase != null)
					{
						settingsGroupBox.Text = string.Format("Initialize Secondary Database - [{0}]", ParentForm.Navigator.SetupInfo.MainDatabase.SecondaryDatabaseName);
					}
				}
			}
		}

		void matchBackupsLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			if (ParentForm != null)
			{
				using (DatabaseBackupMatchingForm form = new DatabaseBackupMatchingForm(ParentForm.Navigator.SetupInfo, ParentForm.Navigator.ListOfBackups))
				{
					if (form.ShowDialog(this) == DialogResult.OK)
					{
						ParentForm.Navigator.SetupInfo.RestoreDataDirectoryOverride = form.RestoreDataDirectoryOverride;
						ParentForm.Navigator.SetupInfo.RestoreLogDirectoryOverride = form.RestoreLogDirectoryOverride;
					}
				}
			}
		}

		new LogShippingSetupForm ParentForm
		{
			get { return base.ParentForm as LogShippingSetupForm; }
		}
	}
}
