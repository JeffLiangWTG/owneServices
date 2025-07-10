using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.Client.EDI.Mail.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using WTG.ErrorReporting;

namespace Enterprise.Client.EDI.ServiceTask.Testing
{
	[TestedType(typeof(IssueManagerProcessorServiceTask))]
	class IssueManagerProcessorServiceTaskTest : ServiceTaskTestCase<IssueManagerProcessorServiceTask>
	{
		public void TestServiceTaskCanRunInAnyBranch()
		{
			ErrorReporter.Clear();
			AssertNotNull(GetHostedServiceAttributes().Single(x => x.CanRunInAnyBranch));
			var serviceTask = new IssueManagerProcessorServiceTask();
			var logger = new TestServiceLogger();
			serviceTask.ServiceLogger = logger;
			AssertEquals($"Precondition failed, errors in error reporter: {string.Join("\n\n", ErrorReporter.ExceptionsThrown)}", 0, ErrorReporter.TotalErrorCount);
			using (ClearUserContext())
			using (Env.Instance.TemporaryServiceTaskContext(serviceTask.GetType().Name, canRunInAnyBranch: true))
			{
				AssertNoExceptionThrown(() => Internal_TestDownloadFromWebService(1, 1));
				AssertNoExceptionThrown(() => Internal_TestDownloadFromWebServiceDisabled(DatabaseTypes.Codes.Test));
				AssertNoExceptionThrown(() => Internal_TestDownloadFromWebService(1, 1));
			}
			AssertEquals("No Exception Report", 0, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
		}

		public void TestProcess_ExceptionParsingXML()
		{
			ErrorReporter.Clear();
			var serviceTask = new IssueManagerProcessorServiceTask();

			var uri = new Uri("https://errors.unit-test.cargowise.net");
			var accessToken = Guid.NewGuid().ToString("n");
			EDIDataRegistry.Instance.ErrorReportingServiceURIs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { uri.ToString() });
			EDIDataRegistry.Instance.ErrorReportingServiceTesting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			EDIDataRegistry.Instance.ErrorReportingServiceAccessToken.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, accessToken);
			EDIDataRegistry.Instance.ErrorReportingServiceMaxResults.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);
			var mockClient = new Mock<IErrorReportingClient>(MockBehavior.Strict);
			var identifiers = new[] { "TestProcess_ExceptionParsingXMLIdentifier" };
			mockClient.Setup(x => x.Dispose());
			mockClient.Setup(x => x.RetrieveCrashReportIdentifiersAsync(ErrorReportType.EnterpriseXml, accessToken, 1, It.IsAny<CancellationToken>())).ReturnsAsync(identifiers);
			mockClient.Setup(x => x.RetrieveCrashReportAsync(ErrorReportType.EnterpriseXml, identifiers[0], accessToken, It.IsAny<CancellationToken>())).ReturnsAsync(delegate (ErrorReportType _, string x_, string x__, CancellationToken x___)
			{
				var data = Encoding.UTF8.GetBytes("TestProcess_ExceptionParsingXML test data");
				return new MemoryStream(data);
			});
			mockClient.Setup(c => c.DeleteCrashReportAsync(ErrorReportType.EnterpriseXml, identifiers[0], accessToken, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

			var mockProvider = new Mock<IErrorReportingClientProvider>(MockBehavior.Strict);
			mockProvider.Setup(x => x.CreateClient(uri, It.IsAny<TimeSpan?>())).Returns(mockClient.Object);
			serviceTask.ErrorReportingClientProvider = mockProvider.Object;

			var mockLogger = new Mock<ILogger>(MockBehavior.Default);
			mockLogger.Setup(x => x.Log(LogType.Debug, It.IsRegex("Processing \\[\\S+\\]"))).Throws(new Exception("Throw exception for TestProcess_ExceptionParsingXML"));
			serviceTask.ServiceLogger = mockLogger.Object;
			AssertEquals($"Precondition failed, errors in error reporter: {string.Join("\n\n", ErrorReporter.ExceptionsThrown)}", 0, ErrorReporter.TotalErrorCount);
			var issueProcessorDirectory = ReportProcessorHelper.GetReportDirectoryPath("IssueProcessor");
			using (new DisposableAction(() => Directory.Delete(issueProcessorDirectory, true)))
			using (ClearUserContext())
			using (Env.Instance.TemporaryServiceTaskContext(serviceTask.GetType().Name, canRunInAnyBranch: true))
			{
				AssertNoExceptionThrown(() => serviceTask.RunTask(new CancellationToken()));
			}
			AssertEquals("No Exception Report", 0, ErrorReporter.TotalErrorCount);
			mockLogger.Verify(x => x.Log(LogType.Information, "Process completed successfully"), Times.Once);
			mockLogger.Verify(x => x.Log(LogType.Warning, It.IsRegex("Unexpected error while processing error report \\[\\S+\\]"), It.IsAny<Exception>()), Times.Once);
			ErrorReporter.Clear();
		}

		static IDisposable ClearUserContext()
		{
			var userContext = EnvProxy.Instance.CurrentUserContext;
			EnvProxy.Instance.ClearUserContext();
			(EnvProxy.Instance as IEnvironmentForTest)?.ResetSecurityForTest();
			return new DisposableAction(() =>
			{
				EnvProxy.Instance.SetUserContext(userContext);
			});
		}

		[ExpectNoExceptions]
		public void TestDownloadFromWebService_Disabled_DbTypeTest() => Internal_TestDownloadFromWebServiceDisabled(DatabaseTypes.Codes.Test);
		[ExpectNoExceptions]
		public void TestDownloadFromWebService_Disabled_DbTypeTraining() => Internal_TestDownloadFromWebServiceDisabled(DatabaseTypes.Codes.Training);
		internal void Internal_TestDownloadFromWebServiceDisabled(string databaseTypeCode)
		{
			ObjectFactory.Get<IProductRegistration>().KeyForTest.DatabaseTypeForTest = databaseTypeCode;
			var uri = new Uri("https://errors.unit-test.cargowise.net");
			EDIDataRegistry.Instance.ErrorReportingServiceURIs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { uri.ToString() });
			var mockLogger = new Mock<ILogger>(MockBehavior.Strict);
			mockLogger.Setup(x => x.Log(LogType.Error, "Error Reporting Service integration is disabled for non-Production installations."));
			var mockProvider = new Mock<IErrorReportingClientProvider>(MockBehavior.Strict);
			var task = new IssueManagerProcessorServiceTask { ErrorReportingClientProvider = mockProvider.Object, ServiceLogger = mockLogger.Object };
			var directory = Temp.GetNewTempSubdirectory();
			try
			{
				task.DownloadFromWebService_ForTestAsync(directory).Wait();
			}
			finally
			{
				TempDirectory.DeleteDirectory(directory);
			}

			mockLogger.VerifyAll();
		}

