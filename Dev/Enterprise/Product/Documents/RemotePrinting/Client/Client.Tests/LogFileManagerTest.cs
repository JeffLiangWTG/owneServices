using System;
using System.IO;
using System.Text;
using NUnit.Framework;

namespace Enterprise.RemotePrinting.Client.Tests
{
	class LogFileManagerTest : TestCase
	{
		public void TestCleanOldLogFiles()
		{
			var logManager = new LogFileManagerForTest();
			var logs = new StringBuilder();
			var outputDirectoryForFile = logManager.OutputDirectory_Exposed;
			outputDirectoryForFile.Create();

			var fileNamePrefixForTest = "LogForTest";

			var fileName1 = Path.Combine(outputDirectoryForFile.FullName, fileNamePrefixForTest + "1" + logManager.FileNameExtensionWithDotForTest);
			var fileName2 = Path.Combine(outputDirectoryForFile.FullName, fileNamePrefixForTest + "2" + logManager.FileNameExtensionWithDotForTest);
			var fileName3 = Path.Combine(outputDirectoryForFile.FullName, fileNamePrefixForTest + "3" + logManager.FileNameExtensionWithDotForTest);

			try
			{
				File.WriteAllText(fileName1, "1");
				File.WriteAllText(fileName2, "2");
				File.WriteAllText(fileName3, "3");

				new FileInfo(fileName1).LastWriteTime = DateTime.Now.AddDays(-10);
				new FileInfo(fileName2).LastWriteTime = DateTime.Now.AddDays(-5);
				new FileInfo(fileName3).LastWriteTime = DateTime.Now.AddDays(-2);

				logManager.CleanOldLogFiles(3, x => logs.Append(x));

				Assert("Should delete 10 days old file", !File.Exists(fileName1));
				Assert("Should delete 5 days old file", !File.Exists(fileName2));
				Assert("Should keep 2 days old file", File.Exists(fileName3));
				AssertContains("Cleaned 2 log files older than 3 days.", logs.ToString());
			}
			finally
			{
				if (File.Exists(fileName1))
				{
					File.Delete(fileName1);
				}
				if (File.Exists(fileName2))
				{
					File.Delete(fileName2);
				}
				if (File.Exists(fileName3))
				{
					File.Delete(fileName3);
				}
				if (Directory.Exists(outputDirectoryForFile.FullName))
				{
					Directory.Delete(outputDirectoryForFile.FullName);
				}
			}
		}

		[TestRequiresAdministrativePrivileges("Admin privilege is required to edit the Registry")]
		public void TestDefaultValues()
		{
			var logConfiguration = LogFileManager.Instance.GetLogConfiguration();

			CombineAssertions(() =>
			{
				AssertEquals("Enable clean old log", true, logConfiguration.ShouldClearOldLog);
				AssertEquals("Day to keep old log files", 14, logConfiguration.DayToKeepOldLogFile);
			});
		}

		class LogFileManagerForTest : LogFileManager
		{
			public string LogFileDirectoryForTest => "LogFileForTest";

			protected override DirectoryInfo OutputDirectory => new DirectoryInfo(Path.Combine(base.OutputDirectory.FullName, LogFileDirectoryForTest));

			public DirectoryInfo OutputDirectory_Exposed => OutputDirectory;

			public string FileNameExtensionWithDotForTest => ".txt";
		}
	}
}
