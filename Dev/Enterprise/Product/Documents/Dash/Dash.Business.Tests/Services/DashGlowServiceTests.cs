using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CargoWise.EntityFramework.Testing;
using Enterprise.Dash.Business.Extensions;
using Enterprise.Dash.Business.Services;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GlowInterop;
using Moq;

namespace Enterprise.Dash.Business.Tests.Services
{
	public class DashGlowServiceTests : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			base.SetUp();
			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://localhost/Glow");
		}

		public void TestMatchOrganisations_SuccessfulResponse()
		{
			// arrange
			var docPk = Guid.NewGuid();
			var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);
			var clientFactoryMock = new Mock<IGlowServiceClientFactory>();
			var dashErrorReporterMock = new Mock<IDashErrorReporter>();
			var httpClientMock = new Mock<IGlowServiceClient>();

			clientFactoryMock
				.Setup(factory => factory.Create(It.IsAny<Uri>()))
				.Returns(httpClientMock.Object);

			httpClientMock
				.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), HttpCompletionOption.ResponseHeadersRead))
				.Returns(Task.FromResult(expectedResponse));

			var dashGlowService = new DashGlowService(clientFactoryMock.Object, dashErrorReporterMock.Object);

			// act
			var response = dashGlowService.MatchOrganisations(docPk);

			// assert
			AssertNotNull(response);
			AssertEquals(expectedResponse.StatusCode, response.StatusCode);
		}

		public void TestMatchOrganisations_FailureResponse_ThrowsException()
		{
			// arrange
			var docPk = Guid.NewGuid();
			var apiPath = $"api/dash/organization/match/{docPk}";
			var failedResponse = new HttpResponseMessage(HttpStatusCode.GatewayTimeout) { Content = new StringContent("Error") };
			var clientFactoryMock = new Mock<IGlowServiceClientFactory>();
			var dashErrorReporterMock = new Mock<IDashErrorReporter>();
			var httpClientMock = new Mock<IGlowServiceClient>();

			clientFactoryMock
				.Setup(factory => factory.Create(It.IsAny<Uri>()))
				.Returns(httpClientMock.Object);

			httpClientMock
				.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), HttpCompletionOption.ResponseHeadersRead))
				.Returns(Task.FromResult(failedResponse));

			var dashGlowService = new DashGlowService(clientFactoryMock.Object, dashErrorReporterMock.Object);

			var exception = AssertExceptionThrown<HttpRequestException>(() => dashGlowService.MatchOrganisations(docPk));

			// assert
			var expectedMessage = $"Request failed with HttpStatusCode: {failedResponse.StatusCode}: {failedResponse.Content.ReadAsString()}";
			AssertEquals(expectedMessage, exception.Message);

			dashErrorReporterMock.Verify(r => r.GatherAdditionalInformation(apiPath), Times.Once);
		}

		public void TestMatchProductCodes_SuccessfulResponse()
		{
			// arrange
			var docPk = Guid.NewGuid();
			var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);
			var clientFactoryMock = new Mock<IGlowServiceClientFactory>();
			var dashErrorReporterMock = new Mock<IDashErrorReporter>();
			var httpClientMock = new Mock<IGlowServiceClient>();

			clientFactoryMock
				.Setup(factory => factory.Create(It.IsAny<Uri>()))
				.Returns(httpClientMock.Object);

			httpClientMock
				.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), HttpCompletionOption.ResponseHeadersRead))
				.Returns(Task.FromResult(expectedResponse));

			var dashGlowService = new DashGlowService(clientFactoryMock.Object, dashErrorReporterMock.Object);

			// act
			var response = dashGlowService.MatchProductCodes(docPk);

			// assert
			AssertNotNull(response);
			AssertEquals(HttpStatusCode.OK, response.StatusCode);
		}

		public void TestMatchProductCodes_FailureResponse_ThrowsException()
		{
			// arrange
			var docPk = Guid.NewGuid();
			var apiPath = $"api/dash/document/{docPk}/productcode/match";
			var failedResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError) { Content = new StringContent("Server Error") };
			var clientFactoryMock = new Mock<IGlowServiceClientFactory>();
			var dashErrorReporterMock = new Mock<IDashErrorReporter>();
			var httpClientMock = new Mock<IGlowServiceClient>();

			clientFactoryMock
				.Setup(factory => factory.Create(It.IsAny<Uri>()))
				.Returns(httpClientMock.Object);

			httpClientMock
				.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), HttpCompletionOption.ResponseHeadersRead))
				.Returns(Task.FromResult(failedResponse));

			var dashGlowService = new DashGlowService(clientFactoryMock.Object, dashErrorReporterMock.Object);

			// act
			var exception = AssertExceptionThrown<HttpRequestException>(() => dashGlowService.MatchProductCodes(docPk));

			// assert
			var expectedMessage = $"Request failed with HttpStatusCode: {failedResponse.StatusCode}: {failedResponse.Content.ReadAsString()}";
			AssertEquals(expectedMessage, exception.Message);

			dashErrorReporterMock.Verify(r => r.GatherAdditionalInformation(apiPath), Times.Once);
		}
	}
}
