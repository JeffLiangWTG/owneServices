using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;
using WTG.ErrorReporting;
using WTG.ErrorReporting.Extensibility;

namespace Enterprise.RemotePrinting.Client.Tests
{
	public class ErrorReporterTest : TestCase
	{
		public void TestReportError()
		{
			var testReporter = new ErrorReporterForTest();
			ErrorReporter.Instance = testReporter;

			try
			{
				testReporter.ResetSequence();

				var ex = new ApplicationException("Big boom");

				using (UpdateProcessor.OverrideInstalledVersionForTest("1.2.3"))
				{
					ErrorReporter.ReportError("key1", "m1", ex);
				}

				AssertEquals(1, testReporter.Client.PostedErrorReports.Count);

				var reportText = testReporter.Client.PostedErrorReports[0];

				AssertContains("<EDI_Exception_Report>", reportText);

				AssertContains("<Key>key1</Key>", reportText);
				AssertContains("<ExceptionDescription>m1</ExceptionDescription>", reportText);

				AssertContains("<ExceptionDetails>", reportText);
				AssertContains("<ExceptionType>System.ApplicationException</ExceptionType>", reportText);
				AssertContains("<Message>Big boom</Message>", reportText);

				AssertContains("<OSInfo>", reportText);
				AssertContains("<PCInfo>", reportText);
				AssertContains("<SystemResourcesUsage>", reportText);

				AssertContains("<Company>" + testReporter.GetWebServiceUrl_Exposed() + "</Company>", reportText);
				AssertContains("<Sequence>1</Sequence>", reportText);
				AssertContains("<VersionNumber>1.2.3</VersionNumber>", reportText);

				AssertContains("<CommandLine>" + System.Environment.CommandLine.Replace("&", "&amp;") + "</CommandLine>", reportText);
			}
			finally
			{
				ErrorReporter.Instance = null;
			}
		}

		public void TestErrorReporterIsNotAutoInitializedInTests()
		{
			AssertNoExceptionThrown("Should be able to call ReportError without causing null-reference exception", () => ErrorReporter.ReportError("test"));
			AssertNull("ErrorReporter instance should not be initialized", ErrorReporter.Instance);
		}

		public void TestErrorReporterIsAutoInitialized()
		{
			try
			{
				using (TestingState.SuspendIsRunningTests())
				{
					AssertNotNull(ErrorReporter.Instance);
				}
			}
			finally
			{
				// Ensure ErrorReporter is disabled
				ErrorReporter.Instance = null;
			}
		}

		public void TestHandlePreLoopExceptionExposedDoNotReportError()
		{
			var testReporter = new ErrorReporterForTest();
			ErrorReporter.Instance = testReporter;

			try
			{
				var controller = new TestController();

				string errorMessage = null;
				controller.ProcessFailed += (_, log) => { errorMessage = log.Message; };

				controller.HandlePreLoopExceptionExposed(new ApplicationException("Test"));

				AssertNotNull("Should log error message", errorMessage);
				AssertContains("Test", errorMessage);

				AssertEquals("Should not post error report", 0, testReporter.Client.PostedErrorReports.Count);
			}
			finally
			{
				ErrorReporter.Instance = null;
			}
		}

		public void TestErrorReporter()
		{
			var log = new StringBuilder();
			var mock = new Mock<IRemotePrintingServiceAdaptor>();
			var webClient = new WebClientForTesting(mock.Object, true);
			var testController = new ControllerForTesting("~TEST_SERVER", null, webClient);

			AssertNull(ErrorReporter.Instance);

			try
			{
				var testReporter = new ErrorReporterForTest();
				ErrorReporter.Instance = testReporter;
				testController.Run();

				AssertNotNull("Error Reporter should be initialized", ErrorReporter.Instance);
			}
			finally
			{
				ErrorReporter.Instance = null;
			}
		}

		public void TestHandleOrReportHandled()
		{
			var testReporter = new ErrorReporterForTest();
			testReporter.HandleError += (sender, args) => { args.Handled = args.Exception is IOException; };

			testReporter.HandleOrReport(new IOException("super-duper-test"), false);

			AssertEquals("Should not report handled error", 0, testReporter.Client.PostedErrorReports.Count);
		}

		public void TestHandleOrReportNotHandled()
		{
			var testReporter = new ErrorReporterForTest();
			testReporter.HandleError += (sender, args) => { args.Handled = args.Exception is IOException; };

			testReporter.HandleOrReport(new ApplicationException("super-duper-test"), false);

			AssertEquals("Should report not-handled error", 1, testReporter.Client.PostedErrorReports.Count);
			AssertContains("super-duper-test", testReporter.Client.PostedErrorReports[0]);
		}

