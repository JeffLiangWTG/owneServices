using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Principal;
using CargoWise.IO;
using Enterprise.RemotePrinting.Types;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.RemotePrinting.Client.Tests
{
	public class LogCollectorTest : TestCase
	{
		public void TestGetCompressedLogFile()
		{
			var testDirectory = Temp.GetNewTempSubdirectory();
			var logCollector = new LogCollectorForTest(testDirectory);

			try
			{
				if (!Directory.Exists(testDirectory))
				{
					Directory.CreateDirectory(testDirectory);
				}

				File.WriteAllText(Path.Combine(testDirectory, "Log_abc.txt"), "test data abc");
				File.WriteAllText(Path.Combine(testDirectory, "Log_xyz.txt"), "test data xyz");
				File.WriteAllText(Path.Combine(testDirectory, "ServiceLog_abc.txt"), "test data service abc");
				File.WriteAllText(Path.Combine(testDirectory, "OtherLog_abc.txt"), "test data other abc");

				var subDirectory = Path.Combine(testDirectory, "subTestDirectory");
				if (!Directory.Exists(subDirectory))
				{
					Directory.CreateDirectory(subDirectory);
				}

				File.WriteAllText(Path.Combine(subDirectory, "Log_sub_abc.txt"), "test data abc");
				File.WriteAllText(Path.Combine(subDirectory, "Log_sub_xyz.txt"), "test data xyz");

				var zipFile = logCollector.GetCompressedLogFile(LogTypes.Log | LogTypes.ServiceLog, DateTime.Today, DateTime.Today.AddDays(1));

				var extractPath = Path.Combine(testDirectory, "zip");
				ZipFile.ExtractToDirectory(zipFile, extractPath);
				logCollector.DeleteTempLogFiles(zipFile, 2, false);

				var extractedFiles = new DirectoryInfo(extractPath).GetFiles("*", SearchOption.AllDirectories).Select(a => a.Name).ToList();

				AssertEquals(5, extractedFiles.Count);
				AssertCollectionContains("Log_abc.txt", extractedFiles);
				AssertCollectionContains("Log_xyz.txt", extractedFiles);
				AssertCollectionContains("ServiceLog_abc.txt", extractedFiles);
				AssertCollectionContains("Log_sub_abc.txt", extractedFiles);
				AssertCollectionContains("Log_sub_xyz.txt", extractedFiles);

				var extractedDirectory = new DirectoryInfo(extractPath).GetDirectories("*", SearchOption.AllDirectories).Select(a => a.Name).ToList();
				AssertCollectionContains("subTestDirectory", extractedDirectory);

				AssertEquals("test data abc", File.ReadAllText(Path.Combine(extractPath, "Log_abc.txt")));
				AssertEquals("test data xyz", File.ReadAllText(Path.Combine(extractPath, "Log_xyz.txt")));
				AssertEquals("test data service abc", File.ReadAllText(Path.Combine(extractPath, "ServiceLog_abc.txt")));
				AssertEquals("test data abc", File.ReadAllText(Path.Combine(extractPath, subDirectory, "Log_sub_abc.txt")));
			}
			finally
			{
				TempDirectory.DeleteDirectory(testDirectory);
			}
		}

		public void TestGetCompressedLogFileWindowsEventErrorLogs()
		{
			var testDirectory = Temp.GetNewTempSubdirectory();
			var logCollector = new LogCollectorForTest(testDirectory);

			try
			{
				if (!Directory.Exists(testDirectory))
				{
					Directory.CreateDirectory(testDirectory);
				}

				logCollector.WindowsEventsForTest = new List<EventRecord>
				{
					new EventRecordForTest("Event 1", new DateTime(2021, 1, 1)),
					new EventRecordForTest("Event 2", new DateTime(2022, 3, 4, 12, 15, 40)),
					new EventRecordForTest("Event 3", null),
				};

				var zipFile = logCollector.GetCompressedLogFile(LogTypes.Log | LogTypes.WindowsEvent, DateTime.Today, DateTime.Today.AddDays(1));

				var extractPath = Path.Combine(testDirectory, "zip");
				ZipFile.ExtractToDirectory(zipFile, extractPath);
				logCollector.DeleteTempLogFiles(zipFile, 2, false);

				var extractedFiles = new DirectoryInfo(extractPath).GetFiles("*", SearchOption.AllDirectories).Select(a => a.Name).ToList();

				AssertEquals(1, extractedFiles.Count);
				AssertCollectionContains("WindowsEventLogs.txt", extractedFiles);

				var logText = File.ReadAllText(Path.Combine(extractPath, "WindowsEventLogs.txt")).Trim();
				AssertEquals(@"2021-01-01 00:00:00 - TestProvider: Event 1

2022-03-04 12:15:40 - TestProvider: Event 2

 - TestProvider: Event 3",
					logText);
			}
			finally
			{
				TempDirectory.DeleteDirectory(testDirectory);
			}
		}

		public void TestGetCompressedLogFileWindowsEventErrorLogs_Error()
		{
			var testDirectory = Temp.GetNewTempSubdirectory();
			var logCollector = new LogCollectorForTest(testDirectory);

			try
			{
				if (!Directory.Exists(testDirectory))
				{
					Directory.CreateDirectory(testDirectory);
				}

				logCollector.ErrorForGetWebPrintWindowsEvents = "Failed to get new WindowsEvents log";

				var zipFile = logCollector.GetCompressedLogFile(LogTypes.Log | LogTypes.WindowsEvent, DateTime.Today, DateTime.Today.AddDays(1));

				var extractPath = Path.Combine(testDirectory, "zip");
				ZipFile.ExtractToDirectory(zipFile, extractPath);
				logCollector.DeleteTempLogFiles(zipFile, 2, false);

				var extractedFiles = new DirectoryInfo(extractPath).GetFiles("*", SearchOption.AllDirectories).Select(a => a.Name).ToList();

				AssertEquals(1, extractedFiles.Count);
				AssertCollectionContains("WindowsEventLogs.txt", extractedFiles);

				AssertContains("Failed to get new WindowsEvents log", File.ReadAllText(Path.Combine(extractPath, "WindowsEventLogs.txt")));
			}
			finally
			{
				TempDirectory.DeleteDirectory(testDirectory);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "Testing")]
		public void TestRetrieveLogsAndPostToServer()
		{
			var webClientForTest = new WebClientForTesting(null);
			var controller = new HubClientController("TestServer", null, null, null, webClientForTest);

			var testDirectory = Temp.GetNewTempSubdirectory();
			var logCollector = new LogCollectorForTest(testDirectory);

			try
			{
				if (!Directory.Exists(testDirectory))
				{
					Directory.CreateDirectory(testDirectory);
				}
				File.WriteAllText(Path.Combine(testDirectory, "Log_abc.txt"), "test data abc");

				var startDate = DateTime.Today.AddDays(-3);
				var endDate = DateTime.Today.AddDays(1);
				controller.RetrieveLogsAndPostToServer("aaa@bbb.ccc", startDate, endDate, LogTypes.Log, logCollector);

				AssertNotNull(webClientForTest.LastUploadedLogFileName);

				var zipFile = Path.Combine(testDirectory, "testFile.zip");
				using (var fs = new FileStream(zipFile, FileMode.Create, FileAccess.ReadWrite))
				using (var bw = new BinaryWriter(fs))
				{
					bw.Write(webClientForTest.LastUploadedLogFile);
					bw.Close();
				}
				var extractPath = Path.Combine(testDirectory, "zip");
				ZipFile.ExtractToDirectory(zipFile, extractPath);
				var extractedFiles = new DirectoryInfo(extractPath).GetFiles().Select(a => a.Name).ToList();

				AssertEquals(1, extractedFiles.Count);
				AssertCollectionContains("Log_abc.txt", extractedFiles);

				var expectedComments = "Archive with log files for Print Server 'TestServer' from " + startDate.ToString("dd-MMM-yyyy") + " to " + endDate.AddDays(-1).ToString("dd-MMM-yyyy");
				AssertEquals(expectedComments, webClientForTest.LastUploadedLogComments);
			}
			finally
			{
				TempDirectory.DeleteDirectory(testDirectory);
			}
		}

		[TestRequiresAdministrativePrivileges("Admin privilege is required to create Windows Application Logs.")]
		public void TestGetWindowsEvents()
		{
			const string TestLogSource = "WebPrint Test";
			var testLogMessage = "Test Event " + Guid.NewGuid();
			CreateTestEntry(TestLogSource, testLogMessage);

			var testDirectory = Temp.GetNewTempSubdirectory();
			var logCollector = new LogCollectorForTest(testDirectory) { UseBaseGetWebPrintWindowsEvents = true };

			try
			{
				var eventRecords = logCollector.GetWebPrintWindowsEventsExposed(DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1));

				Assert("Should have WebPrint related Windows Events", eventRecords.Count > 0);
				AssertEquals(testLogMessage, eventRecords[eventRecords.Count - 1].FormatDescription());
			}
			finally
			{
				TempDirectory.DeleteDirectory(testDirectory);
			}
		}

		static void CreateTestEntry(string testLogSource, string message)
		{
			const string LogName = "Application";

			if (!EventLog.SourceExists(testLogSource))
			{
				EventLog.CreateEventSource(testLogSource, LogName);
			}
			var eventLog = new EventLog
			{
				Log = LogName,
				Source = testLogSource
			};
			eventLog.SafeWriteEntry(message, EventLogEntryType.Information);
		}
	}

	class LogCollectorForTest : LogCollector
	{
		public LogCollectorForTest(string logFilesPath) : base(logFilesPath) { }

		public List<EventRecord> GetWebPrintWindowsEventsExposed(DateTime startDate, DateTime endDate) => GetWebPrintWindowsEvents(startDate, endDate);

		protected override List<EventRecord> GetWebPrintWindowsEvents(DateTime startDate, DateTime endDate)
		{
			if (UseBaseGetWebPrintWindowsEvents)
			{
				return base.GetWebPrintWindowsEvents(startDate, endDate);
			}

			if (!string.IsNullOrEmpty(ErrorForGetWebPrintWindowsEvents))
			{
				throw new Exception(ErrorForGetWebPrintWindowsEvents);
			}
			else
			{
				return WindowsEventsForTest ?? new List<EventRecord>();
			}
		}

		public string ErrorForGetWebPrintWindowsEvents { get; set; }

		public List<EventRecord> WindowsEventsForTest { get; set; }

		public bool UseBaseGetWebPrintWindowsEvents { get; set; }
	}

	class EventRecordForTest : EventRecord
	{
		public EventRecordForTest(string descriptionText, DateTime? timeCreated)
		{
			DescriptionText = descriptionText;
			TimeCreated = timeCreated;
		}

		public string DescriptionText { get; set; }

		public override string FormatDescription() => DescriptionText;

		public override string FormatDescription(IEnumerable<object> values) => DescriptionText;

		public override string ToXml() => "<event>" + DescriptionText + "</event>";

		public override int Id => 0;
		public override byte? Version => null;
		public override byte? Level => null;
		public override int? Task => null;
		public override short? Opcode => null;
		public override long? Keywords => null;
		public override long? RecordId => null;
		public override string ProviderName => "TestProvider";
		public override Guid? ProviderId => null;
		public override string LogName => "TestLog";
		public override int? ProcessId => null;
		public override int? ThreadId => null;
		public override string MachineName => "TestMachine";
		public override SecurityIdentifier UserId => null;
		public override DateTime? TimeCreated { get; }
		public override Guid? ActivityId => null;
		public override Guid? RelatedActivityId => null;
		public override int? Qualifiers => null;
		public override string LevelDisplayName => "TestLevel";
		public override string OpcodeDisplayName => "TestOpcode";
		public override string TaskDisplayName => "TestTask";
		public override IEnumerable<string> KeywordsDisplayNames => Enumerable.Empty<string>();
		public override EventBookmark Bookmark => null;
		public override IList<EventProperty> Properties => new List<EventProperty>();
	}
}
