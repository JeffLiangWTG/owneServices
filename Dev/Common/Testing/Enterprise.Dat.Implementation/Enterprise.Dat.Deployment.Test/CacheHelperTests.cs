using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dat.Integration;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Dat.Implementation.Testing
{
	sealed class CacheHelperTests : TestCase
	{
		public void TestCacheHelperShouldForceServerNameAndSubDirectoryParameters()
		{
			_ = AssertExceptionThrown<ArgumentException>(() => new CacheHelper(string.Empty, "TESTDIR"));
			_ = AssertExceptionThrown<ArgumentNullException>(() => new CacheHelper(null, "TESTDIR"));
			_ = AssertExceptionThrown<ArgumentException>(() => new CacheHelper("string.Empty", string.Empty));
			_ = AssertExceptionThrown<ArgumentNullException>(() => new CacheHelper("null", null));
		}

		public void TestShouldCleanupServerName()
		{
			var dirtyServerName = @"\\TestServerName\";
			var cleanServerName = "TestServerName";
			var cacheHelper = new CacheHelper(dirtyServerName, "TESTDIR");

			AssertEquals("Should remove preceding and trailing backslashes chars", cleanServerName, cacheHelper.CacheServerName);

			dirtyServerName = @"TestServerName\Instance";
			cacheHelper = new CacheHelper(dirtyServerName, "TESTDIR");

			AssertEquals("Should remove instance or directory names", cleanServerName, cacheHelper.CacheServerName);
		}

		public void TestCacheDirectoryShouldStartWithServerName()
		{
			var cacheHelper = new CacheHelper(serverName, "TESTDIR");

			AssertStartsWith("Should start with server name in network path notation", $@"\\{System.Environment.MachineName}", cacheHelper.CacheDirectory);
		}

		public void TestGenerateLocalCacheFilePathShouldMatchCleanupNamingSchema()
		{
			const string filenamePrefix = "CW";
			var sourceBackupPath = @"\\SomeNetworkMachine\some\local\path\OdysseyNoDescription.bak";

			var localCachePath = cacheHelper.GetCacheFilePath(sourceBackupPath);

			Assert(Path.GetFileNameWithoutExtension(localCachePath).StartsWith(filenamePrefix));
		}

		public void TestGenerateLocalCacheFilePathShouldRemoveSqlInstanceName()
		{
			var sqlInstanceName = $@"{System.Environment.MachineName}\SQLInstance";
			const string sourceBackupPath = @"\\SomeNetworkMachine\some\local\path\OdysseyNoDescription.bak";
			var expectedPath =
				$@"\\{System.Environment.MachineName}\{cacheDirectory}\CWOdysseyNoDescription.bak";

			cacheHelper = new CacheHelperForTest(sqlInstanceName, cacheDirectory);

			var localCachePath = cacheHelper.GetCacheFilePath(sourceBackupPath);

			AssertEquals(expectedPath, localCachePath);
		}

		public void TestCacheFileLocally()
		{
			Assert("PreCondition: network file should exist", File.Exists(networkFileName));
			Assert("PreCondition: locally cached file should not exist", !File.Exists(cacheFileName));

			var resultPath = cacheHelper.GetFile(networkFileName);
			Assert("File should be cached locally", File.Exists(cacheFileName));
			AssertEquals("Returned path should match expected path", cacheFileName, resultPath);
			AssertFilesAreSame(networkFileName, resultPath);

			var modifiedTime = DateTime.Now.AddSeconds(10);
			File.SetLastWriteTime(networkFileName, modifiedTime);
			_ = cacheHelper.GetFile(networkFileName);
			AssertEquals("Updated version of the file should be copied", File.GetLastWriteTime(cacheFileName), modifiedTime);

			var localModifiedTime = modifiedTime.AddSeconds(10);
			File.SetLastWriteTime(cacheFileName, localModifiedTime);
			_ = cacheHelper.GetFile(networkFileName);
			AssertEquals("Network file should not be copied as the local file is newer.", File.GetLastWriteTime(cacheFileName), localModifiedTime);
		}

		public void TestOverwriteReadonlyFile()
		{
			CreateFile(cacheFileName);
			Assert("PreCondition: network file should exist", File.Exists(networkFileName));
			Assert("PreCondition: local file should exist", File.Exists(cacheFileName));

			var networkModifiedTime = DateTime.Now;
			var localModifiedTime = networkModifiedTime.AddSeconds(-10);

			File.SetLastWriteTime(cacheFileName, localModifiedTime);
			File.SetAttributes(cacheFileName, FileAttributes.ReadOnly);
			var resultPath = cacheHelper.GetFile(networkFileName);

			AssertEquals("Returned path should match expected path", cacheFileName, resultPath);
			var span = File.GetLastWriteTime(cacheFileName) - networkModifiedTime;
			Assert("File cacher should be able to update local files when they are marked ReadOnly.", span.TotalMilliseconds < 15);
		}

		public void TestFileIsNotUpdatedWhenTimeStampsMatch()
		{
			CreateFile(cacheFileName);
			Assert("PreCondition: network file should exist", File.Exists(networkFileName));
			Assert("PreCondition: local file should exist", File.Exists(cacheFileName));

			using (StreamWriter writer = new StreamWriter(networkFileName))
			{
				writer.Write("Network");
				writer.Flush();
			}

			using (StreamWriter writer = new StreamWriter(cacheFileName))
			{
				writer.Write("Local");
				writer.Flush();
			}

			var networkModifiedTime = DateTime.Now.AddSeconds(-10);
			File.SetLastWriteTime(networkFileName, networkModifiedTime);
			File.SetLastWriteTime(cacheFileName, networkModifiedTime);

			var resultPath = cacheHelper.GetFile(networkFileName);
			AssertEquals("Returned path should match expected path", cacheFileName, resultPath);

			string localFileContents;
			using (StreamReader reader = new StreamReader(cacheFileName))
			{
				localFileContents = reader.ReadToEnd();
			}

			AssertEquals("File should not be updated, as timestamps match", "Local", localFileContents);
		}

		public void TestCleanupOldCachedFilesDeletesOldFiles()
		{
			File.WriteAllText(Path.Combine(cacheDirectory, "CW201201.bak"), string.Empty);
			File.WriteAllText(Path.Combine(cacheDirectory, "CW201201_Audit.bak"), string.Empty);

			foreach (var file in Directory.GetFiles(cacheDirectory))
			{
				File.SetCreationTimeUtc(file, DateTime.UtcNow.Subtract(TimeSpan.FromDays(30)));
				File.SetLastAccessTimeUtc(file, DateTime.UtcNow.Subtract(TimeSpan.FromDays(9)));
			}

			cacheHelper.CleanupOldCacheFiles();

			AssertArrayEqualsByElements(Array.Empty<string>(), Directory.GetFiles(cacheDirectory));
		}

		public void TestCleanupOldCachedFilesPreservesRecentlyUsedFiles()
		{
			File.WriteAllText(Path.Combine(cacheDirectory, "CW201201.bak"), string.Empty);
			File.WriteAllText(Path.Combine(cacheDirectory, "CW201201_Audit.bak"), string.Empty);

			foreach (var file in Directory.GetFiles(cacheDirectory))
			{
				File.SetCreationTimeUtc(file, DateTime.UtcNow.Subtract(TimeSpan.FromDays(30)));
				File.SetLastAccessTimeUtc(file, DateTime.UtcNow.Subtract(TimeSpan.FromHours(9)));
			}

			cacheHelper.CleanupOldCacheFiles();

			AssertArrayEqualsByElements(new[]
				{
					Path.Combine(cacheDirectory, "CW201201.bak"),
					Path.Combine(cacheDirectory,"CW201201_Audit.bak"),
				},
				Directory.GetFiles(cacheDirectory));
		}

		public void TestCleanupOldCachedFilesShouldNotThrowExceptions()
		{
			Directory.Delete(cacheDirectory, recursive: true);

			Assert("PreCondition: cache directory should not exists", !Directory.Exists(cacheDirectory));

			AssertNoExceptionThrown(() => cacheHelper.CleanupOldCacheFiles());
		}

		public void TestTryDeleteCacheFileShouldDeleteTheFile()
		{
			CreateFile(cacheFileName);

			Assert("File should exists before the test", File.Exists(cacheFileName));

			cacheHelper.TryDeleteFile(cacheFileName);

			Assert("File should not exists", !File.Exists(cacheFileName));
		}

		public void TestTryDeleteCacheFileShouldFailSilentlyWhenFileNotExists()
		{
			Assert("File should not exists before the test", !File.Exists(cacheFileName));
			AssertNoExceptionThrown(() => cacheHelper.TryDeleteFile(cacheFileName));
			Assert("File should exists", !File.Exists(cacheFileName));
		}

		public void TestTryDeleteCacheFileShouldFailSilentlyWhenCannotDeleteFile()
		{
			using (var file = File.OpenWrite(cacheFileName))
			{
				Assert("File should exists before the test", File.Exists(cacheFileName));
				AssertNoExceptionThrown(() => cacheHelper.TryDeleteFile(cacheFileName));
				Assert("File should exists after the test", File.Exists(cacheFileName));
			}
		}

		public void TestGetFileShouldVerifyEvenForExistingCache()
		{
			Assert("PreCondition: locally cached file should not exist", !File.Exists(cacheFileName));

			var initialResult = cacheHelper.GetFile(networkFileName);
			AssertNotNullOrEmpty("PreCondition: GetFile should return local path", initialResult);
			Assert("PreCondition: locally cached file should exist", File.Exists(cacheFileName));
			cacheHelperMock.Verify(c => c.Verify, Times.Once(), "PreCondition: Verify callback should be called once");

			cacheHelper.Verify = path => true;

			var finalResult = cacheHelper.GetFile(networkFileName);

			AssertNotNullOrEmpty("GetFile should return local path", finalResult);
			Assert("Locally cached file should exist", File.Exists(cacheFileName));
			cacheHelperMock.Verify(c => c.Verify, Times.Exactly(3), "Verify callback should be called 3 times");
		}

		public void TestConcurrentCacheRequestWaitForTheFirstOne()
		{
			Assert("PreCondition: network file should exist", File.Exists(networkFileName));
			Assert("PreCondition: locally cached file should not exist", !File.Exists(cacheFileName));

			var cacheHelperMock1 = new Mock<CacheHelperForTest>(serverName, cacheDirectory.Replace(":", "$"), null) { CallBase = true };
			var cacheHelperMock2 = new Mock<CacheHelperForTest>(serverName, cacheDirectory.Replace(":", "$"), null) { CallBase = true };

			var cacheHelper1 = cacheHelperMock1.Object;
			var cacheHelper2 = cacheHelperMock2.Object;

			var machine1 = Task.Run(() => cacheHelper1.GetFile(networkFileName));

			Thread.Sleep(TimeSpan.FromMilliseconds(50));

			var machine2 = Task.Run(() => cacheHelper2.GetFile(networkFileName));

			Task.WaitAll(machine1, machine2);

			Assert("File should be cached locally", File.Exists(cacheFileName));
			AssertLessThan("Second machine copy should start after the first machine is finished", cacheHelper1.CopyFileEndTime, cacheHelper2.CopyFileStartTime);
			AssertNotEquals("The first machine should call copy file", default(DateTime), cacheHelper1.CopyFileStartTime);
			AssertNotEquals("The second machine should not copy file", default(DateTime), cacheHelper2.CopyFileStartTime);
		}

		public void TestConcurrentCacheRequestShouldFailOnceTimeoutIsReached()
		{
			Assert("PreCondition: network file should exist", File.Exists(networkFileName));
			Assert("PreCondition: locally cached file should not exist", !File.Exists(cacheFileName));

			var cacheHelperMock1 = new Mock<CacheHelperForTest>(serverName, cacheDirectory.Replace(":", "$"), null) { CallBase = true };
			var cacheHelperMock2 = new Mock<CacheHelperForTest>(serverName, cacheDirectory.Replace(":", "$"), null) { CallBase = true };

			var cacheHelper1 = cacheHelperMock1.Object;
			var cacheHelper2 = cacheHelperMock2.Object;

			var getFileTimeout = TimeSpan.FromSeconds(1);

			var machine1 = Task.Run(() => cacheHelper1.GetFile(networkFileName));

			Thread.Sleep(TimeSpan.FromMilliseconds(50));

			var machine2 = Task.Run(() => cacheHelper2.GetFile(networkFileName, getFileTimeout));

			Task.WaitAll(machine1, machine2);

			CombineAssertions(() =>
			{
				Assert("File should be cached locally", File.Exists(cacheFileName));
				AssertNotEquals("The first machine should call copy file", default(DateTime), cacheHelper1.CopyFileStartTime);
				AssertEquals("The second machine should not call copy file", default(DateTime), cacheHelper2.CopyFileStartTime);
				AssertEquals("The first machine should successfully cache the file", cacheFileName, machine1.Result);
				AssertNull("The second machine should fail to cache the file", machine2.Result);
			});
		}

		public void TestShouldDisposeResourcesGracefullyWhenSucceed()
		{
			var cachedFilePath = cacheHelper.GetFile(networkFileName);

			AssertEquals($"{nameof(CacheHelper.GetFile)} should return cached file path", cacheFileName, cachedFilePath);

			var lockFile = cacheFileName + ".lock";

			Assert("Distributed lock should be disposed.", !File.Exists(lockFile));
		}

		public void TestShouldDisposeResourcesGracefullyWhenFailed()
		{
			_ = cacheHelperMock.Setup(c => c.Verify).Returns(path => false);
			var cachedFilePath = cacheHelperMock.Object.GetFile(networkFileName);

			AssertEquals($"{nameof(CacheHelper.GetFile)} should return null when failed", null, cachedFilePath);

			var lockFile = cacheFileName + ".lock";

			Assert("Distributed lock should be disposed.", !File.Exists(lockFile));
		}

		public void TestVerifyShouldBeCalledWhenSupplied()
		{
			cacheHelper.Verify = path => true;

			var resultPath = cacheHelper.GetFile(networkFileName);

			AssertNotNullOrEmpty(resultPath);
			cacheHelperMock.Verify(c => c.Verify, Times.Exactly(2), "Verify callback should be called twice");
		}

		public void TestVerifyIsOptional()
		{
			var resultPath = cacheHelper.GetFile(networkFileName);

			AssertNotNullOrEmpty(resultPath);
			cacheHelperMock.Verify(c => c.Verify, Times.Once(), "Verify callback should be called only once");
		}

		public void TestGetFileShouldFailIfWhenVerifyFails()
		{
			_ = cacheHelperMock.Setup(c => c.Verify).Returns(path => false);

			var returnPath = cacheHelperMock.Object.GetFile(networkFileName);

			AssertNull(returnPath);
		}

		public void TestGetFileShouldDeleteCacheFileWhenVerifyFailed()
		{
			_ = cacheHelperMock.Setup(c => c.Verify).Returns(path => true);

			var cachedFile = cacheHelperMock.Object.GetFile(networkFileName);
			AssertEquals(cacheFileName, cachedFile);
			AssertNotNull(cachedFile);
			Assert(File.Exists(cachedFile));

			_ = cacheHelperMock.Setup(c => c.Verify).Returns(path => false);
			cachedFile = cacheHelperMock.Object.GetFile(networkFileName);
			AssertNull(cachedFile);
			Assert(!File.Exists(cachedFile));
		}

		public void TestDefaultTimeoutValue()
		{
			var cacheHelperMock1 = new Mock<CacheHelperForTest>(serverName, cacheDirectory.Replace(":", "$"), null) { CallBase = true };

			_ = cacheHelperMock1.Object.GetFile(networkFileName);

			cacheHelperMock1.Protected().VerifyGet<TimeSpan>("DefaultTimeout", Times.Once());
			AssertEquals(TimeSpan.FromMinutes(1), cacheHelperMock1.Object.DefaultTimeoutExposed);
		}

		public void TestShouldLogWhenLoggerExists()
		{
			var taskLoggerMock = new Mock<ITaskLogger>();
			var cacheHelperMock1 = new Mock<CacheHelperForTest>(serverName, cacheDirectory.Replace(":", "$"), taskLoggerMock.Object) { CallBase = true };

			var getFileResult = cacheHelperMock1.Object.GetFile(networkFileName);
			taskLoggerMock.Verify(logger => logger.RecordTask("Copying file to cache location"), Times.Once());
			taskLoggerMock.Verify(logger => logger.RecordTask("Verifying cached file"), Times.Never);
			taskLoggerMock.Verify(logger => logger.RecordTask("Cleaning old files"), Times.Never);
			AssertEquals(cacheFileName, getFileResult);

			cacheHelperMock1.Object.CleanupOldCacheFiles();
			taskLoggerMock.Verify(logger => logger.RecordTask("Cleaning old files"), Times.Once());

			cacheHelperMock1.Setup(c => c.Verify).Returns(path => true);
			_ = cacheHelperMock1.Object.GetFile(networkFileName);
			taskLoggerMock.Verify(logger => logger.RecordTask("Verifying cached file"), Times.Once());

			using (var writer = new StreamWriter(cacheFileName))
			{
				cacheHelperMock1.Object.TryDeleteFile(cacheFileName);
				taskLoggerMock.Verify(logger => logger.RecordInfo(It.Is<string>(m => m.StartsWith("Delete file failed:"))), Times.Once());
			}

			Directory.Delete(cacheDirectory, recursive: true);
			cacheHelperMock1.Object.CopyFileExposed(networkFileName, cacheFileName);
			taskLoggerMock.Verify(logger => logger.RecordInfo(It.Is<string>(m => m.StartsWith("CopyFile failed:"))), Times.Once());
			Directory.CreateDirectory(cacheDirectory);

			cacheHelperMock1.Protected().Setup("CopyFile", exactParameterMatch: false, ItExpr.IsAny<string>(), ItExpr.IsAny<string>()).Throws(new Exception());
			_ = cacheHelperMock1.Object.GetFile(networkFileName);
			taskLoggerMock.Verify(logger => logger.RecordInfo("Cannot get cache file: One or more errors occurred."), Times.Once());
		}

		void AssertFilesAreSame(string filePath1, string filePath2)
		{
			var fileHash1 = GetFileHash(filePath1);
			var fileHash2 = GetFileHash(filePath2);

			AssertEquals(fileHash1, fileHash2);
		}

		string GetFileHash(string filename)
		{
			using var hash = SHA1.Create();
			var clearBytes = File.ReadAllBytes(filename);
			var hashedBytes = hash.ComputeHash(clearBytes);
			return ConvertBytesToHex(hashedBytes);
		}

		string ConvertBytesToHex(byte[] bytes)
		{
			var sb = new StringBuilder();

			foreach (var t in bytes)
			{
				sb.Append(t.ToString("x"));
			}
			return sb.ToString();
		}

		protected override void SetUp()
		{
			serverName = System.Environment.MachineName;

			networkDirectory = Path.Combine(TempForTest.TempPath, "NetworkPath");
			if (!Directory.Exists(networkDirectory))
			{
				_ = Directory.CreateDirectory(networkDirectory);
			}

			var tempFile = TempForTest.GetTempFileName();
			var tempFileName = Path.GetFileName(tempFile);
			networkFileName = Path.Combine(networkDirectory, Path.GetFileName(tempFileName));
			File.Move(tempFile, networkFileName);

			cacheDirectory = Path.Combine(TempForTest.TempPath, "CachePath");
			if (!Directory.Exists(cacheDirectory))
			{
				_ = Directory.CreateDirectory(cacheDirectory);
			}

			cacheFileName = $@"\\{serverName}\{cacheDirectory.Replace(":", "$")}\CW{tempFileName}";
			cacheHelperMock = new Mock<CacheHelper>(serverName, cacheDirectory.Replace(":", "$"), null) { CallBase = true };
			cacheHelper = cacheHelperMock.Object;
		}

		protected override void TearDown()
		{
			if (Directory.Exists(networkDirectory))
			{
				Directory.Delete(networkDirectory, recursive: true);
			}

			if (Directory.Exists(cacheDirectory))
			{
				Directory.Delete(cacheDirectory, recursive: true);
			}
		}

		void CreateFile(string fileName)
		{
			File.Delete(fileName);
			using (File.Create(fileName))
			{
			}
		}

		string serverName;
		string networkFileName;
		string networkDirectory;
		string cacheFileName;
		string cacheDirectory;
		CacheHelper cacheHelper;
		Mock<CacheHelper> cacheHelperMock;
	}

	public class CacheHelperForTest : CacheHelper
	{
		public CacheHelperForTest(string cacheServerName, string cacheSubDirectory, ITaskLogger taskLogger = null) : base(cacheServerName, cacheSubDirectory, taskLogger)
		{
		}

		public DateTime GetFileStartTime { get; private set; }

		public DateTime GetFileEndTime { get; private set; }

		public DateTime CopyFileStartTime { get; private set; }

		public DateTime CopyFileEndTime { get; private set; }

		public TimeSpan DefaultTimeoutExposed => DefaultTimeout;

		public override string GetFile(string networkFilePath, TimeSpan timeout = default)
		{
			GetFileStartTime = DateTime.UtcNow;
			var baseResult = base.GetFile(networkFilePath, timeout);
			GetFileEndTime = DateTime.UtcNow;
			return baseResult;
		}

		protected override void CopyFile(string networkFilePath, string cacheFilePath)
		{
			CopyFileStartTime = DateTime.UtcNow;
			base.CopyFile(networkFilePath, cacheFilePath);
			Thread.Sleep(TimeSpan.FromSeconds(2));
			CopyFileEndTime = DateTime.UtcNow;
		}

		public void CopyFileExposed(string networkFilePath, string cacheFilePath) =>
			CopyFile(networkFilePath, cacheFilePath);
	}
}
