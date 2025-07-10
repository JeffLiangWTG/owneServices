using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NLog;
using NLog.Targets;
using NUnit.Framework;

namespace Enterprise.RemotePrinting.Client.Tests
{
	class LogWriterTest : TestCase
	{
		public void TestLogWithEncodingMessage()
		{
			var directoryPathForTest = new DirectoryInfo(Path.Combine(Constants.WebPrintClientDataPath, "LogForTest"));

			try
			{
				var fileName = new FileInfo(Path.Combine(directoryPathForTest.ToString(), "LogFileForTest.txt")).FullName;
				var fileTarget = LogWriter.RegisterFileTarget(directoryPathForTest.ToString(), "LogForTest", WebPrintEventLogEntryType.Information, WebPrintEventLogEntryType.Error, "*", 500);
				fileTarget.FileName = fileName;

				LogManager.ReconfigExistingLoggers();
				LogWriter.Append(WebPrintEventLogEntryType.Information, "中文测试");

				using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				using (var stream = new StreamReader(fileStream))
				{
					var log = stream.ReadToEnd();
					AssertContains("中文测试", log);
				}
			}
			finally
			{
				LogWriter.UnregisterAllLogTargets();
				if (directoryPathForTest.Exists)
				{
					foreach (var tempfile in directoryPathForTest.EnumerateFiles())
					{
						File.Delete(tempfile.FullName);
					}
					directoryPathForTest.Delete();
				}
			}
		}

		public void TestRegisterFileTargetDefaultValues()
		{
			var directoryPathForTest = new DirectoryInfo(Path.Combine(Constants.WebPrintClientDataPath, "LogForTest"));

			try
			{
				//initialise target 
				var fileTarget = LogWriter.RegisterFileTarget(directoryPathForTest.ToString(), "LogForTest", WebPrintEventLogEntryType.Information, WebPrintEventLogEntryType.Error, "*", 500);

				AssertContains("LogForTest_${date:format=yyyyMMdd}.txt", fileTarget.FileName.ToString());
				AssertEquals("ArchiveOldFileOnStartup ", true, fileTarget.ArchiveOldFileOnStartup);
				AssertEquals("ArchiveNumbering", ArchiveNumberingMode.Sequence, fileTarget.ArchiveNumbering);
				AssertEquals("MaxArchiveFiles", 1000, fileTarget.MaxArchiveFiles);
				AssertEquals("Layout", "[${longdate}] ${message}", fileTarget.Layout.ToString());
				AssertEquals("Encoding", Encoding.UTF8, fileTarget.Encoding);
				AssertEquals("KeepFileOpen", false, fileTarget.KeepFileOpen);
				AssertEquals("Name", "LogForTest_Information_" + Process.GetCurrentProcess().Id, fileTarget.Name);
			}
			finally
			{
				LogWriter.UnregisterAllLogTargets();
				if (directoryPathForTest.Exists)
				{
					foreach (var tempfile in directoryPathForTest.EnumerateFiles())
					{
						File.Delete(tempfile.FullName);
					}
					directoryPathForTest.Delete();
				}
			}
		}

