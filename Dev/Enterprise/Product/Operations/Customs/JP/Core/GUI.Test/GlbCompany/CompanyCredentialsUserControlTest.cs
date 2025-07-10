using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing
{
	[TestedType(typeof(CompanyCredentialsUserControl))]
	sealed class CompanyCredentialsUserControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			using var control = new CompanyCredentialsUserControl();
			Assert("NaccsMailboxGroupBox", control.FindSingle<ZGroupBox>("NaccsMailboxGroupBox").Visible);
			Assert("NaccsMailboxPanel", control.FindSingle<ZPanel>("NaccsMailboxPanel").Visible);
			Assert("MailboxTextBox", control.FindSingle<ZLabel>("MailboxDomainLabel").Visible);
			Assert("MailboxTextBox", control.FindSingle<ZTextBox>("MailboxTextBox").Visible);
			Assert("PasswordTextbox", control.FindSingle<ZTextBox>("PasswordTextbox").Visible);
			Assert("HasReceivedCheckBox", control.FindSingle<ZCheckBox>("HasReceivedCheckBox").Visible);
			Assert("StatusDropEdit", control.FindSingle<ZDropEdit>("StatusDropEdit").Visible);
		}
	}
}
