using System.Net;
using Enterprise.MailManager.MailFilters.Testing;
using MailKit;
using MailKit.Net.Imap;
using NUnit.Framework;

namespace Enterprise.MailManager.ExternalMailInterface.Testing
{
	public class SimpleImapServerTest : TestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			SimpleMailServerForTest.SetUp<SimpleImapServer>();
		}

		protected override void TearDown()
		{
			base.TearDown();
			SimpleMailServerForTest.TearDown();
		}

		[ExpectNoExceptions]
		public void TestBasic_ImapServer()
		{
			using (var client = new ImapClient())
			{
				client.Connect(IPAddress.Loopback.ToString(), MailTestHelpers.ImapPort);
				client.Authenticate(MailTestHelpers.Username1, MailTestHelpers.Password1);
				var inbox = client.Inbox;
				inbox.Open(FolderAccess.ReadOnly);
				_ = inbox.Count;
				inbox.Close();
				client.Disconnect(true);
			}
		}
	}
}
