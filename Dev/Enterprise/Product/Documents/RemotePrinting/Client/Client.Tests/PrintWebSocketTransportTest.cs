using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Http;
using Moq;
using NUnit.Framework;

namespace Enterprise.RemotePrinting.Client.Tests
{
	class PrintWebSocketTransportTest : TestCase
	{
		public void TestLostConnection()
		{
			var client = new Mock<IHttpClient>();
			var connection = new Mock<IConnection>();

			var transportMock = new Mock<PrintWebSocketTransport>(client.Object) { CallBase = true };
			connection.Setup(x => x.Transport).Returns(transportMock.Object);
			transportMock.Object.Dispose();

			AssertNoExceptionThrown(() => transportMock.Object.LostConnection(connection.Object));
		}
	}
}
