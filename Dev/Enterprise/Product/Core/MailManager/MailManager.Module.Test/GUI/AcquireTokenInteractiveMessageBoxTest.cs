using System.Windows.Forms;
using Enterprise.MailManager.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MailManager.Testing.GUI
{
#if !WINZOR
	public class AcquireTokenInteracyiveMessageBoxTest : ZMessageBoxTest
	{
		public void TestSignButtonClickCheckWebUrlLauncher()
		{
			using (var messageBox = new AcquireTokenInteractiveMessageBox("TestMessage", "TestCode", "https://testWise"))
			{
				WebUrlLauncher.ClearLastUrlLaunched();
				AssertNullOrEmpty("check LastUrlLaunched is empty", WebUrlLauncher.LastUrlLaunched);

				messageBox.Show();
				var signButton = (ZButton)messageBox.Controls.Find("SignButton", true)[0];
				Application.DoEvents();
				signButton.PerformClick();
				
				AssertEquals("check LastUrlLaunched", "https://testWise", WebUrlLauncher.LastUrlLaunched);
			}
		}

		[DeveloperOnlyTest]
		public void TestSignButtonClickCheckCodeIsCopied()
		{
			using (var messageBox = new AcquireTokenInteractiveMessageBox("TestMessage", "TestCode", "https://testWise"))
			{
				SafeClipboard.Clear();
				AssertNullOrEmpty("check Code is empty", SafeClipboard.GetText());

				messageBox.Show();
				var signButton = (ZButton)messageBox.Controls.Find("SignButton", true)[0];
				Application.DoEvents();
				signButton.PerformClick();

				AssertEquals("check SafeClipboard", "TestCode", SafeClipboard.GetText());
			}
		}
	}
#endif
}
