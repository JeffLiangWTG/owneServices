using System.Drawing;
using System.Windows.Forms;

using CargoWise.BrandManager;
using CargoWise.Windows.UI;

using Enterprise.Environment;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Startup
{
	public partial class LoginForResolveLockoutForm : KForm
	{
		public LoginForResolveLockoutForm()
		{
			InitializeComponent();

			if (!this.IsDesignMode())
			{
				EnterpriseLogoPictureBox.Image = BrandingFactory.Instance.ProductLogo;
				Icon = BrandingFactory.Instance.ProductIcon;

				DetailsLabel.Text = string.Format(DetailsLabel.Text, BrandingFactory.Instance.ProductName);
			}
		}

		#region Implementation

		void LoginForResolveLockoutForm_Load(object sender, System.EventArgs e)
		{
			PasswordTextBox.Text = "";

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
				MessageLabel.Text = errorMessage;
				MessageLabel.ForeColor = Color.Red;
			}
		}

		public enum ResolveLockoutAction
		{
			ResetLockout,
			KeepWithUpgrade
		}

		public ResolveLockoutAction SelectedAction
		{
			get
			{
				return radioUpgrade.Checked ? ResolveLockoutAction.KeepWithUpgrade : ResolveLockoutAction.ResetLockout;
			}
		}

		#endregion
	}
}
