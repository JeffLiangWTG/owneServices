using System.Net;
using Enterprise.MailManager.MailFilters.Testing;
using MailKit.Net.Pop3;
using NUnit.Framework;

namespace Enterprise.MailManager.ExternalMailInterface.Testing
{
	public class SimplePop3ServerTest : TestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			SimpleMailServerForTest.SetUp<SimplePop3Server>();
		}

		protected override void TearDown()
		{
			base.TearDown();
			SimpleMailServerForTest.TearDown();
		}

		[ExpectNoExceptions]
		public void TestBasic_Pop3Server()
		{
			using (var client = new Pop3Client())
			{
				client.Connect(IPAddress.Loopback.ToString(), MailTestHelpers.Pop3Port);
				client.Authenticate(MailTestHelpers.Username1, MailTestHelpers.Password1);
				_ = client.GetMessageCount();
				client.Disconnect(true);
			}
		}
	}
}
