using System;
using System.Reflection;
using System.Windows.Forms;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	sealed class BinaryKeyControlTest : TestCase
	{
		public void TestSetValueSetsHexKeyTextBoxText()
		{
			using (var control = new BinaryKeyControl(64))
			{
				control.hexadecimalKeyTextBox.Text = string.Empty;
				control.Value = "ABC123";
				AssertEquals("ABC123", control.hexadecimalKeyTextBox.Text);
			}
		}

		public void TestGetValueGetsHexKeyTextBoxText()
		{
			using (var control = new BinaryKeyControl(64))
			{
				control.hexadecimalKeyTextBox.Text = "ABC456";
				AssertEquals("ABC456", control.Value);
			}
		}

		public void TestKeyGeneration()
		{
			using (var control = new BinaryKeyControl(64))
			{
				control.hexadecimalKeyTextBox.Text = string.Empty;
				control.passphraseForKeyGenerationTextBox.Text = "TEST PHRASE";
				control.GenerateKeyButton_Click_Internal(control.generateKeyButton, EventArgs.Empty);
				AssertEquals("Generated Key", "547C89AD1019E2224CA9632E1689C9C015018828A9961EDB3E41E48B98926859968A5D1F5F6B64E73291962CB669CA3BA9664560AF8240EAD06DB86C0891ED89", control.hexadecimalKeyTextBox.Text);
			}
		}

		public void TestKeyGenerationOverwrites()
		{
			using (var control = new BinaryKeyControl(64))
			{
				control.hexadecimalKeyTextBox.Text = "There is already a key in here";
				control.passphraseForKeyGenerationTextBox.Text = "TEST PHRASE";
				control.GenerateKeyButton_Click_Internal(control.generateKeyButton, EventArgs.Empty);
				AssertEquals("Generated Key", "547C89AD1019E2224CA9632E1689C9C015018828A9961EDB3E41E48B98926859968A5D1F5F6B64E73291962CB669CA3BA9664560AF8240EAD06DB86C0891ED89", control.hexadecimalKeyTextBox.Text);
			}
		}

		public void TestPasswordCharAndCharacterCasing()
		{
			using (var control = new BinaryKeyControl(64))
			{
				AssertEquals("hexadecimalKeyTextBox.PasswordChar", '*', control.hexadecimalKeyTextBox.PasswordChar);
				AssertEquals("hexadecimalKeyTextBox.CharacterCasing", CharacterCasing.Upper, control.hexadecimalKeyTextBox.CharacterCasing);
			}
		}

		public void TestViewPassword()
		{
			using (var control = new BinaryKeyControlForTest(64))
			{
				control.Show();
				control.hexadecimalKeyTextBox.Text = "THISISTHEPASSWORD";

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
				control.viewKeyButton.PerformClick();
				AssertNull("No message should be shown.", UnitTestUserNotification.Instance.LastMessage.Text);

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				control.LoginPassword = "IAmTest";
				control.viewKeyButton.PerformClick();
				AssertEquals("An error message should be shown.", "Incorrect Developer Password", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				control.LoginPassword = User.MasterPassword;
				control.viewKeyButton.PerformClick();
				AssertEquals("The password should be displayed.", "THISISTHEPASSWORD", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		void KeyGenerationTestShouldNotGenerateWithInput(string input)
		{
			using (var control = new BinaryKeyControl(64))
			{
				control.hexadecimalKeyTextBox.Text = string.Empty;
				control.passphraseForKeyGenerationTextBox.Text = input;
				control.GenerateKeyButton_Click_Internal(control.generateKeyButton, EventArgs.Empty);
				AssertEquals("Should not generate key", string.Empty, control.hexadecimalKeyTextBox.Text);

				var lastMessage = UnitTestUserNotification.Instance.LastMessage;
				AssertEquals("Please enter a passphrase to use for key generation.", lastMessage.Text);
				AssertEquals("Invalid Input", lastMessage.Caption);
				AssertEquals(true, lastMessage.WasError);
			}
		}

		public void TestKeyGeneration_Blank()
		{
			KeyGenerationTestShouldNotGenerateWithInput(string.Empty);
		}

		public void TestKeyGeneration_Whitespace()
		{
			KeyGenerationTestShouldNotGenerateWithInput(@"    

				");
		}

		#region class BinaryKeyControlForTest

		class BinaryKeyControlForTest : BinaryKeyControl
		{
			protected override bool IsValidPassword(DeveloperLoginForm loginForm)
			{
				((ZTextBox)typeof(DeveloperLoginForm).GetField("PasswordTextBox", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(loginForm)).Text = LoginPassword;
				return base.IsValidPassword(loginForm);
			}

			public string LoginPassword;

			public BinaryKeyControlForTest(int keySize) : base(keySize)
			{
			}
		}

		#endregion
	}
}
