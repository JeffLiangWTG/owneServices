using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNet.SignalR.Client;
using Moq;
using NUnit.Framework;

namespace Enterprise.RemotePrinting.Client.Tests
{
	public class PrintDefaultHttpClientTest : TestCase
	{
		public void TestAllowAutoRedirect()
		{
			var connection = new Mock<IConnection>();
			connection.Setup(c => c.Certificates).Returns(new X509CertificateCollection());
			var client = new PrintDefaultHttpClientForTest();
			client.Initialize(connection.Object);
			var handler = client.CreateHandler_Exposed() as HttpClientHandler;

			AssertEquals(false, handler.AllowAutoRedirect);
		}

		class PrintDefaultHttpClientForTest : PrintDefaultHttpClient
		{
			public HttpMessageHandler CreateHandler_Exposed() => base.CreateHandler();
		}
	}
}
