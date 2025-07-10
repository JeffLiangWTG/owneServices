using System.Net;
using Enterprise.MailManager.MailFilters.Testing;
using MailKit.Net.Smtp;
using MimeKit;
using NUnit.Framework;

namespace Enterprise.MailManager.ExternalMailInterface.Testing
{
	public class SimpleSmtpServerTest : TestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			SimpleMailServerForTest.SetUp<SimpleSmtpServer>();
		}

		protected override void TearDown()
		{
			base.TearDown();
			SimpleMailServerForTest.TearDown();
		}

		[ExpectNoExceptions]
		public void TestBasic_SmtpServer()
		{
			var message = new MimeMessage();
			message.From.Add(new MailboxAddress(MailTestHelpers.Username1, MailTestHelpers.UserEmailAddress1));
			message.To.Add(new MailboxAddress(MailTestHelpers.Username2, MailTestHelpers.UserEmailAddress2));
			message.Subject = "TestBasic_SmtpServer";
			message.Body = new TextPart("plain") { Text = "TestBasic_SmtpServer" };

			using (var client = new SmtpClient())
			{
				client.Connect(IPAddress.Loopback.ToString(), MailTestHelpers.SmtpPort);
				client.Authenticate(MailTestHelpers.Username1, MailTestHelpers.Password1);
				client.Send(message);
				client.Disconnect(true);
			}
		}
	}
}
