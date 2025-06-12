using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using XH.XT.Monitoring.HealthCheckService.HealthChecks;
using XH.XT.Monitoring.HealthCheckService.XTRESTAPI;

namespace XH.XT.Monitoring.HealthCheckService.IntegrationTests.xTHealthCheck
{
	[TestFixture]
	[System.Runtime.Versioning.SupportedOSPlatform("windows")]
	public class CTMHealthCheckTests : TestBase<CTMHealthCheck>
	{
		protected override Action<IApplicationBuilder> CustomAppConfigureAction
		{
			get
			{
				return (app) =>
				{
					app.UsePathBase($"{PathBase}");
				};
			}
		}

		protected override bool UsingServiceMock => true;

		Mock<IDateTimeProvider> datetimeProviderMock;
		Mock<XTRestClient> xTRestClientMock;
		Mock<HttpMessageInvoker> httpClientMock;

		[SetUp]
		public void TestSetup()
		{
			HealthCheckOptions = new Dictionary<string, string>
			{
				{ $"Options:{PathBase}/wtg/status:0", "ClickToMonitorHealthCheckDirectxT" }
			};
			datetimeProviderMock = new Mock<IDateTimeProvider>();
			xTRestClientMock = new Mock<XTRestClient>();
			httpClientMock = new Mock<HttpMessageInvoker>(new HttpClientHandler());

			xTRestOptionsMock.Setup(x => x.Get(It.Is<string>(s => s.Equals(XTRestSettings.DirectxT, StringComparison.Ordinal)))).Returns(new XTRestSettings
			{
				AlarmServerBaseUrl = "https://xtdirectserver:1111",
				XTRestBaseUrl = "http://xtdirectserver:2222",
				AccessToken = "X3QBqWPYCyQjLXhtJ89206tx+n9+JbwZGGB6ZIWGccImo0Pk/8wbRdYq1xIvIfCwk2G5BjH4bheI98tSyhB9Meh6Qo3seHbzXGcFjQD7ucjLPxwFZvC2lIDF5DL8zDkDr4fyNvI8MsD3qha1UTEgnxSOegKuXiJM3Q8g5XAWhPE=",
				WorkspaceTimeoutInSeconds = 5
			});
			xTRestOptionsMock.Setup(x => x.Get(It.Is<string>(s => s.Equals(XTRestSettings.XHub, StringComparison.Ordinal)))).Returns(new XTRestSettings
			{
				AlarmServerBaseUrl = "https://xhubserver:3333",
				XTRestBaseUrl = "http://xhubserver:4444",
				AccessToken = "X3QBqWPYCyQjLXhtJ89206tx+n9+JbwZGGB6ZIWGccImo0Pk/8wbRdYq1xIvIfCwk2G5BjH4bheI98tSyhB9Meh6Qo3seHbzXGcFjQD7ucjLPxwFZvC2lIDF5DL8zDkDr4fyNvI8MsD3qha1UTEgnxSOegKuXiJM3Q8g5XAWhPE=",
				WorkspaceTimeoutInSeconds = 10
			});
			xTRestClientMock = new Mock<XTRestClient>(
				xTRestOptionsMock.Object,
				httpClientMock.Object,
				new HttpClientSettings()
				{
					TimeoutInSeconds = 2
				},
				datetimeProviderMock.Object)
			{ CallBase = true };
			ServicesMock.DateTimeProvider = datetimeProviderMock;
			ServicesMock.XTRestClientMock = xTRestClientMock;
		}

