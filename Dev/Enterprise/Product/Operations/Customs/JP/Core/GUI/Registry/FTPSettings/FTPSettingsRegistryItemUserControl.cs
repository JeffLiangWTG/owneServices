using System;
using System.Windows.Forms;
using CargoWise.IO;
using Enterprise.Core.Forms;
using Enterprise.Customs.JP.Common;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.GUI
{
	public partial class FTPSettingsRegistryItemUserControl : RegistryZUserControl
	{
		public FTPSettingsRegistryItemUserControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			InFolderTextBox.ReadOnly = readOnly;
			OutFolderTextBox.ReadOnly = readOnly;
			PasswordTextBox.ReadOnly = readOnly;
			PasswordViewButton.ReadOnly = readOnly;
			PortCaclEdit.ReadOnly = readOnly;
			ServerTextBox.ReadOnly = readOnly;
			UserNameTextBox.ReadOnly = readOnly;
			PassiveNoRadioButton.ReadOnly = readOnly;
			PassiveYesRadioButton.ReadOnly = readOnly;
		}

		void FTPStatusTestButton_Click(object sender, EventArgs e)
		{
			var bizo = DataSource as FTPSettings;
			var server = bizo.Server;
			var userName = bizo.UserName;
			var password = bizo.Password;
			var passive = bizo.Passive;
			var port = bizo.Port;

			try
			{
				var ftpProcessor = new FtpProcessor(server + ":" + port, userName, password, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30), passive);
				ftpProcessor.ListDirectory(@"/");
				bizo.Status = "Valid – Connection to the designated FTP server succeeded.";
			}
			catch
			{
				bizo.Status = "Error – Connection to the designated FTP server failed. Please check the provided information.";
			}
		}

		void ViewButton_Click(object sender, EventArgs e)
		{
			ViewPassword();
		}

		void ViewPassword()
		{
			using (var loginForm = new DeveloperLoginForm())
			{
				if (ZFormModaliser.ShowDialogWithoutDispose(loginForm) == DialogResult.OK)
				{
					Globals.Message.ShowInformation(PasswordTextBox.Text, Res.GetString("116ba7cc-7014-42bb-8aae-b3b01b42a123", "Password"));
				}
			}
		}
	}
}
