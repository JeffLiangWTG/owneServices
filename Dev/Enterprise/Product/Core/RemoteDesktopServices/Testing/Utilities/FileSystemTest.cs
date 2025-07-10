using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using CargoWise.Common;
using CargoWise.IO;
using Enterprise.RemoteDesktopServices.Server.TrackingInfo;
using NUnit.Framework;

namespace Enterprise.RemoteDesktopServices.Testing
{
	class FileSystemTest : TestCase
	{
		public void TestFileExists_FileDoesExist()
		{
			// Arrange
			using (var tempDir = new TempDirectory())
			{
				var filePath = Temp.GetTempFileName(tempDir);

				// Act
				var result = FileSystem.FileExists(filePath);

				// Assert
				AssertEquals(ExistenceState.ExistingNormally, result);
				AssertFileExistsLogs(filePath, $"File.Exists = true | {ExistenceState.ExistingNormally}");
			}
		}

		public void TestFileExists_FileDoesExistButNoPermission()
		{
			// Arrange
			using (var tempDir = new TempDirectory())
			{
				var parentDir = Path.Combine(tempDir, Path.GetRandomFileName());
				Directory.CreateDirectory(parentDir);

				// Clear file's permissions
				var filePath = Temp.GetTempFileName(parentDir);
				var fileInfo = new FileInfo(filePath);
				var fileAcls = fileInfo.GetAccessControl();
				fileAcls.SetAccessRuleProtection(true, false);
				fileInfo.SetAccessControl(fileAcls);

				// Clear all parent directory's permissions
				var parentDirInfo = new DirectoryInfo(parentDir);
				var parentDirAcls = parentDirInfo.GetAccessControl();
				parentDirAcls.SetAccessRuleProtection(true, false);
				parentDirInfo.SetAccessControl(parentDirAcls);

				using (new DisposableAction(() =>
				{
					// Restore all the permissions
					var newParentDirAcls = new DirectorySecurity();
					newParentDirAcls.SetAccessRuleProtection(false, true);
					parentDirInfo.SetAccessControl(newParentDirAcls);

					var newFileAcls = new FileSecurity();
					newFileAcls.SetAccessRuleProtection(false, true);
					fileInfo.SetAccessControl(newFileAcls);
				}))
				{
					// Act
					var result = FileSystem.FileExists(filePath);

					// Assert
					AssertEquals(false, File.Exists(filePath));
					AssertNotNull("Permission", parentDirInfo.GetAccessControl());
					AssertEquals(ExistenceState.ExistingButNoPermission, result);
					AssertFileExistsLogs(filePath, $"File.GetAccessControl no exception | {ExistenceState.ExistingButNoPermission}");
				}
			}
		}

		public void TestFileExists_DirectoryDoesExistButNoPermission()
		{
			// Arrange
			using (var tempDir = new TempDirectory())
			{
				var dirPath = Path.Combine(tempDir, Path.GetRandomFileName());
				Directory.CreateDirectory(dirPath);

				var dirInfo = new DirectoryInfo(dirPath);
				var dirAcls = dirInfo.GetAccessControl();
				dirAcls.SetAccessRuleProtection(true, false);
				dirInfo.SetAccessControl(dirAcls);

				var tempDirInfo = new DirectoryInfo(tempDir);
				var tempDirAcls = tempDirInfo.GetAccessControl();
				tempDirAcls.SetAccessRuleProtection(true, false);
				tempDirInfo.SetAccessControl(tempDirAcls);

				using (new DisposableAction(() =>
				{
					// Restore all the permissions
					var newTempDirAcls = new DirectorySecurity();
					newTempDirAcls.SetAccessRuleProtection(false, true);
					tempDirInfo.SetAccessControl(newTempDirAcls);

					var newDirAcls = new DirectorySecurity();
					newDirAcls.SetAccessRuleProtection(false, true);
					dirInfo.SetAccessControl(newDirAcls);
				}))
				{
					// Act
					var result = FileSystem.FileExists(dirPath);

					// Assert
					AssertEquals(false, Directory.Exists(dirPath));
					AssertNotNull("Permission", dirInfo.GetAccessControl());
					AssertEquals(result, ExistenceState.ExistingButNoPermission);
					AssertFileExistsLogs(dirPath, $"File.GetAccessControl no exception | {ExistenceState.ExistingButNoPermission}");
				}
			}
		}

		public void TestFileExists_FileDoesNotExist()
		{
			// Arrange
			using (var tempDir = new TempDirectory())
			{
				var filePath = Path.Combine(tempDir, Path.GetRandomFileName());

				// Act
				var result = FileSystem.FileExists(filePath);

				// Assert
				AssertEquals(ExistenceState.NotExisting, result);
				AssertLogs(filePath, "FileExists", $"File.GetAccessControl | {ExistenceState.NotExisting}: System.IO.FileNotFoundException:", true);
			}
		}

		public void TestFileExists_FileDoesNotExistButSameNameDirectoryExisting()
		{
			// Arrange
			using (var tempDir = new TempDirectory())
			{
				var dirPath = Path.Combine(tempDir, Path.GetRandomFileName());
				Directory.CreateDirectory(dirPath);

				// Act
				var result = FileSystem.FileExists(dirPath);

				// Assert
				AssertEquals(true, Directory.Exists(dirPath));
				AssertEquals(ExistenceState.ExistingButNotFile, result);
				AssertFileExistsLogs(dirPath, $"Directory.Exists = true | {ExistenceState.ExistingButNotFile}");
			}
		}

