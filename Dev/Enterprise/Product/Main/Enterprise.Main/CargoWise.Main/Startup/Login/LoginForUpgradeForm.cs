using System.Drawing;
using System.Windows.Forms;
using CargoWise.BrandManager;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.ZArchitecture.GUI;
using Res = CargoWise.Main.Res;

namespace Enterprise.Startup
{
	public partial class LoginForUpgradeForm : ZChildForm
	{
		public LoginForUpgradeForm(string versionInfo, bool isDocManagerUpgrade, string dbName)
		{
			InitializeComponent();
			VersionChangeInfo = versionInfo;
			IsDocManagerUpgrade = isDocManagerUpgrade;
			MainDbName = dbName;

			if (!this.IsDesignMode())
			{
				EnterpriseLogoPictureBox.Image = BrandingFactory.Instance.ProductLogo;
				Icon = BrandingFactory.Instance.ProductIcon;
			}
		}

		#region Implementation

		readonly string VersionChangeInfo;
		readonly bool IsDocManagerUpgrade;
		readonly string MainDbName;

		void LoginForUpgradeForm_Load(object sender, System.EventArgs e)
		{
			UpgradeMessageLabel.Text = Res.GetString("41122B96-B4C5-48E0-AA6E-972731A0AD7A", "To deploy the new version of {0}, the database must be updated. Please log in as System Administrator to complete the upgrade process, or click Cancel Upgrade to continue using the previous version.", BrandingFactory.Instance.ProductName);
			PasswordTextBox.Text = "";
			VersionChangeInfoLabel.Text = VersionChangeInfo;

			if (IsDocManagerUpgrade)
			{
				ControlDpiScalingHelper.SetHeight(this, Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(80), false);
				ControlDpiScalingHelper.SetHeight(ref WarningLabel, WarningLabel.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(80), false);
				WarningLabel.Text = Res.GetString("8467AFE4-0B61-4A90-BB13-6729FAFE7F4C", @"WARNING, PLEASE NOTE:
Upgrading to this version will take longer than usual. It requires upgrading all the image databases ({0}) in addition to the main database.", MainDbName + "_SDxxx");
			}

			UserNameTextBox.Focus();
		}

		void OKButton_Click(object sender, System.EventArgs e)
		{
			string errorMessage;
			if (Env.LoginController.ValidateAndRegisterUserForUpgrade(UserNameTextBox.Text, PasswordTextBox.Text, out errorMessage))
			{
				this.DialogResult = DialogResult.OK;
			}
			else
			{
				UpgradeMessageLabel.Text = errorMessage;
				UpgradeMessageLabel.ForeColor = Color.Red;
			}
		}

		#endregion
	}
}
