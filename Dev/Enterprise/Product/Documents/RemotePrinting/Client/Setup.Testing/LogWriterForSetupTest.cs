using System;
using System.IO;
using Enterprise.RemotePrinting.Client;
using Enterprise.RemotePrinting.Client.Setup.Custom;
using NUnit.Framework;

namespace Setup.Testing
{
	public class LogWriterForSetupTest : TestCase
	{
		public void TestGetLogFilePath()
		{
			var logWriter = new LogWriterForSetupTesting("Abc");
			var logFilePath = logWriter.LastLogFileName;
			var path = Path.GetDirectoryName(logFilePath);
			var fileName = Path.GetFileName(logFilePath);

			AssertEquals(Constants.LogsFolder, path);
			AssertEquals("InstallLog_20240725_144615_Abc.txt", fileName);

			logWriter = new LogWriterForSetupTesting("");
			logFilePath = logWriter.LastLogFileName;
			fileName = Path.GetFileName(logFilePath);
			AssertEquals("InstallLog_20240725_144615_EMPTY.txt", fileName);

			logWriter = new LogWriterForSetupTesting(null);
			logFilePath = logWriter.LastLogFileName;
			fileName = Path.GetFileName(logFilePath);
			AssertEquals("InstallLog_20240725_144615_EMPTY.txt", fileName);
		}

		public void TestWriteLog()
		{
			var logWriter = new LogWriterForSetupTesting("Abc");
			logWriter.WriteLog("Test message");

			AssertEquals("InstallLog_20240725_144615_Abc.txt", Path.GetFileName(logWriter.LastLogFileName));
			AssertContains("Test message", logWriter.LastLogMessage);
		}

		public class LogWriterForSetupTesting : LogWriterForSetup
		{
			public LogWriterForSetupTesting(string logName) : base(logName)
			{
			}

			protected override void WriteLogCore(string message)
			{
				LastLogMessage = message;

				if (FullLogMessage == null)
				{
					FullLogMessage = LastLogMessage;
				}
				else
				{
					FullLogMessage += LastLogMessage;
				}
			}

			override protected DateTime GetCurrentTime() => new DateTime(2024, 7, 25, 14, 46, 15);

			public string LastLogFileName => LogFilePath;
			public string LastLogMessage { get; set; }
			public string FullLogMessage { get; set; }
		}
	}
}