		public void TestFileExists_InvalidCharacterInPath()
		{
			// Arrange
			var path = "bad::<path>?";

			// Act
			var result = FileSystem.FileExists(path);

			// Assert
			AssertEquals(ExistenceState.InvalidCharacterInPath, result);
			AssertLogs(path, "FileExists", $"File.GetAccessControl | {ExistenceState.InvalidCharacterInPath} | 'bad::<path>?': System.ArgumentException:", true);
		}

		public void TestDirectoryExists_DirectoryDoesExist()
		{
			// Arrange
			using (var tempDir = new TempDirectory())
			{
				// Act
				var result = FileSystem.DirectoryExists(tempDir);

				// Assert
				AssertEquals(ExistenceState.ExistingNormally, result);
				AssertDirectoryExistsLogs(tempDir, $"Directory.Exists = true | {ExistenceState.ExistingNormally}");
			}
		}

		public void TestDirectoryExists_DirectoryDoesExistButNoPermission()
		{
			// Arrange
			using (var tempDir = new TempDirectory())
			{
				var dirPath = Path.Combine(tempDir, Path.GetRandomFileName());
				Directory.CreateDirectory(dirPath);

				var dirInfo = new DirectoryInfo(dirPath);
				var dirAcls = dirInfo.GetAccessControl();
				dirAcls.SetAccessRuleProtection(true, false);
				dirInfo.SetAccessControl(dirAcls);

				var tempDirInfo = new DirectoryInfo(tempDir);
				var tempDirAcls = tempDirInfo.GetAccessControl();
				tempDirAcls.SetAccessRuleProtection(true, false);
				tempDirInfo.SetAccessControl(tempDirAcls);

				using (new DisposableAction(() =>
				{
					// Restore all the permissions
					var newTempDirAcls = new DirectorySecurity();
					newTempDirAcls.SetAccessRuleProtection(false, true);
					tempDirInfo.SetAccessControl(newTempDirAcls);

					var newDirAcls = new DirectorySecurity();
					newDirAcls.SetAccessRuleProtection(false, true);
					dirInfo.SetAccessControl(newDirAcls);
				}))
				{
					// Act
					var result = FileSystem.DirectoryExists(dirPath);

					// Assert
					AssertEquals(false, Directory.Exists(dirPath));
					AssertNotNull("Permission", dirInfo.GetAccessControl());
					AssertDirectoryExistsLogs(dirPath, $"Directory.GetAccessControl no exception | {ExistenceState.ExistingButNoPermission}");
				}
			}
		}

		public void TestDirectoryExists_DirectoryDoesNotExist()
		{
			// Arrange
			var tempDir = new TempDirectory();
			tempDir.Dispose();

			// Act
			var result = FileSystem.DirectoryExists(tempDir);

			// Assert
			AssertEquals(ExistenceState.NotExisting, result);
			AssertLogs(tempDir, "DirectoryExists", $"Directory.GetAccessControl | {ExistenceState.NotExisting}: System.IO.DirectoryNotFoundException:", true);
		}

		public void TestDirectoryExists_DirectoryDoesNotExistButSameNameFileExisting()
		{
			// Arrange
			var tempDir = new TempDirectory();
			tempDir.Dispose();

			File.Create(tempDir).Dispose();

			// Act
			var result = FileSystem.DirectoryExists(tempDir);

			// Assert
			AssertEquals(true, File.Exists(tempDir));
			AssertEquals(ExistenceState.ExistingButNotDirectory, result);
			AssertDirectoryExistsLogs(tempDir, $"File.Exists = true | {ExistenceState.ExistingButNotDirectory}");

			// Cleanup
			File.Delete(tempDir);
		}

		public void TestDirectoryExists_InvalidCharacterInPath()
		{
			// Arrange
			var path = "bad::<path>?";

			// Act
			var result = FileSystem.DirectoryExists(path);

			// Assert
			AssertEquals(ExistenceState.InvalidCharacterInPath, result);
			AssertLogs(path, "DirectoryExists", $"Directory.GetAccessControl | {ExistenceState.InvalidCharacterInPath} | 'bad::<path>?': System.ArgumentException:", true);
		}

		void AssertFileExistsLogs(string filePath, string middleLog)
		{
			AssertLogs(filePath, "FileExists", middleLog);
		}

		void AssertDirectoryExistsLogs(string filePath, string middleLog)
		{
			AssertLogs(filePath, "DirectoryExists", middleLog);
		}

		void AssertLogs(string filePath, string methodName, string returnString, bool hasException = false)
		{
			AssertEquals("Should record 3 logs", 3, Loggers.Count);
			AssertEquals("Should record begin", $"Begin {methodName}({filePath})", Loggers[0]);
			if (hasException)
			{
				AssertStartsWith("Should record return value and exception", returnString, Loggers[1]);
			}
			else
			{
				AssertEquals("Should record return value", returnString, Loggers[1]);
			}

			AssertEquals("Should record end", $"End {methodName}({filePath})", Loggers[2]);
		}

		List<string> Loggers;

		protected override void SetUp()
		{
			Loggers = new List<string>();
			TrackingInfoLogger.Instance.OnNewLog += AppendLog;
			base.SetUp();
		}

		protected override void TearDown()
		{
			Loggers.Clear();
			TrackingInfoLogger.Instance.OnNewLog -= AppendLog;
			base.TearDown();
		}

		void AppendLog(object sender, string e)
		{
			Loggers.Add(e);
		}
	}
}
