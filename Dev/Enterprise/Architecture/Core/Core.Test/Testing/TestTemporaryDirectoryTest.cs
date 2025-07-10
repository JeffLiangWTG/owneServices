using System;
using System.IO;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class TestTemporaryDirectoryTest : TestCase
	{
		public void TestDirectoryCreatedAndRemoved()
		{
			DirectoryInfo actualDirectory;
			using (TestTemporaryDirectory tempDir = new TestTemporaryDirectory())
			{
				actualDirectory = tempDir.Directory;
				AssertEquals("Directory should be created", true, actualDirectory.Exists);

				CreateFile(Path.Combine(actualDirectory.FullName, "splaty.dat"));
			}
			actualDirectory = new DirectoryInfo(actualDirectory.FullName); // refresh
			AssertEquals("Directory should be deleted now", false, actualDirectory.Exists);
		}

		void CreateFile(string fileName)
		{
			using (File.Create(fileName))
			{
			}
		}

		[ExpectException(typeof(ObjectDisposedException))]
		public void TestAccessingDirectoryAfterDisposed()
		{
			TestTemporaryDirectory tempDir;
			using (tempDir = new TestTemporaryDirectory())
			{
			}
			object notUsed = tempDir.Directory;
		}
	}
}