		public void TestDownloadFromWebService_Single_Each() => Internal_TestDownloadFromWebService(1, 1);
		public void TestDownloadFromWebService_Single_Enterprise() => Internal_TestDownloadFromWebService(1, 3);
		public void TestDownloadFromWebService_Single_Glow() => Internal_TestDownloadFromWebService(3, 1);
		public void TestDownloadFromWebService_Many() => Internal_TestDownloadFromWebService(7, 3);
		public void TestDownloadFromWebService_ErrorReportUnavailableException()
		{
			var mockLogger = new Mock<ILogger>(MockBehavior.Strict);
			mockLogger.Setup(x => x.Log(LogType.Warning, string.Format("Failed to download report with ID {0} of type {1} from https://errors.unit-test.cargowise.net/: {2}", It.IsAny<string>(), ErrorReportType.EnterpriseXml, "bad thing Enterprise")));
			mockLogger.Setup(x => x.Log(LogType.Warning, string.Format("Failed to download report with ID {0} of type {1} from https://errors.unit-test.cargowise.net/: {2}", It.IsAny<string>(), ErrorReportType.GlowXmlEnterpriseCompatible, "bad thing Glow")));
			mockLogger.Setup(x => x.Log(LogType.Information, "Downloaded 1 error report from https://errors.unit-test.cargowise.net/ for type EnterpriseXml"));
			mockLogger.Setup(x => x.Log(LogType.Information, "Downloaded 1 error report from https://errors.unit-test.cargowise.net/ for type GlowXmlEnterpriseCompatible"));
			var task = new IssueManagerProcessorServiceTask { ServiceLogger = mockLogger.Object };
			var mockClient = new Mock<IErrorReportingClient>(MockBehavior.Strict);
			mockClient.Setup(x => x.Dispose());
			var uri = new Uri("https://errors.unit-test.cargowise.net");
			EDIDataRegistry.Instance.ErrorReportingServiceURIs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { uri.ToString() });
			var maxNumberOfReports = 2;
			EDIDataRegistry.Instance.ErrorReportingServiceMaxResults.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 2);
			var accessToken = Guid.NewGuid().ToString("n");
			EDIDataRegistry.Instance.ErrorReportingServiceAccessToken.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, accessToken);
			EDIDataRegistry.Instance.ErrorReportingServiceTesting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var mockProvider = new Mock<IErrorReportingClientProvider>(MockBehavior.Strict);
			mockProvider.Setup(x => x.CreateClient(uri, It.IsAny<TimeSpan?>())).Returns(mockClient.Object);
			task.ErrorReportingClientProvider = mockProvider.Object;
			var reportListForEnterprise = new List<string>();
			for (var i = 0; i < 2; i++)
			{
				var id = Guid.NewGuid().ToString();
				reportListForEnterprise.Add(id);
				mockClient.Setup(c => c.DeleteCrashReportAsync(ErrorReportType.EnterpriseXml, id, accessToken, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
			}

			var reportListforGlow = new List<string>();
			for (var i = 0; i < 2; i++)
			{
				var id = Guid.NewGuid().ToString();
				reportListforGlow.Add(id);
				mockClient.Setup(c => c.DeleteCrashReportAsync(ErrorReportType.GlowXmlEnterpriseCompatible, id, accessToken, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
			}

			mockClient.Setup(x => x.RetrieveCrashReportIdentifiersAsync(ErrorReportType.EnterpriseXml, accessToken, maxNumberOfReports, It.IsAny<CancellationToken>())).ReturnsAsync(reportListForEnterprise);
			mockClient.Setup(x => x.RetrieveCrashReportAsync(ErrorReportType.EnterpriseXml, reportListForEnterprise[0], accessToken, It.IsAny<CancellationToken>())).ThrowsAsync(new ErrorReportUnavailableException("bad thing Enterprise"));
			mockClient.Setup(x => x.RetrieveCrashReportAsync(ErrorReportType.EnterpriseXml, reportListForEnterprise[1], accessToken, It.IsAny<CancellationToken>())).ReturnsAsync(delegate(ErrorReportType _, string x_, string x__, CancellationToken x___)
			{
				var data = Encoding.UTF8.GetBytes("Surprise Enterprise Exception!!");
				return new MemoryStream(data);
			});
			mockClient.Setup(x => x.RetrieveCrashReportIdentifiersAsync(ErrorReportType.GlowXmlEnterpriseCompatible, accessToken, maxNumberOfReports, It.IsAny<CancellationToken>())).ReturnsAsync(reportListforGlow);
			mockClient.Setup(x => x.RetrieveCrashReportAsync(ErrorReportType.GlowXmlEnterpriseCompatible, reportListforGlow[0], accessToken, It.IsAny<CancellationToken>())).ThrowsAsync(new ErrorReportUnavailableException("bad thing Glow"));
			mockClient.Setup(x => x.RetrieveCrashReportAsync(ErrorReportType.GlowXmlEnterpriseCompatible, reportListforGlow[1], accessToken, It.IsAny<CancellationToken>())).ReturnsAsync(delegate(ErrorReportType _, string x_, string x__, CancellationToken x___)
			{
				var data = Encoding.UTF8.GetBytes("Surprise Glow Exception!!");
				return new MemoryStream(data);
			});
			var directory = Temp.GetNewTempSubdirectory();
			AssertEquals("Should have no files in directory (sanity check).", 0, Directory.EnumerateFiles(directory).Count());
			task.DownloadFromWebService_ForTestAsync(directory).Wait();
			var files = Directory.EnumerateFiles(directory);
			AssertEquals("2 files should have been processed", 2, files.Count());
			TempDirectory.DeleteDirectory(directory);
		}

		public void TestDownloadFromWebService_TaskCanceledException()
		{
			var strUri = "https://errors.unit-test.cargowise.net";
			var logs = new StringBuilder();
			Internal_TestDownloadFromWebService_WithException(strUri, logs, new TaskCanceledException());

			AssertGreaterThan(logs.Length, 0);
			AssertEquals(string.Format("{0}{1} : {2}/{0}{1} : {2}/", nameof(LogType.Warning), "Timeout when downloading from web service", strUri), logs.ToString());
		}

		public void TestDownloadFromWebService_ShouldBeAbleToFindTheUrl_WhenDownloadFailed()
		{
			var strUri = "https://errors.unit-test.cargowise.net";
			var logs = new StringBuilder();
			Internal_TestDownloadFromWebService_WithException(strUri, logs, new NotImplementedException());

			AssertGreaterThan(logs.Length, 0);
			AssertEquals(string.Format("{0}{1} : {2}/{0}{1} : {2}/", nameof(LogType.Error), "Error downloading from web service", strUri), logs.ToString());
		}

		void Internal_TestDownloadFromWebService_WithException(string strUri, StringBuilder logs, Exception ex)
		{
			var mockLogger = new Mock<ILogger>(MockBehavior.Strict);
			mockLogger.Setup(x => x.Log(It.IsAny<LogType>(), It.IsAny<string>(), It.IsAny<Exception>()))
				.Callback<LogType, string, Exception>((type, str, ex) => logs.Append(type.ToString()).Append(str));
			mockLogger.Setup(x => x.Log(It.IsAny<LogType>(), It.IsAny<string>()))
				.Callback<LogType, string>((type, str) => logs.Append(str));

			EDIDataRegistry.Instance.ErrorReportingServiceTesting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var uri = new Uri(strUri);
			EDIDataRegistry.Instance.ErrorReportingServiceURIs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { uri.ToString() });

			EDIDataRegistry.Instance.ErrorReportingServiceMaxResults.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 2);

			var accessToken = Guid.NewGuid().ToString("n");
			EDIDataRegistry.Instance.ErrorReportingServiceAccessToken.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, accessToken);

			var mockProvider = new Mock<IErrorReportingClientProvider>(MockBehavior.Strict);
			mockProvider.Setup(x => x.CreateClient(uri, It.IsAny<TimeSpan?>())).Throws(ex);

			var task = new IssueManagerProcessorServiceTask();
			task.ErrorReportingClientProvider = mockProvider.Object;
			task.ServiceLogger = mockLogger.Object;

			task.DownloadFromWebService_ForTestAsync("").Wait();

			mockLogger.Verify(x => x.Log(It.IsAny<LogType>(), It.Is<string>(s => s.Contains(strUri)), It.IsAny<Exception>()), Times.Exactly(2));
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
		void Internal_TestDownloadFromWebService(int numEnterpriseReports, int numGlowReports)
		{
			ObjectFactory.Get<IProductRegistration>().KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Production;
			var mockLogger = new Mock<ILogger>(MockBehavior.Strict);
			if (numEnterpriseReports == 1)
			{
				mockLogger.Setup(x => x.Log(LogType.Information, "Downloaded 1 error report from https://errors.unit-test.cargowise.net/ for type EnterpriseXml"));
			}
			else
			{
				mockLogger.Setup(x => x.Log(LogType.Information, string.Format("Downloaded {0} error reports from https://errors.unit-test.cargowise.net/ for type EnterpriseXml", numEnterpriseReports)));
			}

			if (numGlowReports == 1)
			{
				mockLogger.Setup(x => x.Log(LogType.Information, "Downloaded 1 error report from https://errors.unit-test.cargowise.net/ for type GlowXmlEnterpriseCompatible"));
			}
			else
			{
				mockLogger.Setup(x => x.Log(LogType.Information, string.Format("Downloaded {0} error reports from https://errors.unit-test.cargowise.net/ for type GlowXmlEnterpriseCompatible", numGlowReports)));
			}

			var task = new IssueManagerProcessorServiceTask { ServiceLogger = mockLogger.Object };
			var mockClient = new Mock<IErrorReportingClient>(MockBehavior.Strict);
			mockClient.Setup(x => x.Dispose());
			var uri = new Uri("https://errors.unit-test.cargowise.net");
			EDIDataRegistry.Instance.ErrorReportingServiceURIs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { uri.ToString() });
			var maxNumberOfReports = Math.Max(numEnterpriseReports, numGlowReports);
			EDIDataRegistry.Instance.ErrorReportingServiceMaxResults.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, maxNumberOfReports);
			var accessToken = Guid.NewGuid().ToString("n");
			EDIDataRegistry.Instance.ErrorReportingServiceAccessToken.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, accessToken);
			var mockProvider = new Mock<IErrorReportingClientProvider>(MockBehavior.Strict);
			mockProvider.Setup(x => x.CreateClient(uri, It.IsAny<TimeSpan?>())).Returns(mockClient.Object);
			task.ErrorReportingClientProvider = mockProvider.Object;
			var reportListForEnterprise = new List<string>();
			for (var i = 0; i < numEnterpriseReports; i++)
			{
				var id = Guid.NewGuid().ToString();
				reportListForEnterprise.Add(id);
				mockClient.Setup(c => c.DeleteCrashReportAsync(ErrorReportType.EnterpriseXml, id, accessToken, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
			}

			var tplTaskForEnterprise = Task.FromResult<IList<string>>(reportListForEnterprise);
			var reportListforGlow = new List<string>();
			for (var i = 0; i < numGlowReports; i++)
			{
				var id = Guid.NewGuid().ToString();
				reportListforGlow.Add(id);
				mockClient.Setup(c => c.DeleteCrashReportAsync(ErrorReportType.GlowXmlEnterpriseCompatible, id, accessToken, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
			}

			var tplTaskForGlow = Task.FromResult<IList<string>>(reportListforGlow);
			mockClient.Setup(x => x.RetrieveCrashReportIdentifiersAsync(ErrorReportType.EnterpriseXml, accessToken, maxNumberOfReports, It.IsAny<CancellationToken>())).Returns(tplTaskForEnterprise);
			mockClient.Setup(x => x.RetrieveCrashReportAsync(ErrorReportType.EnterpriseXml, It.IsAny<string>(), accessToken, It.IsAny<CancellationToken>())).ReturnsAsync(delegate(ErrorReportType _, string x_, string x__, CancellationToken x___)
			{
				var data = Encoding.UTF8.GetBytes("Surprise Enterprise Exception!!");
				return new MemoryStream(data);
			});
			mockClient.Setup(x => x.RetrieveCrashReportIdentifiersAsync(ErrorReportType.GlowXmlEnterpriseCompatible, accessToken, maxNumberOfReports, It.IsAny<CancellationToken>())).Returns(tplTaskForGlow);
			mockClient.Setup(x => x.RetrieveCrashReportAsync(ErrorReportType.GlowXmlEnterpriseCompatible, It.IsAny<string>(), accessToken, It.IsAny<CancellationToken>())).ReturnsAsync(delegate(ErrorReportType _, string x_, string x__, CancellationToken x___)
			{
				var data = Encoding.Unicode.GetBytes("Surprise Glow Exception!!");
				return new MemoryStream(data);
			});
			var directory = Temp.GetNewTempSubdirectory();
			AssertEquals("Should have no files in directory (sanity check).", 0, Directory.EnumerateFiles(directory).Count());
			task.DownloadFromWebService_ForTestAsync(directory).Wait();
			var files = Directory.EnumerateFiles(directory);
			AssertEquals(string.Format("Should have {0} file(s) in directory", numEnterpriseReports + numGlowReports), numEnterpriseReports + numGlowReports, files.Count());
			var totalEnterprise = 0;
			var totalGlow = 0;
			foreach (var filePath in files)
			{
				var fileData = File.ReadAllBytes(filePath);
				var isEnterprise = Encoding.UTF8.GetString(fileData) == "Surprise Enterprise Exception!!";
				var isGlow = Encoding.Unicode.GetString(fileData) == "Surprise Glow Exception!!";
				AssertEquals("File data should match error report data", true, isEnterprise || isGlow);
				if (isEnterprise)
				{
					totalEnterprise++;
				}

				if (isGlow)
				{
					totalGlow++;
				}
			}

			AssertEquals(numEnterpriseReports, totalEnterprise);
			AssertEquals(numGlowReports, totalGlow);

			mockLogger.Setup(x => x.Log(LogType.Debug, It.IsAny<string>()));

			try
			{
				var factoryProvider = new BusinessObjectFactoryProvider();
				task.Process(directory, factoryProvider, deleteFilesAfterProcessed: true);
			}
			finally
			{
				TempDirectory.DeleteDirectory(directory);
			}

			mockLogger.Verify(x => x.Log(LogType.Debug, It.Is<string>(s => s.Contains("Processing"))),
				Times.Exactly(numEnterpriseReports + numGlowReports));
		}

		public void TestCleanUpTmpFiles()
		{
			ObjectFactory.Get<IProductRegistration>().KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Production;
			var mockLogger = new Mock<ILogger>(MockBehavior.Default);
			var task = new IssueManagerProcessorServiceTask { ServiceLogger = mockLogger.Object };

			var directory = Temp.GetNewTempSubdirectory();
			var id = Guid.NewGuid().ToString();
			var fileName = id + ".tmp";
			try
			{
				File.WriteAllText(Path.Combine(directory, fileName), @"<?xml version=""1.0"" ?>
<metadata>
</metadata>");
				task.CleanUpTmpFiles_ForTest(directory);

				AssertEquals(id + ".xml", Path.GetFileName(Directory.EnumerateFiles(directory).Single()));
			}
			finally
			{
				TempDirectory.DeleteDirectory(directory);
			}
		}
	}
}
