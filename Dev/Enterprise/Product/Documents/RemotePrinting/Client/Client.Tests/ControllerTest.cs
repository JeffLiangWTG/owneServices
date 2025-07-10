using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.Win32;
using Moq;
using NUnit.Framework;
using static Enterprise.RemotePrinting.Client.Tests.ConnectionRegistryManagerTest;

namespace Enterprise.RemotePrinting.Client.Tests
{
	public class ControllerTest : TestCase
	{
		[TestRequiresAdministrativePrivileges("Admin privilege is required to edit the Registry")]
		public void TestProtectWebServicePasswordForAllConfigurationsWhenInitialiseWebServiceClient()
		{
			var controller = new TestController2();
			var webConfig1 = new WebClientConfiguration("http://test.url1", "user1", "pwd1", 1, "machine1", false, "proxy1", 43, "proxyuser1", "proxypwd1", false, false, 5000, false, 0, 0, 100, 100, false, false, 0, 0, 0, false, 4, 60, true, 0, GetWebClientUpdateConfigurationForTest(), false, 100);
			var webConfig2 = new WebClientConfiguration("http://test.url2", "user2", "pwd2", 2, "machine2", true, "proxy2", 43, "proxyuser2", "proxypwd2", true, false, 7000, true, 300, 5, 100, 100, false, false, 0, 0, 0, false, 0, 0, false, 15, GetWebClientUpdateConfigurationForTest(), false, 100);
			var manager = controller.ConnectionRegistryManager_Exposed;
			var configName1 = "configForTest1";
			var configName2 = "configForTest2";
			controller.ConfigName = configName1;

			try
			{
				manager.LoadFromRegistry(configName1);
				manager.SaveRemotePrintingRegistryValues(webConfig1);
				manager.LoadFromRegistry(configName2);
				manager.SaveRemotePrintingRegistryValues(webConfig2);

				controller.InitialiseWebServiceClient_Exposed();

				var config1 = manager.GetWebClientConfiguration(configName1);
				var config2 = manager.GetWebClientConfiguration(configName2);

				CombineAssertions(() =>
				{
					AssertNotEquals("pwd1", config1.WebServicePwd);
					AssertEquals("pwd1", ProtectedDataHelper.Unprotect(config1.WebServicePwd));

					AssertNotEquals("pwd2", config2.WebServicePwd);
					AssertEquals("pwd2", ProtectedDataHelper.Unprotect(config2.WebServicePwd));
				});
			}
			finally
			{
				manager.DeleteFromRegistry(configName1);
				manager.DeleteFromRegistry(configName2);
				Registry.CurrentUser.DeleteSubKey(((ConnectionRegistryManagerForTest)manager).webPrintKeyName_Exposed, false);
				Registry.CurrentUser.DeleteSubKey(TestEdiKeyName, false);
			}
		}

		public void TestHandleWebException_RelatedDBConnectionException()
		{
			var exceptionMessage = $"The request failed with HTTP status 538: Timeout expired. The timeout period elapsed prior to obtaining a connection from the pool. This may have occurred because all pooled connections were in use and max pool size was reached.";
			var expectedMessage = $@"
Web Print suspended due to all pooled database connections are in use, please try again later or contact us if the problem persists.
Error details:
Timeout expired. The timeout period elapsed prior to obtaining a connection from the pool. This may have occurred because all pooled connections were in use and max pool size was reached.

Please contact your system administrator if the problem persists.

Retrying in 120 seconds.
";

			var logs = new List<string>();
			var controller = new TestController();
			controller.ShowError += (_, e) => logs.Add(e.Message);
			var ex = new WebException(exceptionMessage, WebExceptionStatus.ProtocolError);
			var handled = controller.HandleWebExceptionExposed(ex, false, true);

			Assert("Should handle exception", handled);
			AssertEquals(1, logs.Count);
			AssertEquals(expectedMessage, logs[0]);
		}

