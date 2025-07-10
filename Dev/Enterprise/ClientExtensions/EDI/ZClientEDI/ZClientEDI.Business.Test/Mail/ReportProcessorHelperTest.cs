using System;
using System.IO;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.Environment;
using Enterprise.ServiceManager.Shared;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Client.EDI.Mail.Business.Test
{
	internal class ReportProcessorHelperTest : TestCase
	{
		public void TestGetReportDirectory()
		{
			string expectedTempPath = Path.Combine(Env.TempPath, "TestTempReportDirectory");
			AssertEquals("Path to Temp folder is returned", expectedTempPath, ReportProcessorHelper.GetReportDirectoryPath("TestTempReportDirectory"));
			ReportProcessorHelper.reportDirectoryForTesting = null;
			string expectedUserPath = Path.Combine(ServiceManagerHelper.GetLogFilesDirectory(Db.ServerName, Db.DatabaseName), "TestUserReportDirectory");
			AssertEquals("Path to User folder is returned", expectedUserPath, ReportProcessorHelper.GetReportDirectoryPath("TestUserReportDirectory"));
			// reset to original value
			ReportProcessorHelper.reportDirectoryForTesting = Env.TempPath;
		}

		public void TestSaveReport()
		{
			ReportProcessorHelper.SaveReport(null, "folder", "abc", "123", "somedata");
			string expectedPath = Path.Combine(ReportProcessorHelper.GetReportDirectoryPath("folder"), "abc-123.xml");
			Assert(File.Exists(expectedPath));
			AssertEquals("somedata", File.ReadAllText(expectedPath));
			ReportProcessorHelper.ClearReportsFromTesting("folder");
			Assert("Report is cleared successfully", !File.Exists(expectedPath));

			TestServiceLogger logger = new TestServiceLogger();
			ReportProcessorHelper.SaveReport(logger, "folder", "abc", "???", "somedata");
			AssertEquals("log count", 1, logger.Count);
			Assert("log contents", logger[0].StartsWith("Error|Couldn't save file abc-???.xml|System.ArgumentException: Illegal characters in path"));
			ReportProcessorHelper.ClearReportsFromTesting("folder");
			AssertEquals(typeof(ArgumentException), ErrorReporter.LastExceptionReported.GetType());
			AssertEquals("Couldn't save file", ErrorReporter.LastKeyReported);
			AssertEquals(ErrorReporter.LastExceptionReported.Message, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}
	}
}
