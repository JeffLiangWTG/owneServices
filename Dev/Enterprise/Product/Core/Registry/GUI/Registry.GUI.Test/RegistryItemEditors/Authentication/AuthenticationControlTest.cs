using System;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI.Testing
{
	sealed class AuthenticationControlTest : NUnit.Framework.TestCase
	{
		public void TestInvalidUsername()
		{
			using (AuthenticationControl control = new AuthenticationControl(true))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				control.GeneratePasswordButton.PerformClick();
				AssertEquals("An error message should be shown.", Res.GetString("90EA2100-3301-4BA0-9C2E-8D3FD70074EB", "Please enter a username."), UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(UnitTestUserNotification.Instance.LastMessage.WasError);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				control.UserName.Text = "us3r|nam$";
				control.GeneratePasswordButton.PerformClick();
				AssertEquals("An error message should be shown.", Res.GetString("67D7EFF0-AF15-45D8-BC43-4E550DD83B5E", "Username cannot contain special characters. Please use alphanumeric characters only."), UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(UnitTestUserNotification.Instance.LastMessage.WasError);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				control.Value = "CWUSER|";
				control.GeneratePasswordButton.PerformClick();
				AssertNull("No message should be shown.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestPasswordTextBoxAccessPermissions()
		{
			using (AuthenticationControl control = new AuthenticationControl(true))
			{
				control.Value = "CWUSER|TemporaryPassword";

				AssertEquals("Masked", '*', control.Password.PasswordChar);

				UnitTestUserNotification.Instance.AddUserResponse("YES");
				control.GeneratePasswordButton.PerformClick();
				AssertEquals("A warning must be shown.", Res.GetString("37543F9A-259A-49A5-9C4E-8958D1A79F3F", "WARNING: Reset Password - Irreversible Action. Would you like to proceed?"), UnitTestUserNotification.Instance.LastMessage.Text);

				AssertNotNullOrEmpty(control.Password.Text);
				AssertEquals("Unmasked", '\0', control.Password.PasswordChar);

				control.OnRegistryFormSave();

				AssertEquals("Masked", '*', control.Password.PasswordChar);
			}
		}

		public void TestNewPasswordHasNoWarning()
		{
			using (AuthenticationControl control = new AuthenticationControl(true))
			{
				control.Value = "CWUSER|";

				AssertEquals("Masked", '*', control.Password.PasswordChar);

				control.GeneratePasswordButton.PerformClick();

				AssertNotNullOrEmpty(control.Password.Text);
				AssertEquals("Unmasked", '\0', control.Password.PasswordChar);
			}
		}

		public void TestGenerateValidPassword()
		{
			using (AuthenticationControl control = new AuthenticationControl(true))
			{
				var length = control.passwordLength;
				var password = control.GeneratePassword(length);
				AssertEquals(true, IsBase64String(password));
				AssertEquals(false, password.Contains("|"));
			}
		}

		bool IsBase64String(string input)
		{
			try
			{
				Convert.FromBase64String(input);
				return true;
			}
			catch (FormatException)
			{
				return false;
			}
		}
	}
}