		public void TestLogArchiveLogFileName()
		{
			var directoryPathForTest = new DirectoryInfo(Path.Combine(Constants.WebPrintClientDataPath, "LogForTest"));

			try
			{
				//initialise target with small size
				var fileTarget = LogWriter.RegisterFileTarget(directoryPathForTest.ToString(), "LogForTest", WebPrintEventLogEntryType.Information, WebPrintEventLogEntryType.Error, "*", 500);
				fileTarget.FileName = new FileInfo(Path.Combine(directoryPathForTest.ToString(), "LogFileForTest.txt")).FullName;
				fileTarget.MaxArchiveFiles = 2;

				LogManager.ReconfigExistingLoggers();

				LogWriter.Append(WebPrintEventLogEntryType.Information, "test1");

				System.Threading.Thread.Sleep(500);

				LogWriter.Append(WebPrintEventLogEntryType.Warning, "Some very big message : abcdefghijklmnopqrstuvwxyz0123456789 abcdefghijklmnopqrstuvwxyz0123456789 abcdefghijklmnopqrstuvwxyz0123456789 abcdefghijklmnopqrstuvwxyz0123456789 abcdefghijklmnopqrstuvwxyz0123456789 abcdefghijklmnopqrstuvwxyz0123456789 abcdefghijklmnopqrstuvwxyz0123456789 abcdefghijklmnopqrstuvwxyz0123456789 abcdefghijklmnopqrstuvwxyz0123456789 abcdefghijklmnopqrstuvwxyz0123456789");
				LogWriter.Append(WebPrintEventLogEntryType.Information, "We created a new log file.");

				var files = directoryPathForTest.GetFiles();

				AssertContains("Current log file", "LogFileForTest.txt", files[1].FullName);
				AssertContains("Archived log file", "LogFileForTest.0.txt", files[0].FullName);
				AssertEquals("currentLogFile and archived log should be present ", 2, files.Length);

				LogWriter.Append(WebPrintEventLogEntryType.Warning, "Some very big message : abcdefghijklmnopqrstuvwxyz0123456789 abcdefghijklmnopqrstuvwxyz0123456789 abcdefghijklmnopqrstuvwxyz0123456789 abcdefghijklmnopqrstuvwxyz0123456789 abcdefghijklmnopqrstuvwxyz0123456789 abcdefghijklmnopqrstuvwxyz0123456789 abcdefghijklmnopqrstuvwxyz0123456789 abcdefghijklmnopqrstuvwxyz0123456789 abcdefghijklmnopqrstuvwxyz0123456789 abcdefghijklmnopqrstuvwxyz0123456789");
				LogWriter.Append(WebPrintEventLogEntryType.Warning, "Some very big message : abcdefghijklmnopqrstuvwxyz0123456789 abcdefghijklmnopqrstuvwxyz0123456789 abcdefghijklmnopqrstuvwxyz0123456789 abcdefghijklmnopqrstuvwxyz0123456789 abcdefghijklmnopqrstuvwxyz0123456789 abcdefghijklmnopqrstuvwxyz0123456789 abcdefghijklmnopqrstuvwxyz0123456789 abcdefghijklmnopqrstuvwxyz0123456789 abcdefghijklmnopqrstuvwxyz0123456789 abcdefghijklmnopqrstuvwxyz0123456789");

				AssertEquals("MaxArchiveFiles set to 2 - two archived file and current log file.", 3, directoryPathForTest.GetFiles().Length);
			}
			finally
			{
				LogWriter.UnregisterAllLogTargets();
				if (directoryPathForTest.Exists)
				{
					foreach (var tempfile in directoryPathForTest.EnumerateFiles())
					{
						File.Delete(tempfile.FullName);
					}
					directoryPathForTest.Delete();
				}
			}
		}

		public void TestRegisterTarget()
		{
			var logfileTarget = new FileTarget();
			logfileTarget.FileName = "TestRegisterTarget";
			try
			{
				LogWriter.RegisterLogTarget(logfileTarget, WebPrintEventLogEntryType.Information, WebPrintEventLogEntryType.Error);
				AssertContains("TestRegisterTarget", (LogManager.Configuration.AllTargets[0] as FileTarget).FileName.ToString());
				AssertEquals("TestRegisterTarget", 1, LogManager.Configuration.AllTargets.Count);
			}
			finally
			{
				LogWriter.UnregisterTargetAndDispose(logfileTarget);
			}
		}

