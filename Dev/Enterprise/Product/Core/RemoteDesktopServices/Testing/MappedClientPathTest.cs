using System;
using System.IO;
using System.Security.AccessControl;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.IO;
using Enterprise.ZArchitecture.Core.Testing;
using NUnit.Framework;

namespace Enterprise.RemoteDesktopServices.Testing
{
	class MappedClientPathTest : TestCase
	{
		public void TestGetMappedPath_VolumeSeparatorFormatError()
		{
			// Arrange
			var inputs = new[]
			{
				"C",
				@":\bad",
			};

			foreach (var input in inputs)
			{
				var client = new MappedClientPath();

				// Act
				var mappedPath = client.GetMappedPath(input);

				// Assert
				AssertNull(mappedPath);
			}
		}

		public void TestGetMappedPath_RdpOrCitrix()
		{
			// Arrange
			var dataSource = new (bool isCitrix, string expectedMappedPath)[]
			{
				(false, @"\\tsclient\C\parent\a.txt"),
				(true, @"\\Client\C$\parent\a.txt"),
			};

			foreach (var data in dataSource)
			{
				using (DisableParentExistenceCheckTemporarily(null))
				using (ObjectFactory.Substitute<TerminalService>(new ZTerminalServiceForTest(isCitrixICA: data.isCitrix)))
				{
					var client = new MappedClientPath();

					var filePath = @"C:\parent\a.txt";

					// Act
					var mappedPath = client.GetMappedPath(filePath);

					// Assert
					AssertEquals(data.expectedMappedPath, mappedPath);
				}
			}
		}

		public void TestGetMappedPath_DrivePath()
		{
			// Arrange
			var inputs = new[]
			{
				@"C:",
				@"C:\",
				@"C:/",
				@"C:\\",
				@"C:\\\",
				@"C://",
				@"C:/\",
				@"C:\/",
			};

			foreach (var input in inputs)
			{
				using (DisableParentExistenceCheckTemporarily(
					s =>
					{
						AssertEquals("Volume path need to be checked", @"\\tsclient\C", s);
						return ExistenceState.ExistingNormally;
					}))
				{
					var client = new MappedClientPath();

					// Act
					var mappedPath = client.GetMappedPath(input);

					// Assert
					AssertEquals(@"\\tsclient\C", mappedPath);
				}
			}
		}

		public void TestGetMappedPath_SubPathFollowingMultipleSlashesAfterVolumeSeparator()
		{
			// Arrange
			var inputs = new[]
			{
				@"C:\\sub-path",
				@"C:\\\sub-path",
				@"C://sub-path",
				@"C:/\sub-path",
			};

			foreach (var input in inputs)
			{
				using (DisableParentExistenceCheckTemporarily(null))
				{
					var client = new MappedClientPath();

					// Act
					var mappedPath = client.GetMappedPath(input);

					// Assert
					AssertEquals(@"\\tsclient\C\sub-path", mappedPath);
				}
			}
		}

		public void TestGetMappedPath_CheckParentNormally()
		{
			// Arrange
			var dataSource = new (string input, string expectedParent, string expectedResult)[]
			{
				(@"C:\a", @"\\tsclient\C", @"\\tsclient\C\a"),
				(@"C:\\a", @"\\tsclient\C", @"\\tsclient\C\a"),
				(@"C:\\a\b", @"\\tsclient\C\a", @"\\tsclient\C\a\b"),
				(@"C:\\a\b\c.txt", @"\\tsclient\C\a\b", @"\\tsclient\C\a\b\c.txt"),
			};

			foreach (var data in dataSource)
			{
				using (DisableParentExistenceCheckTemporarily(
					s =>
					{
						AssertEquals("Volume path need to be checked", data.expectedParent, s);
						return ExistenceState.ExistingNormally;
					}))
				{
					var client = new MappedClientPath();

					// Act
					var mappedPath = client.GetMappedPath(data.input);

					// Assert
					AssertEquals(data.expectedResult, mappedPath);
				}
			}
		}

