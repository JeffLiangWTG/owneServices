using System;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MailManager.ExternalMailInterface.Testing
{
	sealed class MailSenderProviderTest : TestCaseWithFactory
	{
		public void TestInstanceIsMailSenderProvider()
		{
			var provider = ObjectFactory.Get<IMailSenderProvider>();
			AssertType<MailSenderProvider>(provider);
		}

		public void TestProviderInstanceRespectRegistry()
		{
			try
			{
				Globals.IsTest_ForTest.Value = false;

				var logger = new LoggerForTest();
				using (Env.Registry.RawRegistry.UseOAuth2ForOutgoing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, OAuth2TypeList.Codes.Ms365))
				using (Env.Registry.RawRegistry.UseGraphApiForOutgoing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (var sender = ObjectFactory.Get<IMailSenderProvider>().GetSender(logger))
				{
					AssertType<GraphMailSender>(sender);
					AssertNotNull((sender as GraphMailSender).logger);
				}

				var smtp = MailKitMailSenderTest.GetMockSmtpClientImpl();

				using (Env.Registry.RawRegistry.UseGraphApiForOutgoing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				using (Env.Registry.RawRegistry.SMTPServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "server"))
				using (Env.Registry.RawRegistry.SMTPUsername.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "user"))
				using (Env.Registry.RawRegistry.SMTPPassword.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "password"))
				using (var sender = ObjectFactory.Get<IMailSenderProvider>().GetSender(logger))
				{
					var mailKitMailSender = sender as MailKitMailSender;
					mailKitMailSender.SmtpClientImpl = smtp;
					mailKitMailSender.Connect();

					AssertNotNull(mailKitMailSender.logger);
					var expectedConfiguration = $"From: user Server: server";
					AssertEquals("Primary SMTP settings should be used as default.", expectedConfiguration, mailKitMailSender.GetSendingInfo());
				}
			}
			finally
			{
				Globals.IsTest_ForTest.ResetValue();
			}
		}

		public void TestMailKitSenderWillBeUsedIfMailFromAddressCanMatchSecondarySMTPServerEvenThoughUseGraphApiForOutgoingSetToTrue()
		{
			try
			{
				Globals.IsTest_ForTest.Value = false;

				var logger = new LoggerForTest();

				var secondaryConfigurations = CreateTestSecondarySmtpServerConfigurations();
				var smtpServer1 = secondaryConfigurations[0];

				var smtp = MailKitMailSenderTest.GetMockSmtpClientImpl();

				var expectedConfiguration = $"From: {smtpServer1.SMTPUsername} Server: {smtpServer1.SMTPServer}";

				DataRegistry.Instance.UseGraphApiForOutgoing = true;
				var mailSenderProvider = ObjectFactory.Get<IMailSenderProvider>();

				using (Env.Registry.RawRegistry.UseOAuth2ForOutgoing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, OAuth2TypeList.Codes.Ms365))
				using (Env.Registry.RawRegistry.SMTPServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "server"))
				using (Env.Registry.RawRegistry.SMTPUsername.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "user"))
				using (Env.Registry.RawRegistry.SMTPPassword.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "password"))
				using (PhysicalServerDataRegistry.Instance.SecondarySMTPServers.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, secondaryConfigurations))
				using (var sender = mailSenderProvider.GetSender(logger, from: "Darren@0xp2b.onmicrosoft.com"))
				{
					AssertType<GraphMailSender>(sender);
					AssertNotNull((sender as GraphMailSender).logger);
				}

				using (PhysicalServerDataRegistry.Instance.SecondarySMTPServers.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, secondaryConfigurations))
				using (var sender = mailSenderProvider.GetSender(logger, from: "test@domain1.com"))
				{
					AssertType<MailKitMailSender>(sender);
					var mailKitMailSender = sender as MailKitMailSender;
					mailKitMailSender.SmtpClientImpl = smtp;
					mailKitMailSender.Connect();
					AssertNotNull(mailKitMailSender.logger);
					AssertEquals("Secondary SMTP settings should be used as default.", expectedConfiguration, mailKitMailSender.GetSendingInfo());
				}
			}
			finally
			{
				Globals.IsTest_ForTest.ResetValue();
			}
		}

		public void TestMailKitSenderUseOverridenConfigurationIfMatches()
		{
			try
			{
				Globals.IsTest_ForTest.Value = false;

				var logger = new LoggerForTest();

				var secondaryConfigurations = CreateTestSecondarySmtpServerConfigurations();
				var smtpServer1 = secondaryConfigurations[0];

				var smtp = MailKitMailSenderTest.GetMockSmtpClientImpl();

				var expectedConfiguration = $"From: {smtpServer1.SMTPUsername} Server: {smtpServer1.SMTPServer}";

				using (PhysicalServerDataRegistry.Instance.SecondarySMTPServers.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, secondaryConfigurations))
				using (var sender = ObjectFactory.Get<IMailSenderProvider>().GetSender(logger, from: "test@domain1.com"))
				{
					var mailKitMailSender = sender as MailKitMailSender;
					mailKitMailSender.SmtpClientImpl = smtp;
					mailKitMailSender.Connect();
					AssertEquals("Secondary SMTP settings should be used since 'domain1.com' is mapped via Supported Domain.", expectedConfiguration, mailKitMailSender.GetSendingInfo());
				}

				using (PhysicalServerDataRegistry.Instance.SecondarySMTPServers.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, secondaryConfigurations))
				using (var sender = ObjectFactory.Get<IMailSenderProvider>().GetSender(logger, from: "sender@server1.com"))
				{
					var mailKitMailSender = sender as MailKitMailSender;
					mailKitMailSender.SmtpClientImpl = smtp;
					mailKitMailSender.Connect();
					AssertEquals("Secondary SMTP settings should be used since 'sender@server1.com' is mapped via Secondary Sender Address.", expectedConfiguration, mailKitMailSender.GetSendingInfo());
				}

				var smtpServer2 = secondaryConfigurations[1];
				expectedConfiguration = $"From: {smtpServer2.SMTPUsername} Server: {smtpServer2.SMTPServer}";

				using (PhysicalServerDataRegistry.Instance.SecondarySMTPServers.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, secondaryConfigurations))
				using (var sender = ObjectFactory.Get<IMailSenderProvider>().GetSender(logger, from: "test@domain3.com"))
				{
					var mailKitMailSender = sender as MailKitMailSender;
					mailKitMailSender.SmtpClientImpl = smtp;
					mailKitMailSender.Connect();
					AssertEquals("Secondary SMTP settings should be used since 'domain3.com' is mapped via Supported Domain.", expectedConfiguration, mailKitMailSender.GetSendingInfo());
				}

				expectedConfiguration = $"From: user Server: server";

				using (Env.Registry.RawRegistry.SMTPServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "server"))
				using (Env.Registry.RawRegistry.SMTPUsername.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "user"))
				using (Env.Registry.RawRegistry.SMTPPassword.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "password"))
				using (PhysicalServerDataRegistry.Instance.SecondarySMTPServers.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, secondaryConfigurations))
				using (var sender = ObjectFactory.Get<IMailSenderProvider>().GetSender(logger, from: "test@xdomain3.comx"))
				{
					var mailKitMailSender = sender as MailKitMailSender;
					mailKitMailSender.SmtpClientImpl = smtp;
					mailKitMailSender.Connect();
					AssertEquals("Primary SMTP settings should be used since no secondary SMTP setting mapped.", expectedConfiguration, mailKitMailSender.GetSendingInfo());
				}
			}
			finally
			{
				Globals.IsTest_ForTest.ResetValue();
			}
		}

		SecondarySMTPServerCollection CreateTestSecondarySmtpServerConfigurations()
		{
			var collection = new SecondarySMTPServerCollection();
			var server1 = collection.AddNew();
			server1.SMTPServer = "mail.server1.com";
			server1.SMTPPort = 587;
			server1.SMTPSecureConnection = ZArchitecture.Core.SecureConnectionTypes.SSL;
			server1.SMTPUsername = "user1@server1.com";
			server1.SMTPPassword = "password1";
			server1.SMTPSenderAddress = "sender@server1.com";
			server1.SupportedDomains = "domain1.com, domain2.com";

			var server2 = collection.AddNew();
			server2.SMTPServer = "mail.server2.com";
			server2.SMTPPort = 110;
			server2.SMTPSecureConnection = ZArchitecture.Core.SecureConnectionTypes.TLS;
			server2.SMTPUsername = "invalid sender address";
			server2.SMTPPassword = "password2";
			server2.SMTPSenderAddress = "sender@server2.com";
			server2.SupportedDomains = "domain3.com, domain4.com";

			return collection;
		}
	}
}