		public void TestLogSpecificLoggerName()
		{
			var directoryPathForTest = new DirectoryInfo(Path.Combine(Constants.WebPrintClientDataPath, "LogForTest"));

			try
			{
				//initialise target with small size
				var fileTarget = LogWriter.RegisterFileTarget(directoryPathForTest.ToString(), "LogForTest", WebPrintEventLogEntryType.Information, WebPrintEventLogEntryType.Error);
				fileTarget.FileName = new FileInfo(Path.Combine(directoryPathForTest.ToString(), "LogFileForTest.txt")).FullName;

				var specificLoggerNamePattern = "SpecificLogger";
				var fileTargetSpecifique = LogWriter.RegisterFileTarget(directoryPathForTest.ToString(), "LogSpecificForTest", WebPrintEventLogEntryType.Information, WebPrintEventLogEntryType.Error, specificLoggerNamePattern, 500);
				fileTargetSpecifique.FileName = new FileInfo(Path.Combine(directoryPathForTest.ToString(), "LogSpecificFileForTest.txt")).FullName;

				var specificLoggerNamePattern2 = "SpecificLogger2";
				var fileTargetSpecific2 = LogWriter.RegisterFileTarget(directoryPathForTest.ToString(), "LogSpecificFor2Test", WebPrintEventLogEntryType.Information, WebPrintEventLogEntryType.Error, specificLoggerNamePattern2, 500);
				fileTargetSpecific2.FileName = new FileInfo(Path.Combine(directoryPathForTest.ToString(), "LogSpecificFileFor2Test.txt")).FullName;

				LogManager.ReconfigExistingLoggers();

				LogWriter.Append(WebPrintEventLogEntryType.Warning, "This is a warning message.", specificLoggerNamePattern);
				LogWriter.Append(WebPrintEventLogEntryType.Information, "This is an information message.");
				LogWriter.Append(WebPrintEventLogEntryType.Warning, "This is a warning message for SpecificLogger2.", specificLoggerNamePattern2);
				LogWriter.Append(WebPrintEventLogEntryType.SystemInformation, "This is a system information message.");

				var files = directoryPathForTest.GetFiles();
				var expectedSpecificLog = files.Where(f => f.Name == "LogSpecificFileForTest.txt").First();
				var expectedSpecificLog2 = files.Where(f => f.Name == "LogSpecificFileFor2Test.txt").First();
				var expectedLog = files.Where(f => f.Name == "LogFileForTest.txt").First();

				using (var fileStream = new FileStream(expectedSpecificLog.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				using (var stream = new StreamReader(fileStream))
				{
					var log = stream.ReadToEnd();
					AssertContains("This is a warning message.", log);
					AssertNotContains("This is an information message.", log);
					AssertNotContains("This is a warning message for SpecificLogger2.", log);
				}

				using (var fileStream = new FileStream(expectedSpecificLog2.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				using (var stream = new StreamReader(fileStream))
				{
					var log = stream.ReadToEnd();
					AssertNotContains("This is a warning message.", log);
					AssertNotContains("This is an information message.", log);
					AssertContains("This is a warning message for SpecificLogger2.", log);
				}

				using (var fileStream = new FileStream(expectedLog.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				using (var stream = new StreamReader(fileStream))
				{
					var log = stream.ReadToEnd();
					AssertContains("This is a warning message.", log);
					AssertContains("This is an information message.", log);
					AssertContains("This is a warning message for SpecificLogger2.", log);
					AssertContains("This is an information message.", log);
				}
			}
			finally
			{
				LogWriter.UnregisterAllLogTargets();
				if (directoryPathForTest.Exists)
				{
					foreach (var tempfile in directoryPathForTest.EnumerateFiles())
					{
						File.Delete(tempfile.FullName);
					}
					directoryPathForTest.Delete();
				}
			}
		}

		public void TestUnregisterTarget()
		{
			var logfileTarget = new FileTarget();
			logfileTarget.FileName = "TestUnregisterTarget";
			var logfileTarget2 = new FileTarget();
			logfileTarget2.FileName = "TestUnregisterTarget2";

			LogWriter.RegisterLogTarget(logfileTarget, WebPrintEventLogEntryType.Information, WebPrintEventLogEntryType.Error);
			LogWriter.RegisterLogTarget(logfileTarget2, WebPrintEventLogEntryType.Information, WebPrintEventLogEntryType.Error);
			AssertContains("TestUnregisterTarget", (LogManager.Configuration.AllTargets[0] as FileTarget).FileName.ToString());
			AssertContains("TestUnregisterTarget2", (LogManager.Configuration.AllTargets[1] as FileTarget).FileName.ToString());
			AssertEquals(2, LogManager.Configuration.AllTargets.Count);

			LogWriter.UnregisterTargetAndDispose(logfileTarget);
			AssertEquals("only logfileTarget2 remain", 1, LogManager.Configuration.AllTargets.Count);
			AssertEquals("only logfileTarget2 rule remain", 1, LogManager.Configuration.LoggingRules.Count);

			LogWriter.UnregisterTargetAndDispose(logfileTarget2);
			AssertEquals("There shoule be no rule remain as we have unregister all rules", 0, LogManager.Configuration.LoggingRules.Count);
		}

		public void TestUnregisterAllTargets()
		{
			var logfileTarget = new FileTarget() { FileName = "TestUnregisterAllTarget" };
			var logfileTarget1 = new FileTarget() { FileName = "TestUnregisterAllTarget1" };
			var logfileTarget2 = new FileTarget() { FileName = "TestUnregisterAllTarget2" };

			LogWriter.RegisterLogTarget(logfileTarget, WebPrintEventLogEntryType.Information, WebPrintEventLogEntryType.Error);
			LogWriter.RegisterLogTarget(logfileTarget1, WebPrintEventLogEntryType.Information, WebPrintEventLogEntryType.Error);
			LogWriter.RegisterLogTarget(logfileTarget2, WebPrintEventLogEntryType.Information, WebPrintEventLogEntryType.Error);
			AssertContains("TestUnregisterAllTarget", (LogManager.Configuration.AllTargets[0] as FileTarget).FileName.ToString());
			AssertContains("TestUnregisterAllTarget1", (LogManager.Configuration.AllTargets[1] as FileTarget).FileName.ToString());
			AssertContains("TestUnregisterAllTarget2", (LogManager.Configuration.AllTargets[2] as FileTarget).FileName.ToString());

			LogWriter.UnregisterAllLogTargets();
			AssertNull(LogManager.Configuration);
		}

		public void TestRegisterTarget_SystemInfo()
		{
			try
			{
				var logfileTarget = new FileTarget() { FileName = "TestRegisterTarget_SystemInfo" };
				LogWriter.RegisterLogTarget(logfileTarget, WebPrintEventLogEntryType.SystemInformation, WebPrintEventLogEntryType.Error);

				var logManager = LogManager.Configuration;
				AssertEquals("One Rule should be set for the FileTarget --  SystemInfo", 1, LogManager.Configuration.LoggingRules.Count);
				AssertEquals("Rule should be present in LoggingRules ", true, logManager.LoggingRules.Any(rule => rule.RuleName == logfileTarget.Name));
			}
			finally
			{
				LogWriter.UnregisterAllLogTargets();
			}
		}

		public void TestGetLogLevel()
		{
			AssertEquals(LogLevel.Trace, LogWriter.GetLogLevel(WebPrintEventLogEntryType.VerboseInformation));
			AssertEquals(LogLevel.Info, LogWriter.GetLogLevel(WebPrintEventLogEntryType.SystemInformation));
			AssertEquals(LogLevel.Info, LogWriter.GetLogLevel(WebPrintEventLogEntryType.Information));
			AssertEquals(LogLevel.Warn, LogWriter.GetLogLevel(WebPrintEventLogEntryType.Warning));
			AssertEquals(LogLevel.Error, LogWriter.GetLogLevel(WebPrintEventLogEntryType.Error));
		}

		public void TestRegisterTextBoxBaseTarget()
		{
			using (var form = new Form() { Name = "SangoForm" })
			using (var box = new RichTextBox())
			using (var box2 = new TextBox() { Name = "Sango" })
			{
				form.Controls.Add(box2);
				form.Controls.Add(box);
				box.CreateControl();
				form.Show();
				var expectingString = "this string should be written in the textbox";
				LogWriter.RegisterTextBoxBaseTarget(box2, WebPrintEventLogEntryType.Information, WebPrintEventLogEntryType.Error);
				LogWriter.RegisterTextBoxBaseTarget(box, WebPrintEventLogEntryType.Information, WebPrintEventLogEntryType.Error);
				LogWriter.Append(WebPrintEventLogEntryType.Information, expectingString);

				AssertContains(expectingString, box2.Text);
				AssertContains(expectingString, box.Text);
				LogWriter.UnregisterAllLogTargets();
				AssertNull(LogManager.Configuration);
			}
		}

		public void TestRegisterEventLogTarget()
		{
			try
			{
				LogWriter.RegisterEventLogTarget("WebPrintEventLog", WebPrintEventLogEntryType.Error, WebPrintEventLogEntryType.Error);

				AssertEquals("Config contains One logger : EventLogTarget", 1, LogManager.Configuration.AllTargets.Where(targets => targets.Name == "WebPrintEventLog").Count());
				AssertEquals("Config contains EventLogTarget", 1, LogManager.Configuration.LoggingRules.Count);
			}
			finally
			{
				LogWriter.UnregisterAllLogTargets();
			}
		}
	}
}
