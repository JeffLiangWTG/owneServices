using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using XH.XT.Monitoring.HealthCheckService.HealthChecks;

namespace XH.XT.Monitoring.HealthCheckService.IntegrationTests.xTHealthCheck
{
	[TestFixture]
	[System.Runtime.Versioning.SupportedOSPlatform("windows")]
	public class CustomsServersHealthCheckTests : TestBase<CustomsServersHealthCheck>
	{
		protected override Action<IApplicationBuilder> CustomAppConfigureAction => app => app.UsePathBase(PathBase);

		protected override bool UsingServiceMock => true;

		[Test]
		public void Test_AllHealthy()
		{
			ServicesMock.HttpClientFactoryProvider = CreateMockHttpClientFactory();
			ServicesMock.SmtpClient = CreateMockSmtpClient();

			var response = GetClient().GetAsync($"{PathBase}/wtg/status").Result;
			var expectedResult = $"""
			                      {ExpectedHeathCheckUrlRow}
			                      {ExpectedHeathCheckServerHostName}
			                      INFO(CustomsServers): All configured servers are healthy.
			                      """;

			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
			Assert.AreEqual("text/plain", response.Content.Headers.ContentType.MediaType);
			Assert.AreEqual("utf-8", response.Content.Headers.ContentType.CharSet);

			var actual = response.Content.ReadAsStringAsync().Result;
			Assert.AreEqual(expectedResult, actual);
		}

		[Test]
		public void Test_WebServer_Unhealthy()
		{
			ServicesMock.HttpClientFactoryProvider =
				CreateMockHttpClientFactory(new List<HttpStatusCode> { HttpStatusCode.BadRequest, HttpStatusCode.BadGateway });

			ServicesMock.SmtpClient = CreateMockSmtpClient();
			var response = GetClient().GetAsync($"{PathBase}/wtg/status").Result;
			var expectedResult = $"""
			                      {ExpectedHeathCheckUrlRow}
			                      {ExpectedHeathCheckServerHostName}
			                      ERROR(TestWebServer1): UnHealthy:The web server response code not in expected:400 BadRequest.
			                      ERROR(TestWebServer2): UnHealthy:The web server response code not in expected:502 BadGateway.
			                      INFO(CustomsServers): Healthy interface(s): TestMailServer1, TestMailServer2
			                      """;

			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
			Assert.AreEqual("text/plain", response.Content.Headers.ContentType.MediaType);
			Assert.AreEqual("utf-8", response.Content.Headers.ContentType.CharSet);

			var actual = response.Content.ReadAsStringAsync().Result;
			Assert.AreEqual(expectedResult, actual);
		}

		[Test]
		public void Test_WebServer_PartHealthy()
		{
			var httpClientMock = new Mock<HttpClient>();
			using var okResponse = new HttpResponseMessage(HttpStatusCode.OK);
			using var badRequestResponse = new HttpResponseMessage(HttpStatusCode.BadRequest);
			httpClientMock
				.SetupSequence(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(okResponse)
				.ReturnsAsync(badRequestResponse)
				.ReturnsAsync(okResponse)
				.ReturnsAsync(okResponse);

			var httpClientFactoryMock = new Mock<IHttpClientFactory>();
			httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClientMock.Object);

			ServicesMock.HttpClientFactoryProvider = httpClientFactoryMock;
			ServicesMock.SmtpClient = CreateMockSmtpClient();
			var response = GetClient().GetAsync($"{PathBase}/wtg/status").Result;
			var expectedResult = $"""
			                      {ExpectedHeathCheckUrlRow}
			                      {ExpectedHeathCheckServerHostName}
			                      ERROR(TestWebServer2): UnHealthy:The web server response code not in expected:400 BadRequest.
			                      INFO(CustomsServers): Healthy interface(s): TestWebServer1, TestMailServer1, TestMailServer2
			                      """;

			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
			Assert.AreEqual("text/plain", response.Content.Headers.ContentType.MediaType);
			Assert.AreEqual("utf-8", response.Content.Headers.ContentType.CharSet);

			var actual = response.Content.ReadAsStringAsync().Result;
			Assert.AreEqual(expectedResult, actual);
		}

		[Test]
		public void Test_SmtpServer_Unhealthy_Cancelled()
		{
			ServicesMock.HttpClientFactoryProvider = CreateMockHttpClientFactory();
			ServicesMock.SmtpClient = CreateMockSmtpClient(cancelled: true);
			var response = GetClient().GetAsync($"{PathBase}/wtg/status").Result;
			var expectedResult = $"""
				                      {ExpectedHeathCheckUrlRow}
				                      {ExpectedHeathCheckServerHostName}
				                      ERROR(TestMailServer1): UnHealthy:The health check is cancelled due to the Exception and it will check in next run: cancelled exception.
				                      ERROR(TestMailServer2): UnHealthy:The health check is cancelled due to the Exception and it will check in next run: cancelled exception.
				                      INFO(CustomsServers): Healthy interface(s): TestWebServer1, TestWebServer2
				                      """;

			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
			Assert.AreEqual("text/plain", response.Content.Headers.ContentType.MediaType);
			Assert.AreEqual("utf-8", response.Content.Headers.ContentType.CharSet);

			var actual = response.Content.ReadAsStringAsync().Result;
			Assert.AreEqual(expectedResult, actual);
		}

