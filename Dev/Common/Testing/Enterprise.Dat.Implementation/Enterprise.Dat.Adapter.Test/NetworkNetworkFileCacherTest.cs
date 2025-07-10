using System;
using System.IO;
using CargoWise.IO;
using NUnit.Framework;

namespace Enterprise.Dat.Implementation.Testing
{
	sealed class NetworkNetworkFileCacherTest : TestCase
	{
		public void TestCacheFileLocally()
		{
			CreateFile(networkFileName);
			File.Delete(localFileName);

			Assert("PreCondition: network file should exist", File.Exists(networkFileName));
			Assert("PreCondition: locally cached file should not exist", !File.Exists(localFileName));

			NetworkFileCacher.CacheFileLocally(networkFileName, localFileName);
			Assert("File should be cached locally", File.Exists(localFileName));

			DateTime modifiedTime = DateTime.Now.AddSeconds(10);
			File.SetLastWriteTime(networkFileName, modifiedTime);
			NetworkFileCacher.CacheFileLocally(networkFileName, localFileName);
			AssertEquals("Updated version of the file should be copied", File.GetLastWriteTime(localFileName), modifiedTime);

			DateTime localModifiedTime = modifiedTime.AddSeconds(10);
			File.SetLastWriteTime(localFileName, localModifiedTime);
			NetworkFileCacher.CacheFileLocally(networkFileName, localFileName);
			AssertEquals("Network file should not be copied as the local file is newer.", File.GetLastWriteTime(localFileName), localModifiedTime);
		}

		public void TestOverwriteReadonlyFile()
		{
			CreateFile(networkFileName);
			CreateFile(localFileName);
			Assert("PreCondition: network file should exist", File.Exists(networkFileName));
			Assert("PreCondition: local file should exist", File.Exists(localFileName));

			DateTime networkModifiedTime = DateTime.Now;
			DateTime localModifiedTime = networkModifiedTime.AddSeconds(-10);

			File.SetLastWriteTime(localFileName, localModifiedTime);
			File.SetAttributes(localFileName, FileAttributes.ReadOnly);
			NetworkFileCacher.CacheFileLocally(networkFileName, localFileName);
			TimeSpan span = File.GetLastWriteTime(localFileName) - networkModifiedTime;
			Assert("File cacher should be able to update local files when they are marked ReadOnly.", span.TotalMilliseconds < 15);
		}

		public void TestFileIsNotUpdatedWhenTimeStampsMatch()
		{
			CreateFile(networkFileName);
			CreateFile(localFileName);
			Assert("PreCondition: network file should exist", File.Exists(networkFileName));
			Assert("PreCondition: local file should exist", File.Exists(localFileName));

			using (StreamWriter writer = new StreamWriter(networkFileName))
			{
				writer.Write("Network");
				writer.Flush();
			}

			using (StreamWriter writer = new StreamWriter(localFileName))
			{
				writer.Write("Local");
				writer.Flush();
			}

			DateTime networkModifiedTime = DateTime.Now.AddSeconds(-10);
			DateTime localModifiedTime = networkModifiedTime;
			File.SetLastWriteTime(networkFileName, networkModifiedTime);
			File.SetLastWriteTime(localFileName, localModifiedTime);

			NetworkFileCacher.CacheFileLocally(networkFileName, localFileName);

			string localFileContents = "";
			using (StreamReader reader = new StreamReader(localFileName))
			{
				localFileContents = reader.ReadToEnd();
			}

			AssertEquals("File should not be updated, as timestamps match", "Local", localFileContents);
		}

		public void TestCreatesRequiredDirectoryStructure()
		{
			CreateFile(networkFileName);
			string localDirectory = Path.Combine(TempForTest.TempPath, Guid.NewGuid().ToString());
			string localSubDirectory = Path.Combine(localDirectory, "ForNetworkNetworkFileCacher");
			string localFileInSubDirectory = Path.Combine(localSubDirectory, "TestFile");

			Assert("PreCondition: Test file should not exist", !File.Exists(localFileInSubDirectory));
			Assert("PreCondition: Subdirectory should not exist", !Directory.Exists(localSubDirectory));
			Assert("PreCondition: Directory should not exist", !Directory.Exists(localDirectory));

			NetworkFileCacher.CacheFileLocally(networkFileName, localFileInSubDirectory);
			Assert("File should be copied locally and the necessary subdirectories should be created.", File.Exists(localFileInSubDirectory));

			Directory.Delete(localDirectory, true);
		}

