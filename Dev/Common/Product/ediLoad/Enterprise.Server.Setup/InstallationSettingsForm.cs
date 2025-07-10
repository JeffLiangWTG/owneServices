using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using CargoWise.BrandManager;
using CargoWise.Common;

namespace Enterprise.Server.Setup
{
	sealed partial class InstallationSettingsForm : Form
	{
		readonly Dictionary<Button, TextBox> browseButtonMap = new Dictionary<Button, TextBox>();
		readonly SetupConfiguration configuration;

		public InstallationSettingsForm(SetupConfiguration configuration)
		{
			this.configuration = configuration;
			InitializeComponent();
			this.Icon = BrandingFactory.Instance.ProductIcon;
			browseButtonMap[buttonBrowseData] = textBoxData;
			browseButtonMap[buttonBrowseLog] = textBoxLog;
			installationSettingsBindingSource.Add(configuration.InstallationSettings);
		}

		void ButtonBrowse_Click(object sender, EventArgs e)
		{
			TextBox pathBox = browseButtonMap[sender as Button];
			folderBrowserDialog.SelectedPath = pathBox.Text;
			if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
			{
				pathBox.Text = folderBrowserDialog.SelectedPath;
				// Hack.
				// When user sets paths for Log and Data for DB using findbox only - it sets selected path to pathBox control 
				// BUT does not update string bound to pathBox.
				// setting focus (and bringin it back) fixes this bug
				// AlexK.
				pathBox.Focus();
				(sender as Button).Focus();
			}
		}

		void ButtonHelp_Click(object sender, EventArgs e)
		{
			LaunchInstallationGuide();
		}

		void InstallationSettingsForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (DialogResult == DialogResult.OK)
			{
				InstallationSettingsValidator validator = new InstallationSettingsValidator(configuration, this);
				validator.Validate(e);
			}
		}

		void InstallationSettingsForm_HelpRequested(object sender, HelpEventArgs hlpevent)
		{
			LaunchInstallationGuide();
			hlpevent.Handled = true;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1078:Do not use Process.Start to open a file or url, use WebUrlLauncher or FileOpener for proper integration with Remote Desktop Services. False alarm if you are running a process for a reason other than to open a file or url.", Justification = "No Access to Enterprise.ZArchitecture.GUI")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1076:MessageBoxShow", Justification = "Baseline")]
		void LaunchInstallationGuide()
		{
			string pdfFullPath = Path.Combine(configuration.InstallationSettings.CDPath, "CargoWiseOne_Installation_Guide.pdf");
			try
			{
				Process.Start(pdfFullPath);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Win32Exception win32Ex = ex as Win32Exception;
				const int ERROR_NO_ASSOCIATION = 1155;
				if ((win32Ex != null) && (win32Ex.NativeErrorCode == ERROR_NO_ASSOCIATION))
				{
					if (MessageBox.Show("Adobe Acrobat Reader is required to read the Installation Guide.\r\nWould you like to install Adobe Acrobat Reader now?\r\nClicking Yes will take you to the Adobe website.", BrandingFactory.Instance.ProductName + " Setup", MessageBoxButtons.YesNo) == DialogResult.Yes)
					{
						Process.Start("http://get.adobe.com/reader/");
					}
				}
				else
				{
					MessageBox.Show("Setup was unable to start Adobe Acrobat Reader.\r\nPlease open the Installation Guide PDF file from the setup CD.", BrandingFactory.Instance.ProductName + " Setup");
				}
			}
		}

		void checkBoxSQLServerMachineNameUseDefault_CheckedChanged(object sender, EventArgs e)
		{
			if (checkBoxSQLServerMachineNameUseDefault.Checked)
			{
				comboBoxDatabase.Visible = true;
				textBoxUserNominatedInstance.Visible = false;
			}
			else
			{
				comboBoxDatabase.Visible = false;
				textBoxUserNominatedInstance.Visible = true;
			}
		}
	}
}
