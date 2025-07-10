using Enterprise.MasterFiles.Business;
using Enterprise.Security.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	public class LoginForWithTwoCredentialTest : TransactionedTestCase
	{
		public void TestInvalidTwoLogin()
		{
			using (LoginFormWithTwoCredentialSupportBranchDepartmentLevel testForm = new LoginFormWithTwoCredentialSupportBranchDepartmentLevel(null, null, null, null, null, null))
			{
				testForm.Show();

				testForm.LoginTextBox_ForTestOnly.Text = "username1";
				testForm.PasswordTextBox_ForTestOnly.Text = "password1";

				testForm.LoginTextBox2.Text = "username2";
				testForm.PasswordTextBox2.Text = "password2";

				testForm.OKButton_ForTestOnly.PerformClick();

				AssertNull(testForm.Credentials.UserSecurity);
				AssertNull(testForm.Credentials2.UserSecurity);
			}
		}

		public void TestValidTwoLogin()
		{
			SecurityTestObject.CreateTestUser(true, "", "us1", "username1", "password1");
			SecurityTestObject.CreateTestUser(true, "", "us2", "username2", "password2");

			using (LoginFormWithTwoCredentialSupportBranchDepartmentLevel testForm = new LoginFormWithTwoCredentialSupportBranchDepartmentLevel(null, null, null, null, null, null))
			{
				testForm.Show();

				testForm.LoginTextBox_ForTestOnly.Text = "username1";
				testForm.PasswordTextBox_ForTestOnly.Text = "password1";

				testForm.LoginTextBox2.Text = "username2";
				testForm.PasswordTextBox2.Text = "password2";

				testForm.OKButton_ForTestOnly.PerformClick();

				AssertNotNull(testForm.Credentials.UserSecurity);
				AssertNotNull(testForm.Credentials2.UserSecurity);
			}
		}

		public void TestPopulateFirstCredentialWithCurrentStaff()
		{
			using (LoginFormWithTwoCredentialSupportBranchDepartmentLevel testForm = new LoginFormWithTwoCredentialSupportBranchDepartmentLevel(null, null, null, null, null, null))
			{
				testForm.Show();
				testForm.PopulateFirstCredentialWithCurrentStaff();
				AssertEquals(GlbStaff.CurrentUser.GS_LoginName, testForm.LoginTextBox_ForTestOnly.Text);
				AssertEquals("*********", testForm.PasswordTextBox_ForTestOnly.Text);
			}
		}

		public void TestErrorWhenSameUserEntered()
		{
			using (LoginFormWithTwoCredentialSupportBranchDepartmentLevel testForm = new LoginFormWithTwoCredentialSupportBranchDepartmentLevel(null, null, null, null, null, null))
			{
				testForm.Show();

				testForm.LoginTextBox_ForTestOnly.Text = "username1";
				testForm.PasswordTextBox_ForTestOnly.Text = "password1";

				testForm.LoginTextBox2.Text = "username1";
				testForm.PasswordTextBox2.Text = "password1";

				testForm.OKButton_ForTestOnly.PerformClick();
				AssertEquals("User 1 and user 2 cannot be the same user.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestPasswordTextBoxCharacterCasing()
		{
			using (LoginFormWithTwoCredentialSupportBranchDepartmentLevel testForm = new LoginFormWithTwoCredentialSupportBranchDepartmentLevel(null, null, null, null, null, null))
			{
				AssertEquals(System.Windows.Forms.CharacterCasing.Normal, testForm.PasswordTextBox_ForTestOnly.CharacterCasing);
				AssertEquals(System.Windows.Forms.CharacterCasing.Normal, testForm.PasswordTextBox2.CharacterCasing);
			}
		}
	}
}