		public void TestHandleWebException_BadRequest_DoNotThrowWithExecutedSuccessfullyBefore()
		{
			var logs = new List<string>();
			var operations = new HashSet<string>();

			var mock = new Mock<IRemotePrintingServiceAdaptor>();
			mock.Setup(m => m.ExecutedSuccessfullyOperations).Returns(operations);
			var webClient = new WebClient(mock.Object);

			var controller = new TestController();
			controller.ShowError += (_, e) => logs.Add(e.Message);
			controller.WebServiceClientForTest = webClient;

			var ex = new WebException("The request failed with HTTP status 400: Bad request.", WebExceptionStatus.ProtocolError);
			var expectedMessage = @"
Web Print suspended due to bad request.
Please contact your system administrator if the problem persists.
";

			Assert("Should not handle bad request", !controller.HandleWebExceptionExposed(ex, false, false, "GetJobsCompressed2"));
			AssertEquals(1, logs.Count);
			AssertEquals(expectedMessage, logs[0]);

			logs.Clear();
			operations.Add("GetJobsCompressed2");

			Assert("Should handle bad request", controller.HandleWebExceptionExposed(ex, false, false, "GetJobsCompressed2"));
			AssertEquals(1, logs.Count);
			AssertEquals(expectedMessage, logs[0]);
		}

		public void TestHandleWebException_ConnectFailure()
		{
			var logs = new List<string>();

			var controller = new TestController();
			controller.ShowInformation += (_, e) => logs.Add(e.Message);

			var ex = new WebException("Error happened", WebExceptionStatus.ConnectFailure);

			Assert("Should not handle connect failure error", controller.HandleWebExceptionExposed(ex, false, false));
			AssertEquals(1, logs.Count);
		}

		public void TestHandleWebException_RequestEntityTooLarge()
		{
			var logs = new List<string>();
			var controller = new TestController();
			controller.ShowError += (_, e) => logs.Add(e.Message);
			var response = new Mock<HttpWebResponse>();
			response.Setup(m => m.StatusCode).Returns(HttpStatusCode.RequestEntityTooLarge);
			var ex = new WebException("The request failed with HTTP status 413: Request entity too large: header-line too large.", null, WebExceptionStatus.ProtocolError, response.Object);
			var handled = controller.HandleWebExceptionExposed(ex, false, true);
			var expectedMessage = @"
Web Print suspended due to the request entity too large.
Please contact your system administrator if the problem persists.
";

			Assert("Should handle request entity too large error", handled);
			AssertEquals(1, logs.Count);
			AssertEquals(expectedMessage, logs[0]);
		}

		public void TestHandleWebException_DatabaseUpgradedException()
		{
			AssertHandleDatabaseUpgradedException(535, "the database needs to be upgraded.");
			AssertHandleDatabaseUpgradedException(536, "the RemotePrinting Web Service needs to be upgraded and restarted.");
			AssertHandleDatabaseUpgradedException(537, "the database is in the process of being upgraded.");
		}

		void AssertHandleDatabaseUpgradedException(int statusCode, string errorMessage)
		{
			var exceptionMessage = $"The request failed with HTTP status {statusCode}: The database has been upgraded and the application must be restarted to get the new version.";
			var expectedMessage = $@"
Web Print suspended due to {errorMessage}
Error details:
The database has been upgraded and the application must be restarted to get the new version.

Please contact your system administrator if the problem persists.

Retrying in 120 seconds.
";

			var logs = new List<string>();
			var controller = new TestController();
			controller.ShowError += (_, e) => logs.Add(e.Message);
			var ex = new WebException(exceptionMessage, WebExceptionStatus.ProtocolError);
			var handled = controller.HandleWebExceptionExposed(ex, false, true);

			Assert("Should handle database upgraded error", handled);
			AssertEquals(1, logs.Count);
			AssertEquals(expectedMessage, logs[0]);
		}

		public void TestHandelWebException_Timeout()
		{
			AssertHandelWebException_Timeout(WebExceptionStatus.Timeout, "Error happened");
			AssertHandelWebException_Timeout(WebExceptionStatus.KeepAliveFailure, "KeepAlive failed");
			AssertHandelWebException_Timeout(WebExceptionStatus.RequestCanceled, "The request was aborted: The request was canceled.");
			AssertHandelWebException_Timeout(WebExceptionStatus.ReceiveFailure, "Receive Failure");
		}

		void AssertHandelWebException_Timeout(WebExceptionStatus status, string errorMessage)
		{
			var logs = new List<string>();

			var controller = new TestController();
			controller.ShowInformation += (_, e) => logs.Add(e.Message);

			controller.ConfigSettingForTest = new WebClientConfiguration
			{
				RemotePrintingServiceTimeoutInSeconds = 10
			};

			var ex = new WebException(errorMessage, status);

			Assert("Should not handle timeout error", !controller.HandleWebExceptionExposed(ex, shouldHandleTimeout: false, false));
			AssertEquals(0, logs.Count);

			Assert("Should handle timeout error", controller.HandleWebExceptionExposed(ex, shouldHandleTimeout: true, false));
			AssertEquals(1, logs.Count);

			var expectedLog =
$@"Timeout while executing web operation: {errorMessage}
Web Exception Status: {status}
Timeout interval: 10s
";

			AssertEquals(expectedLog, logs[0]);
		}

