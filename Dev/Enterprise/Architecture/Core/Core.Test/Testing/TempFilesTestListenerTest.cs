using System;
using System.IO;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	public class TempFilesTestListenerTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestNoChangeTriggersNothing()
		{
			TempFilesTestListener listener = new TempFilesTestListener(TimeSpan.Zero);
			TestCase testCase = new EmptyTest();
			listener.StartAllTests(EnvProxy.Instance.Time.CurrentLocalDateTime);
			try
			{
				listener.StartTest(testCase, EnvProxy.Instance.Time.CurrentLocalDateTime);
				listener.EndTest(testCase, EnvProxy.Instance.Time.CurrentLocalDateTime);
			}
			finally
			{
				listener.EndAllTests(EnvProxy.Instance.Time.CurrentLocalDateTime);
			}
		}

		public void TestAddingFileTriggersChange()
		{
			string fileName = Path.Combine(EnvProxy.Instance.TempPath, "Hello.txt");
			if (File.Exists(fileName))
			{
				File.Delete(fileName);
			}

			TempFilesTestListener listener = new TempFilesTestListener(TimeSpan.FromSeconds(10));
			TestCase testCase = new EmptyTest();
			listener.StartAllTests(EnvProxy.Instance.Time.CurrentLocalDateTime);
			try
			{
				listener.StartTest(testCase, EnvProxy.Instance.Time.CurrentLocalDateTime);
				File.WriteAllText(fileName, "Hello");
				try
				{
					listener.EndTest(testCase, EnvProxy.Instance.Time.CurrentLocalDateTime);
					Fail("AssertionFailedError should have been thrown by now.");
				}
				catch (AssertionFailedError ex)
				{
					AssertContains("Hello.txt", ex.Message);
				}
				finally
				{
					TempFile.Delete(fileName);
				}
			}
			finally
			{
				listener.EndAllTests(EnvProxy.Instance.Time.CurrentLocalDateTime);
			}
		}

		public void TestAddingDirectoryTriggersChange()
		{
			string directoryName = Path.Combine(EnvProxy.Instance.TempPath, "HelloDirectory");
			if (Directory.Exists(directoryName))
			{
				Directory.Delete(directoryName);
			}
			TempFilesTestListener listener = new TempFilesTestListener(TimeSpan.FromSeconds(10));
			TestCase testCase = new EmptyTest();
			listener.StartAllTests(EnvProxy.Instance.Time.CurrentLocalDateTime);
			try
			{
				listener.StartTest(testCase, EnvProxy.Instance.Time.CurrentLocalDateTime);
				AssertEquals("Precondition: Directory does not exist", false, Directory.Exists(directoryName));
				Directory.CreateDirectory(directoryName);
				AssertEquals("DirectoryExists", true, Directory.Exists(directoryName));
				bool threwAssertionFailedError = false;
				try
				{
					listener.EndTest(testCase, EnvProxy.Instance.Time.CurrentLocalDateTime);
				}
				catch (AssertionFailedError ex)
				{
					threwAssertionFailedError = true;
					AssertContains("HelloDirectory", ex.Message);
				}
				finally
				{
					AssertEquals("PostCondition: Directory should exist", true, Directory.Exists(directoryName));
					Directory.Delete(directoryName);
					AssertEquals("PostCondition: Directory should not exist", false, Directory.Exists(directoryName));
					if (!threwAssertionFailedError)
					{
						Fail("EndTest should have thrown AssertionFailedError");
					}
				}
			}
			finally
			{
				listener.EndAllTests(EnvProxy.Instance.Time.CurrentLocalDateTime);
			}
		}

		[ExpectNoExceptions]
		public void TestRemovingFileDoesNotError()
		{
			TempFilesTestListener listener = new TempFilesTestListener(TimeSpan.FromSeconds(10));
			TestCase testCase = new EmptyTest();
			listener.StartAllTests(EnvProxy.Instance.Time.CurrentLocalDateTime);
			string fileName = Path.Combine(EnvProxy.Instance.TempPath, "Hello.txt");
			File.WriteAllText(fileName, "Hello");
			try
			{
				listener.StartTest(testCase, EnvProxy.Instance.Time.CurrentLocalDateTime);
				File.Delete(fileName);
				listener.EndTest(testCase, EnvProxy.Instance.Time.CurrentLocalDateTime);
			}
			finally
			{
				listener.EndAllTests(EnvProxy.Instance.Time.CurrentLocalDateTime);
			}
		}

		[ExpectNoExceptions]
		public void TestRemovingDirectoryDoesNotError()
		{
			TempFilesTestListener listener = new TempFilesTestListener(TimeSpan.FromSeconds(10));
			TestCase testCase = new EmptyTest();
			listener.StartAllTests(EnvProxy.Instance.Time.CurrentLocalDateTime);
			string directoryName = Path.Combine(EnvProxy.Instance.TempPath, "Hello");
			Directory.CreateDirectory(directoryName);
			try
			{
				listener.StartTest(testCase, EnvProxy.Instance.Time.CurrentLocalDateTime);
				Directory.Delete(directoryName);
				listener.EndTest(testCase, EnvProxy.Instance.Time.CurrentLocalDateTime);
			}
			finally
			{
				listener.EndAllTests(EnvProxy.Instance.Time.CurrentLocalDateTime);
			}
		}

		public void TestStartAllTestRemovesTempFilesFirst()
		{
			TempFilesTestListener listener = new TempFilesTestListener(TimeSpan.FromSeconds(10));
			string fileName1 = Path.Combine(EnvProxy.Instance.TempPath, "Hello1.txt");
			File.WriteAllText(fileName1, "Hello");
			File.SetAttributes(fileName1, FileAttributes.Normal);
			string fileName2 = Path.Combine(EnvProxy.Instance.TempPath, "Hello2.txt");
			File.WriteAllText(fileName2, "Hello");
			File.SetAttributes(fileName2, FileAttributes.ReadOnly);
			Assert(File.Exists(fileName1));
			Assert(File.Exists(fileName2));
			listener.StartAllTests(EnvProxy.Instance.Time.CurrentLocalDateTime);
			Assert(!File.Exists(fileName1));
			Assert(!File.Exists(fileName2));
		}

		class EmptyTest : TestCase
		{
		}
	}
}
