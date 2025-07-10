using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.MailFilters.Testing;
using NUnit.Framework;

namespace Enterprise.MailManager.ExternalMailInterface.Testing
{
	sealed class MailKitMailSenderForDevelopersOnlyTest : TestCaseWithFactory
	{
		const string GmailAccount = "bret.ehlert.cargowise.test@gmail.com";
		const string GmailPassword = "KeFEV6me";

		readonly string testRecipient = ""; // "bret.ehlert@cargowise.com"; // Set this to your email address to test

		[ExpectNoExceptions]
		[DeveloperOnlyTest]
		public void TestGmailWithTLS()
		{
			if (testRecipient == null)
			{
				throw new Exception("This test actually sends an email using gmail. Set testRecipient to your email address to run the test and receive the result.");
			}

			var (mailServerConfiguration, userPasswordAuthConfiguration, _) = MailKitMailSenderTest.GetConfigurationForTest(
				server: "smtp.gmail.com",
				port: 587,
				secureConnectionTypes: ZArchitecture.Core.SecureConnectionTypes.TLS,
				userName: GmailAccount,
				password: GmailPassword);

			var mailItem = Factory.New<MailItem>();
			mailItem.MI_From = GmailAccount;
			mailItem.AddRecipientForUserCommunication(testRecipient);
			mailItem.MI_Subject = "Gmail With TLS";
			mailItem.MI_Body = "This email was sent via Gmail using TLS";

			using (var sender = new MailKitMailSender(mailServerConfiguration, userPasswordAuthConfiguration))
			{
				MailTestHelpers.SetServerCertificateValidationCallback(sender.SmtpClientImpl);
				sender.Send(mailItem);
			}
		}

		[ExpectNoExceptions]
		[DeveloperOnlyTest]
		public void TestGmailWithSSL()
		{
			if (testRecipient == null)
			{
				throw new Exception("This test actually sends an email using gmail. Set testRecipient to your email address to run the test and receive the result.");
			}

			var (mailServerConfiguration, userPasswordAuthConfiguration, _) = MailKitMailSenderTest.GetConfigurationForTest(
				server: "smtp.gmail.com",
				port: 465,
				secureConnectionTypes: ZArchitecture.Core.SecureConnectionTypes.SSL,
				userName: GmailAccount,
				password: GmailPassword);

			var mailItem = Factory.New<MailItem>();
			mailItem.MI_From = GmailAccount;
			mailItem.AddRecipientForUserCommunication(testRecipient);
			mailItem.MI_Subject = "Gmail With SSL";
			mailItem.MI_Body = "This email was sent via Gmail using SSL";

			using (var sender = new MailKitMailSender(mailServerConfiguration, userPasswordAuthConfiguration))
			{
				MailTestHelpers.SetServerCertificateValidationCallback(sender.SmtpClientImpl);
				sender.Send(mailItem);
			}
		}
	}
}