		public void TestHandleWebException_BadGateway()
		{
			var logs = new List<string>();

			var controller = new TestController();
			controller.ShowError += (_, e) => logs.Add(e.Message);

			var ex = new WebException("Something something status 502 Some gateway error", WebExceptionStatus.ProtocolError);

			Assert("Should handle gateway error", controller.HandleWebExceptionExposed(ex, true, true));
			AssertEquals(1, logs.Count);

			var expectedMessage = $@"
Web Print suspended due to a gateway error.
Error details:
Some gateway error

{Controller.ContactAdminMessage}
Exception details:";

			AssertStartsWith("Expected log message", expectedMessage, logs[0]);
		}

		public void TestHandleWebException_Unauthorized()
		{
			var logs = new List<string>();

			var controller = new TestController();
			controller.ShowError += (_, e) => logs.Add(e.Message);
			controller.ConfigName = "TestConfig";

			var ex = new WebException("Something something status 401 Unauthorized ", WebExceptionStatus.ProtocolError);

			Assert("Should handle gateway error", controller.HandleWebExceptionExposed(ex, true, true));
			AssertEquals(1, logs.Count);

			var expectedMessage = $@"
Web Print suspended due to incorrect credentials.
Please verify that the User and Password in WebPrint Configuration for 'TestConfig' match the login details in CargoWise One Registry at Web > Web Services in registry items Web Services User Login, Web Services User Password, and Web Services Alternative Credentials.
{Controller.ContactAdminMessage}";

			AssertStartsWith("Expected log message", expectedMessage, logs[0]);
		}

		public void TestHandleWebException_GatewayTimeout()
		{
			var logs = new List<string>();

			var controller = new TestController();
			controller.ShowError += (_, e) => logs.Add(e.Message);

			var ex = new WebException("Something something status 504 Some gateway error", WebExceptionStatus.ProtocolError);

			Assert("Should handle gateway error", controller.HandleWebExceptionExposed(ex, true, true));
			AssertEquals(1, logs.Count);

			var expectedMessage = @"
Web Print suspended due to a slow connection or connection break to gateway.";

			AssertStartsWith("Expected log message", expectedMessage, logs[0]);
		}

		public void TestHandleServerException_AggregateException_Handled()
		{
			var logs = new List<string>();

			var controller = new TestController();
			controller.ShowInformation += (_, e) => logs.Add(e.Message);

			controller.ConfigSettingForTest = new WebClientConfiguration
			{
				RemotePrintingServiceTimeoutInSeconds = 10
			};

			var ex1 = new WebException("Error happened 1", WebExceptionStatus.Timeout);
			var ex2 = new WebException("Error happened 2", WebExceptionStatus.Timeout);
			var aggregateEx = new AggregateException(ex1, ex2);
			var topEx = new ApplicationException("Something", aggregateEx);

			Assert("Should handle all errors", controller.HandleServerExceptionExposed(topEx, shouldHandleTimeout: true, shouldSleepIfProtocolError: false));
			AssertEquals(2, logs.Count);

			var expectedLog1 =
@"Timeout while executing web operation: Error happened 1
Web Exception Status: Timeout
Timeout interval: 10s
";
			AssertEquals(expectedLog1, logs[0]);

			var expectedLog2 =
@"Timeout while executing web operation: Error happened 2
Web Exception Status: Timeout
Timeout interval: 10s
";
			AssertEquals(expectedLog2, logs[1]);
		}

		public void TestHandleServerException_AggregateException_NotHandled()
		{
			var logs = new List<string>();

			var controller = new TestController();
			controller.ShowInformation += (_, e) => logs.Add(e.Message);

			controller.ConfigSettingForTest = new WebClientConfiguration
			{
				RemotePrintingServiceTimeoutInSeconds = 10
			};

			var ex1 = new WebException("Error happened 1", WebExceptionStatus.Timeout);
			var ex2 = new InvalidOperationException("Error happened 2");
			var aggregateEx = new AggregateException(ex1, ex2);
			var topEx = new ApplicationException("Something", aggregateEx);

			Assert("Some errors were not handled", !controller.HandleServerExceptionExposed(topEx, shouldHandleTimeout: true, shouldSleepIfProtocolError: false));
			AssertEquals(1, logs.Count);

			var expectedLog1 =
				@"Timeout while executing web operation: Error happened 1
Web Exception Status: Timeout
Timeout interval: 10s
";
			AssertEquals(expectedLog1, logs[0]);
		}