		public void TestHandleOrReportWebExceptionDetails()
		{
			var testReporter = new ErrorReporterForTest();
			testReporter.HandleError += (sender, args) => { args.Handled = args.Exception is IOException; };

			testReporter.HandleOrReport(new ApplicationException("super-duper-test", new WebException("web-exception", WebExceptionStatus.ConnectFailure)), false);

			AssertEquals("Should report not-handled error", 1, testReporter.Client.PostedErrorReports.Count);
			AssertContains("super-duper-test", testReporter.Client.PostedErrorReports[0]);
			AssertContains("<WebExceptionStatus>" + WebExceptionStatus.ConnectFailure + "</WebExceptionStatus>", testReporter.Client.PostedErrorReports[0]);
		}

		public void TestHandleOrReportReportsWhenHandleErrorIsNotHooked()
		{
			var testReporter = new ErrorReporterForTest();
			testReporter.HandleOrReport(new ApplicationException("super-duper-test", new WebException("web-exception", WebExceptionStatus.ConnectFailure)), false);

			AssertEquals("Should report not-handled error", 1, testReporter.Client.PostedErrorReports.Count);
			AssertContains("HandleError is not hooked", testReporter.Client.PostedErrorReports[0]);
		}

		public void TestIgnoreWebExceptionWhenReportError()
		{
			AssertIgnoreExceptionWhenReportError(new WebException("NameResolutionFailure", WebExceptionStatus.NameResolutionFailure));
			AssertIgnoreExceptionWhenReportError(new WebException("SecureChannelFailure", WebExceptionStatus.SecureChannelFailure));
			AssertIgnoreExceptionWhenReportError(new WebException("SendFailure", WebExceptionStatus.SendFailure));
			AssertIgnoreExceptionWhenReportError(new WebException("ReceiveFailure", WebExceptionStatus.ReceiveFailure));
		}

		public void TestIgnoreHttpRequestExceptionWhenReportError()
		{
			AssertIgnoreExceptionWhenReportError(new HttpRequestException("Response status code does not indicate success: 401 (Authorization Required)."));
			AssertIgnoreExceptionWhenReportError(new HttpRequestException("Response status code does not indicate success: 403 (Forbidden)."));
			AssertIgnoreExceptionWhenReportError(new HttpRequestException("Response status code does not indicate success: 404 (Not Found)."));
			AssertIgnoreExceptionWhenReportError(new HttpRequestException("Response status code does not indicate success: 408 (Request Time-out)."));
			AssertIgnoreExceptionWhenReportError(new HttpRequestException("Response status code does not indicate success: 503 (Service Unavailable)."));

			AssertIgnoreExceptionWhenReportError(new HttpRequestException("Error while copying content to a stream.",
				new IOException("Unable to read data from the transport connection: The connection was closed.", ErrorReporter.ConnectionDroppedWhileSending)));
		}

		void AssertIgnoreExceptionWhenReportError(Exception ex)
		{
			try
			{
				var errorReport = new ErrorReporterForTestWebException();
				ErrorReporter.Instance = errorReport;

				errorReport.Client.ExcecptionForTest = ex;
				AssertNoExceptionThrown($"{ex.GetType().Name}-{ex.Message}", () => ErrorReporter.ReportError("Test Message"));

				errorReport.Client.ExcecptionForTest = new AggregateException("", ex);
				AssertNoExceptionThrown($"AggregatedException-{ex.GetType().Name}-{ex.Message}", () => ErrorReporter.ReportError("Test Message"));
			}
			finally
			{
				ErrorReporter.Instance = null;
			}
		}

		public void TestExceptionMessageHandler()
		{
			var testReporter = new ErrorReporterForTest();
			ErrorReporter.Instance = testReporter;

			var ex1 = new ApplicationException("Test message 1");
			var ex2 = new ApplicationException("Test message 2", ex1);
			var ex3 = new ApplicationException("Test message 3", ex2);
			var message = ErrorReporter.GetExceptionMessage(ex3);
			AssertEquals("Should return innermost exception message", "Test message 1", message);
			message = ErrorReporter.GetExceptionMessage(null);
			AssertEquals("Should return empty string for null exception", string.Empty, message);

			var httpRequestException = new HttpRequestException("Test message 4");
			var targetInvocationException = new TargetInvocationException("Test message 5", httpRequestException);
			message = ErrorReporter.GetExceptionMessage(targetInvocationException);
			AssertEquals("Should return innermost exception message for HttpRequestException", "Test message 4", message);

			var aggregateException = new AggregateException("Test message 6", targetInvocationException);
			message = ErrorReporter.GetExceptionMessage(aggregateException);
			AssertEquals("Should return innermost exception message for AggregateException", "Test message 4", message);

			var innerEx1 = new ApplicationException("Inner exception 1");
			var innerEx2 = new ApplicationException("Inner exception 2");
			var aggregateExWithTwoInnerExceptions = new AggregateException("Test message 7", innerEx1, innerEx2);
			message = ErrorReporter.GetExceptionMessage(aggregateExWithTwoInnerExceptions);
			var aggregatedMessage = innerEx1.Message + System.Environment.NewLine + innerEx2.Message;
			AssertEquals("Should return first inner exception message for AggregateException with two inner exceptions", aggregatedMessage, message);

			ErrorReporter.Instance = null;
		}

