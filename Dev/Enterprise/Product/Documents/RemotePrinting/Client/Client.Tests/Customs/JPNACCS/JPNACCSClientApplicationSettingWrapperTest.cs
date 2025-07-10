using Enterprise.RemotePrinting.Client.RemotePrintServer;
using NUnit.Framework;

namespace Enterprise.RemotePrinting.Client.Tests
{
	sealed class JPNACCSClientApplicationSettingWrapperTest : TestCase
	{
		public void TestProperties()
		{
			var mailboxInfo = new MailBoxInfo { CompanyCode = "TST", MailBox = "xxx@MAIL.TEST.NACCS6", DecryptedMailBoxPassword = "123" };

			IJPNACCSClientApplicationSetting setting = new JPNACCSClientApplicationSettingWrapper(new JPNACCSClientSetting()
			{
				DomainName = "WebPrint.WisetechGlobal.com",
				NACCSMailbox = "NACCS@MAIL.TEST.NACCS6",
				MailBoxInfos = new[] { mailboxInfo },
				ReceivingInterval = 3,
				SendingInterval = 15,
				InterchangeCountPerBatchOnReceivingValue = 100,
				XTIdleConnectionKeepAliveInSecondsValue = 60d,
				XTIdleConnectionRetryPauseInSecondsValue = 15d,
				Verbose = true,
				xTApplicationNode = "WTLCTU_JPC",
				xTServerAddress = "xttest-eservices.wisegrid.net:61002",
				xTServerCertificate = JPNACCSSettingManagerForTesting.Certificate,
				DecryptedxTPassword = "TEST12345"
			}, "JPNACCSWebPrintClient");

			CombineAssertions(() =>
			{
				Assert("Verbose", setting.Verbose);
				AssertEquals("DomainName", "WebPrint.WisetechGlobal.com", setting.DomainName);
				AssertEquals("NACCSMailbox", "NACCS@MAIL.TEST.NACCS6", setting.NACCSMailbox);

				AssertEquals("xTApplicationNode", "WTLCTU_JPC", setting.xTApplicationNode);
				AssertEquals("xTServerAddress", "xttest-eservices.wisegrid.net:61002", setting.xTServerAddress);
				AssertEquals("xTServerCertificate", JPNACCSSettingManagerForTesting.Certificate, setting.xTServerCertificate);
				AssertEquals("xTPassword", "TEST12345", setting.xTPassword);

				AssertEquals("ReceivingInterval", 180, setting.ReceivingInterval);
				AssertEquals("SendingInterval", 15, setting.SendingInterval);
				AssertEquals("RetryInterval", 5d, setting.RetryInterval);
				AssertEquals("MaxRetries", 3, setting.MaxRetries);

				AssertEquals("InterchangeCountPerBatchOnReceivingValue", 100, setting.DirectxTMessagingConfig.InterchangeCountPerBatchOnReceivingValue);
				AssertEquals("XTIdleConnectionKeepAliveInSecondsValue", 60d, setting.DirectxTMessagingConfig.XTIdleConnectionKeepAliveInSecondsValue);
				AssertEquals("XTIdleConnectionRetryPauseInSecondsValue", 15d, setting.DirectxTMessagingConfig.XTIdleConnectionRetryPauseInSecondsValue);
				AssertArrayEqualsByElements("NACCSMailbox", new[] { mailboxInfo }, setting.Mailboxes);
			});
		}
	}
}
