using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.Environment;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Client.UPE.ServiceTask.Testing
{
	internal class UPEServiceTaskTest : TestCaseWithFactory
	{
		public void TestTryMoveOrDeleteFile()
		{
			var task = new UPEServiceTaskForTest(Logger);
			string testDirectory = Path.Combine(Env.TempPath, "UPEBATCHPROCESSORTEST");
			string archiveDirectory = Path.Combine(testDirectory, "ARCHIVE");
			try
			{
				Directory.CreateDirectory(archiveDirectory);
				string testFile1 = Path.Combine(testDirectory, "TestFile1");
				CreateFile(testFile1);
				task.TryMoveOrDeleteFile(testFile1, "");
				Assert("Should be deleted", !File.Exists(testFile1));
				AssertEquals("Should not be archived, archive directory is not specified", 0, Directory.GetFiles(archiveDirectory).Length);
				CreateFile(testFile1);
				task.TryMoveOrDeleteFile(testFile1, archiveDirectory);
				Assert("Should be moved", !File.Exists(testFile1));
				AssertEquals("Should be archived", 1, Directory.GetFiles(archiveDirectory).Length);
				DeleteDirectoryContentRecursively(archiveDirectory);
				CreateFile(testFile1);
				using (File.OpenWrite(testFile1))
				{
					task.TryMoveOrDeleteFile(testFile1, archiveDirectory);
				}

				Assert("File was in use, should not be moved/deleted", File.Exists(testFile1));
				AssertEquals("Should not be archived", 0, Directory.GetFiles(archiveDirectory).Length);
				AssertEquals("There should be a warning", 1, ((TestServiceLogger)task.ServiceLogger).Count);
				Assert("There should be a warning", Logger.ToString().Contains("Cannot archive or delete file \"" + testFile1 + "\""));
			}
			finally
			{
				TempDirectory.DeleteDirectory(testDirectory);
			}
		}

		void DeleteDirectoryContentRecursively(string path)
		{
			if (Directory.Exists(path))
			{
				foreach (string fileName in Directory.GetFiles(path))
				{
					File.SetAttributes(fileName, ~FileAttributes.ReadOnly);
					File.Delete(fileName);
				}

				foreach (string directoryName in Directory.GetDirectories(path))
				{
					TempDirectory.DeleteDirectory(directoryName);
				}
			}
		}

		public void TestTryDeleteFile()
		{
			var task = new UPEServiceTaskForTest(Logger);
			string testDirectory = Path.Combine(Env.TempPath, "UPEBATCHPROCESSORTEST");
			string archiveDirectory = Path.Combine(testDirectory, "ARCHIVE");
			try
			{
				Directory.CreateDirectory(archiveDirectory);
				string testFile1 = Path.Combine(testDirectory, "TestFile1");
				CreateFile(testFile1);
				task.TryDeleteFile(testFile1);
				Assert("Should be deleted", !File.Exists(testFile1));
				AssertEquals("Should not be archived, archive directory is not specified", 0, Directory.GetFiles(archiveDirectory).Length);
			}
			finally
			{
				TempDirectory.DeleteDirectory(testDirectory);
			}
		}

		void CreateFile(string path)
		{
			using (File.Create(path))
			{
			}
		}

		TestServiceLogger Logger
		{
			get
			{
				return logger ?? (logger = new TestServiceLogger());
			}
		}

		TestServiceLogger logger;
	}
}