		public void TestReportOnce()
		{
			try
			{
				var testReporter = new SimpleErrorReporterForTest();
				ErrorReporter.Instance = testReporter;

				ErrorReporter.ReportOnce("key1", "message 1");
				ErrorReporter.ReportOnce("key2", "message 2");

				AssertEquals("Should have 2 new error reports", 2, testReporter.ReportedErrors.Count);
				AssertContains("key1", testReporter.ReportedErrors[0].Key);
				AssertContains("message 1", testReporter.ReportedErrors[0].Value);
				AssertContains("key2", testReporter.ReportedErrors[1].Key);
				AssertContains("message 2", testReporter.ReportedErrors[1].Value);

				// Try again
				ErrorReporter.ReportOnce("key1", "message 3");

				AssertEquals("Should not repeat same error message yet", 2, testReporter.ReportedErrors.Count);

				// Try again a bit late
				testReporter.CurrentTimeUtcOverride = DateTime.UtcNow.AddHours(6);
				ErrorReporter.ReportOnce("key1", "message 4");

				AssertEquals("Should not repeat same error message yet", 2, testReporter.ReportedErrors.Count);

				// Try again 1 day later
				testReporter.CurrentTimeUtcOverride = DateTime.UtcNow.AddHours(24);
				ErrorReporter.ReportOnce("key1", "message 5");

				AssertEquals("Should have 1 new error reports", 3, testReporter.ReportedErrors.Count);
				AssertContains("message 5", testReporter.ReportedErrors[2].Value);

				// Try again
				testReporter.CurrentTimeUtcOverride = DateTime.UtcNow.AddHours(30);
				ErrorReporter.ReportOnce("key1", "message 6");

				AssertEquals("Should update last reported time and not repeat same error message yet", 3, testReporter.ReportedErrors.Count);
			}
			finally
			{
				ErrorReporter.Instance = null;
			}
		}

		public void TestGetExceptionKey()
		{
			var ex = new AggregateException("Ex1", new InvalidOperationException("Ex2", new ApplicationException("Ex3")));
			var key = ErrorReporter.GetExceptionKey(ex);

			AssertStartsWith("Should refer to innermost exception", typeof(ApplicationException).FullName, key);
			AssertContains("Should include exception message", "Ex3", key);
		}

		public void TestReportOnce_NullKey()
		{
			try
			{
				var testReporter = new SimpleErrorReporterForTest();
				ErrorReporter.Instance = testReporter;

				ErrorReporter.ReportOnce(null, "message 1", new InvalidOperationException("Ex1"));
				ErrorReporter.ReportOnce(null, "message 2", new InvalidOperationException("Ex2"));

				AssertEquals("Should have 2 new error reports", 2, testReporter.ReportedErrors.Count);
				AssertContains("message 1", testReporter.ReportedErrors[0].Value);
				AssertContains("message 2", testReporter.ReportedErrors[1].Value);

				// Try again
				ErrorReporter.ReportOnce(null, "message 3", new InvalidOperationException("Ex1"));

				AssertEquals("Should not repeat same error message yet", 2, testReporter.ReportedErrors.Count);

				// Try again a bit late
				testReporter.CurrentTimeUtcOverride = DateTime.UtcNow.AddHours(6);
				ErrorReporter.ReportOnce(null, "message 4", new InvalidOperationException("Ex1"));

				AssertEquals("Should not repeat same error message yet", 2, testReporter.ReportedErrors.Count);

				// Try again 1 day later
				testReporter.CurrentTimeUtcOverride = DateTime.UtcNow.AddHours(24);
				ErrorReporter.ReportOnce(null, "message 5", new InvalidOperationException("Ex1"));

				AssertEquals("Should have 1 new error reports", 3, testReporter.ReportedErrors.Count);
				AssertContains("message 5", testReporter.ReportedErrors[2].Value);

				// Try again
				testReporter.CurrentTimeUtcOverride = DateTime.UtcNow.AddHours(30);
				ErrorReporter.ReportOnce(null, "message 6", new InvalidOperationException("Ex1"));

				AssertEquals("Should update last reported time and not repeat same error message yet", 3, testReporter.ReportedErrors.Count);
			}
			finally
			{
				ErrorReporter.Instance = null;
			}
		}

