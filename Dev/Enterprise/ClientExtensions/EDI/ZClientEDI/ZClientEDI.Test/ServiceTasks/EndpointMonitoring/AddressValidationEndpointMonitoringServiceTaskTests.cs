using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
#if NETFRAMEWORK
using System.Runtime.Serialization;
#endif
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Common;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Client.EDI.ServiceTasks.Testing
{
	[TestedType(typeof(AddressValidationEndpointMonitoringServiceTask))]
	class AddressValidationEndpointMonitoringServiceTaskTests : ServiceTaskTestCase<AddressValidationEndpointMonitoringServiceTask>
	{
		public void TestHostedService()
		{
			var attribute = GetHostedServiceAttributes().Single();
			AssertEquals("AXM", attribute.Code);
			AssertEquals("Address Validation Endpoint Monitoring", attribute.Description);
			AssertEquals("SYS", attribute.Category);
			AssertEquals("Enterprise.Client.EDI.ServiceTasks.AddressValidationEndpointMonitoringServiceTask", attribute.TypeName);
			AssertEquals("ZClientEDI", attribute.TypeAssemblyName);
			AssertEquals(expected: true, attribute.CanRunInAnyBranch);
			AssertEquals(expected: true, attribute.IsMandatory);
			AssertEquals(expected: false, attribute.AllowsMultipleInstances);
			AssertEquals("1minute", attribute.MinimumPeriod);
			AssertEquals("30minutes", attribute.DefaultScheduleRunEvery);
		}

		[ExpectNoExceptions]
		public void TestNoAvsMonitoringEndpointConfigured()
		{
			var task = new AddressValidationEndpointMonitoringServiceTaskForTest();
			using (EDIDataRegistry.Instance.AvsMonitoringEndpointsItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Array.Empty<string>()))
			{
				task.RunTask(CancellationToken.None);
			}

			task.MockedLogger.Verify(x => x.Log(It.IsAny<LogType>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
			task.MockedNotifier.Verify(x => x.Notify(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()), Times.Never);
			AssertNull(ErrorReporter.LastExceptionReported);
		}

		[ExpectNoExceptions]
		public void TestServiceTaskCancelled()
		{
			const string UnexpectedException = "Unexpected exception occurred in AVS endpoint monitoring!";

			var tokenSource = new CancellationTokenSource();
			tokenSource.Cancel();

			var task = new AddressValidationEndpointMonitoringServiceTaskForTest();
			using (EDIDataRegistry.Instance.AvsMonitoringEndpointsItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new [] { "https://fake.url/" }))
			{
				task.RunTask(tokenSource.Token);
			}

			task.MockedLogger.Verify(x => x.Log(LogType.Error, UnexpectedException, It.IsAny<OperationCanceledException>()), Times.Once);
			task.MockedNotifier.Verify(x => x.Notify(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()), Times.Never);
			AssertType<OperationCanceledException>(ErrorReporter.LastExceptionReported);

			ErrorReporter.Clear();
		}

		[ExpectNoExceptions]
		public void TestInvalidEndpointConfigured()
		{
			const string UnexpectedException = "Unexpected exception occurred in AVS endpoint monitoring!";

			var task = new AddressValidationEndpointMonitoringServiceTaskForTest();
			using (EDIDataRegistry.Instance.AvsMonitoringEndpointsItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new [] { "https://fake.url/", "InvalidURL" }))
			{
				task.RunTask(CancellationToken.None);
			}

			task.MockedLogger.Verify(x => x.Log(LogType.Error, UnexpectedException, It.IsAny<UriFormatException>()), Times.Once);
			task.MockedNotifier.Verify(x => x.Notify(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()), Times.Never);
			AssertType<UriFormatException>(ErrorReporter.LastExceptionReported);

			ErrorReporter.Clear();
		}

		[ExpectNoExceptions]
		public void TestUnexpectedExceptionFromMonitoring()
		{
			const string UnexpectedException = "Unexpected exception occurred in AVS endpoint monitoring!";

			var task = new AddressValidationEndpointMonitoringServiceTaskForTest();
			task.MockedHandler.Protected().As<IMockHttpMessageHandler>()
				.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
				.Throws<FakeException>();
			using (EDIDataRegistry.Instance.AvsMonitoringEndpointsItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new [] { "https://fake.url/" }))
			{
				task.RunTask(CancellationToken.None);
			}

			task.MockedLogger.Verify(x => x.Log(LogType.Error, UnexpectedException, It.IsAny<AggregateException>()), Times.Once);
			task.MockedNotifier.Verify(x => x.Notify(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()), Times.Never);
			AssertType<FakeException>(ErrorReporter.LastExceptionReported.InnerException);

			ErrorReporter.Clear();
		}

		[ExpectNoExceptions]
		public void TestAllEndpointsAvailable()
		{
			var task = new AddressValidationEndpointMonitoringServiceTaskForTest();
			task.MockedHandler.Protected().As<IMockHttpMessageHandler>()
				.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(@"{""ServiceAvailable"":""true""}") }));
			using (EDIDataRegistry.Instance.AvsMonitoringEndpointsItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new [] { "https://fake.url1/", "https://fake.url2/" }))
			{
				task.RunTask(CancellationToken.None);
			}

			task.MockedNotifier.Verify(x => x.Notify(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()), Times.Never);
		}

		[ExpectNoExceptions]
		public void TestUnhealthyEndpoints()
		{
			const string NotifyTitle = "[AVS Monitoring]: Unhealthy Endpoint(s)";
			const string NotifyMessage = @"The following AVS endpoint(s) were unhealthy!

Endpoint [fake.url2] is unhealthy:
	Fake DiagnosticMessage!

Endpoint [fake.url3] is unhealthy:
	[Spectrum Accessible] Spectrum Server A is accessible.
[Spectrum Disabled] Spectrum Server B is disabled.
[Database Error] Database connection has failed.


Endpoint [fake.url4] is unhealthy:
	[Spectrum Accessible] Spectrum Server A is accessible.
[Spectrum Inaccessible] Spectrum Server B is inaccessible.
[Database Healthy] Database connection is OK.


Endpoint [fake.url5] is unhealthy:
	Response status code does not indicate success: 500 (Internal Server Error).
";

			var responses = new []
			{
				new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(@"{""ServiceAvailable"":""true""}") },
				new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(@"{""ServiceAvailable"":""false"", ""DiagnosticMessage"":""Fake DiagnosticMessage!""}") },
				new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(@"{""ServiceAvailable"":""true"", ""DiagnosticMessage"":""[Spectrum Accessible] Spectrum Server A is accessible.\r\n[Spectrum Disabled] Spectrum Server B is disabled.\r\n[Database Error] Database connection has failed.\r\n""}") },
				new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(@"{""ServiceAvailable"":""true"", ""DiagnosticMessage"":""[Spectrum Accessible] Spectrum Server A is accessible.\r\n[Spectrum Inaccessible] Spectrum Server B is inaccessible.\r\n[Database Healthy] Database connection is OK.\r\n""}") },
				new HttpResponseMessage { StatusCode = HttpStatusCode.InternalServerError }
			};
			var responseCount = 0;

			var task = new AddressValidationEndpointMonitoringServiceTaskForTest();
			task.MockedHandler.Protected().As<IMockHttpMessageHandler>()
				.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
				.Returns((HttpRequestMessage request, CancellationToken token) => Task.FromResult(responses[responseCount++]));
			using (EDIDataRegistry.Instance.AvsMonitoringEndpointsItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new [] { "https://fake.url1/v2/", "https://fake.url2/v2/", "https://fake.url3/v2/", "https://fake.url4/v2/", "https://fake.url5/v2/" }))
			{
				task.RunTask(CancellationToken.None);
			}

			task.MockedLogger.Verify(x => x.Log(LogType.Error, "Endpoint [fake.url2] is unhealthy!"), Times.Once);
			task.MockedLogger.Verify(x => x.Log(LogType.Error, "Endpoint [fake.url3] is unhealthy!"), Times.Once);
			task.MockedLogger.Verify(x => x.Log(LogType.Error, "Endpoint [fake.url4] is unhealthy!"), Times.Once);
			task.MockedLogger.Verify(x => x.Log(LogType.Error, "Endpoint [fake.url5] is unhealthy!"), Times.Once);
			task.MockedNotifier.Verify(x => x.Notify(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()), Times.Once);
			task.MockedNotifier.Verify(x => x.Notify(NotifyTitle, NotifyMessage, It.Is<Dictionary<string, string>>(attachments => attachments.Count == 4)), Times.Once);
		}

		[ExpectNoExceptions]
		public void TestNotifyFailed()
		{
			var response = new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(@"{""ServiceAvailable"":""false"" }") };

			var task = new AddressValidationEndpointMonitoringServiceTaskForTest();
			task.MockedHandler.Protected().As<IMockHttpMessageHandler>()
				.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
				.Returns((HttpRequestMessage request, CancellationToken token) => Task.FromResult(response));
			task.MockedNotifier.Setup(x => x.Notify(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IDictionary<string, string>>())).Throws(new InvalidOperationException());
			using (EDIDataRegistry.Instance.AvsMonitoringEndpointsItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new [] { "https://fake.url/v2/" }))
			{
				task.RunTask(CancellationToken.None);
			}

			task.MockedLogger.Verify(x => x.Log(LogType.Error, "Endpoint [fake.url] is unhealthy!"), Times.Once);
			task.MockedLogger.Verify(x => x.Log(LogType.Error, "Cannot create and save notification email!", It.IsAny<InvalidOperationException>()), Times.Once);
			AssertType<InvalidOperationException>(ErrorReporter.LastExceptionReported);

			ErrorReporter.Clear();
		}

		[ExpectNoExceptions]
		public void TestEndpointTimeout()
		{
			const string NotifyTitle = "[AVS Monitoring]: Unhealthy Endpoint(s)";
			const string NotifyMessage = @"The following AVS endpoint(s) were unhealthy!

Endpoint [fake.url] is unhealthy:
	A task was canceled.
";

			var task = new AddressValidationEndpointMonitoringServiceTaskForTest();
			task.MockedHandler.Protected().As<IMockHttpMessageHandler>()
				.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
				.Throws<TaskCanceledException>();
			using (RawDataRegistry.Instance.AddressValidationWebServiceTimeout.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 5))
			using (EDIDataRegistry.Instance.AvsMonitoringEndpointsItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new [] { "https://fake.url/v2/" }))
			{
				task.RunTask(CancellationToken.None);
			}

			AssertEquals(5, (int)task.ExposedClient.Timeout.TotalSeconds);
			task.MockedLogger.Verify(x => x.Log(LogType.Error, "Endpoint [fake.url] is unhealthy!"), Times.Once);
			task.MockedNotifier.Verify(x => x.Notify(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()), Times.Once);
			task.MockedNotifier.Verify(x => x.Notify(NotifyTitle, NotifyMessage, It.Is<Dictionary<string, string>>(attachments => attachments.Count == 1)), Times.Once);
		}

		[ExpectNoExceptions]
		public void TestNotifyUnhealthyEndpointsEvenUnexpectedExceptionOccurred()
		{
			const string UnexpectedException = "Unexpected exception occurred in AVS endpoint monitoring!";
			const string NotifyTitle = "[AVS Monitoring]: Unhealthy Endpoint(s)";
			const string NotifyMessage = @"The following AVS endpoint(s) were unhealthy!

Endpoint [fake.url1] is unhealthy:
	Fake DiagnosticMessage!
";

			var response = new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(@"{""ServiceAvailable"":""false"", ""DiagnosticMessage"":""Fake DiagnosticMessage!""}") };

			var tokenSource = new CancellationTokenSource();

			var task = new AddressValidationEndpointMonitoringServiceTaskForTest();
			task.MockedHandler.Protected().As<IMockHttpMessageHandler>()
				.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
				.Returns((HttpRequestMessage request, CancellationToken token) =>
				{
					tokenSource.Cancel();
					return Task.FromResult(response);
				});
			using (EDIDataRegistry.Instance.AvsMonitoringEndpointsItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new [] { "https://fake.url1/v2/", "https://fake.url2/v2/", "https://fake.url3/v2/" }))
			{
				task.RunTask(tokenSource.Token);
			}

			task.MockedLogger.Verify(x => x.Log(LogType.Error, "Endpoint [fake.url1] is unhealthy!"), Times.Once);
			task.MockedLogger.Verify(x => x.Log(LogType.Error, UnexpectedException, It.IsAny<OperationCanceledException>()), Times.Once);
			task.MockedNotifier.Verify(x => x.Notify(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()), Times.Once);
			task.MockedNotifier.Verify(x => x.Notify(NotifyTitle, NotifyMessage, It.Is<Dictionary<string, string>>(attachments => attachments.Count == 1)), Times.Once);
			AssertType<OperationCanceledException>(ErrorReporter.LastExceptionReported);

			ErrorReporter.Clear();
		}

		[ExpectNoExceptions]
		public void TestComprehensiveCheckQuery()
		{
			var task = new AddressValidationEndpointMonitoringServiceTaskForTest();
			var mockedHandler = task.MockedHandler.Protected().As<IMockHttpMessageHandler>();
			mockedHandler
				.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(@"{""ServiceAvailable"":""true""}") }));
			using (EDIDataRegistry.Instance.AvsMonitoringEndpointsItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new [] { "https://fake.url/v2/" }))
			{
				task.RunTask(CancellationToken.None);
			}

			mockedHandler.Verify((x => x.SendAsync(
				It.Is<HttpRequestMessage>(hrm => hrm.RequestUri.ToString() != null),
				It.IsAny<CancellationToken>())
			), Times.Once());
		}

		[ExpectNoExceptions]
		public void TestThrowCriticalException()
		{
			var task = new AddressValidationEndpointMonitoringServiceTaskForTest();
			task.MockedHandler.Protected().As<IMockHttpMessageHandler>()
				.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
				.Throws(new FakeException { IsCriticalException = true });
			using (EDIDataRegistry.Instance.AvsMonitoringEndpointsItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new [] { "https://fake.url/" }))
			{
				var exception = AssertExceptionThrown<AggregateException>(() => task.RunTask(CancellationToken.None)).InnerException;
				AssertType<FakeException>(exception);
			}

			task.MockedNotifier.Verify(x => x.Notify(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()), Times.Never);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
	}

	class AddressValidationEndpointMonitoringServiceTaskForTest : AddressValidationEndpointMonitoringServiceTask
	{
		public AddressValidationEndpointMonitoringServiceTaskForTest()
		{
			ServiceLogger = MockedLogger.Object;
		}

		public Mock<IStatusNotifier> MockedNotifier { get; } = new Mock<IStatusNotifier>();
		public Mock<HttpMessageHandler> MockedHandler { get; } = new Mock<HttpMessageHandler>();
		public Mock<ILogger> MockedLogger { get; } = new Mock<ILogger>();
		public HttpClient ExposedClient => Client;

		protected override IStatusNotifier GetNewStatusNotifier() => MockedNotifier.Object;
		protected override HttpClient GetNewHttpClient() => new HttpClient(MockedHandler.Object);
	}

	[Serializable]
	class FakeException : Exception, ICriticalException
	{
		public FakeException()
		{
		}

#if NETFRAMEWORK
		protected FakeException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
#endif

		public bool IsCriticalException { get; set; }
	}

	interface IMockHttpMessageHandler
	{
		Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken);
	}
}
