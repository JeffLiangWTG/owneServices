using System.IO;
using System.Threading;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Client.UPE.Testing;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	internal class NonPersistentIOExecuterTest : TestCase
	{
		public void TestIOTaskUsingString()
		{
			FileToDelete = CreateTestFile();
			Assert("File exists", File.Exists(FileToDelete));
			IOTaskDelegate deleteFileTest = new IOTaskDelegate(DeleteFile);
			Executer.ExecuteIOTask(deleteFileTest, FileToDelete);
			Assert("File not exist", !File.Exists(FileToDelete));
		}

		public void TestIOTaskUsingFileInfo()
		{
			FileInfo file = new FileInfo(CreateTestFile());
			Assert("File exists", file.Exists);
			IOFileTaskDelegate deleteFileTest = new IOFileTaskDelegate(DeleteFile);
			Executer.ExecuteIOFileTask(deleteFileTest, file);
			Assert("File not exist", !File.Exists(file.FullName));
		}

		public void TestRetryAttempts()
		{
			Attempts = 0;
			Executer.TimeOutSeconds = 120;
			Executer.MaxRetries = 5;
			ZDateTime start = ZDateTime.Now;
			FileInfo file = new FileInfo(CreateTestFile());
			file.IsReadOnly = true;
			Assert("File exists", file.Exists);
			IOFileTaskDelegate deleteFileTest = new IOFileTaskDelegate(DeleteFileAttempt);
			Executer.ExecuteIOFileTask(deleteFileTest, file);
			Assert("File exists", file.Exists);
			AssertEquals("5 attempts", 5, Attempts);
			Assert("Less than timeout", (ZDateTime.Now - start).Seconds < Executer.TimeOutSeconds);
			file.IsReadOnly = false;
			file.Delete();
			ErrorReporter.Clear();
		}

		public void TestTimeoutAttempts()
		{
			Attempts = 0;
			Executer.TimeOutSeconds = 2;
			Executer.MaxRetries = 5000;
			ZDateTime start = ZDateTime.Now;
			FileInfo file = new FileInfo(CreateTestFile());
			file.IsReadOnly = true;
			Assert("File exists", file.Exists);
			IOFileTaskDelegate deleteFileTest = new IOFileTaskDelegate(DeleteFileAttempt);
			Executer.ExecuteIOFileTask(deleteFileTest, file);
			Assert("File exists", file.Exists);
			Assert("Attempts made", Attempts > 0);
			Assert("Timed out", (ZDateTime.Now - start).Seconds >= Executer.TimeOutSeconds);
			file.IsReadOnly = false;
			file.Delete();
			ErrorReporter.Clear();
		}

		string CreateTestFile()
		{
			string filename = Env.GetTempFileName();
			File.AppendAllText(filename, "contents");
			return filename;
		}

		void DeleteFile()
		{
			File.Delete(FileToDelete);
		}

		void DeleteFile(FileInfo file)
		{
			file.Delete();
		}

		void DeleteFileAttempt(FileInfo file)
		{
			Attempts++;
			Thread.Sleep(20);
			file.Delete();
		}

		string FileToDelete = string.Empty;
		int Attempts;

		NonPersistentIOExecuter Executer
		{
			get { return executer ?? (executer = new NonPersistentIOExecuter(TestHelper.Buffer)); }
		}
		NonPersistentIOExecuter executer;

		UPETestHelper TestHelper
		{
			get { return testHelper ?? (testHelper = new UPETestHelper()); }
		}
		UPETestHelper testHelper;
	}
}