		public void TestHandleOrReport_ReportOnce()
		{
			var testReporter = new SimpleErrorReporterForTest();

			testReporter.HandleOrReport(new ApplicationException("super-duper-test"), true);

			AssertEquals("Should report not-handled error", 1, testReporter.ReportedErrors.Count);
			AssertContains("super-duper-test", testReporter.ReportedErrors[0].Value);

			testReporter.HandleOrReport(new ApplicationException("super-duper-test"), true);

			AssertEquals("Should not report same error yet", 1, testReporter.ReportedErrors.Count);

			testReporter.CurrentTimeUtcOverride = DateTime.UtcNow.AddHours(24);
			testReporter.HandleOrReport(new ApplicationException("super-duper-test"), true);

			AssertEquals("Should report error again after 1 day", 2, testReporter.ReportedErrors.Count);
			AssertContains("super-duper-test", testReporter.ReportedErrors[0].Value);
			AssertContains("super-duper-test", testReporter.ReportedErrors[1].Value);
		}

		public void TestReportBuilderAdditionalDetailContributors()
		{
			var reportBuilder = ErrorReporter.CreateReportBuilderExposedForTest();

			var additionalDetailContributors = typeof(EnterpriseErrorReportBuilder)
				.GetField("additionalDetailContributors", BindingFlags.Instance | BindingFlags.NonPublic)
				?.GetValue(reportBuilder)
				as List<IAdditionalDetailContributor>;

			AssertNotNull("Cannot retrieve additionalDetailContributors field value from reportBuilder", additionalDetailContributors);

			Assert("Should contain WebExceptionExtension report builder contributor", additionalDetailContributors.Any(c => c is WebExceptionExtension));
			Assert("Should contain HttpClientExceptionExtension report builder contributor", additionalDetailContributors.Any(c => c is HttpClientExceptionExtension));
		}

		[TestRequiresAdministrativePrivileges("Creating new source in Windows Event Log")]
		public void TestFillWindowsEventErrors()
		{
			const string TestLogSource = "WebPrint Test";

			if (!EventLog.SourceExists(TestLogSource))
			{
				EventLog.CreateEventSource(TestLogSource, "Application");
			}
			var eventLog = new EventLog
			{
				Log = "Application",
				Source = TestLogSource
			};
			eventLog.SafeWriteEntry("Test error", EventLogEntryType.Error);

			string expectedTime = "";
			for (int index = eventLog.Entries.Count - 1; index > 0; index--)
			{
				var eventLogEntry = eventLog.Entries[index];
				if (eventLogEntry.EntryType == EventLogEntryType.Error && eventLogEntry.Source == TestLogSource)
				{
					expectedTime = eventLogEntry.TimeGenerated.ToString("yyyy-MM-dd HH:mm:ss");
					break;
				}
			}

			var testReporter = new ErrorReporterForTest();
			ErrorReporter.Instance = testReporter;

			try
			{
				var ex = new ApplicationException("Big boom");

				ErrorReporter.ReportError("key1", "m1", ex);

				AssertEquals(1, testReporter.Client.PostedErrorReports.Count);

				var reportText = testReporter.Client.PostedErrorReports[0];

				AssertContains("<WindowsEventLog>", reportText);

				var expectedErrorText =
$@"<WindowsEventLog>
    <Event>{expectedTime} - {TestLogSource}: Test error</Event>";

				AssertContains(expectedErrorText, reportText);
			}
			finally
			{
				ErrorReporter.Instance = null;
			}
		}

		public void TestCatchExceptionToRestartApplication()
		{
			var errorMessage = "";
			var restarted = false;
			var testReporter = new ErrorReporterForTest();
			testReporter.RestartApplication += (sender, e) =>
			{
				errorMessage = e.Message;
				restarted = true;
			};
			ErrorReporter.Instance = testReporter;

			try
			{
				testReporter.HandleOrReport(new CargoWise.PdfiumWrapper.CannotFoundPdfiumLibraryException("Canot found pdfium.ll for test", new DllNotFoundException("Canot found dll for test")), true);
				AssertEquals("Canot found pdfium.ll for test", errorMessage);
				AssertEquals(true, restarted);

				restarted = false;
				testReporter.HandleOrReport(new CargoWise.PdfiumWrapper.CannotDestroyPdfiumLibraryException("Canot destory pdfium.ll for test", new DllNotFoundException("Canot found dll for test")), true);
				AssertEquals("Canot destory pdfium.ll for test", errorMessage);
				AssertEquals(true, restarted);
			}
			finally
			{
				ErrorReporter.Instance = null;
			}
		}