		[Test]
		public void Test_SmtpServer_Unhealthy_Authentication()
		{
			ServicesMock.SmtpClient = CreateMockSmtpClient(new List<bool> { false, true });
			ServicesMock.HttpClientFactoryProvider = CreateMockHttpClientFactory();
			var response = GetClient().GetAsync($"{PathBase}/wtg/status").Result;
			var expectedResult = $"""
				                      {ExpectedHeathCheckUrlRow}
				                      {ExpectedHeathCheckServerHostName}
				                      ERROR(TestMailServer2): UnHealthy:Authentication failed:auth failed.
				                      INFO(CustomsServers): Healthy interface(s): TestWebServer1, TestWebServer2, TestMailServer1
				                      """;

			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
			Assert.AreEqual("text/plain", response.Content.Headers.ContentType.MediaType);
			Assert.AreEqual("utf-8", response.Content.Headers.ContentType.CharSet);

			var actual = response.Content.ReadAsStringAsync().Result;
			Assert.AreEqual(expectedResult, actual);
		}

		[Test]
		public void Test_SmtpServer_PartHealthy()
		{
			ServicesMock.SmtpClient = CreateMockSmtpClient(new List<bool> { false, true });
			ServicesMock.HttpClientFactoryProvider = CreateMockHttpClientFactory();
			var response = GetClient().GetAsync($"{PathBase}/wtg/status").Result;
			var expectedResult = $"""
				                      {ExpectedHeathCheckUrlRow}
				                      {ExpectedHeathCheckServerHostName}
				                      ERROR(TestMailServer2): UnHealthy:Authentication failed:auth failed.
				                      INFO(CustomsServers): Healthy interface(s): TestWebServer1, TestWebServer2, TestMailServer1
				                      """;

			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
			Assert.AreEqual("text/plain", response.Content.Headers.ContentType.MediaType);
			Assert.AreEqual("utf-8", response.Content.Headers.ContentType.CharSet);

			var actual = response.Content.ReadAsStringAsync().Result;
			Assert.AreEqual(expectedResult, actual);
		}

		[TestCase("TestFiles.Customs.MailServers.InvalidName.json", "DataAnnotation validation failed for 'CustomsMailServer' members: 'Name' with the error: 'The Name field is required.'.", TestName = "{m}_CustomsMailServer_InvalidName")]
		[TestCase("TestFiles.Customs.MailServers.InvalidHost.json", "DataAnnotation validation failed for 'CustomsMailServer' members: 'Host' with the error: 'The Host field is required.'.", TestName = "{m}_CustomsMailServer_InvalidHost")]
		[TestCase("TestFiles.Customs.MailServers.InvalidPort.json", "DataAnnotation validation failed for 'CustomsMailServer' members: 'Port' with the error: 'The field Port must be between 25 and 65535.'.", TestName = "{m}_CustomsMailServer_InvalidPort")]
		[TestCase("TestFiles.Customs.WebServers.InvalidUrl.json", "DataAnnotation validation failed for 'CustomsWebServer' members: 'Url' with the error: 'The Url field is not a valid fully-qualified http, https, or ftp URL.'.", TestName = "{m}_CustomsWebServer_InvalidUrl")]
		public void Test_InvalidConfiguration(string invalidConfigurationFileName, string expectedErrorMessage)
		{
			Assert.That(() => GetClient(invalidConfigurationFileName), Throws.TypeOf<OptionsValidationException>().And.Message.EqualTo(expectedErrorMessage));
		}

		Mock<ISmtpClient> CreateMockSmtpClient(IList<bool> throwExceptionOnAuth = null, bool cancelled = false)
		{
			throwExceptionOnAuth ??= new List<bool>();
			var mockSmtpClient = new Mock<ISmtpClient>();
			mockSmtpClient.Setup(x => x.ConnectAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()));
			if (cancelled)
			{
				mockSmtpClient
					.Setup(x => x.AuthenticateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
					.Throws(new OperationCanceledException("cancelled exception."));
			}
			else
			{
				mockSmtpClient
					.Setup(x => x.AuthenticateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
					.Returns(() =>
					{
						if (throwExceptionOnAuth.Count > smtpCallingCount && throwExceptionOnAuth[smtpCallingCount])
						{
							throw new AuthenticationException("auth failed.");
						}
						smtpCallingCount++;
						return Task.CompletedTask;
					});
			}

			mockSmtpClient.Setup(x => x.DisconnectAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()));

			return mockSmtpClient;
		}

		Mock<IHttpClientFactory> CreateMockHttpClientFactory(IList<HttpStatusCode> returnStatusCode = null)
		{
			returnStatusCode ??= new List<HttpStatusCode>();
			var httpClientMock = new Mock<HttpClient>();
			httpClientMock.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(() =>
				{
					var result = new HttpResponseMessage(returnStatusCode.Count > httpCallingCount ? returnStatusCode[httpCallingCount] : HttpStatusCode.OK);
					httpCallingCount++;
					return result;
				});

			var httpClientFactoryMock = new Mock<IHttpClientFactory>();
			httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClientMock.Object);

			return httpClientFactoryMock;
		}

		[SetUp]
		public void SetUp()
		{
			httpCallingCount = 0;
			smtpCallingCount = 0;
		}

		int httpCallingCount;
		int smtpCallingCount;
	}
}