		public void TestHandleSignalRHttpClientException()
		{
			var logs = new List<string>();

			var controller = new TestController();
			controller.ShowInformation += (_, e) => logs.Add(e.Message);

			var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
			response.Content = new StringContent("Test error content");
			var ex = new HttpClientException(response);
			var exceptionMessage = @"SignalR has some errors, the error response body as below: 
Test error content
Exception details: 
StatusCode: 400, ReasonPhrase: 'Bad Request', Version: 1.1, Content: System.Net.Http.StringContent, Headers:
{
  Content-Type: text/plain; charset=utf-8
}
Microsoft.AspNet.SignalR.Client.HttpClientException


";

			Assert("Error should be handled", controller.HandleServerExceptionExposed(ex, shouldHandleTimeout: false, shouldSleepIfProtocolError: false));
			AssertEquals(1, logs.Count);
			AssertEquals(exceptionMessage, logs[0]);
		}

		public void TestIsBelongToWebOperationTimeout()
		{
			AssertIsBelongToWebOperationTimeout(WebExceptionStatus.Timeout, true);
			AssertIsBelongToWebOperationTimeout(WebExceptionStatus.KeepAliveFailure, true);
			AssertIsBelongToWebOperationTimeout(WebExceptionStatus.ReceiveFailure, true);
			AssertIsBelongToWebOperationTimeout(WebExceptionStatus.SendFailure, true);
			AssertIsBelongToWebOperationTimeout(WebExceptionStatus.RequestCanceled, true);

			AssertIsBelongToWebOperationTimeout(WebExceptionStatus.ProtocolError, false);
			AssertIsBelongToWebOperationTimeout(WebExceptionStatus.NameResolutionFailure, false);
			AssertIsBelongToWebOperationTimeout(WebExceptionStatus.ConnectionClosed, false);
			AssertIsBelongToWebOperationTimeout(WebExceptionStatus.UnknownError, false);
		}

		void AssertIsBelongToWebOperationTimeout(WebExceptionStatus status, bool expectIsBelong)
		{
			var ex = new WebException("Error happened", status);
			AssertEquals(status.ToString(), expectIsBelong, TestController.IsBelongToWebOperationTimeoutForTest(ex));
		}

		public void TestHandleWebException_Timeout()
		{
			var logs = new List<string>();

			var controller = new TestController();
			controller.ShowInformation += (_, e) => logs.Add(e.Message);

			controller.ConfigSettingForTest = new WebClientConfiguration
			{
				RemotePrintingServiceTimeoutInSeconds = 10
			};

			var response = new Mock<HttpWebResponse>();
			response.Setup(m => m.StatusCode).Returns(HttpStatusCode.RequestTimeout);

			var ex = new WebException("Some timeout", null, WebExceptionStatus.Timeout, response.Object);

			var expectedMessage = @"Timeout while executing web operation: Some timeout
Web Exception Status: Timeout
Response Code: RequestTimeout
Timeout interval: 10s
";

			Assert("Error should be handled", controller.HandleServerExceptionExposed(ex, shouldHandleTimeout: true, shouldSleepIfProtocolError: false));
			AssertEquals(1, logs.Count);
			AssertEquals(expectedMessage, logs[0]);
		}

		public void TestProcessStartingInfo()
		{
			var logs = new List<string>();

			var controller = new TestController();
			controller.ProcessStarting += (_, e) => logs.Add(e.Message);

			controller.ConfigName = "Test Config";
			var config = new WebClientConfiguration
			{
				WebServiceUrl = "Test URL"
			};

			using (UpdateProcessor.OverrideInstalledVersionForTest("3.4.5"))
			{
				controller.LogProcessStartnigInfoExposed(config);
			}

			AssertEquals(1, logs.Count);

			const string ExpectedMessage =
@"Process Started

Client Build Version: 3.4.5
Configuration Name: Test Config
WebService URL: Test URL
";

			AssertEquals("Process started message should be logged via ProcessStarting event", ExpectedMessage, logs[0]);
		}