		public void TestGetMappedPath_ParentDoesExistNormally()
		{
			// Arrange
			using (LeaveUnmappedTemporarily())
			using (var tempDir = new TempDirectory())
			{
				var tempFile = Path.Combine(tempDir, Path.GetRandomFileName());
				var client = new MappedClientPath();

				// Act
				var mappedPath = client.GetMappedPath(tempFile);

				// Assert
				AssertEquals(true, Directory.Exists(tempDir));
				AssertNotNull(mappedPath);
			}
		}

		public void TestGetMappedPath_ParentDoesExistButDirectoryExistsReturnsFalse()
		{
			// Arrange
			using (LeaveUnmappedTemporarily())
			using (var tempDir = new TempDirectory())
			{
				var parentDir = Path.Combine(tempDir, Path.GetRandomFileName());
				Directory.CreateDirectory(parentDir);

				// Clear all parent directory's permissions
				var parentDirInfo = new DirectoryInfo(parentDir);
				var parentDirAcls = parentDirInfo.GetAccessControl();
				parentDirAcls.SetAccessRuleProtection(true, false);
				parentDirInfo.SetAccessControl(parentDirAcls);

				// Clear all parent of parent directory's permissions
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

					var newParentDirAcls = new DirectorySecurity();
					newParentDirAcls.SetAccessRuleProtection(false, true);
					parentDirInfo.SetAccessControl(newParentDirAcls);
				}))
				{
					var unmappedPath = Path.Combine(parentDir, Path.GetRandomFileName());
					var client = new MappedClientPath();

					// Act
					var mappedPath = client.GetMappedPath(unmappedPath);

					// Assert
					AssertEquals("Parent directory doesn't seem existing", false, Directory.Exists(parentDir));
					AssertNotNull("Permission", parentDirInfo.GetAccessControl());
					AssertNotNull(mappedPath);
				}
			}
		}

		public void TestGetMappedPath_ParentDoesNotExist()
		{
			// Arrange
			using (LeaveUnmappedTemporarily())
			{
				var tempDir = new TempDirectory();
				tempDir.Dispose();

				var tempFile = Path.Combine(tempDir, Path.GetRandomFileName());
				var client = new MappedClientPath();

				// Act
				var mappedPath = client.GetMappedPath(tempFile);

				// Assert
				AssertEquals(false, Directory.Exists(tempDir));
				AssertExceptionThrown<DirectoryNotFoundException>(() => new DirectoryInfo(tempDir).GetAccessControl());
				AssertNull(mappedPath);
			}
		}

		public void TestGetMappedPath_ParentDoesNotExist_SameNameFileExistingInsteadOfParentDirectory()
		{
			// Arrange
			using (LeaveUnmappedTemporarily())
			{
				var tempDir = new TempDirectory();
				tempDir.Dispose();

				File.Create(tempDir.DirectoryName).Dispose();

				var unmappedPath = Path.Combine(tempDir, Path.GetRandomFileName());
				var client = new MappedClientPath();

				// Act
				var mappedPath = client.GetMappedPath(unmappedPath);

				// Assert
				AssertEquals(false, Directory.Exists(tempDir));
				AssertNotNull("File Permission", new DirectoryInfo(tempDir).GetAccessControl());
				AssertNull(mappedPath);

				// Cleanup
				File.Delete(tempDir.DirectoryName);
			}
		}

		public void TestGetMappedPath_FileDoesExist()
		{
			// Arrange
			using (LeaveUnmappedTemporarily())
			using (var tempDir = new TempDirectory())
			{
				var tempFile = Temp.GetTempFileName(tempDir);
				var client = new MappedClientPath();

				// Act
				var mappedPath = client.GetMappedPath(tempFile);

				// Assert
				AssertNotNull(mappedPath);
				AssertEquals(tempFile, mappedPath);
				AssertEquals(true, File.Exists(mappedPath));
			}
		}

		public void TestGetMappedPath_FileDoesNotExist()
		{
			// Arrange
			using (LeaveUnmappedTemporarily())
			using (var tempDir = new TempDirectory())
			{
				var tempFile = Path.Combine(tempDir, Path.GetRandomFileName());
				var client = new MappedClientPath();

				// Act
				var mappedPath = client.GetMappedPath(tempFile);

				// Assert
				AssertNotNull(mappedPath);
				AssertEquals(tempFile, mappedPath);
				AssertEquals(false, File.Exists(tempFile));
			}
		}

		public void TestGetMappedPath_DirectoryDoesExist()
		{
			// Arrange
			using (LeaveUnmappedTemporarily())
			using (var tempDir = new TempDirectory())
			{
				var unmappedPath = tempDir.DirectoryName;
				var client = new MappedClientPath();

				// Act
				var mappedPath = client.GetMappedPath(unmappedPath);

				// Assert
				AssertNotNull(mappedPath);
				AssertEquals(unmappedPath, mappedPath);
				AssertEquals(true, Directory.Exists(unmappedPath));
			}
		}

		public void TestGetMappedPath_DirectoryDoesNotExist()
		{
			// Arrange
			using (LeaveUnmappedTemporarily())
			{
				var tempDir = new TempDirectory();
				var unmappedPath = tempDir.DirectoryName;
				tempDir.Dispose();

				var client = new MappedClientPath();

				// Act
				var mappedPath = client.GetMappedPath(unmappedPath);

				// Assert
				AssertNotNull(mappedPath);
				AssertEquals(unmappedPath, mappedPath);
				AssertEquals(false, Directory.Exists(unmappedPath));
			}
		}

		public void TestGetMappedPathOfExistingFile_VolumeSeparatorFormatError()
		{
			// Arrange
			var inputs = new[]
			{
				"C",
				@":\bad",
			};

			foreach (var input in inputs)
			{
				var client = new MappedClientPath();

				// Act
				var exception = AssertExceptionThrown<FileNotFoundException>(() => client.GetMappedPathOfExistingFile(input));

				// Assert
				AssertEquals(exception.Message, $"Cannot find file. Mapping Result: {DriveMappingResult.VolumeSeparatorFormatError}");
			}
		}

		public void TestGetMappedPathOfExistingFile_RdpOrCitrix()
		{
			// Arrange
			var dataSource = new (bool isCitrix, string expectedMappedPath)[]
			{
				(false, @"\\tsclient\C\parent\a.txt"),
				(true, @"\\Client\C$\parent\a.txt"),
			};

			foreach (var data in dataSource)
			{
				using (DisableParentExistenceCheckTemporarily(null))
				using (DisableFileExsistenceCheckTemporarily(null))
				using (ObjectFactory.Substitute<TerminalService>(new ZTerminalServiceForTest(isCitrixICA: data.isCitrix)))
				{
					var client = new MappedClientPath();
					var filPath = @"C:\parent\a.txt";

					// Act
					var mappedPath = client.GetMappedPathOfExistingFile(filPath);

					// Assert
					AssertEquals(data.expectedMappedPath, mappedPath);
				}
			}
		}

		public void TestGetMappedPathOfExistingFile_SubFilePathFollowingMulitipleSlashesAfterVolumeSeparator()
		{
			// Arrange
			var inputs = new[]
			{
				@"C:\\sub-path\a.txt",
				@"C:\\\sub-path\a.txt",
				@"C://sub-path\a.txt",
				@"C:/\sub-path\a.txt",
			};

			foreach (var input in inputs)
			{
				using (DisableParentExistenceCheckTemporarily(null))
				using (DisableFileExsistenceCheckTemporarily(null))
				{
					var client = new MappedClientPath();

					// Act
					var mappedPath = client.GetMappedPathOfExistingFile(input);

					// Assert
					AssertEquals(@"\\tsclient\C\sub-path\a.txt", mappedPath);
				}
			}
		}

		public void TestGetMappedPathOfExistingFile_CheckParentNormally()
		{
			// Arrange
			var dataSource = new (string input, string expectedParent, string expectedResult)[]
			{
				(@"C:\a.txt", @"\\tsclient\C", @"\\tsclient\C\a.txt"),
				(@"C:\\a.txt", @"\\tsclient\C", @"\\tsclient\C\a.txt"),
				(@"C:\\a\b.txt", @"\\tsclient\C\a", @"\\tsclient\C\a\b.txt"),
				(@"C:\\a\b\c.txt", @"\\tsclient\C\a\b", @"\\tsclient\C\a\b\c.txt"),
			};

			foreach (var data in dataSource)
			{
				using (DisableParentExistenceCheckTemporarily(
					s =>
					{
						AssertEquals("Volume path need to be checked", data.expectedParent, s);
						return ExistenceState.ExistingNormally;
					}))
				using (DisableFileExsistenceCheckTemporarily(null))
				{
					var client = new MappedClientPath();

					// Act
					var mappedPath = client.GetMappedPathOfExistingFile(data.input);

					// Assert
					AssertEquals(data.expectedResult, mappedPath);
				}
			}
		}

		public void TestGetMappedPathOfExistingFile_ParentDoesNotExist()
		{
			// Arrange
			using (LeaveUnmappedTemporarily())
			{
				var tempDir = new TempDirectory();
				tempDir.Dispose();

				var tempFile = Path.Combine(tempDir, Path.GetRandomFileName());
				var client = new MappedClientPath();

				// Act
				var exception = AssertExceptionThrown<FileNotFoundException>(() => client.GetMappedPathOfExistingFile(tempFile));

				// Assert
				AssertEquals(exception.Message, $"File Existence State: {ExistenceState.ParentDirectoryNotExisting}, Directory Existence State:{ExistenceState.NotExisting}");
			}
		}

		public void TestGetMappedPathOfExistingFile_SameNameDirectoryExistingInsteadOfFile()
		{
			// Arrange
			using (LeaveUnmappedTemporarily())
			using (var tempDir = new TempDirectory())
			{
				var directory = Path.Combine(tempDir, Path.GetRandomFileName());
				Directory.CreateDirectory(directory);

				var client = new MappedClientPath();

				// Act
				var exception = AssertExceptionThrown<FileNotFoundException>(() => client.GetMappedPathOfExistingFile(directory));

				// Assert
				AssertEquals(true, Directory.Exists(directory));
				AssertEquals(exception.Message, $"File Existence State: {ExistenceState.ExistingButNotFile}, Directory Existence State:{ExistenceState.ExistingNormally}");
			}
		}

		public void TestGetMappedPathOfExistingFile_FileDoesExist()
		{
			// Arrange
			using (LeaveUnmappedTemporarily())
			using (var tempDir = new TempDirectory())
			{
				var tempFile = Temp.GetTempFileName(tempDir);
				var client = new MappedClientPath();

				// Act
				var mappedPath = client.GetMappedPathOfExistingFile(tempFile);

				// Assert
				AssertNotNull(mappedPath);
				AssertEquals(tempFile, mappedPath);
			}
		}

		public void TestGetMappedPathOfExistingFile_FileDoesExist_HasNoPermissionToParent()
		{
			// Arrange
			using (LeaveUnmappedTemporarily())
			using (var tempDir = new TempDirectory())
			{
				var parentDir = Path.Combine(tempDir, Path.GetRandomFileName());
				Directory.CreateDirectory(parentDir);

				// Protect file's permissions
				var tempFile = Temp.GetTempFileName(parentDir);
				var fileInfo = new FileInfo(tempFile);
				var fileAcls = fileInfo.GetAccessControl();
				fileAcls.SetAccessRuleProtection(true, true);
				fileInfo.SetAccessControl(fileAcls);

				// Clear all parent directory's permissions
				var parentDirInfo = new DirectoryInfo(parentDir);
				var parentDirAcls = parentDirInfo.GetAccessControl();
				parentDirAcls.SetAccessRuleProtection(true, false);
				parentDirInfo.SetAccessControl(parentDirAcls);

				// Clear all parent of parent directory's permissions
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

					var newParentDirAcls = new DirectorySecurity();
					newParentDirAcls.SetAccessRuleProtection(false, true);
					parentDirInfo.SetAccessControl(newParentDirAcls);
				}))
				{
					var client = new MappedClientPath();

					// Act
					var mappedPath = client.GetMappedPathOfExistingFile(tempFile);

					// Assert
					AssertEquals("Parent directory doesn't seem existing", false, Directory.Exists(parentDir));
					AssertNotNull("Directory Permission", parentDirInfo.GetAccessControl());
					AssertEquals(true, File.Exists(tempFile));

					AssertNotNull(mappedPath);
					AssertEquals(tempFile, mappedPath);
				}
			}
		}

		public void TestGetMappedPathOfExistingFile_FileDoesNotExist()
		{
			// Arrange
			using (LeaveUnmappedTemporarily())
			using (var tempDir = new TempDirectory())
			{
				var tempFile = Path.Combine(tempDir, Path.GetRandomFileName());
				var client = new MappedClientPath();

				// Act
				var exception = AssertExceptionThrown<FileNotFoundException>(() => client.GetMappedPathOfExistingFile(tempFile));

				// Assert
				AssertEquals(exception.Message, $"File Existence State: {ExistenceState.NotExisting}, Directory Existence State:{ExistenceState.ExistingNormally}");
			}
		}

		public void TestGetMappedPathOfExistingFile_FileDoesExist_HasNoPermissionToFile()
		{
			// Arrange
			using (LeaveUnmappedTemporarily())
			using (var tempDir = new TempDirectory())
			{
				var parentDir = Path.Combine(tempDir, Path.GetRandomFileName());
				Directory.CreateDirectory(parentDir);

				// Clear file's permissions
				var tempFile = Temp.GetTempFileName(parentDir);
				var tempFileInfo = new FileInfo(tempFile);
				var fileAcls = tempFileInfo.GetAccessControl();
				fileAcls.SetAccessRuleProtection(true, false);
				tempFileInfo.SetAccessControl(fileAcls);

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
					tempFileInfo.SetAccessControl(newFileAcls);
				}))
				{
					var client = new MappedClientPath();

					// Act
					var exception = AssertExceptionThrown<FileNotFoundException>(() => client.GetMappedPathOfExistingFile(tempFile));

					// Assert
					AssertEquals(false, File.Exists(tempFile));
					AssertNotNull("Permission", parentDirInfo.GetAccessControl());

					AssertEquals(exception.Message, $"File Existence State: {ExistenceState.ExistingButNoPermission}, Directory Existence State:{ExistenceState.ExistingNormally}");
				}
			}
		}

		static IDisposable LeaveUnmappedTemporarily()
			=> new DisposableAction(
				() => MappedClientPath.LeaveThePathUnMappedForTesting = true,
				() => MappedClientPath.LeaveThePathUnMappedForTesting = false);

		static IDisposable DisableParentExistenceCheckTemporarily(Func<string, ExistenceState> predicate)
			=> new DisposableAction(
				() => MappedClientPath.DoesParentDirectoryExistTestMock = predicate ?? (_ => ExistenceState.ExistingNormally),
				() => MappedClientPath.DoesParentDirectoryExistTestMock = null);

		static IDisposable DisableFileExsistenceCheckTemporarily(Func<string, ExistenceState> mock)
			=> new DisposableAction(
				() => MappedClientPath.DoesFileExistTestMock = mock ?? (_ => ExistenceState.ExistingNormally),
				() => MappedClientPath.DoesFileExistTestMock = null);
	}
}
