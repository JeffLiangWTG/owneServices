using System;
using System.Net.Http;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using ServiceManager.Integration.ServiceHostClient;
using ServiceManager.Integration.ServiceHostClient.Abstractions;
using ServiceManager.Shared.CW;
using WTG.Foundation.Http;

namespace Enterprise.ServiceManager.HostClient.Testing
{
	sealed class EDIServiceHostClientFactoryTest : TestCase
	{
		public void TestGetNewServiceHostClient()
		{
			// Arrange
			using var factory = new EDIServiceHostClientFactory();
			// Act
			var client = factory.GetNewServiceHostClient("a", "b", "c");
			// Assert
			Assert("Client is not null", client != null);
			Assert("Client is of proper type", client is ServiceHostClient);
		}

		[ExpectNoExceptions]
		public void TestDisposesServiceHostHttpClient()
		{
			// Arrange
			var httpClientMock = new Mock<HttpClient>();
			httpClientMock
				.Protected()
				.Setup(nameof(IDisposable.Dispose), ItExpr.IsAny<bool>());
			var serviceHostHttpClientMock = new Mock<ServiceHostHttpClient>(
				MockBehavior.Strict,
				Mock.Of<IServiceHostErrorReporter>(),
				TimeSpan.Zero,
				Mock.Of<IHttpClientFactory>(clientFactory => clientFactory.CreateNew(It.IsAny<HttpMessageHandler>()) == httpClientMock.Object),
				Mock.Of<IJsonDeserializer>());
			var factory = new EDIServiceHostClientFactory(serviceHostHttpClientMock.Object);

			// Act
			factory.Dispose();

			// Assert
			httpClientMock
				.Protected()
				.Verify(nameof(IDisposable.Dispose), Times.Once(), ItExpr.IsAny<bool>());
		}
	}
}
