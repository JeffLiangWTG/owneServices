using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Messaging.GUI.Test
{
	public class InboundBasicAuthenticationUserControlTest : TestCaseWithFactory
	{
		[DeveloperOnlyTest]
		public void TestGeneratePassword_CopyToClipboard()
		{
			var auth = Factory.NewWithValidTestData<EDICommunicationAuth>();
			auth.ECA_AuthorizationMode = EDICommunicationAuthModesList.Codes.BasicAuthentication;

			var party = Factory.NewWithValidTestData<EDICommunicationParty>();
			party.ECP_Name = "n1";

			using (var form = new ConfigContainerForm(party))
			{
				form.Show();

				form.ConfigControl.UsernameTextBoxValue = "user";
				form.ConfigControl.GeneratePasswordButton_Click();
				form.ConfigControl.SetPasswordToClipboard();
				Application.DoEvents();
				AssertEquals(form.ConfigControl.PasswordTextBoxObject.Text, SafeClipboard.GetText());
			}
		}

		public void TestGeneratePassword_CheckPasswordChar_LoadSavedInformation()
		{
			var auth = Factory.NewWithValidTestData<EDICommunicationAuth>();
			auth.ECA_AuthorizationMode = EDICommunicationAuthModesList.Codes.BasicAuthentication;

			var party = Factory.NewWithValidTestData<EDICommunicationParty>();
			party.ECP_Name = "n1";

			var partyConfig = Factory.NewWithValidTestData<EDICommunicationPartyConfig>();
			partyConfig.ECC_ECA_Auth = auth.PK;
			partyConfig.ECC_ECP_Party = party.PK;
			partyConfig.ECC_Direction = EDICommunicationPartyConfigDirectionsList.Codes.Inbound;

			using (var form = new ConfigContainerForm(party))
			{
				form.Show();
				form.ConfigControl.GeneratePasswordButton_Click();

				AssertNullOrEmpty(form.ConfigControl.PasswordTextBoxObject.Text);
				Assert(!form.ConfigControl.CopyPasswordToClipboardButtonVisibility);

				auth.ECA_Username = "user";
				form.ConfigControl.GeneratePasswordButton_Click();
				AssertNotEquals('*', form.ConfigControl.PasswordTextBoxObject.PasswordChar.ToString());
				Assert(form.ConfigControl.CopyPasswordToClipboardButtonVisibility);

				form.ConfigControl.GeneratePasswordButtonFocus();

				Factory.Save();

				var authView = Factory.Load<EDICommunicationAuth>(auth.PK);
				AssertEquals("user", authView.Username);
			}
		}

		public void TestCheckPasswordChar_PasswordEnabled_CopyToClipboardVisibility()
		{
			var auth = Factory.NewWithValidTestData<EDICommunicationAuth>();
			auth.ECA_AuthorizationMode = EDICommunicationAuthModesList.Codes.BasicAuthentication;
			auth.ECA_Username = "user";
			auth.ECA_Password = "pass";

			var party = Factory.NewWithValidTestData<EDICommunicationParty>();
			party.ECP_Name = "n1";

			var partyConfig = Factory.NewWithValidTestData<EDICommunicationPartyConfig>();
			partyConfig.ECC_ECA_Auth = auth.PK;
			partyConfig.ECC_ECP_Party = party.PK;
			partyConfig.ECC_Direction = EDICommunicationPartyConfigDirectionsList.Codes.Inbound;

			Factory.Save();

			using (var form = new ConfigContainerForm(party))
			{
				form.Show();

				AssertEquals("*", form.ConfigControl.PasswordTextBoxObject.PasswordChar.ToString());
				Assert(!form.ConfigControl.PasswordTextBoxObject.Enabled);
				Assert(!form.ConfigControl.CopyPasswordToClipboardButtonVisibility);
			}
		}

		class ConfigContainerForm : ZForm
		{
			public ConfigContainerForm(EDICommunicationParty party)
				: base(party)
			{
				ConfigControl = new InboundBasicAuthenticationUserControl();
				Controls.Add(ConfigControl);
				BindingSource.SetBindingMember(ConfigControl, ".");
			}

			public InboundBasicAuthenticationUserControl ConfigControl { get; }
		}
	}
}
