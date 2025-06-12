using System;
using System.Net;
using Elasticsearch.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using Moq;
using Nest;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using XH.XT.Monitoring.HealthCheckService.ElasticSearch;
using XH.XT.Monitoring.HealthCheckService.HealthChecks;

namespace XH.XT.Monitoring.HealthCheckService.IntegrationTests.xTHealthCheck
{
	[TestFixture]
	[System.Runtime.Versioning.SupportedOSPlatform("windows")]
	public class TrackingMessageHealthCheckTests : TestBase<TrackingMessageHealthCheck>
	{
		protected override Action<IApplicationBuilder> CustomAppConfigureAction => app => app.UsePathBase(PathBase);

		protected override bool UsingServiceMock => true;

		[Test]
		public void xTHealthCheck_TrackingMessage_Test_WtgStatus_Unhealthy()
		{
			var elasticClientMock = new Mock<IElasticClient>();
			var searchResponseMock = new Mock<ISearchResponse<ExtendedElasticArchiveMessage>>();
			searchResponseMock.Setup(x => x.Documents).Returns(Array.Empty<ExtendedElasticArchiveMessage>());

			elasticClientMock.Setup(x => x.Search(It.IsAny<Func<SearchDescriptor<ExtendedElasticArchiveMessage>, ISearchRequest>>())).Returns(searchResponseMock.Object);

			ServicesMock.ElasticClientMock = elasticClientMock;
			var response = GetClient().GetAsync($"{PathBase}/wtg/status").Result;
			var expectedResult = $"""
				{ExpectedHeathCheckUrlRow}
				{ExpectedHeathCheckServerHostName}
				ERROR(TrackingTest1): Processing backlog delay exceeded 45 minutes
				ERROR(TrackingTest2): Processing backlog delay exceeded 30 minutes
				""";

			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
			Assert.AreEqual("text/plain", response.Content.Headers.ContentType.MediaType);
			Assert.AreEqual("utf-8", response.Content.Headers.ContentType.CharSet);

			var actual = response.Content.ReadAsStringAsync().Result;
			Assert.AreEqual(expectedResult, actual);

			var senderNo = 1;
			elasticClientMock.Verify(x => x.Search(It.Is<Func<SearchDescriptor<ExtendedElasticArchiveMessage>, ISearchRequest>>(s => VerifySearchRequest(s, ref senderNo))), Times.Exactly(2));
		}

		[Test]
		public void xTHealthCheck_TrackingMessage_Test_WtgStatus_Healthy()
		{
			var elasticClientMock = new Mock<IElasticClient>();
			var searchResponseMock = new Mock<ISearchResponse<ExtendedElasticArchiveMessage>>();
			searchResponseMock.Setup(x => x.Documents).Returns(new[] { new ExtendedElasticArchiveMessage() });

			elasticClientMock.Setup(x => x.Search(It.IsAny<Func<SearchDescriptor<ExtendedElasticArchiveMessage>, ISearchRequest>>())).Returns(searchResponseMock.Object);

			ServicesMock.ElasticClientMock = elasticClientMock;
			var response = GetClient().GetAsync($"{PathBase}/wtg/status").Result;
			var expectedResult = $"""
				{ExpectedHeathCheckUrlRow}
				{ExpectedHeathCheckServerHostName}
				INFO(TrackingTest1): Healthy
				INFO(TrackingTest2): Healthy
				""";

			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
			Assert.AreEqual("text/plain", response.Content.Headers.ContentType.MediaType);
			Assert.AreEqual("utf-8", response.Content.Headers.ContentType.CharSet);

			var actual = response.Content.ReadAsStringAsync().Result;
			Assert.AreEqual(expectedResult, actual);

			var senderNo = 1;
			elasticClientMock.Verify(x => x.Search(It.Is<Func<SearchDescriptor<ExtendedElasticArchiveMessage>, ISearchRequest>>(s => VerifySearchRequest(s, ref senderNo))), Times.Exactly(2));
		}

		[Test]
		public void xTHealthCheck_TrackingMessage_Test_WtgStatus_PartlyHealthy()
		{
			var elasticClientMock = new Mock<IElasticClient>();
			var responseUnEmptyDocMock = new Mock<ISearchResponse<ExtendedElasticArchiveMessage>>();
			responseUnEmptyDocMock.Setup(x => x.Documents).Returns(new[] { new ExtendedElasticArchiveMessage() });

			var responseEmptyDocMock = new Mock<ISearchResponse<ExtendedElasticArchiveMessage>>();
			responseEmptyDocMock.Setup(x => x.Documents).Returns(Array.Empty<ExtendedElasticArchiveMessage>());

			elasticClientMock.Setup(x => x.Search(It.Is<Func<SearchDescriptor<ExtendedElasticArchiveMessage>, ISearchRequest>>(f => DoesRequestContains(f, "Sender1")))).Returns(responseUnEmptyDocMock.Object);
			elasticClientMock.Setup(x => x.Search(It.Is<Func<SearchDescriptor<ExtendedElasticArchiveMessage>, ISearchRequest>>(f => DoesRequestContains(f, "Sender2")))).Returns(responseEmptyDocMock.Object);

			ServicesMock.ElasticClientMock = elasticClientMock;
			var response = GetClient().GetAsync($"{PathBase}/wtg/status").Result;
			var expectedResult = $"""
				{ExpectedHeathCheckUrlRow}
				{ExpectedHeathCheckServerHostName}
				ERROR(TrackingTest2): Processing backlog delay exceeded 30 minutes
				INFO(TrackingMessage): Healthy interface(s): TrackingTest1
				""";

			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
			Assert.AreEqual("text/plain", response.Content.Headers.ContentType.MediaType);
			Assert.AreEqual("utf-8", response.Content.Headers.ContentType.CharSet);

			var actual = response.Content.ReadAsStringAsync().Result;
			Assert.AreEqual(expectedResult, actual);
		}

		[Test]
		public void xTHealthCheck_TrackingMessage_Test_Validate_Data_Annotations_InvalidDescription()
		{
			Assert.That(() => GetClient("TestFiles.TrackingMessage.InvalidTrackingMessages.json"),
				Throws.TypeOf<OptionsValidationException>()
				.And.Message.EqualTo("DataAnnotation validation failed for 'TrackingMessage' members: 'Description' with the error: 'The Description field is required.'."));
		}

		bool VerifySearchRequest(Func<SearchDescriptor<ExtendedElasticArchiveMessage>, ISearchRequest> searchDescriptorFunc, ref int senderNo)
		{
			var client = new ElasticClient();
			var request = searchDescriptorFunc.Invoke(new SearchDescriptor<ExtendedElasticArchiveMessage>());
			var actual = client.RequestResponseSerializer.SerializeToString(request);
			var expect = GetEmbeddedResourceAsString($"xTHealthCheck.TestFiles.TrackingMessage.SearchRequestSender{senderNo++}.json");
			return JToken.DeepEquals(JObject.Parse(expect), JObject.Parse(actual));
		}

		bool DoesRequestContains(Func<SearchDescriptor<ExtendedElasticArchiveMessage>, ISearchRequest> searchDescriptorFunc, string senderId) =>
			new ElasticClient().RequestResponseSerializer.SerializeToString(searchDescriptorFunc.Invoke(new SearchDescriptor<ExtendedElasticArchiveMessage>())).Contains(senderId, StringComparison.Ordinal);
	}
}