		[Test]
		public void xTHealthCheck_CTM_Test_WtgStatus_Success()
		{
			HealthCheckOptions.Add($"Options:{PathBase}/wtg/status:1", "ClickToMonitorHealthCheckXHub");

			httpClientMock.Setup(x => x.SendAsync(It.Is<HttpRequestMessage>(req => req.RequestUri.ToString().EndsWith("/uat")), It.IsAny<CancellationToken>()))
				.ReturnsAsync(() =>
				{
					return new HttpResponseMessage
					{
						StatusCode = HttpStatusCode.OK,
						Content = new StringContent(new JObject(new JProperty("token", GenerateToken())).ToString())
					};
				});

			httpClientMock.Setup(x => x.SendAsync(It.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("/ConfigApi/v1/objects?")), It.IsAny<CancellationToken>()))
				.ReturnsAsync(() =>
				{
					return new HttpResponseMessage
					{
						StatusCode = HttpStatusCode.OK,
						Content = new StringContent(GetEmbeddedResourceAsString("xTHealthCheck.TestFiles.ClickToMonitor.XTFolderList.json"))
					};
				});

			httpClientMock.Setup(x => x.SendAsync(It.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("/CTM/v1/ListCTM?")), It.IsAny<CancellationToken>()))
				.ReturnsAsync(() =>
				{
					return new HttpResponseMessage
					{
						StatusCode = HttpStatusCode.NoContent
					};
				});

			var expectedResult = $"""
				{ExpectedHeathCheckUrlRow}
				{ExpectedHeathCheckServerHostName}
				INFO(ClickToMonitorDirectxT): Healthy
				INFO(ClickToMonitorXHub): Healthy
				""";

			var response = GetClient().GetAsync($"{PathBase}/wtg/status").Result;
			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
			Assert.AreEqual("text/plain", response.Content.Headers.ContentType?.MediaType);
			Assert.AreEqual("utf-8", response.Content.Headers.ContentType.CharSet);

			var actual = response.Content.ReadAsStringAsync().Result;
			Assert.AreEqual(expectedResult, actual);

