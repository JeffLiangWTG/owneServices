using System.Windows.Forms;
using NUnit.Framework;
using static Enterprise.Startup.LoginForResolveLockoutForm;

namespace Enterprise.Startup.Testing
{
	sealed class LoginForResolveLockoutFormTest : TransactionedTestCase
	{
		public void TestPasswordTextBox()
		{
			using (LoginForResolveLockoutForm testLoginForResolveLockoutForm = new LoginForResolveLockoutForm())
			{
				testLoginForResolveLockoutForm.Show();
				AssertEquals("Password Character", '*', testLoginForResolveLockoutForm.PasswordTextBox.PasswordChar);
				AssertEquals("the password text has a default maxlength as a token might be input.", 32767, testLoginForResolveLockoutForm.PasswordTextBox.MaxLength);
			}
		}

		public void TestControlsOnForm()
		{
			using (LoginForResolveLockoutForm testLoginForResolveLockoutForm = new LoginForResolveLockoutForm())
			{
				testLoginForResolveLockoutForm.Show();

				try
				{
					AssertEquals("Group box tab index", 0, testLoginForResolveLockoutForm.LoginGroupBox.TabIndex);
					AssertEquals("Username label tab index", 0, testLoginForResolveLockoutForm.UserNameLabel.TabIndex);
					AssertEquals("Username tab index", 1, testLoginForResolveLockoutForm.UserNameTextBox.TabIndex);
					AssertEquals("Password label tab index", 2, testLoginForResolveLockoutForm.PasswordLabel.TabIndex);
					AssertEquals("Password tab index", 3, testLoginForResolveLockoutForm.PasswordTextBox.TabIndex);
					AssertEquals("Ok button tab index", 1, testLoginForResolveLockoutForm.OKButton.TabIndex);
					AssertEquals("Cancel button tab index", 2, testLoginForResolveLockoutForm.Cancel_Button.TabIndex);
					AssertEquals("UserName box should have focus", true, testLoginForResolveLockoutForm.UserNameTextBox.Focused);
					AssertEquals("Cancel DialogResult", DialogResult.Cancel, testLoginForResolveLockoutForm.Cancel_Button.DialogResult);
					AssertEquals("OK DialogResult", DialogResult.None, testLoginForResolveLockoutForm.OKButton.DialogResult);
					AssertEquals("Accept button should be OK button", testLoginForResolveLockoutForm.OKButton, testLoginForResolveLockoutForm.AcceptButton);
				}
				finally
				{
					testLoginForResolveLockoutForm.Close();
				}
			}
		}

		public void TestActionOptions()
		{
			using (LoginForResolveLockoutForm testLoginForResolveLockoutForm = new LoginForResolveLockoutForm())
			{
				testLoginForResolveLockoutForm.Show();

				try
				{
					AssertEquals("Reset lockout option should be checked", true, testLoginForResolveLockoutForm.radioResetLockout.Checked);
					AssertEquals("Upgrade option should not be checked", false, testLoginForResolveLockoutForm.radioUpgrade.Checked);
					AssertEquals("SelectedAction should be ResetLockout", ResolveLockoutAction.ResetLockout, testLoginForResolveLockoutForm.SelectedAction);

					testLoginForResolveLockoutForm.radioUpgrade.PerformClick();

					AssertEquals("Reset lockout option should not be checked", false, testLoginForResolveLockoutForm.radioResetLockout.Checked);
					AssertEquals("Upgrade option should be checked", true, testLoginForResolveLockoutForm.radioUpgrade.Checked);
					AssertEquals("SelectedAction should be KeepWithUpgrade", ResolveLockoutAction.KeepWithUpgrade, testLoginForResolveLockoutForm.SelectedAction);
				}
				finally
				{
					testLoginForResolveLockoutForm.Close();
				}
			}
		}
	}
}