		protected override void TearDown()
		{
			ErrorReporter.Instance = null;
			ConnectionRegistryManagerTest.ConnectionRegistryManagerForTest.DeleteWebPrintConfig();
			base.TearDown();
		}
	}

	public class ErrorReportingClientThrowExceptionForTest : IErrorReportingClient
	{
		public Uri ServiceUri => throw new NotImplementedException();

		public Exception ExcecptionForTest { get; set; }

		public Task DeleteCrashReportAsync(ErrorReportType type, string identifier, string accessToken, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public void Dispose()
		{
		}

		public Task PostCrashReportAsync(IOpaqueErrorReport errorReport, CancellationToken cancellationToken)
		{
			return Task.Run(() =>
			{
				throw ExcecptionForTest;
			});
		}

		public Task<Stream> RetrieveCrashReportAsync(ErrorReportType type, string identifier, string accessToken, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public Task<IList<string>> RetrieveCrashReportIdentifiersAsync(ErrorReportType type, string accessToken, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public Task<IList<string>> RetrieveCrashReportIdentifiersAsync(ErrorReportType type, string accessToken, int maxResults, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}

	public class ErrorReporterForTestWebException : ErrorReporter
	{
		protected override IErrorReportingClient GetErrorReportingClient(Uri serviceUri)
		{
			return Client;
		}

		protected override Task PostCrashReportAsync(IErrorReportingClient client, EnterpriseErrorReportBuilder reportBuilder, int retryCounter)
		{
			var task = base.PostCrashReportAsync(client, reportBuilder, retryCounter);
			task.Wait();
			return task;
		}

		public ErrorReportingClientThrowExceptionForTest Client { get; } = new ErrorReportingClientThrowExceptionForTest();
	}

	public class ErrorReportingClientForTest : IErrorReportingClient
	{
		public List<string> PostedErrorReports { get; } = new List<string>();

		public void Dispose()
		{
		}

		public async Task PostCrashReportAsync(IOpaqueErrorReport errorReport, CancellationToken cancellationToken)
		{
			var reportText = await errorReport.ToStringAsync(Encoding.UTF8);
			PostedErrorReports.Add(reportText);
		}

		public Task<IList<string>> RetrieveCrashReportIdentifiersAsync(ErrorReportType type, string accessToken, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public Task<IList<string>> RetrieveCrashReportIdentifiersAsync(ErrorReportType type, string accessToken, int maxResults, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public Task<Stream> RetrieveCrashReportAsync(ErrorReportType type, string identifier, string accessToken, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public Task DeleteCrashReportAsync(ErrorReportType type, string identifier, string accessToken, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public Uri ServiceUri => new Uri("uri");
	}

	public class ErrorReporterForTest : ErrorReporter
	{
		public ErrorReporterForTest(bool enableHookUnhandledExceptionsCore = false)
		{
			doNothing = enableHookUnhandledExceptionsCore;
		}

		protected override IErrorReportingClient GetErrorReportingClient(Uri serviceUri)
		{
			return Client;
		}

		public ErrorReportingClientForTest Client { get; } = new ErrorReportingClientForTest();

		protected override void HookUnhandledExceptionsCore()
		{
			if (!doNothing)
			{
				base.HookUnhandledExceptionsCore();
			}
		}

		readonly bool doNothing;

		public string GetWebServiceUrl_Exposed()
		{
			return GetWebServiceUrl();
		}
	}

	public class SimpleErrorReporterForTest : ErrorReporter
	{
		public List<KeyValuePair<string, string>> ReportedErrors { get; } = new List<KeyValuePair<string, string>>();

		protected override void HookUnhandledExceptionsCore()
		{
		}

		protected override Task ReportErrorCoreAsync(string key, string message, Exception ex = null)
		{
			ReportedErrors.Add(new KeyValuePair<string, string>(key, message));
			return Task.CompletedTask;
		}

		protected override DateTime GetCurrentTimeUtc()
		{
			return CurrentTimeUtcOverride ?? base.GetCurrentTimeUtc();
		}

		public DateTime? CurrentTimeUtcOverride { get; set; }
	}
}
