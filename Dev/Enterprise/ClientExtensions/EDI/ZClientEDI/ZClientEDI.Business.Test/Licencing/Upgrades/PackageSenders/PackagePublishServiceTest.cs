using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business.Test
{
	public class PackagePublishServiceTest : TestCaseWithFactory
	{
		public void TestPermissionToDeployFolder()
		{
			var accessHelper = new PackagePublishService();

			bool result = accessHelper.TryToGetControl(testPath);
			AssertEquals(true, result);

			var subDirPath = Path.Combine(testPath, "WebFolderAccessHelperTest");
			var subDirectory = new DirectoryInfo(subDirPath);
			var filePath = Path.Combine(subDirPath, "testfile.txt");

			try
			{
				subDirectory.Create();
				File.Create(filePath).Close();
				Assert(File.Exists(filePath));
			}
			finally
			{
				try
				{
					DeleteDirectory(subDirectory);
				}
				finally
				{
					accessHelper.TryToReleaseControl(testPath);
				}
			}
		}

		public void TestDeleteOldUpgradePackages()
		{
			string basePath = Path.Combine(Env.TempPath, Guid.NewGuid().ToString());

			DirectoryInfo baseDirectory = new DirectoryInfo(basePath);
			baseDirectory.Create();
			try
			{
				VersionNumber gpr = new VersionNumber(1, 4, 3603, 781);
				VersionNumber std = new VersionNumber(1, 4, 3659, 362);
				VersionNumber dpr = new VersionNumber(1, 4, 3702, 26);
				string gprPath = RunPublish(basePath, gpr);
				string dprPath = RunPublish(basePath, dpr);
				string stdPath = RunPublish(basePath, std);

				Assert(File.Exists(stdPath));
				Assert(File.Exists(dprPath));
				Assert(File.Exists(gprPath));

				File.SetCreationTime(gprPath, ZDateTime.Now.AddDays(-15).ToDateTime());
				File.SetCreationTime(dprPath, ZDateTime.Now.AddDays(-13).ToDateTime());
				RunPublish(basePath, dpr);

				Assert(File.Exists(stdPath));
				Assert(File.Exists(dprPath));
				Assert(!File.Exists(gprPath));
			}
			finally
			{
				baseDirectory.Delete(true);
			}
		}

		#region Checksum

		public void TestVerifyCheckSum()
		{
			string basePath = Path.Combine(Env.TempPath, Guid.NewGuid().ToString());

			DirectoryInfo baseDirectory = new DirectoryInfo(basePath);
			baseDirectory.Create();

			try
			{
				VersionNumber ver = new VersionNumber(1, 4, 3603, 781);
				string targetDir = Path.Combine(basePath, "!@#");
				string tempPath = Path.Combine(basePath, ReleaseBuild.GetPackageName(ver));
				string targetPath = Path.Combine(targetDir, ReleaseBuild.GetPackageName(ver));
				File.WriteAllBytes(tempPath, new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });

				Thread.Sleep(200);

				byte[] result;
				using (var md5 = MD5.Create())
				{
					using (var stream = File.OpenRead(tempPath))
					{
						result = md5.ComputeHash(stream);
					}
				}

				var service = new PackagePublishServiceForTest();
				service.FilenameForBadChecksum = targetPath;

				var success = service.PublishPackage(tempPath, targetDir);

				var original = Convert.ToBase64String(result);
				var copied = Convert.ToBase64String(new byte[] { 1, 2, 3 });

				Assert(success);
				AssertEquals(1, service.RetryCounter);
				AssertEquals($@"Checksum mismatch for [{Path.GetFileName(tempPath)}].
Original
Length: {result.Length}
Base64: [{original}]
Target
Length: {3}
Base64: [{copied}]", ErrorReporter.LastMessageReported);

				Assert(File.Exists(targetPath));
			}
			finally
			{
				baseDirectory.Delete(true);
				ErrorReporter.Clear();
			}
		}

		public void TestRetryIfChecksumNetworkError()
		{
			string basePath = Path.Combine(Env.TempPath, Guid.NewGuid().ToString());

			DirectoryInfo baseDirectory = new DirectoryInfo(basePath);
			baseDirectory.Create();

			try
			{
				VersionNumber ver = new VersionNumber(1, 4, 3603, 781);
				string targetDir = Path.Combine(basePath, "!@#");
				string tempPath = Path.Combine(basePath, ReleaseBuild.GetPackageName(ver));
				string targetPath = Path.Combine(targetDir, ReleaseBuild.GetPackageName(ver));
				File.WriteAllBytes(tempPath, new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });

				Thread.Sleep(200);

				byte[] result;
				using (var md5 = MD5.Create())
				{
					using (var stream = File.OpenRead(tempPath))
					{
						result = md5.ComputeHash(stream);
					}
				}

				var service = new PackagePublishServiceForTest();
				service.FilenameForIOExceptionOnChecksum = targetPath;
				service.MaxBadRetry = 2;

				var success = service.PublishPackage(tempPath, targetDir);

				var original = Convert.ToBase64String(result);
				var copied = Convert.ToBase64String(new byte[] { 1, 2, 3 });

				Assert(success);
				AssertEquals(2, service.RetryCounter);
				AssertEquals($@"", ErrorReporter.LastMessageReported);

				Assert(File.Exists(targetPath));
			}
			finally
			{
				baseDirectory.Delete(true);
				ErrorReporter.Clear();
			}
		}

		public void TestRetryIfChecksumNetworkError_Fail()
		{
			string basePath = Path.Combine(Env.TempPath, Guid.NewGuid().ToString());

			DirectoryInfo baseDirectory = new DirectoryInfo(basePath);
			baseDirectory.Create();

			try
			{
				VersionNumber ver = new VersionNumber(1, 4, 3603, 781);
				string targetDir = Path.Combine(basePath, "!@#");
				string tempPath = Path.Combine(basePath, ReleaseBuild.GetPackageName(ver));
				string targetPath = Path.Combine(targetDir, ReleaseBuild.GetPackageName(ver));
				File.WriteAllBytes(tempPath, new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });

				Thread.Sleep(200);

				byte[] result;
				using (var md5 = MD5.Create())
				{
					using (var stream = File.OpenRead(tempPath))
					{
						result = md5.ComputeHash(stream);
					}
				}

				var service = new PackagePublishServiceForTest();
				service.FilenameForIOExceptionOnChecksum = targetPath;

				var success = service.PublishPackage(tempPath, targetDir);

				var original = Convert.ToBase64String(result);

				Assert(success);
				AssertEquals(3, service.RetryCounter);
				AssertEquals($@"Checksum mismatch for [{Path.GetFileName(tempPath)}].
Original
Length: {result.Length}
Base64: [{original}]
Target
Length: {0}
Base64: []", ErrorReporter.LastMessageReported);

				Assert(File.Exists(targetPath));
			}
			finally
			{
				baseDirectory.Delete(true);
				ErrorReporter.Clear();
			}
		}

		public void TestNotVerifyCheckSumWithFileCopyFailure()
		{
			string basePath = Path.Combine(Env.TempPath, Guid.NewGuid().ToString());

			var baseDirectory = new DirectoryInfo(basePath);
			baseDirectory.Create();

			try
			{
				var ver = new VersionNumber(1, 4, 3603, 781);
				string targetDir = Path.Combine(basePath, "!@#");
				string tempPath = Path.Combine(basePath, ReleaseBuild.GetPackageName(ver));
				string targetPath = Path.Combine(targetDir, ReleaseBuild.GetPackageName(ver));
				File.WriteAllBytes(tempPath, new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });

				var service = new PackagePublishServiceForTest() { ThrowsIOExceptionInCopyFile = true };
				service.FilenameForBadChecksum = targetPath;

				var success = service.PublishPackage(tempPath, targetDir);

				AssertEquals(false, success);
				AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
			}
			finally
			{
				baseDirectory.Delete(true);
				ErrorReporter.Clear();
			}
		}

		#endregion

		#region Corrupted File

		public void TestIsPackageAlreadyPublished_WithCorruptedPackageFile()
		{
			var basePath = Path.Combine(Env.TempPath, Guid.NewGuid().ToString());

			var baseDirectory = new DirectoryInfo(basePath);
			baseDirectory.Create();

			var genericBuildPath = Path.Combine(baseDirectory.FullName, "Generic");
			var genericBuildDirectory = new DirectoryInfo(genericBuildPath);
			genericBuildDirectory.Create();

			string targetDir = Path.Combine(genericBuildPath, "!@#");
			EDIDataRegistry.Instance.WebServerGenericPathRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, targetDir);

			try
			{
				var buildVersion = new VersionNumber(22, 10, 25, 123);
				var packageName = ReleaseBuild.GetPackageName(buildVersion);

				var service = new PackagePublishService();
				AssertEquals("File not exist", TriState.False, service.IsPackageAlreadyPublished(packageName, string.Empty));

				RunPublish(genericBuildPath, buildVersion, throwIOException: true);
				AssertEquals("File exists but marked as corrupted", TriState.False, service.IsPackageAlreadyPublished(packageName, string.Empty));

				RunPublish(genericBuildPath, buildVersion, throwIOException: false);
				AssertEquals("Good file exists", TriState.True, service.IsPackageAlreadyPublished(packageName, string.Empty));
			}
			finally
			{
				baseDirectory.Delete(true);
			}
		}

		public void TestPublish_HandleCorruptedPackageFile()
		{
			var basePath = Path.Combine(Env.TempPath, Guid.NewGuid().ToString());
			var baseDirectory = new DirectoryInfo(basePath);
			baseDirectory.Create();

			var genericBuildPath = Path.Combine(baseDirectory.FullName, "Generic");
			var genericBuildDirectory = new DirectoryInfo(genericBuildPath);
			genericBuildDirectory.Create();

			var enterpriseAaaBuildPath = Path.Combine(baseDirectory.FullName, "ClientSpecific", "AAA");
			var enterpriseAaaBuildDirectory = new DirectoryInfo(enterpriseAaaBuildPath);
			enterpriseAaaBuildDirectory.Create();

			var enterpriseBbbBuildPath = Path.Combine(baseDirectory.FullName, "ClientSpecific", "BBB");
			var enterpriseBbbBuildDirectory = new DirectoryInfo(enterpriseBbbBuildPath);
			enterpriseBbbBuildDirectory.Create();

			try
			{
				var buildVersion = new VersionNumber(22, 10, 25, 123);
				string genericPublishPath = RunPublish(genericBuildPath, buildVersion, throwIOException: true);
				string enterpriseAaaPublishPath = RunPublish(enterpriseAaaBuildPath, buildVersion);
				string enterpriseBbbPublishPath = RunPublish(enterpriseBbbBuildPath, buildVersion);

				Assert(File.Exists(genericPublishPath));
				Assert(File.Exists(enterpriseAaaPublishPath));
				Assert(File.Exists(enterpriseBbbPublishPath));

				AssertEquals("File is corrupted", 4, File.ReadAllBytes(genericPublishPath).Length);
				AssertEquals("File is good", 10, File.ReadAllBytes(enterpriseAaaPublishPath).Length);
				AssertEquals("File is good", 10, File.ReadAllBytes(enterpriseBbbPublishPath).Length);

				var corruptedFilePathList = EDIDataRegistry.Instance.CorruptedUpgradePackageFilePath.Value;
				AssertEquals(1, corruptedFilePathList.Count);
				AssertEquals("Corrupted file path is added to registry", genericPublishPath, corruptedFilePathList[0].Description);

				genericPublishPath = RunPublish(genericBuildPath, buildVersion);
				Assert(File.Exists(genericPublishPath));
				Assert(File.Exists(enterpriseAaaPublishPath));
				Assert(File.Exists(enterpriseBbbPublishPath));

				AssertEquals("Correputed file is replaced", 10, File.ReadAllBytes(genericPublishPath).Length);
				corruptedFilePathList = EDIDataRegistry.Instance.CorruptedUpgradePackageFilePath.Value;
				AssertEquals("Corrupted file path is removed from registry", 0, corruptedFilePathList.Count);
			}
			finally
			{
				baseDirectory.Delete(true);
			}
		}

		#endregion

		#region Logging

		public void TestLastErrorMessage()
		{
			EDIDataRegistry.Instance.WebServerUserName = "WebServerUserName";
			EDIDataRegistry.Instance.WebServerPassword = "WebServerPassword";
			var service = new PackagePublishService();
			var invalidPath = $@"c:\{ZGuid.NewZGuid()}";

			var result = service.TryToGetControl(invalidPath);
			AssertEquals(false, result);
			AssertEquals($@"The network path {invalidPath} provided as the destination for web deployment is invalid.
Please check the 'Web -> Deployment Details -> Web Root Path' {Enterprise.Core.Constants.ProductName} registry setting.", service.LastErrorMessage);
			EDIDataRegistry.Instance.WebServerUserName = "";
			EDIDataRegistry.Instance.WebServerPassword = "";
			result = service.TryToGetControl("");
			AssertEquals(true, result);
			AssertEquals($@"The network path {invalidPath} provided as the destination for web deployment is invalid.
Please check the 'Web -> Deployment Details -> Web Root Path' {Enterprise.Core.Constants.ProductName} registry setting.", service.LastErrorMessage);
		}

		#endregion

		class PackagePublishServiceForTest : PackagePublishService
		{
			public int MaxBadRetry = 3;
			public int RetryCounter;
			public string FilenameForBadChecksum = "";
			public string FilenameForIOExceptionOnChecksum = "";
			protected override byte[] CalculateChecksum(string filename)
			{
				var badChecksum = new byte[] { 1, 2, 3 };
				if (filename != FilenameForBadChecksum || RetryCounter >= MaxBadRetry)
				{
					return base.CalculateChecksum(filename);
				}

				RetryCounter++;
				return badChecksum;
			}

			protected override byte[] ComputeHash(string filename, MD5 md5)
			{
				if (filename == FilenameForIOExceptionOnChecksum && RetryCounter < MaxBadRetry)
				{
					RetryCounter++;
					throw new IOException();
				}

				return base.ComputeHash(filename, md5);
			}

			public bool ThrowsIOExceptionInCopyFile { get; set; }
			protected override void CopyFile(string packagePath, string targetPath)
			{
				base.CopyFile(packagePath, targetPath);
				if (ThrowsIOExceptionInCopyFile)
				{
					throw new IOException("Invalid path.");
				}
			}
		}

		string RunPublish(string basePath, VersionNumber ver, bool throwIOException = false)
		{
			string targetDir = Path.Combine(basePath, "!@#");
			string tempPath = Path.Combine(basePath, ReleaseBuild.GetPackageName(ver));
			string targetPath = Path.Combine(targetDir, ReleaseBuild.GetPackageName(ver));

			var fileContent = !throwIOException ? new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 } : new byte[] { 0, 0, 0, 0 };
			File.WriteAllBytes(tempPath, fileContent);

			var service = new PackagePublishServiceForTest() { ThrowsIOExceptionInCopyFile = throwIOException };
			service.PublishPackage(tempPath, targetDir);
			if (throwIOException)
			{
				AssertStartsWith("Error message should contain exception details", "System.IO.IOException: Invalid path.", service.LastErrorMessage);
			}

			return targetPath;
		}

		void DeleteDirectory(DirectoryInfo directory)
		{
			try
			{
				directory.Delete(true);
			}
			catch
			{
				TempDirectory.DeleteDirectory(directory);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			tempDirectory = new TempDirectory();
			testPath = @"\\" + System.Environment.MachineName + @"\" + tempDirectory.DirectoryName.Replace(":", "$");
		}

		protected override void TearDown()
		{
			tempDirectory.Dispose();
			base.TearDown();
		}

		TempDirectory tempDirectory;
		string testPath;
	}
}
