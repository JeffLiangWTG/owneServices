using System;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using Enterprise.ZArchitecture.GlowInterop;
using Moq;

namespace Enterprise.Dash.ServiceTasks.Tests.Extensions
{
	public static class MoqExtensions
	{
		public static (Mock<IGlowServiceClientFactory>, Mock<IGlowServiceClient>) SetupResponse(
			this Mock<IGlowServiceClient> mockGlowServiceClient,
			Expression<Func<HttpRequestMessage, bool>> expectedHttpRequest,
			HttpStatusCode statusCode,
			string content)
		{
			mockGlowServiceClient
				.Setup(x => x.SendAsync(It.Is(expectedHttpRequest), It.IsAny<HttpCompletionOption>()))
				.ReturnsAsync(new HttpResponseMessage() { StatusCode = statusCode, Content = new StringContent(content) });
			return (CreateGlowServiceClientFactoryMock(mockGlowServiceClient), mockGlowServiceClient);
		}

		public static (Mock<IGlowServiceClientFactory>, Mock<IGlowServiceClient>) SetupResponse(
			this Mock<IGlowServiceClient> mockGlowServiceClient,
			Expression<Func<HttpRequestMessage, bool>> expectedHttpRequest,
			Exception exception)
		{
			mockGlowServiceClient
				.Setup(x => x.SendAsync(It.Is(expectedHttpRequest), It.IsAny<HttpCompletionOption>()))
				.ThrowsAsync(exception);
			return (CreateGlowServiceClientFactoryMock(mockGlowServiceClient), mockGlowServiceClient);
		}

		public static void VerifyRequest(this Mock<IGlowServiceClient> glowServiceClientMock, Expression<Func<HttpRequestMessage, bool>> expectedHttpRequest, Times times)
			=> glowServiceClientMock.Verify(x => x.SendAsync(It.Is(expectedHttpRequest), It.IsAny<HttpCompletionOption>()), times);

		static Mock<IGlowServiceClientFactory> CreateGlowServiceClientFactoryMock(Mock<IGlowServiceClient> mockGlowServiceClient)
		{
			var glowServiceClientFactoryMock = new Mock<IGlowServiceClientFactory>();
			glowServiceClientFactoryMock
				.Setup(x => x.Create(It.IsAny<Uri>()))
				.Returns(mockGlowServiceClient.Object);
			return glowServiceClientFactoryMock;
		}
	}
}
