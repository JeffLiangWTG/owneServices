using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Startup.Testing
{
	sealed class LoginForUpgradeFormTest : TransactionedTestCase
	{
		[RequiresSTA]
		public void TestIsDocManagerUpgrade()
		{
			int normalFormHeight = 0;
			int normalWarningLabelHeight = 0;

			using (LoginForUpgradeForm testForm = new LoginForUpgradeForm("", false, "Odyssey"))
			{
				testForm.Show();
				normalFormHeight = testForm.Height;
				normalWarningLabelHeight = testForm.WarningLabel.Height;
				Assert("Warning label text should not have warning in it. Text: " + testForm.WarningLabel, testForm.WarningLabel.Text.IndexOf("WARNING") == -1);
			}

			using (LoginForUpgradeForm testForm = new LoginForUpgradeForm("", true, "Odyssey"))
			{
				testForm.Show();
				Assert("Warning Label text should have warning in it. Text: " + testForm.WarningLabel, testForm.WarningLabel.Text.IndexOf("WARNING") != -1);
				AssertEquals("Form height should be larger when the warning is visible", normalFormHeight + ControlDpiScalingHelper.ScaleToCurrentDpiY(80), testForm.Height);
				AssertEquals("Warning label should be larger when the warning is visible", normalWarningLabelHeight + ControlDpiScalingHelper.ScaleToCurrentDpiY(80), testForm.WarningLabel.Height);
			}
		}

		public void TestPasswordTextBox()
		{
			using (LoginForUpgradeForm testLoginForUpgradeForm = new LoginForUpgradeForm("", false, ""))
			{
				testLoginForUpgradeForm.Show();
				AssertEquals("Password Character", '*', testLoginForUpgradeForm.PasswordTextBox.PasswordChar);
			}
		}

		public void TestControlsOnForm()
		{
			using (LoginForUpgradeForm testLoginForUpgradeForm = new LoginForUpgradeForm("", false, ""))
			{
				testLoginForUpgradeForm.Show();

				try
				{
					Assert("DB Upgrade Form must be a ZChildForm", testLoginForUpgradeForm is ZChildForm);
					AssertEquals("Group box tab index", 0, testLoginForUpgradeForm.LoginGroupBox.TabIndex);
					AssertEquals("Username tab index", 1, testLoginForUpgradeForm.UserNameTextBox.TabIndex);
					AssertEquals("Password tab index", 3, testLoginForUpgradeForm.PasswordTextBox.TabIndex);
					AssertEquals("Ok button tab index", 1, testLoginForUpgradeForm.OKButton.TabIndex);
					AssertEquals("Cancel button tab index", 2, testLoginForUpgradeForm.Cancel_Button.TabIndex);
					AssertEquals("UserName box should have focus", true, testLoginForUpgradeForm.UserNameTextBox.Focused);
					AssertEquals("Cancel DialogResult", DialogResult.Cancel, testLoginForUpgradeForm.Cancel_Button.DialogResult);
					AssertEquals("OK DialogResult", DialogResult.None, testLoginForUpgradeForm.OKButton.DialogResult);
					AssertEquals("Accept button should be OK button", testLoginForUpgradeForm.OKButton, testLoginForUpgradeForm.AcceptButton);
				}
				finally
				{
					testLoginForUpgradeForm.Close();
				}
			}
		}
	}
}
