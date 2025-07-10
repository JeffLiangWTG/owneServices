using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.Testing
{
	public class LoginFormWithRequestAndNoCredentialsBasherTest : LoginFormWithRequestBasherTest
	{
		public void TestControlsVisibilty()
		{
			using (LoginFormWithRequestAndNoCredentials form = new LoginFormWithRequestAndNoCredentials())
			{
				form.Show();
				var okButton = (ZButton)form.Controls.Find("OKButton", true)[0];
				AssertNotNull(okButton);
				AssertEquals(false, okButton.Visible);
				var approvalRequestButton = (ZButton)form.Controls.Find("ApprovalRequestButton", true)[0];
				AssertNotNull(approvalRequestButton);
				AssertEquals(true, approvalRequestButton.Visible);
				var loginTextBox = (ZTextBox)form.Controls.Find("LoginTextBox", true)[0];
				AssertNotNull(loginTextBox);
				AssertEquals(false, loginTextBox.Visible);
				var passwordTextBox = (ZTextBox)form.Controls.Find("PasswordTextBox", true)[0];
				AssertNotNull(passwordTextBox);
				AssertEquals(false, passwordTextBox.Visible);
			}
		}
	}
}
