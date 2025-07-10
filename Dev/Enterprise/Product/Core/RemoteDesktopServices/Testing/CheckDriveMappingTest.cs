using System;
using System.IO;
using System.Security.AccessControl;
using CargoWise.Common;
using CargoWise.IO;
using Enterprise.RemoteDesktopServices.Server;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.RemoteDesktopServices.Testing
{
	class CheckDriveMappingTest : TransactionedTestCase
	{
		public void TestCheckDriveMapping_Disabled()
		{
			// Arrange
			using (var tempDir = new TempDirectory())
			using (new DisposableAction(
				() => MappedClientPath.LeaveThePathUnMappedForTesting = true,
				() => MappedClientPath.LeaveThePathUnMappedForTesting = false))
			{
				EnvProxy.Instance.Registry.RemoteAppEnableDragDropLite = false;

				var filePath = Temp.GetTempFileName(tempDir);

				// Act
				var result = new CheckDriveMappingHandlerMock().DoHandleExposed(filePath);

				// Assert
				AssertEquals(DriveMappingResult.Disabled, result);
			}
		}

		public void TestCheckDriveMapping_Success()
		{
			// Arrange
			using (var tempDir = new TempDirectory())
			using (new DisposableAction(
				() =>
				{
					MappedClientPath.LeaveThePathUnMappedForTesting = true;
					EnvProxy.Instance.Registry.RemoteAppEnableDragDropLite = true;
				},
				() =>
				{
					MappedClientPath.LeaveThePathUnMappedForTesting = false;
				}))
			{
				var filePath = Temp.GetTempFileName(tempDir);

				// Act
				var result = new CheckDriveMappingHandlerMock().DoHandleExposed(filePath);

				// Assert
				AssertEquals(DriveMappingResult.Success, result);
			}
		}

		public void TestCheckDriveMapping_VolumeSeparatorFormatError()
		{
			// Arrange
			using (var tempDir = new TempDirectory())
			using (new DisposableAction(
				() =>
				{
					MappedClientPath.LeaveThePathUnMappedForTesting = true;
					EnvProxy.Instance.Registry.RemoteAppEnableDragDropLite = true;
				},
				() =>
				{
					MappedClientPath.LeaveThePathUnMappedForTesting = false;
				}))
			{
				const string FilePath = ":\\a.txt";

				// Act
				var result = new CheckDriveMappingHandlerMock().DoHandleExposed(FilePath);

				// Assert
				AssertEquals(DriveMappingResult.FileDoesNotExist, result);
			}
		}

		public void TestCheckDriveMapping_ParentDirectoryDoesNotExist()
		{
			// Arrange
			using (new DisposableAction(
				() =>
				{
					MappedClientPath.LeaveThePathUnMappedForTesting = true;
					EnvProxy.Instance.Registry.RemoteAppEnableDragDropLite = true;
				},
				() =>
				{
					MappedClientPath.LeaveThePathUnMappedForTesting = false;
				}))
			{
				var tempDir = new TempDirectory();
				tempDir.Dispose();

				var filePath = Path.Combine(tempDir, "a.txt");

				// Act
				var result = new CheckDriveMappingHandlerMock().DoHandleExposed(filePath);

				// Assert
				AssertEquals(DriveMappingResult.FileDoesNotExist, result);
			}
		}

		public void TestCheckDriveMapping_FileDoesNotExist()
		{
			// Arrange
			using (var tempDir = new TempDirectory())
			using (new DisposableAction(
				() =>
				{
					MappedClientPath.LeaveThePathUnMappedForTesting = true;
					EnvProxy.Instance.Registry.RemoteAppEnableDragDropLite = true;
				},
				() =>
				{
					MappedClientPath.LeaveThePathUnMappedForTesting = false;
				}))
			{
				var filePath = Path.Combine(tempDir, "a.txt");

				// Act
				var result = new CheckDriveMappingHandlerMock().DoHandleExposed(filePath);

				// Assert
				AssertEquals(DriveMappingResult.FileDoesNotExist, result);
			}
		}

		public void TestCheckDriveMappingDoesNotHaveReadPermissionToExistingFile()
		{
			// Arrange
			using (var tempDir = new TempDirectory())
			using (new DisposableAction(
				() =>
				{
					MappedClientPath.LeaveThePathUnMappedForTesting = true;
					EnvProxy.Instance.Registry.RemoteAppEnableDragDropLite = true;
				},
				() =>
				{
					MappedClientPath.LeaveThePathUnMappedForTesting = false;
				}))
			{
				var filePath = Temp.GetTempFileName(tempDir);

				var fileInfo = new FileInfo(tempDir);
				var fileAcls = fileInfo.GetAccessControl();
				fileAcls.SetAccessRuleProtection(true, false);
				fileInfo.SetAccessControl(fileAcls);

				var dirInfo = new DirectoryInfo(tempDir);
				var dirAcls = dirInfo.GetAccessControl();
				dirAcls.SetAccessRuleProtection(true, false);
				dirInfo.SetAccessControl(dirAcls);

				using (new DisposableAction(
					() =>
					{
						var newDirAcls = new DirectorySecurity();
						newDirAcls.SetAccessRuleProtection(false, true);
						new DirectoryInfo(tempDir).SetAccessControl(newDirAcls);

						var newFileAcls = new FileSecurity();
						newFileAcls.SetAccessRuleProtection(false, true);
						new FileInfo(tempDir).SetAccessControl(newFileAcls);
					}))
				{
					var handler = new DragDropLiteHandler();

					// Act
					var result = new CheckDriveMappingHandlerMock().DoHandleExposed(filePath);

					// Assert
					AssertEquals(DriveMappingResult.FileDoesNotExist, result);
				}
			}
		}

		class CheckDriveMappingHandlerMock : CheckDriveMappingHandler
		{
			readonly IEnterpriseChannel channel = new EnterpriseChannelMock();

			public DriveMappingResult DoHandleExposed(string message)
			{
				return DoHandle(channel, message);
			}
		}

		class EnterpriseChannelMock : IEnterpriseChannel
		{
			public bool IsConnected => throw new NotImplementedException();

			public bool Send(byte[] data)
			{
				throw new NotImplementedException();
			}
		}
	}
}
