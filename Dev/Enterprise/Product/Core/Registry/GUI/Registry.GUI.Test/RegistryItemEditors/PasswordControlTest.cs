using System.Windows.Forms;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	sealed class PasswordControlTest : NUnit.Framework.TestCase
	{
		public void TestPasswordCharAndCharacterCasing()
		{
			using (PasswordControl control = new PasswordControl())
			{
				AssertEquals("PasswordTextBox.PasswordChar", '*', control.PasswordTextBox.PasswordChar);
				AssertEquals("PasswordTextBox.CharacterCasing", CharacterCasing.Normal, control.PasswordTextBox.CharacterCasing);
			}
		}

		[RequiresSTA]
		public void TestViewPassword()
		{
			using (DummyPasswordControl control = new DummyPasswordControl())
			{
				control.Show();
				control.PasswordTextBox.Text = "ThisIsThePassword";

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
				control.ViewButton.PerformClick();
				AssertNull("No message should be shown.", UnitTestUserNotification.Instance.LastMessage.Text);

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				control.LoginPassword = "IAmBrett";
				control.ViewButton.PerformClick();
				AssertEquals("An error message should be shown.", "Incorrect Developer Password", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				control.LoginPassword = User.MasterPassword;
				control.ViewButton.PerformClick();
				AssertEquals("The password should be displayed.", "ThisIsThePassword", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestText()
		{
			using (PasswordControl control = new PasswordControl())
			{
				control.Text = "This is the text!";
				AssertEquals("Text", "This is the text!", control.Text);
				AssertEquals("PasswordTextBox.Text", "This is the text!", control.PasswordTextBox.Text);
			}
		}

		public void TestReadOnly()
		{
			using (PasswordControl control = new PasswordControl())
			{
				control.ReadOnly = true;
				AssertEquals("ReadOnly", true, control.ReadOnly);
				AssertEquals("PasswordTextBox.ReadOnly", true, control.PasswordTextBox.ReadOnly);

				control.ReadOnly = false;
				AssertEquals("ReadOnly", false, control.ReadOnly);
				AssertEquals("PasswordTextBox.ReadOnly", false, control.PasswordTextBox.ReadOnly);

				control.ReadOnly = true;
				AssertEquals("ReadOnly", true, control.ReadOnly);
				AssertEquals("PasswordTextBox.ReadOnly", true, control.PasswordTextBox.ReadOnly);
			}
		}

		public void TestMaxLength()
		{
			using (PasswordControl control = new PasswordControl())
			{
				Assert("Precondition: PasswordTextBox.MaxLength should not be 1234.", control.PasswordTextBox.MaxLength != 1234);
				control.MaxLength = 1234;
				AssertEquals("MaxLength", 1234, control.MaxLength);
				AssertEquals("PasswordTextBox.MaxLength", 1234, control.PasswordTextBox.MaxLength);
			}
		}
	}
}
