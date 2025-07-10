using System.Net;
using System.Text;
using Enterprise.MailManager.ExternalMailInterface.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.RemotePrinting.Client.Tests
{
	sealed class ProtocolLoggerAdaptorTest : TestCase
	{
		public void TestLogPassword()
		{
			var log = new StringBuilder();

			var notificationsMock = new Mock<INotifications>();
			notificationsMock.Setup(n => n.AddMessage(It.IsAny<string>())).Callback<string>(m => log.AppendLine(m));

			var testUserName = "cargowiseuatcentraltestaccount";
			var testPassword = "C3ntr4lT35t";

			using (var client = new NACCSPOP3Client(new ProtocolLoggerAdaptor(notificationsMock.Object)))
			{
				client.Connect(IPAddress.Loopback.ToString(), 110);
				client.Authenticate(testUserName, testPassword);
				_ = client.GetMessageCount();
				client.Disconnect(true);
			}

			var fullLog = log.ToString();
			AssertContains("C: USER cargowiseuatcentraltestaccount", fullLog);
			AssertContains("C: PASS *************", fullLog);
			AssertNotContains(testPassword, fullLog);
		}

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
	}
}
