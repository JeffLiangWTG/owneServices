using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
#if DEBUG
#else
using Enterprise.ZArchitecture.GUI;
#endif
using Res = Enterprise.ZArchitecture.GUI.Res;

namespace Enterprise.Core.Forms
{
	public partial class DeveloperLoginForm : KForm
	{
		public DeveloperLoginForm()
		{
			InitializeComponent();
			ErrorReporter.SuppressReportingOfErrors = true;
		}

		public string Password
		{
			get { return PasswordTextBox.Text; }
		}

		public bool IsValidPassword
		{
			get
			{
				return CWSupportLoginToken.IsValidToken(Password);
			}
		}

		public void ShowIncorrectPasswordMessage()
		{
			Globals.Message.ShowInformation(Res.GetString("e32376a1-b997-48bd-89ed-36b65561e8fa", "Incorrect Developer Password"), Res.GetString("f57ce6d6-4c6d-436c-abf0-f702c94bb477", "Invalid Password"));
		}

		public static bool TryAuthenticate()
		{
#if DEBUG // SuppressCodeSmell Reason = No need to ask developer's password when it's Debug build
			return true;
#else
			if (ZFilterStripCommonControl.DevloperHasLoggedInOnce || EnvProxy.Instance.CurrentUser.IsDeveloperLogin)
			{
				return true;
			}

			using (DeveloperLoginForm form = new DeveloperLoginForm())
			{
				if (ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK)
				{
					if (form.IsValidPassword)
					{
						ZFilterStripCommonControl.DevloperHasLoggedInOnce = true;
						return true;
					} else {
						form.ShowIncorrectPasswordMessage();
					}
				}
				return false;
			}
#endif
		}

		void OKButton_Click(object sender, System.EventArgs e)
		{
			this.DialogResult = DialogResult.OK;
		}

		void CloseButton_Click(object sender, System.EventArgs e)
		{
			ErrorReporter.SuppressReportingOfErrors = false;
			Close();
		}
	}
}
