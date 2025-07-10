using System;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Enterprise.RemotePrinting.Client.Tests
{
	public class JPNACCSSettingManagerForTesting : JPNACCSClientApplicationSettingManager
	{
		public const string CorrectMachineName = "CorrectMachineName";

		public const string EmptySettingMachine = "EmptySettingMachine";

		public const string Certificate = "-----BEGIN CERTIFICATE-----\nMIIEdDCCAtygAwIBAgIJAMlUsneHB5rTMA0GCSqGSIb3DQEBCwUAMA0xCzAJBgNV\nBAYTAkFVMCAXDTIyMDgzMTEwNDIwMFoYDzIwMjUwODMxMDAwMDAwWjANMQswCQYD\nVQQGEwJBVTCCAaIwDQYJKoZIhvcNAQEBBQADggGPADCCAYoCggGBAL2iLHh4/xTO\nTIOgTm0UHrHyt0QGbEsqjZi8wdifmYwCgQTC06JrK7KqxFHp558FtsE9OWBVvTW/\nZ6NTV4j6vS1xwHMLCzp/cHJNlBO0L3QBnx/PcCCQfU+DLHs6KGd4JtCCcm62F4na\nKNBOrXDYG9n7Iu1boUnwHoBmiiT9KzsizNKYV2MmZbaCmMGco04i5eilbq65Mqhs\nzUpVEP8hf1M5qGZa6NeiP9XMMR8zBUlUCV0F2empgHFYaEAIM7kb6E+N87UNZMJj\nJJ3IcOvkHj4SttakWf9cg1mTx4KLDsKfcWV3G2V8c57aQmBtHbkpRuI46Gtk8xw9\nyp5I3WPi57Z9aQsnv6NYEm8fPuDtTYiTEZ2tzWenlaICALoGDaVXKmj2VDtvNXHs\nchjbzxNkDZzFgu6kYhfbivYDZC1Fxi7BByNgcEOgtf9COfpds0tzbY3NrSg1q7/c\nUhzDe3dlEqD+pUfvBizMwGEtYQGdLek0IDuHPwmXeoSxHIKMqkUq2wIDAQABo4HU\nMIHRMAwGA1UdEwEB/wQCMAAwDgYDVR0PAQH/BAQDAgKkMB0GA1UdJQQWMBQGCCsG\nAQUFBwMCBggrBgEFBQcDATBSBgNVHREESzBJhwQKAkYTgh14dHRlc3QtZXNlcnZp\nY2VzLndpc2VncmlkLm5ldIIiYXUyd3Qtc3h0ci00MDIudGVzdC53aXNlY2xvdWQu\nem9uZTAdBgNVHQ4EFgQUHMcOECTwJglE4IYhZie4KeoKRuowHwYDVR0jBBgwFoAU\nHMcOECTwJglE4IYhZie4KeoKRuowDQYJKoZIhvcNAQELBQADggGBALUEeV2V4XRe\nrFF2zZW/R3Ocldn0dCY032yr4jkU7101+NedOHNC24jouPoFIEewdKaxcisJAcJP\nwR4sve9Eq5L8oJKYXJ41RTyVr+6HpqfZSwQu8pxb06Tyrs/WAC5ChNd6XDfbaBGh\n52aMtCOrkHHO8DPxIft0FtP21QIh2TUW1DHHIsz0mzNukMOBF+oFwPU1y2QC9T27\nb/1mWLp2mTRveW0+8Q+ejqeoNx3jyDXxf5CYxAV/+7gA0t6ElnkwbG8g1I1jX+i9\nkj+nebJmdl5CU1is4n79S0TNfgFYqQ1wZWqa4rPSS919MxZMGeH2z5+yKC9vjhDH\nUV1ZApaScqDpq0hVBfWIwochKCdmAcf1/qkknOfCfx7H9NNgkempwcmLeuqyKJnm\nfTKRTZWaSn+0aPQl3mePLzZge+GFKhe1gU41Qu/l0flgaoN6Looxs3T6bkUT51H0\nuLCg447HVvk/Bd0skJavMhFHn+fvYY1M2aMiff/KmFzsOq2SzdU8aw==\n-----END CERTIFICATE-----\n";

		public JPNACCSSettingManagerForTesting(string localMachineName, WebClient webServiceClient) : base(localMachineName, webServiceClient) { }

		public static string TextForRetrieveStream { get => fTextForRetrieveStream ?? (fTextForRetrieveStream = ""); set => fTextForRetrieveStream = value; }
		[ThreadStatic]
		static string fTextForRetrieveStream;
	}

	sealed class JPNACCSClientApplicationSettingManagerTest : TestCase
	{
		public void TestSettingManagerProvider()
		{
			var mockedClient = JPNACCSTestHelper.CreateFakeWebClient();
			var provider1 = JPNACCSClientApplicationSettingManagerProvider.GetSettingManager("TEST001+http://web.client.config.setting/config", "TEST001", mockedClient);
			var provider2 = JPNACCSClientApplicationSettingManagerProvider.GetSettingManager("TEST001+http://web.client.config.setting/config", "TEST001", null);
			var provider3 = JPNACCSClientApplicationSettingManagerProvider.GetSettingManager("TEST002+http://web.client.config.setting/config", "TEST002", mockedClient);

			AssertEquals("TEST001", provider1.LocalMachineName);
			AssertEquals("TEST002", provider3.LocalMachineName);

			AssertSame("Should return the cached setting manager as they are using same machine names.", provider1, provider2);
			AssertNotSame("Should return a new setting manager as they are using different machine names.", provider1, provider3);

			JPNACCSClientApplicationSettingManagerProvider.Remove("TEST001+http://web.client.config.setting/config");

			var provider4 = JPNACCSClientApplicationSettingManagerProvider.GetSettingManager("TEST001+http://web.client.config.setting/config", "TEST001", mockedClient);
			AssertNotSame("Should return a new setting manager as all cached managers are removed.", provider1, provider4);
		}

		public void TestJPNACCSSetting()
		{
			var mockedClient = JPNACCSTestHelper.CreateFakeWebClient();
			var logger = new StringBuilder();

			var manager = new JPNACCSSettingManagerForTesting("CorrectMachineName", mockedClient);
			manager.LogInformation += (s, e) => logger.AppendLine(e.Message);
			manager.OnSettingDownloaded += CustomseHubTestHelper.OnSettingDownloaded;

			var setting = manager.CurrentSetting;

			Assert("Setting is converted.", setting.IsValid
				&& setting.Verbose
				&& setting.DomainName == "WebPrint.WisetechGlobal.com"
				&& setting.NACCSMailbox == "NACCS@MAIL.TEST.NACCS6"
				&& setting.xTApplicationNode == "WTLCTU_JPC"
				&& setting.xTServerAddress == "xttest-eservices.wisegrid.net:61002"
				&& setting.xTServerCertificate == JPNACCSSettingManagerForTesting.Certificate
				&& setting.xTPassword == "TEST12345"
				&& setting.ReceivingInterval == 180
				&& setting.SendingInterval == 15
				&& setting.DirectxTMessagingConfig.InterchangeCountPerBatchOnReceivingValue == 100
				&& setting.DirectxTMessagingConfig.XTIdleConnectionKeepAliveInSecondsValue == 60d
				&& setting.DirectxTMessagingConfig.XTIdleConnectionRetryPauseInSecondsValue == 15d
				&& setting.Mailboxes.Any(c => c.CompanyCode == "TST" && c.MailBox == "xxx@MAIL.TEST.NACCS6" && c.DecryptedMailBoxPassword == "123"));

			AssertContains("Log for empty setting", "JPNACCS client application setting is valid", logger.ToString());
			logger.Clear();

			var emptySettingManager = new JPNACCSSettingManagerForTesting(JPNACCSSettingManagerForTesting.EmptySettingMachine, mockedClient);
			emptySettingManager.LogInformation += (s, e) => logger.AppendLine(e.Message);

			var emptySetting = emptySettingManager.CurrentSetting;

			Assert("Setting should not be valid", !emptySetting.IsValid);
			AssertContains("Log for empty setting", NACCSConstants.ErrorMessages.ErrorSettingLog, logger.ToString());
		}
	}
}