		public void TestHandleSignalRObjectDisposedException()
		{
			var disposedExceptionForTest = new ObjectDisposedExceptionForTest(@"Cannot access a disposed object.
Object name: 'System.Net.Http.StreamContent'.", null);
			disposedExceptionForTest.StackTraceForTest = @"at System.Net.Http.HttpContent.CheckDisposed()
 at System.Net.Http.HttpContent.ReadAsStreamAsync()
 at Microsoft.AspNet.SignalR.Client.Http.HttpResponseMessageWrapper.GetStream()
 at Microsoft.AspNet.SignalR.Client.Transports.ServerSentEventsTransport.<>c__DisplayClass13_0.<OpenConnection>b__2(Task`1 task)
 at System.Threading.Tasks.Task.Execute()";
			var aggregateException = new AggregateException("Jerry test AggregateException", disposedExceptionForTest);

			var logs = new List<string>();

			var controller = new TestController();
			controller.ShowInformation += (_, e) => logs.Add(e.Message);

			var expectedMessage = @"SignalR occurs ObjectDisposedException with error message: Cannot access a disposed object.
Object name: 'System.Net.Http.StreamContent'., StackTrace: at System.Net.Http.HttpContent.CheckDisposed()
 at System.Net.Http.HttpContent.ReadAsStreamAsync()
 at Microsoft.AspNet.SignalR.Client.Http.HttpResponseMessageWrapper.GetStream()
 at Microsoft.AspNet.SignalR.Client.Transports.ServerSentEventsTransport.<>c__DisplayClass13_0.<OpenConnection>b__2(Task`1 task)
 at System.Threading.Tasks.Task.Execute()";

			Assert("Error should be handled", controller.HandleServerExceptionExposed(aggregateException, shouldHandleTimeout: false, shouldSleepIfProtocolError: false));
			AssertEquals(1, logs.Count);
			AssertEquals(expectedMessage, logs[0]);
		}
	}

	public class TestController : Controller
	{
		public TestController() : base(null)
		{
			ShouldStop = true;
		}

		protected override bool IsMainController => true;

		internal static bool IsBelongToWebOperationTimeoutForTest(WebException webEx)
		{
			return IsBelongToWebOperationTimeout(webEx);
		}

		protected override void Process()
		{
		}

		public void HandlePreLoopExceptionExposed(Exception ex)
		{
			HandlePreLoopException(ex);
		}

		public bool HandleServerExceptionExposed(Exception ex, bool shouldHandleTimeout, bool shouldSleepIfProtocolError)
		{
			return HandleServerException(ex, shouldHandleTimeout, shouldSleepIfProtocolError);
		}

		public bool HandleWebExceptionExposed(WebException webEx, bool shouldHandleTimeout, bool shouldSleepIfProtocolError, string operationName = null)
		{
			return HandleWebException(webEx, shouldHandleTimeout, shouldSleepIfProtocolError, operationName);
		}

		public void LogProcessStartnigInfoExposed(WebClientConfiguration config) => LogProcessStartingInfo(config);

		protected override WebClientConfiguration GetNewConfigSetting(string configName) => ConfigSettingForTest ?? base.GetNewConfigSetting(configName);

		public WebClientConfiguration? ConfigSettingForTest { get; set; }

		public WebClient WebServiceClientForTest { get; set; }

		protected override WebClient WebServiceClient => WebServiceClientForTest ?? base.WebServiceClient;
	}

	class TestController2 : TestController
	{
		protected override bool IsMainController => false;

		public void InitialiseWebServiceClient_Exposed() => InitialiseWebServiceClient();

		ConnectionRegistryManager manager;
		protected override ConnectionRegistryManager ConnectionRegistryManager => manager ?? (manager = new ConnectionRegistryManagerForTest());

		public ConnectionRegistryManager ConnectionRegistryManager_Exposed => ConnectionRegistryManager;
	}

	[Serializable]
	class ObjectDisposedExceptionForTest : ObjectDisposedException
	{
#if NETFRAMEWORK
		protected ObjectDisposedExceptionForTest(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
#endif

		public ObjectDisposedExceptionForTest(string message, Exception innerException) : base(message, innerException)
		{
		}

		public string StackTraceForTest { get; set; }
		public override string StackTrace => StackTraceForTest ?? base.StackTrace;
	}
}