			httpClientMock.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Equals("https://xtdirectserver:1111/CTM/v1/ListCTM?details=True&folders=xt-folder%3A%2FTestProduct&onlyActive=True", StringComparison.Ordinal)), It.IsAny<CancellationToken>()), Times.Exactly(1));
			httpClientMock.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Equals("https://xhubserver:3333/CTM/v1/ListCTM?details=True&folders=xt-folder%3A%2FTestProduct&onlyActive=True", StringComparison.Ordinal)), It.IsAny<CancellationToken>()), Times.Exactly(1));
			httpClientMock.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Equals("http://xhubserver:4444/ConfigApi/v1/objects?parent=xt-folder%3A%2F&inheritedfields=true", StringComparison.Ordinal)), It.IsAny<CancellationToken>()), Times.Exactly(1));
			httpClientMock.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Equals("http://xtdirectserver:2222/ConfigApi/v1/objects?parent=xt-folder%3A%2F&inheritedfields=true", StringComparison.Ordinal)), It.IsAny<CancellationToken>()), Times.Exactly(1));
		}

		[Test]
		public void xTHealthCheck_CTM_Test_WtgStatus_Unhealthy()
		{
			HealthCheckOptions.Add($"Options:{PathBase}/wtg/status:1", "ClickToMonitorHealthCheckXHub");

			var ctmObjectsResponse = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent(GetEmbeddedResourceAsString("xTHealthCheck.TestFiles.ClickToMonitor.CTMObjects.json"))
			};

			httpClientMock.Setup(x => x.SendAsync(It.Is<HttpRequestMessage>(req => req.RequestUri.ToString().EndsWith("/uat")), It.IsAny<CancellationToken>()))
				.ReturnsAsync(() =>
				{
					return new HttpResponseMessage
					{
						StatusCode = HttpStatusCode.OK,
						Content = new StringContent(new JObject(new JProperty("token", GenerateToken())).ToString())
					};
				});
			httpClientMock.Setup(x => x.SendAsync(It.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("/ConfigApi/v1/objects?")), It.IsAny<CancellationToken>()))
				.ReturnsAsync(() =>
				{
					return new HttpResponseMessage
					{
						StatusCode = HttpStatusCode.OK,
						Content = new StringContent(GetEmbeddedResourceAsString("xTHealthCheck.TestFiles.ClickToMonitor.XTFolderList.json"))
					};
				});
			httpClientMock.Setup(x => x.SendAsync(It.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("https://xtdirectserver:1111/CTM/v1/ListCTM?")), It.IsAny<CancellationToken>()))
			.Returns(Task.FromResult(ctmObjectsResponse));
			httpClientMock.Setup(x => x.SendAsync(It.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("https://xhubserver:3333/CTM/v1/ListCTM?")), It.IsAny<CancellationToken>()))
				.ReturnsAsync(() =>
				{
					return new HttpResponseMessage
					{
						StatusCode = HttpStatusCode.NoContent
					};
				});

			// Temporarily omitting " (...) for over ... seconds" due to xT bug - WI00727975
			var expectedResult = $"""
				{ExpectedHeathCheckUrlRow}
				{ExpectedHeathCheckServerHostName}
				ERROR(DirectxT-ctm_contract_mlte_5b898f7a-f289-4462-ba92-12078a80010f): Object: xt-contract:/Playground/SampleComponents/SampleComponents, Type: Monitor Message Lifetime, Description: Processing messages delay has exceeded its configured timeout
				ERROR(DirectxT-ctm_contract_amerrq_5b898f7a-f289-4462-ba92-12078a80010f): Object: xt-contract:/Playground/SampleComponents/SampleComponents, Type: Monitor Error Queue, Description: The queue has exceeded limit
				ERROR(DirectxT-ctm_contract_amoutq_5b898f7a-f289-4462-ba92-12078a80010f): Object: xt-contract:/Playground/SampleComponents/SampleComponents, Type: Monitor Out Queue, Description: The queue has exceeded limit
				ERROR(DirectxT-ctm_node_incsess_4d573d2f-d0cb-4a31-9274-7ebb92001147): Object: xt-node:/Playground/SampleComponents/FTPClientNode, Type: Incoming Sessions, Description: Incoming sessions has exceeded limit
				ERROR(DirectxT-ctm_node_outq_4d573d2f-d0cb-4a31-9274-7ebb92001147): Object: xt-node:/Playground/SampleComponents/FTPClientNode, Type: Outgoing Message Queue, Description: The queue has exceeded limit
				ERROR(DirectxT-ctm_contract_outportfailure_5b898f7a-f289-4462-ba92-12078a80010f): Object: xt-contract:/Playground/SampleComponents/SampleComponents, Type: Outport Failure, Description: The object is on hold status
				ERROR(DirectxT-ctm_node_outportfailure_4d573d2f-d0cb-4a31-9274-7ebb92001147): Object: xt-node:/Playground/SampleComponents/FTPClientNode, Type: Outport Failure, Description: The object is on hold status
				ERROR(DirectxT-ctm_party_outportfailure_fd0be703-430b-4666-bcb1-bd48c9421568): Object: xt-party:/Playground/SampleComponents/DemoParty, Type: Outport Failure, Description: The object is on hold status
				INFO(ClickToMonitorXHub): Healthy
				""";

			var response = GetClient().GetAsync($"{PathBase}/wtg/status").Result;
			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
			Assert.AreEqual("text/plain", response.Content.Headers.ContentType?.MediaType);
			Assert.AreEqual("utf-8", response.Content.Headers.ContentType.CharSet);

			var actual = response.Content.ReadAsStringAsync().Result;
			Assert.AreEqual(expectedResult, actual);
			ctmObjectsResponse.Dispose();
		}

		[Test]
		[TestCaseSource(nameof(ctmUnsuccessfulResponseTestCases))]
		public void xTHealthCheck_CTM_Test_WtgStatus_UnsuccessfulResponse(HttpStatusCode statusCode, string caller, string responseContent, string expectedErrorDetail)
		{
			var token = GenerateToken();
			var stubResponse = new HttpResponseMessage
			{
				StatusCode = statusCode
			};

			if (!string.IsNullOrEmpty(responseContent))
			{
				stubResponse.Content = new StringContent(responseContent);
			}

			httpClientMock.Setup(x => x.SendAsync(It.Is<HttpRequestMessage>(req => req.RequestUri.ToString().EndsWith("/uat")), It.IsAny<CancellationToken>()))
				.ReturnsAsync(() =>
				{
					return new HttpResponseMessage
					{
						StatusCode = HttpStatusCode.OK,
						Content = new StringContent(new JObject(new JProperty("token", GenerateToken())).ToString())
					};
				});
			httpClientMock.Setup(x => x.SendAsync(It.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("/ConfigApi/v1/objects?")), It.IsAny<CancellationToken>()))
				.ReturnsAsync(() =>
				{
					return new HttpResponseMessage
					{
						StatusCode = HttpStatusCode.OK,
						Content = new StringContent(GetEmbeddedResourceAsString("xTHealthCheck.TestFiles.ClickToMonitor.XTFolderList.json"))
					};
				});
			httpClientMock.Setup(x => x.SendAsync(It.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("/CTM/v1/ListCTM?")), It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(stubResponse));

			var expectedResult = $"""
				{ExpectedHeathCheckUrlRow}
				{ExpectedHeathCheckServerHostName}
				ERROR(TestProduct): System.Exception: Failed to process '{caller}' request. Status: {statusCode}.{expectedErrorDetail}
				""";

			var response = GetClient().GetAsync($"{PathBase}/wtg/status").Result;
			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
			Assert.AreEqual("text/plain", response.Content.Headers.ContentType?.MediaType);
			Assert.AreEqual("utf-8", response.Content.Headers.ContentType.CharSet);

			var actual = response.Content.ReadAsStringAsync().Result;
			Assert.AreEqual(expectedResult, actual);
			stubResponse.Dispose();
		}

		[Test]
		public void xTHealthCheck_CTM_Test_WtgStatus_WorkspaceTimeout_RenewToken()
		{
			HealthCheckOptions.Add($"Options:{PathBase}/wtg/status:1", "ClickToMonitorHealthCheckXHub");
			var utcNow = DateTime.Parse("2023-11-27T00:00:00Z", CultureInfo.InvariantCulture).ToUniversalTime();

			var tokenDict = new ConcurrentDictionary<string, string> { };
			var logonTimesDict = new ConcurrentDictionary<string, int> { };

			httpClientMock.Setup(x => x.SendAsync(It.Is<HttpRequestMessage>(req => req.RequestUri.ToString().EndsWith("/uat")), It.IsAny<CancellationToken>()))
				.Returns<HttpRequestMessage, CancellationToken>(
				(request, cancellationToken) =>
				{
					var workspace = request.RequestUri?.Port switch
					{
						1111 or 3333 => "Alarm",
						2222 or 4444 => "Rest",
						_ => throw new ArgumentOutOfRangeException()
					};

					var serverName = request.RequestUri.Host.ToString() switch
					{
						"xtdirectserver" => "Direct",
						"xhubserver" => "XHub",
						_ => throw new ArgumentOutOfRangeException()
					};

					var logonTimeKey = $"{workspace}{serverName}";
					logonTimesDict.AddOrUpdate(logonTimeKey, 1, (key, value) => value + 1);

					var token = GenerateToken(
						datetimeProviderMock.Object.UtcNow.AddSeconds(
							serverName == "Direct" ?
							xTRestOptionsMock.Object.Get(XTRestSettings.DirectxT).WorkspaceTimeoutInSeconds :
							xTRestOptionsMock.Object.Get(XTRestSettings.XHub).WorkspaceTimeoutInSeconds));
					tokenDict[$"{workspace}{serverName}{logonTimesDict[logonTimeKey]}"] = token;
					return Task.FromResult(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(new JObject(new JProperty("token", token)).ToString()) });
				});

			datetimeProviderMock.Setup(x => x.UtcNow).Returns(utcNow);

			httpClientMock.Setup(x => x.SendAsync(It.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("/ConfigApi/v1/objects?")), It.IsAny<CancellationToken>()))
				.ReturnsAsync(() =>
				{
					return new HttpResponseMessage
					{
						StatusCode = HttpStatusCode.OK,
						Content = new StringContent(GetEmbeddedResourceAsString("xTHealthCheck.TestFiles.ClickToMonitor.XTFolderList.json"))
					};
				});

			httpClientMock.Setup(x => x.SendAsync(It.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("/CTM/v1/ListCTM?")), It.IsAny<CancellationToken>()))
				.ReturnsAsync(() =>
				{
					return new HttpResponseMessage
					{
						StatusCode = HttpStatusCode.NoContent
					};
				});
			var expectedResult = $"""
				{ExpectedHeathCheckUrlRow}
				{ExpectedHeathCheckServerHostName}
				INFO(ClickToMonitorDirectxT): Healthy
				INFO(ClickToMonitorXHub): Healthy
				""";

			var response = GetClient().GetAsync($"{PathBase}/wtg/status").Result;
			Assert.Multiple(() =>
			{
				Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
				var actual = response.Content.ReadAsStringAsync().Result;
				Assert.AreEqual(expectedResult, actual);
				Assert.That(logonTimesDict["RestDirect"], Is.EqualTo(1), "RestDirect");
				Assert.That(logonTimesDict["AlarmDirect"], Is.EqualTo(1), "AlarmDirect");
				Assert.That(logonTimesDict["RestXHub"], Is.EqualTo(1), "RestXHub");
				Assert.That(logonTimesDict["AlarmXHub"], Is.EqualTo(1), "AlarmXHub");
			});

			// Both servers have not timed out
			utcNow = utcNow.AddSeconds(2);
			datetimeProviderMock.Setup(x => x.UtcNow).Returns(utcNow);
			response = GetClient().GetAsync($"{PathBase}/wtg/status").Result;
			Assert.Multiple(() =>
			{
				Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
				var actual = response.Content.ReadAsStringAsync().Result;
				Assert.AreEqual(expectedResult, actual);
				Assert.AreEqual(1, logonTimesDict["RestDirect"]);
				Assert.AreEqual(1, logonTimesDict["AlarmDirect"]);
				Assert.AreEqual(1, logonTimesDict["RestXHub"]);
				Assert.AreEqual(1, logonTimesDict["AlarmXHub"]);
			});

			// Direct xT timed out
			utcNow = utcNow.AddSeconds(4);
			datetimeProviderMock.Setup(x => x.UtcNow).Returns(utcNow);
			response = GetClient().GetAsync($"{PathBase}/wtg/status").Result;
			Assert.Multiple(() =>
			{
				Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
				var actual = response.Content.ReadAsStringAsync().Result;
				Assert.AreEqual(expectedResult, actual);
				Assert.AreEqual(2, logonTimesDict["RestDirect"]);
				Assert.AreEqual(2, logonTimesDict["AlarmDirect"]);
				Assert.AreEqual(1, logonTimesDict["RestXHub"]);
				Assert.AreEqual(1, logonTimesDict["AlarmXHub"]);
			});

			// Both servers have timed out
			utcNow = utcNow.AddSeconds(5);
			datetimeProviderMock.Setup(x => x.UtcNow).Returns(utcNow);
			response = GetClient().GetAsync($"{PathBase}/wtg/status").Result;
			Assert.Multiple(() =>
			{
				Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
				var actual = response.Content.ReadAsStringAsync().Result;
				Assert.AreEqual(expectedResult, actual);
				Assert.AreEqual(3, logonTimesDict["RestDirect"]);
				Assert.AreEqual(3, logonTimesDict["AlarmDirect"]);
				Assert.AreEqual(2, logonTimesDict["RestXHub"]);
				Assert.AreEqual(2, logonTimesDict["AlarmXHub"]);
			});

			// Direct xT relogin despite not timing out because remaining time is less than httpclient timeout 2 seconds
			utcNow = utcNow.AddSeconds(4);
			datetimeProviderMock.Setup(x => x.UtcNow).Returns(utcNow);
			response = GetClient().GetAsync($"{PathBase}/wtg/status").Result;
			Assert.Multiple(() =>
			{
				Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
				var actual = response.Content.ReadAsStringAsync().Result;
				Assert.AreEqual(expectedResult, actual);
				Assert.AreEqual(4, logonTimesDict["RestDirect"]);
				Assert.AreEqual(4, logonTimesDict["AlarmDirect"]);
				Assert.AreEqual(2, logonTimesDict["RestXHub"]);
				Assert.AreEqual(2, logonTimesDict["AlarmXHub"]);
			});

			httpClientMock.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(req => DoesRequestMatch(req, "http://xtdirectserver:2222/ConfigApi/v1/objects?", tokenDict["RestDirect1"])), It.IsAny<CancellationToken>()), Times.Exactly(2));
			httpClientMock.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(req => DoesRequestMatch(req, "https://xtdirectserver:1111/CTM/v1/ListCTM?", tokenDict["AlarmDirect1"])), It.IsAny<CancellationToken>()), Times.Exactly(2));
			httpClientMock.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(req => DoesRequestMatch(req, "http://xtdirectserver:2222/ConfigApi/v1/objects?", tokenDict["RestDirect2"])), It.IsAny<CancellationToken>()), Times.Exactly(1));
			httpClientMock.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(req => DoesRequestMatch(req, "https://xtdirectserver:1111/CTM/v1/ListCTM?", tokenDict["AlarmDirect2"])), It.IsAny<CancellationToken>()), Times.Exactly(1));
			httpClientMock.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(req => DoesRequestMatch(req, "http://xtdirectserver:2222/ConfigApi/v1/objects?", tokenDict["RestDirect3"])), It.IsAny<CancellationToken>()), Times.Exactly(1));
			httpClientMock.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(req => DoesRequestMatch(req, "https://xtdirectserver:1111/CTM/v1/ListCTM?", tokenDict["AlarmDirect3"])), It.IsAny<CancellationToken>()), Times.Exactly(1));
			httpClientMock.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(req => DoesRequestMatch(req, "http://xtdirectserver:2222/ConfigApi/v1/objects?", tokenDict["RestDirect4"])), It.IsAny<CancellationToken>()), Times.Exactly(1));
			httpClientMock.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(req => DoesRequestMatch(req, "https://xtdirectserver:1111/CTM/v1/ListCTM?", tokenDict["AlarmDirect4"])), It.IsAny<CancellationToken>()), Times.Exactly(1));

			httpClientMock.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(req => DoesRequestMatch(req, "http://xhubserver:4444/ConfigApi/v1/objects?", tokenDict["RestXHub1"])), It.IsAny<CancellationToken>()), Times.Exactly(3));
			httpClientMock.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(req => DoesRequestMatch(req, "https://xhubserver:3333/CTM/v1/ListCTM?", tokenDict["AlarmXHub1"])), It.IsAny<CancellationToken>()), Times.Exactly(3));
			httpClientMock.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(req => DoesRequestMatch(req, "http://xhubserver:4444/ConfigApi/v1/objects?", tokenDict["RestXHub2"])), It.IsAny<CancellationToken>()), Times.Exactly(2));
			httpClientMock.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(req => DoesRequestMatch(req, "https://xhubserver:3333/CTM/v1/ListCTM?", tokenDict["AlarmXHub2"])), It.IsAny<CancellationToken>()), Times.Exactly(2));
		}

		[Test]
		public void xTHealthCheck_CTM_Test_WtgStatus_MemoryCache_BypassRestApiCall()
		{
			HealthCheckOptions.Add($"Options:{PathBase}/wtg/status:1", "ClickToMonitorHealthCheckXHub");

			httpClientMock.Setup(x => x.SendAsync(It.Is<HttpRequestMessage>(req => req.RequestUri.ToString().EndsWith("/uat")), It.IsAny<CancellationToken>()))
				.ReturnsAsync(() =>
				{
					return new HttpResponseMessage
					{
						StatusCode = HttpStatusCode.OK,
						Content = new StringContent(new JObject(new JProperty("token", GenerateToken())).ToString())
					};
				});

			httpClientMock.Setup(x => x.SendAsync(It.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("/ConfigApi/v1/objects?")), It.IsAny<CancellationToken>()))
				.ReturnsAsync(() =>
				{
					return new HttpResponseMessage
					{
						StatusCode = HttpStatusCode.OK,
						Content = new StringContent(GetEmbeddedResourceAsString("xTHealthCheck.TestFiles.ClickToMonitor.XTFolderList.json"))
					};
				});

			httpClientMock.Setup(x => x.SendAsync(It.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("/CTM/v1/ListCTM?")), It.IsAny<CancellationToken>()))
				.ReturnsAsync(() =>
				{
					return new HttpResponseMessage
					{
						StatusCode = HttpStatusCode.NoContent
					};
				});
			var expectedResult = $"""
				{ExpectedHeathCheckUrlRow}
				{ExpectedHeathCheckServerHostName}
				INFO(ClickToMonitorDirectxT): Healthy
				INFO(ClickToMonitorXHub): Healthy
				""";

			var client = GetClient();
			var response = client.GetAsync($"{PathBase}/wtg/status").Result;
			Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
			Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("text/plain"));
			Assert.That(response.Content.Headers.ContentType.CharSet, Is.EqualTo("utf-8"));
			var actual = response.Content.ReadAsStringAsync().Result;
			Assert.That(actual, Is.EqualTo(expectedResult));

			response = client.GetAsync($"{PathBase}/wtg/status").Result;
			Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
			Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("text/plain"));
			Assert.That(response.Content.Headers.ContentType.CharSet, Is.EqualTo("utf-8"));
			actual = response.Content.ReadAsStringAsync().Result;
			Assert.That(actual, Is.EqualTo(expectedResult));

			httpClientMock.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Equals("https://xtdirectserver:1111/CTM/v1/ListCTM?details=True&folders=xt-folder%3A%2FTestProduct&onlyActive=True", StringComparison.Ordinal)), It.IsAny<CancellationToken>()), Times.Exactly(2));
			httpClientMock.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Equals("https://xhubserver:3333/CTM/v1/ListCTM?details=True&folders=xt-folder%3A%2FTestProduct&onlyActive=True", StringComparison.Ordinal)), It.IsAny<CancellationToken>()), Times.Exactly(2));
			httpClientMock.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Equals("http://xhubserver:4444/ConfigApi/v1/objects?parent=xt-folder%3A%2F&inheritedfields=true", StringComparison.Ordinal)), It.IsAny<CancellationToken>()), Times.Exactly(1));
			httpClientMock.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Equals("http://xtdirectserver:2222/ConfigApi/v1/objects?parent=xt-folder%3A%2F&inheritedfields=true", StringComparison.Ordinal)), It.IsAny<CancellationToken>()), Times.Exactly(1));
		}

		[Test]
		public async Task xTHealthCheck_CTM_Test_Path_With_subFolders()
		{
			HealthCheckOptions.Add($"Options:{PathBase}/TestProductSubFolder/wtg/status:0", "ClickToMonitorHealthCheckDirectxT");
			HealthCheckOptions.Add($"Options:{PathBase}/TestProductSubFolder/wtg/status:1", "ClickToMonitorHealthCheckXHub");

			var configApiResponse = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent(GetEmbeddedResourceAsString("xTHealthCheck.TestFiles.ClickToMonitor.XTSubFolderList.json"))
			};

			httpClientMock.Setup(x => x.SendAsync(It.Is<HttpRequestMessage>(req => req.RequestUri.ToString().EndsWith("/uat")), It.IsAny<CancellationToken>()))
				.ReturnsAsync(() =>
				{
					return new HttpResponseMessage
					{
						StatusCode = HttpStatusCode.OK,
						Content = new StringContent(new JObject(new JProperty("token", GenerateToken())).ToString())
					};
				});
			httpClientMock.Setup(x => x.SendAsync(It.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("/ConfigApi/v1/objects?")), It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(configApiResponse));

			httpClientMock.Setup(x => x.SendAsync(It.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("/CTM/v1/ListCTM?")), It.IsAny<CancellationToken>()))
				.ReturnsAsync(() =>
				{
					return new HttpResponseMessage
					{
						StatusCode = HttpStatusCode.NoContent
					};
				});

			var expectedResult = $"""
				INFO(Health Check Url): http://localhost/TestProduct/TestProductSubFolder/wtg/status
				{ExpectedHeathCheckServerHostName}
				INFO(ClickToMonitorDirectxT): Healthy
				INFO(ClickToMonitorXHub): Healthy
				""";

			var response = await GetClient().GetAsync($"{PathBase}/TestProductSubFolder/wtg/status");

			Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
			Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("text/plain"));
			Assert.That(response.Content.Headers.ContentType.CharSet, Is.EqualTo("utf-8"));
			var actual = response.Content.ReadAsStringAsync().Result;
			Assert.That(actual, Is.EqualTo(expectedResult));

			httpClientMock.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Equals("https://xtdirectserver:1111/CTM/v1/ListCTM?details=True&folders=xt-xview%3A%2FTestProduct%2FTestProductSubFolder&onlyActive=True", StringComparison.Ordinal)), It.IsAny<CancellationToken>()), Times.Exactly(1));
			httpClientMock.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Equals("https://xhubserver:3333/CTM/v1/ListCTM?details=True&folders=xt-xview%3A%2FTestProduct%2FTestProductSubFolder&onlyActive=True", StringComparison.Ordinal)), It.IsAny<CancellationToken>()), Times.Exactly(1));
			httpClientMock.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Equals("http://xtdirectserver:2222/ConfigApi/v1/objects?parent=xt-folder%3A%2FTestProduct&inheritedfields=true", StringComparison.Ordinal)), It.IsAny<CancellationToken>()), Times.Exactly(1));
			httpClientMock.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Equals("http://xhubserver:4444/ConfigApi/v1/objects?parent=xt-folder%3A%2FTestProduct&inheritedfields=true", StringComparison.Ordinal)), It.IsAny<CancellationToken>()), Times.Exactly(1));

			configApiResponse.Dispose();
		}

		string GenerateToken(DateTime? expiry = null)
		{
			var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()));
			var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
			var claims = new[]
			{
				new Claim(ClaimTypes.NameIdentifier, "TestUser"),
				new Claim(ClaimTypes.Role, "Test"),
				new Claim("exp", new DateTimeOffset(expiry ?? DateTime.UtcNow.AddDays(7)).ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture)),
			};
			var token = new JwtSecurityToken("http://localhost",
				"http://localhost",
				claims,
				signingCredentials: credentials);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}

		bool DoesRequestMatch(HttpRequestMessage req, string url, string token)
		{
			return req.RequestUri.ToString().StartsWith(url, StringComparison.Ordinal) && req.Headers.Authorization?.Parameter == token;
		}

		private Mock<IOptionsMonitor<XTRestSettings>> xTRestOptionsMock = new();

		private static TestCaseData[] ctmUnsuccessfulResponseTestCases =
		{
			new TestCaseData(HttpStatusCode.InternalServerError, "ListCTM", null, string.Empty).SetName("xTHealthCheck_CTM_Test_InternalServerError"),
			new TestCaseData(HttpStatusCode.BadRequest, "ListCTM", GetEmbeddedResourceAsString("xTHealthCheck.TestFiles.ClickToMonitor.ProblemDetail.json"), " Detail: \"Workspace timeouted\"").SetName("xTHealthCheck_CTM_Test_BadRequest"),
			new TestCaseData(HttpStatusCode.Unauthorized, "ListCTM", "{\r\n    \"message\": \"Unauthorized\"\r\n}", string.Empty).SetName("xTHealthCheck_CTM_Test_Unauthorized")
		};
	}
}