		public void TestCleanupOldCachedFilesDeletesOldFiles()
		{
			using (var temp = new TempDirectory())
			{
				File.WriteAllText(Path.Combine(temp, "CW201201.bak"), string.Empty);
				File.WriteAllText(Path.Combine(temp, "CW201201_Audit.bak"), string.Empty);

				foreach (var file in Directory.GetFiles(temp))
				{
					File.SetCreationTimeUtc(file, DateTime.UtcNow.Subtract(TimeSpan.FromDays(30)));
					File.SetLastAccessTimeUtc(file, DateTime.UtcNow.Subtract(TimeSpan.FromDays(9)));
				}

				NetworkFileCacher.CleanupOldCachedFiles(temp, "DPR");

				AssertArrayEqualsByElements(Array.Empty<string>(), Directory.GetFiles(temp));
			}
		}

		public void TestCleanupOldCachedFilesPreservesRecentlyUsedFiles()
		{
			using (var temp = new TempDirectory())
			{
				File.WriteAllText(Path.Combine(temp, "CW201201.bak"), string.Empty);
				File.WriteAllText(Path.Combine(temp, "CW201201_Audit.bak"), string.Empty);

				foreach (var file in Directory.GetFiles(temp))
				{
					File.SetCreationTimeUtc(file, DateTime.UtcNow.Subtract(TimeSpan.FromDays(30)));
					File.SetLastAccessTimeUtc(file, DateTime.UtcNow.Subtract(TimeSpan.FromHours(9)));
				}

				NetworkFileCacher.CleanupOldCachedFiles(temp, "DPR");

				AssertArrayEqualsByElements(new[] { Path.Combine(temp, "CW201201.bak"), Path.Combine(temp, "CW201201_Audit.bak") }, Directory.GetFiles(temp));
			}
		}

		public void TestCleanupOldCachedFilesDeletesCurrentReleaseRingFiles()
		{
			using (var temp = new TempDirectory())
			{
				File.WriteAllText(Path.Combine(temp, "DPR.bak"), string.Empty);
				File.WriteAllText(Path.Combine(temp, "DPR_Audit.bak"), string.Empty);
				File.WriteAllText(Path.Combine(temp, "StorageDocs.DPR"), string.Empty);

				foreach (var file in Directory.GetFiles(temp))
				{
					File.SetCreationTimeUtc(file, DateTime.UtcNow.Subtract(TimeSpan.FromDays(30)));
					File.SetLastAccessTimeUtc(file, DateTime.UtcNow.Subtract(TimeSpan.FromHours(9)));
				}

				NetworkFileCacher.CleanupOldCachedFiles(temp, "DPR");

				AssertArrayEqualsByElements(Array.Empty<string>(), Directory.GetFiles(temp));
			}
		}

		public void TestCleanupOldCachedFilesPreservesOtherReleaseRingFiles()
		{
			using (var temp = new TempDirectory())
			{
				File.WriteAllText(Path.Combine(temp, "STD.bak"), string.Empty);
				File.WriteAllText(Path.Combine(temp, "STD_Audit.bak"), string.Empty);
				File.WriteAllText(Path.Combine(temp, "StorageDocs.STD"), string.Empty);

				foreach (var file in Directory.GetFiles(temp))
				{
					File.SetCreationTimeUtc(file, DateTime.UtcNow.Subtract(TimeSpan.FromDays(30)));
					File.SetLastAccessTimeUtc(file, DateTime.UtcNow.Subtract(TimeSpan.FromHours(9)));
				}

				NetworkFileCacher.CleanupOldCachedFiles(temp, "DPR");

				AssertArrayEqualsByElements(new[] { Path.Combine(temp, "STD.bak"), Path.Combine(temp, "STD_Audit.bak"), Path.Combine(temp, "StorageDocs.STD") }, Directory.GetFiles(temp));
			}
		}

		public void TestCleanupOldCachedFilesPreservesRefDbFiles()
		{
			using (var temp = new TempDirectory())
			{
				File.WriteAllText(Path.Combine(temp, "RefDb_Cmr_AU"), string.Empty);
				File.WriteAllText(Path.Combine(temp, "RefDb_Ent_US"), string.Empty);

				foreach (var file in Directory.GetFiles(temp))
				{
					File.SetCreationTimeUtc(file, DateTime.UtcNow.Subtract(TimeSpan.FromDays(30)));
					File.SetLastAccessTimeUtc(file, DateTime.UtcNow.Subtract(TimeSpan.FromHours(9)));
				}

				NetworkFileCacher.CleanupOldCachedFiles(temp, "DPR");

				AssertArrayEqualsByElements(new[] { Path.Combine(temp, "RefDb_Cmr_AU"), Path.Combine(temp, "RefDb_Ent_US") }, Directory.GetFiles(temp));
			}
		}

		protected override void SetUp()
		{
			networkFileName = TempForTest.GetTempFileName();
			localFileName = TempForTest.GetTempFileName();
		}

		protected override void TearDown()
		{
			File.Delete(networkFileName);
			File.Delete(localFileName);
		}

		void CreateFile(string fileName)
		{
			File.Delete(fileName);
			using (File.Create(fileName))
			{
			}
		}

		string networkFileName;
		string localFileName;
	}
}
