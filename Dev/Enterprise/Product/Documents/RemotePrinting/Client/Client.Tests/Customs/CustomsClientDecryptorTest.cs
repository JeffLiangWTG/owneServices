using System;
using System.Text;
using System.Threading;
using CargoWise.eHub.Common;
using Enterprise.RemotePrinting.Client.RemotePrintServer;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.RemotePrinting.Client.Tests;

sealed class CustomsClientDecryptorTest : TestCase
{
	public void TestDecryptWithoutCredential()
	{
		var configuration = new WebClientConfiguration();

		var clientSetting = new CNSWClientSetting();
		var wrapper = new CNSWClientApplicationSettingWrapper(clientSetting);

		AssertNoExceptionThrown("Should not throw any exceptions from an empty setting.", () => new ControllerForSecurityTest(wrapper, configuration).GetCurrentSetting());

		clientSetting.IsDecrypted = false;
		clientSetting.EHubClientPassword = Convert.ToBase64String(Encoding.UTF8.GetBytes("Test123"));

		var controller = new ControllerForSecurityTest(wrapper, configuration);
		var applicationSetting = (ICNSWClientApplicationSetting)controller.GetCurrentSetting();

		AssertEquals("EHubClientPassword", "Test123", applicationSetting.EHubClientPassword);
	}

	public void TestDecryptIncorrectPasswordWithoutCredential()
	{
		var configuration = new WebClientConfiguration();

		var clientSetting = new TWNCATKClientSetting();
		clientSetting.MachineName = "CUS.TST";

		var wrapper = new TWNCATKClientApplicationSettingWrapper(clientSetting);

		AssertNoExceptionThrown("Should not throw any exceptions from an empty setting.", () => new ControllerForSecurityTest(wrapper, configuration).GetCurrentSetting());

		var controller = new ControllerForSecurityTest(wrapper, configuration);

		void AssertEncryptException(string prefix, string accountType)
		{
			clientSetting.IsDecrypted = false;
			clientSetting.EHubClientPassword = prefix + Convert.ToBase64String(Encoding.UTF8.GetBytes("Test123"));

			var message = $@"Incorrect encryption password with unregistered {accountType} account details for ""CUS.TST"", please check the account name and password for connecting to the Service in the current configuration.";
			var exception = AssertExceptionThrown<InvalidOperationException>(() => controller.GetCurrentSetting());
			AssertEquals(message, exception.Message);
		}

		AssertEncryptException("*", "alternative");
		AssertEncryptException("#", "application");
		AssertEncryptException("@", "support");
	}

	public void TestDecryptIncorrectPasswordWithCredential()
	{
		var clientSetting = new TWNCATKClientSetting();
		clientSetting.MachineName = "CUS.TST";

		var wrapper = new TWNCATKClientApplicationSettingWrapper(clientSetting);

		AssertNoExceptionThrown("Should not throw any exceptions from an empty setting.", () => new ControllerForSecurityTest(wrapper, ConfigurationWithCredentials).GetCurrentSetting());

		clientSetting.IsDecrypted = false;
		clientSetting.EHubClientPassword = NormalPassword1;

		var controller = new ControllerForSecurityTest(wrapper, ConfigurationWithCredentials);

		var message = @"Incorrect encryption password with registered account details for ""CUS.TST"", please check the account name and password for connecting to the Service in the current configuration.";
		var exception =  AssertExceptionThrown<InvalidOperationException>(() => controller.GetCurrentSetting());
		AssertEquals(message, exception.Message);
	}

	public void TestDecrypt_CNSWClientSetting()
	{
		var clientSetting = new CNSWClientSetting();
		var wrapper = new CNSWClientApplicationSettingWrapper(clientSetting);

		AssertNoExceptionThrown("Should not throw any exceptions from an empty setting.", () => new ControllerForSecurityTest(wrapper, ConfigurationWithCredentials).GetCurrentSetting());

		clientSetting.IsDecrypted = false;
		clientSetting.EHubClientPassword = "#" + NormalPassword1;

		var controller = new ControllerForSecurityTest(wrapper, ConfigurationWithCredentials);
		var applicationSetting = (ICNSWClientApplicationSetting)controller.GetCurrentSetting();

		AssertEquals("EHubClientPassword", "Z;|/*uuN3;gN=WU--F={", applicationSetting.EHubClientPassword);
	}

	public void TestDecrypt_CLSMSClientSetting()
	{
		var clientSetting = new CLSMSClientSetting();
		var wrapper = new CLSMSClientApplicationSettingWrapper(clientSetting);

		AssertNoExceptionThrown("Should not throw any exceptions from an empty setting.", () => new ControllerForSecurityTest(wrapper, ConfigurationWithCredentials).GetCurrentSetting());

		clientSetting.IsDecrypted = false;
		clientSetting.ApplicationNodePassword = "#" + NodePassword;

		var controller = new ControllerForSecurityTest(wrapper, ConfigurationWithCredentials);
		var applicationSetting = (ICLSMSClientApplicationSetting)controller.GetCurrentSetting();

		AssertEquals("ApplicationNodePassword", SHA512Encryptor.Encrypt("CUSTST"), applicationSetting.ApplicationNodePassword);
	}

