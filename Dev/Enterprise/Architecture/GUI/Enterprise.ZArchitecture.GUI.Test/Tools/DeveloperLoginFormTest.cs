using CargoWise.Common;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Core.Forms
{
	class DeveloperLoginFormTest : NUnit.Framework.TestCase
	{
		public void TestIsValidPassword()
		{
			using (var loginForm = new DeveloperLoginForm())
			{
				loginForm.Show();
				loginForm.PasswordTextBox.Text = CWSupportLoginToken.TokenForTest;
				AssertEquals("IsValidPassword", true, loginForm.IsValidPassword);
				loginForm.PasswordTextBox.Text = "blah";
				AssertEquals("IsValidPassword", false, loginForm.IsValidPassword);
			}

			ErrorReporter.SuppressReportingOfErrors = false;
		}

		public void TestShowIncorrectPasswordMessage()
		{
			using (var loginForm = new DeveloperLoginForm())
			{
				AssertNull("Precondition: No messages should be shown yet.", UnitTestUserNotification.Instance.LastMessage.Text);
				loginForm.ShowIncorrectPasswordMessage();
				AssertEquals("A message should be shown.", "Incorrect Developer Password", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			ErrorReporter.SuppressReportingOfErrors = false;
		}
	}
}
