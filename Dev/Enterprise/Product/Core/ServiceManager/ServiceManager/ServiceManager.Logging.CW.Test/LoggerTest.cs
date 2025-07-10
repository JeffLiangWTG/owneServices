using System;
using System.IO;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ServiceManager.Shared;
using Microsoft.Extensions.Logging;
using Moq;
using NLog;
using NLog.Config;
using NLog.Targets;
using NUnit.Framework;
using ServiceManager.Logging.Abstractions;
using ServiceManager.Logging.CW;
using Logger = ServiceManager.Logging.CW.Logger;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace ServiceManager.Shared.CW.Test
{
	class LoggerTest : TransactionedTestCase
	{
		[TestDate(2007, 6, 19, 8, 38, 10)]
		public void TestLog()
		{
			using (new DummyNLogConfiguration())
			using (var tempDir = new TempDirectory())
			{
				var outputDir = Path.Combine(tempDir.DirectoryName, "TestLog");
				Assert("[PRE-CONDITION] Output directory should NOT exist", !Directory.Exists(outputDir));

				var testLogger = new LoggerForTesting(outputDir, false);

				using (LogManager.Configuration.AllTargets.OfType<FileTarget>().Single())
				{
					AssertEquals("Output directory not created", false, Directory.Exists(outputDir));
					testLogger.Log(LogLevel.Error, "An Error: Line 1\r\nLine 2\r\nLine 3");
					LogManager.Flush(TimeSpan.Zero);
					AssertEquals("Output directory created", true, Directory.Exists(outputDir));

					var logFile = Path.Combine(outputDir, LoggerForTesting.TestProgramCode + "_20070619" + Logger.LogFileExtension);
					AssertEquals("Log file created", true, File.Exists(logFile));

					// Assert Number of Log Lines
					string fileContents;
					using (var stream = new FileStream(logFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
					using (var sr = new StreamReader(stream))
					{
						fileContents = sr.ReadToEnd().TrimEnd();
					}

					var logs = fileContents.Split('\n');
					AssertEquals("Number of log lines (records)", 1, logs.Length);

					// Assert Log Contents
					var logDateTimeStamp = logs[0].Substring(0, 25).Trim();
					AssertEquals("Date Time", "2007-06-19 08:38:10.000", logDateTimeStamp);
					var logLevel = logs[0].Substring(25, 25).Trim();
					AssertEquals("Log Type", nameof(LogLevel.Error), logLevel);
					var logMessage = logs[0].Substring(75).Trim();
					AssertEquals("Log Message", "An Error: Line 1\u21B5Line 2\u21B5Line 3", logMessage);

					testLogger.Log(LogLevel.Information, "An Info: Line 1\r\nLine 2");
					LogManager.Flush(TimeSpan.Zero);
					// Assert Number of Log Lines
					using (var stream = new FileStream(logFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
					using (var sr = new StreamReader(stream))
					{
						fileContents = sr.ReadToEnd().TrimEnd();
					}
					logs = fileContents.Split('\n');
					AssertEquals("Number of log lines (records)", 2, logs.Length);

					// Assert 2nd Log Contents
					logDateTimeStamp = logs[1].Substring(0, 25).Trim();
					AssertEquals("Log 2 Date Time", "2007-06-19 08:38:10.000", logDateTimeStamp);
					logLevel = logs[1].Substring(25, 25).Trim();
					AssertEquals("Log 2 Type", nameof(LogLevel.Information), logLevel);
					logMessage = logs[1].Substring(75).Trim();
					AssertEquals("Log 2 Message", "An Info: Line 1\u21B5Line 2", logMessage);
				}
			}
		}

		public void TestLogMultithreaded()
		{
			using (new DummyNLogConfiguration())
			using (var tempDir = new TempDirectory())
			{
				var outputDir = Path.Combine(tempDir.DirectoryName, "TestLog");
				Assert("[PRE-CONDITION] Output directory should NOT exist", !Directory.Exists(outputDir));

				var testLogger = new LoggerForTesting(outputDir, false);
				using (LogManager.Configuration.AllTargets.OfType<FileTarget>().Single())
				{
					AssertEquals("Output directory not created", false, Directory.Exists(outputDir));

					var threads = new Thread[10];
					for (var i = 0; i < threads.Length; i++)
					{
						threads[i] = new Thread(() =>
						{
							using (Db.DisposableActionForDbConnection())
							{
								var rand = new Random().Next(100);
								for (var j = 0; j < 10; j++)
								{
									testLogger.Log(LogLevel.Information, string.Format("{0} {1}", rand, j));
								}
							}
						});
					}
					Array.ForEach(threads, (t) => t.Start());
					Thread.Sleep(500);
					while (!Array.TrueForAll(threads, (t) => t.ThreadState != ThreadState.Running))
					{
						Thread.Sleep(100);
					}
					LogManager.Flush();
					AssertEquals("Output directory created", true, Directory.Exists(outputDir));
				}
			}
		}

		[TestDate(2007, 6, 19, 8, 38, 10)]
		public void TestLoggingDebug()
		{
			using (new DummyNLogConfiguration())
			using (var tempDir = new TempDirectory())
			{
				var outputDir = Path.Combine(tempDir.DirectoryName, "TestLog");
				Assert("[PRE-CONDITION] Output directory should NOT exist", !Directory.Exists(outputDir));

				var testLogger = new LoggerForTesting(outputDir, false);

				using (LogManager.Configuration.AllTargets.OfType<FileTarget>().Single())
				{
					Assert("Output directory not created", !Directory.Exists(outputDir));

					testLogger.Log(LogLevel.Debug, "An Info: Line 1\r\nLine 2");
					LogManager.Flush(TimeSpan.Zero);
					Assert("Output directory created", !Directory.Exists(outputDir));
				}
			}
		}

		[TestDate(2007, 6, 19, 8, 38, 10)]
		public void TestLoggingDebug_Verbose()
		{
			using (var tempDir = new TempDirectory())
			{
				Assert("TempDirectory created", Directory.Exists(tempDir));

				var outputDir = Path.Combine(tempDir.DirectoryName, "TestLog");
				Assert("[PRE-CONDITION] Output directory should NOT exist", !Directory.Exists(outputDir));

				using (SystemDataRegistry.Instance.ProcessControllerVerboseLogging.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new VerboseLoggingCollection()))
				{
					var testLogger = new LoggerForTesting(outputDir, false);
					using (LogManager.Configuration.AllTargets.OfType<FileTarget>().Single())
					{
						Assert("Output directory not created", !Directory.Exists(outputDir));

						testLogger.Log(LogLevel.Debug, "An Info: Line 1\r\nLine 2");
						LogManager.Flush(exception =>
						{
							AssertNull(exception);
							Assert("Output directory created", Directory.Exists(outputDir));

							var logFile = Path.Combine(outputDir, LoggerForTesting.TestProgramCode + "_20070619" + Logger.LogFileExtension);
							Assert("Log file created", File.Exists(logFile));

							// Assert Number of Log Lines
							string fileContents;
							using (var stream = new FileStream(logFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
							using (var sr = new StreamReader(stream))
							{
								fileContents = sr.ReadToEnd().TrimEnd();
							}

							var logs = fileContents.Split('\n');
							AssertEquals("Number of log lines (records)", 1, logs.Length);

							// Assert Log Contents
							var logDateTimeStamp = logs[0].Substring(0, 25).Trim();
							AssertEquals("Log 2 Date Time", "2007-06-19 08:38:10.000", logDateTimeStamp);
							var logLevel = logs[0].Substring(25, 25).Trim();
							AssertEquals("Log 2 Type", nameof(LogLevel.Debug), logLevel);
							var logMessage = logs[0].Substring(75).Trim();
							AssertEquals("Log 2 Message", "An Info: Line 1\\nLine 2", logMessage);
						},
							TimeSpan.Zero
						);
					}
				}
			}
		}

		public void TestGetFormattedLogLine()
		{
			//Arrange
			var universalDateTime = new DateTime(2007, 6, 18, 22, 38, 10, DateTimeKind.Utc);
			string loggedMessage = "->↵test↵message->↵";

			CombineAssertions(() =>
			{
				Test(123, universalDateTime, "Info", "eye.wtg.zone", 1, loggedMessage, "123                      2007-06-18 22:38:10.000  Information              1                        hostname[eye.wtg.zone]   ->↵test↵message->↵", null);
				Test(123, universalDateTime, "Warn", "eye.wtg.zone", 0, loggedMessage, "123                      2007-06-18 22:38:10.000  Warning                  0                        hostname[eye.wtg.zone]   ->↵test↵message->↵", null);
				Test(123, universalDateTime, null, "eye.wtg.zone", 99999, loggedMessage, "123                      2007-06-18 22:38:10.000                           99999                    hostname[eye.wtg.zone]   ->↵test↵message->↵", null);
				Test(123, universalDateTime, null, "", 99999, loggedMessage, @"123                      2007-06-18 22:38:10.000                           99999                    hostname[]               ->↵test↵message->↵↵System.Exception: Test exception", new Exception("Test exception"));
			});

			void Test(int sequenceId, DateTime dateTime, string logLevel, string hostName, int processId, string message, string expectedLog, Exception ex)
			{
				//Act
				var result = Logger.GetFormattedLogLine(sequenceId, dateTime, logLevel, hostName, processId, message, ex);

				//Assert
				AssertEquals(expectedLog, result);
			}
		}

		public void TestFormatMessage_Escape_Unescape()
		{
			var message = $@"Multiline message.
A tab 	 and some text.
Backslash t character (\t).
Backslash n (\n).
Backslash u21B5 (\u21B5).
Special ({'\u21B5'}).
Special ({'\u2192'}).
Path: c:\somefolder\somefile.ext";

			var expectedSingleLiner = @"Multiline message.↵A tab → and some text.↵Backslash t character (\\t).↵Backslash n (\\n).↵Backslash u21B5 (\\u21B5).↵Special (\↵).↵Special (\→).↵Path: c:\\somefolder\\somefile.ext";
			var singleLiner = Logger.GetEscapedSingleLineString(message);
			var multiliner = Logger.GetUnescapedMultilineString(singleLiner);

			AssertEquals(expectedSingleLiner, singleLiner);
			AssertNotContains("\r", multiliner);
			AssertContains(message.Replace("\r", ""), multiliner);
		}

		public void TestFormatMessage_ReturnsEscapedString()
		{
			var message = $@"This is a multiline message.
With a tab	in it.";

			var exception = new ArgumentException(message);

			var singleLiner = Logger.GetFormattedLogLine(1, new DateTime(2023, 2, 3), "Information", "eye.wtg.zone", 12345, "Exception", exception);
			var expectedSingleLiner = @"Exception↵System.ArgumentException: This is a multiline message.↵With a tab→in it.";

			AssertContains(expectedSingleLiner, singleLiner);
		}

		[ExpectNoExceptions]
		public void TestTrackServiceTaskErrors()
		{
			var errorTracker = AssertTrackServiceTaskErrors(true);
			errorTracker.Verify(tracker => tracker.TrackServiceTaskError("~@T"), Times.Once);
		}

		[ExpectNoExceptions]
		public void TestDoNotTrackServiceTaskErrors()
		{
			var errorTracker = AssertTrackServiceTaskErrors(false);
			errorTracker.VerifyNoOtherCalls();
		}

		Mock<IServiceTaskErrorTracker> AssertTrackServiceTaskErrors(bool trackServiceTaskErrors)
		{
			var serviceTaskErrorTrackerMock = new Mock<IServiceTaskErrorTracker>();
			using (new DummyNLogConfiguration())
			using (var tempDir = new TempDirectory())
			using (ObjectFactory.Substitute(serviceTaskErrorTrackerMock.Object))
			{
				var outputDir = Path.Combine(tempDir.DirectoryName, "TestLog");
				var testLogger = new LoggerForTesting(outputDir, trackServiceTaskErrors);
				using (LogManager.Configuration.AllTargets.OfType<FileTarget>().Single())
				{
					testLogger.Log(LogLevel.Error, "An Info: Line 1\r\nLine 2");
				}
			}
			return serviceTaskErrorTrackerMock;
		}

		class DummyNLogConfiguration : IDisposable
		{
			public DummyNLogConfiguration()
			{
				SystemDataRegistry.Instance.ProcessControllerNLogInternalLoggingEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				SystemDataRegistry.Instance.ProcessControllerVerboseLogging.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new VerboseLoggingCollection());

				LogManager.Configuration = new LoggingConfiguration();
				LogManager.ReconfigExistingLoggers();
			}

			public void Dispose()
			{
				LogManager.Configuration = new LoggingConfiguration();
				LogManager.ReconfigExistingLoggers();
			}
		}
	}

	class LoggerForTesting : LoggerNLogWrapper
	{
		public LoggerForTesting(string directoryPath, bool trackServiceTaskErrors = false)
			: base(Db.ServerName, Db.DatabaseName, TestProgramCode, directoryPath: directoryPath, trackServiceTaskErrors: trackServiceTaskErrors)
		{
		}

		internal const string TestProgramCode = "~@T";
	}

	class ZDateTimeExtensionsTest : TestCase
	{
		public void TestToLoggerString()
		{
			var time = new ZDateTime(2016, 5, 24, 12, 0, 0);
			AssertEquals("2016-05-24 12:00:00.000", time.ToLoggerString());
		}
	}
}
