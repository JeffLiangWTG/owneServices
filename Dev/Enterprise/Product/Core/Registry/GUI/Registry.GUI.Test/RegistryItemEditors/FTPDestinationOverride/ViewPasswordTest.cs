using System.Reflection;
using System.Windows.Forms;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	sealed class ViewPasswordTest : TestCase
	{
		public void TestPasswordCharAndCharacterCasing()
		{
			using (var control = new FTPDestinationOverrideUserControl())
			{
				AssertEquals("passwordTextBox.PasswordChar", '*', control.passwordTextBox.PasswordChar);
				AssertEquals("passwordTextBox.CharacterCasing", CharacterCasing.Normal, control.passwordTextBox.CharacterCasing);
			}
		}

		public void TestViewPassword()
		{
			using (var control = new DummyFTPDestinationOverrideUserControl())
			{
				control.Show();
				control.passwordTextBox.Text = "ThisIsThePassword";

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
				control.viewButton.PerformClick();
				AssertNull("No message should be shown.", UnitTestUserNotification.Instance.LastMessage.Text);

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				control.LoginPassword = "IAmBrett";
				control.viewButton.PerformClick();
				AssertEquals("An error message should be shown.", "Incorrect Developer Password", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				control.LoginPassword = User.MasterPassword;
				control.viewButton.PerformClick();
				AssertEquals("The password should be displayed.", "ThisIsThePassword", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#region class DummyFTPDestinationOverrideUserControl

		class DummyFTPDestinationOverrideUserControl : FTPDestinationOverrideUserControl
		{
			protected override bool IsValidPassword(DeveloperLoginForm loginForm)
			{
				((ZTextBox)typeof(DeveloperLoginForm).GetField("PasswordTextBox", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(loginForm)).Text = LoginPassword;
				return base.IsValidPassword(loginForm);
			}

			public string LoginPassword;
		}

		#endregion
	}
}