	public void TestDecrypt_TWNCATKClientSetting()
	{
		var clientSetting = new TWNCATKClientSetting();
		var wrapper = new TWNCATKClientApplicationSettingWrapper(clientSetting);

		AssertNoExceptionThrown("Should not throw any exceptions from an empty setting.", () => new ControllerForSecurityTest(wrapper, ConfigurationWithCredentials).GetCurrentSetting());

		clientSetting.IsDecrypted = false;
		clientSetting.EHubClientPassword = "#" + NormalPassword1;

		var controller = new ControllerForSecurityTest(wrapper, ConfigurationWithCredentials);
		var applicationSetting = (ITWNCATKClientApplicationSetting)controller.GetCurrentSetting();

		AssertEquals("EHubClientPassword", "Z;|/*uuN3;gN=WU--F={", applicationSetting.EHubClientPassword);
	}

	public void TestDecrypt_JPNACCSClientSetting()
	{
		var clientSetting = new JPNACCSClientSetting();
		var wrapper = new JPNACCSClientApplicationSettingWrapper(clientSetting, "TEST");

		AssertNoExceptionThrown("Should not throw any exceptions from an empty setting.", () => new ControllerForSecurityTest(wrapper, ConfigurationWithCredentials).GetCurrentSetting());

		clientSetting.IsDecrypted = false;
		clientSetting.xTPassword = "#" + NodePassword;
		clientSetting.MailBoxInfos = new[]
		{
			new MailBoxInfo() { CompanyCode = "TST", MailBox = "test001@user.mail.naccs.com", MailBoxPassword = "#" + NormalPassword1 },
			new MailBoxInfo() { CompanyCode = "TST", MailBox = "test002@user.mail.naccs.com", MailBoxPassword = "#" + NormalPassword2 },
		};

		var controller = new ControllerForSecurityTest(wrapper, ConfigurationWithCredentials);
		var applicationSetting = (IJPNACCSClientApplicationSetting)controller.GetCurrentSetting();

		CombineAssertions(() =>
		{
			AssertEquals("xTPassword", SHA512Encryptor.Encrypt("CUSTST"), applicationSetting.xTPassword);
			AssertEquals("Mailboxes[0].MailBox", "test001@user.mail.naccs.com", applicationSetting.Mailboxes[0].MailBox);
			AssertEquals("Mailboxes[0].MailBoxPassword", "Z;|/*uuN3;gN=WU--F={", applicationSetting.Mailboxes[0].DecryptedMailBoxPassword);
			AssertEquals("Mailboxes[0].MailBox", "test002@user.mail.naccs.com", applicationSetting.Mailboxes[1].MailBox);
			AssertEquals("Mailboxes[0].MailBoxPassword", "@TestPassword_0123", applicationSetting.Mailboxes[1].DecryptedMailBoxPassword);
		});
	}

	readonly WebClientConfiguration ConfigurationWithCredentials = new WebClientConfiguration
	{
		WebServiceUser = "CWTest",
		WebServicePwd = ProtectedDataHelper.Protect("CW2025")
	};

	const string NormalPassword1 = @"Boy3V6SIfyM3dAjqoTLnkkAkQMuHZyi82l4Hbibbixg=";
	const string NormalPassword2 = @"JVTLVG2Iv/RNdFs52XWJbEBud+BhMRyhJUYOMk0SHa8=";
	const string NodePassword = @"Uq8fv4H0tE7GS1+Qes6U1TRTRnd5QXDoa5byk5tfGU+GZIv9FSztMl0zj96F3"
				+ @"Pj98w4dXmSQlidAt5wQQvgSQM+Dgd7S7SmLoxr7hk2x0e70FHlx1Zw3kCjiL3UEiGD5HaUdNX4icBp"
				+ @"B40dIdkgWScNe1Y3hUgQlLRgLdGcHUvv/2ZUUXW67pmVm3eNGbe9DptoEONEEohJKaQPyR8ABqJdfT"
				+ @"qe3HIMPG8BQK98Gnf7nue3lWFgxB4nInykw+fcF";

	sealed class ControllerForSecurityTest : CustomsMessageController
	{
		public ControllerForSecurityTest(ICustomseHubClientSetting setting, WebClientConfiguration clientConfiguration)
			: base(string.Empty, CancellationToken.None)
		{
			this.setting = setting;
			this.clientConfiguration = clientConfiguration;
		}

		readonly ICustomseHubClientSetting setting;
		readonly WebClientConfiguration clientConfiguration;

		protected override CustomseHubClientSettingManager CreateNewSettingManager(string machineName)
		{
			var mockSettingManager = new Mock<CustomseHubClientSettingManager>("CUS.TST", null);
			mockSettingManager.Protected().Setup<Func<WebClient, ICustomseHubClientSetting>>("GetEhubClientSetting").Returns((client) => setting);

			return mockSettingManager.Object;
		}

		protected override WebClientConfiguration GetNewConfigSetting(string configName) => clientConfiguration;

		protected override int ProcessCore(ICustomseHubClientSetting setting) => 0;

		public ICustomseHubClientSetting GetCurrentSetting() => SettingManager.CurrentSetting;
	}
}
