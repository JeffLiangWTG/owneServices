using System;
using System.Windows.Forms;

namespace Enterprise.RemotePrinting.Client.Forms
{
	public partial class ServiceAccountDlg : Form
	{
		public ServiceAccountDlg()
		{
			InitializeComponent();
		}

		public string Password { get; set; }

		public string ConfirmedPassword { get; set; }

		public string UserName { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "MessageBox")]
		void ConfirmButton_Click(object sender, EventArgs e)
		{
			UserName = loginTextBox.Text;
			Password = webPrintServicePWD.Text;
			ConfirmedPassword = webPrintServicePWD_Confirmation.Text;
			if (Password != ConfirmedPassword)
			{
				MessageBox.Show("Password does not match", "The Password and Confirm password are not same", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			else
			{
				Close();
			}
		}

		void CancelButton_Click(object sender, EventArgs e)
		{
			UserName = Password = string.Empty;
			Close();
		}
	}
}
