using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.Telematics.ServiceTasks;
using Enterprise.Client.EDI.Telematics.Tca;
using Enterprise.Integration;
using Enterprise.Telematics.Business.Registry;
using Moq;
using Moq.Protected;
using IHttpClientFactory = WTG.Foundation.Http.IHttpClientFactory;

namespace ZClientEDI.Test.Telematics.ServiceTasks
{
	public class EnrolmentSenderTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			base.SetUp();
			TelematicsConfigurationRegistry.Instance.TcaRimUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testUrl);
			httpMessageHandlerMock = new Mock<HttpMessageHandler>();
			httpClientFactoryMock = new Mock<IHttpClientFactory>();
			loggerMock = new Mock<ILogger>();
			enrollmentSender = new EnrolmentSender(httpClientFactoryMock.Object, loggerMock.Object, TimeSpan.FromSeconds(10));
		}

		string testUrl { get; } = "https://test/";
		EnrolmentSender enrollmentSender;
		Mock<HttpMessageHandler> httpMessageHandlerMock;
		Mock<IHttpClientFactory> httpClientFactoryMock;
		Mock<ILogger> loggerMock;

		public void TestEnrollmentReportSentToTca()
		{
			CombineAssertions(
				() =>
				{
					Test($@"{testUrl}enrolment-report/WTG00000001");
					Test($@"{testUrl}enrolment-report/WTG00000002");
				});

			void Test(string expectedUri)
			{
				// Arrange
				httpMessageHandlerMock.Reset();
				var httpClient = new HttpClient(httpMessageHandlerMock.Object);
				httpClientFactoryMock.Setup(factory => factory.CreateNew(It.IsAny<HttpMessageHandler>(), It.IsAny<TimeSpan>())).Returns(httpClient);
				var returnCode = HttpStatusCode.Accepted;
				var enrolmentReport = new EnrolmentReportType();
				var retrievedRequest = string.Empty;
				httpMessageHandlerMock
					.Protected()
					.Setup<Task<HttpResponseMessage>>(
						"SendAsync",
						ItExpr.IsAny<HttpRequestMessage>(),
						ItExpr.IsAny<CancellationToken>())
					.Callback((HttpRequestMessage message, CancellationToken ct) =>
					{
						retrievedRequest = message.Content.ReadAsStringAsync().Result;
					})
					.ReturnsAsync(new HttpResponseMessage()
					{
						StatusCode = returnCode,
						Content = new StringContent("VeryNice!!!"),
					});

				// Act
				AssertNoExceptionThrown(
					() =>
					{
						enrollmentSender.Send(Factory, enrolmentReport);
					});

				// Assert
				httpMessageHandlerMock.Protected().Verify<Task<HttpResponseMessage>>(
					"SendAsync",
					Times.Once(),
					ItExpr.Is<HttpRequestMessage>(
						message =>
							message.Method == HttpMethod.Put &&
							message.RequestUri.AbsoluteUri == expectedUri),
					ItExpr.IsAny<CancellationToken>());
			}
		}

		public void TestUnsuccessfulHttpRequest()
		{
			CombineAssertions(
				() =>
				{
					Test(HttpStatusCode.Forbidden, "NOT NICE!");
					Test(HttpStatusCode.BadGateway, "OH NO!");
				});

			void Test(HttpStatusCode returnCode, string response)
			{
				// Arrange
				httpMessageHandlerMock.Reset();
				loggerMock.Reset();
				var httpClient = new HttpClient(httpMessageHandlerMock.Object);
				httpClientFactoryMock.Setup(factory => factory.CreateNew(It.IsAny<HttpMessageHandler>(), It.IsAny<TimeSpan>())).Returns(httpClient);
				var enrolmentReport = new EnrolmentReportType();
				loggerMock.Setup(logger => logger.Log(It.IsAny<LogType>(), It.IsAny<string>()));
				httpMessageHandlerMock
					.Protected()
					.Setup<Task<HttpResponseMessage>>(
						"SendAsync",
						ItExpr.IsAny<HttpRequestMessage>(),
						ItExpr.IsAny<CancellationToken>())
					.ReturnsAsync(new HttpResponseMessage()
					{
						StatusCode = returnCode,
						Content = new StringContent(response),
					});

				// Act
				AssertNoExceptionThrown(
					() =>
					{
						enrollmentSender.Send(Factory, enrolmentReport);
					});

				// Assert
				httpMessageHandlerMock.Protected().Verify<Task<HttpResponseMessage>>(
					"SendAsync",
					Times.Once(),
					ItExpr.IsAny<HttpRequestMessage>(),
					ItExpr.IsAny<CancellationToken>());
				loggerMock.Verify(logger => logger.Log(LogType.Error, $"TCA Enrollment report failure, Status Code: {returnCode} Response: {response}"));
			}
		}

		public void TestWrongParamsCall()
		{
			CombineAssertions(() =>
			{
				var result = AssertExceptionThrown<ArgumentNullException>(() => new EnrolmentSender(null, loggerMock.Object, TimeSpan.Zero));
				AssertEquals("httpClientFactory", result.ParamName);

				result = AssertExceptionThrown<ArgumentNullException>(() => new EnrolmentSender(httpClientFactoryMock.Object, null, TimeSpan.Zero));
				AssertEquals("logger", result.ParamName);
			});
		}
	}
}
