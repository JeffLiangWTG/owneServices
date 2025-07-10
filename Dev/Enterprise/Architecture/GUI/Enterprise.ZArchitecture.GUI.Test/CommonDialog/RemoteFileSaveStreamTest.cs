using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using CargoWise.IO;
using Enterprise.RemoteDesktopServices;
using Enterprise.RemoteDesktopServices.Server;
using Enterprise.RemoteDesktopServices.Server.TrackingInfo;
using Enterprise.RemoteDesktopServices.Testing;
using Enterprise.ZArchitecture.Core;
using Moq;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class RemoteFileSaveStreamTest : RemoteDesktopServicesTest
	{
		public void TestRemoteFileSaveStream()
		{
			var data = new byte[] { 0, 1, 1, 2, 3, 5, 8, 13, 21, 34, 55, 89, 144 };
			using (var tempFile = TempFile.New())
			{
				File.Delete(tempFile.Filename);
				var mappedClientPath = new Mock<MappedClientPath>();
				mappedClientPath.Setup(o => o.GetMappedPath(tempFile.Filename)).Returns(tempFile.Filename);
				AssertNotNull(mappedClientPath.Object.GetMappedPath(tempFile.Filename));
				using (var remoteStream = new RemoteFileSaveStream(tempFile.Filename, mappedClientPath.Object, 40))
				{
					remoteStream.Write(data, 0, data.Length);
					AssertEquals(false, remoteStream.IsSwitchedToFile);
				}
				System.Threading.Thread.Sleep(1000);

				AssertFileSameAsBytes(tempFile.Filename, data);
				File.Delete(tempFile.Filename);
				AssertEquals(false, File.Exists(tempFile.Filename));
			}
		}

		public void TestRemoteFileSaveStreamWithTempfile()
		{
			var data = new byte[] { 0, 1, 1, 2, 3, 5, 8, 13, 21, 34, 55, 89, 144 };
			var remoteFileName = Temp.GetTempFileName();
			try
			{
				var mappedClientPath = new Mock<MappedClientPath>();
				mappedClientPath.Setup(o => o.GetMappedPath(remoteFileName)).Returns(remoteFileName);
				AssertNotNull(mappedClientPath.Object.GetMappedPath(remoteFileName));
				using (var remoteStream = new RemoteFileSaveStream(remoteFileName, mappedClientPath.Object, 10))
				{
					remoteStream.Write(data, 0, data.Length);
					AssertEquals(true, remoteStream.IsSwitchedToFile);
				}
				System.Threading.Thread.Sleep(1000);

				AssertFileSameAsBytes(remoteFileName, data);
			}
			finally
			{
				File.Delete(remoteFileName);
			}
		}

		public void TestRemoteFileSaveStreamWithMappedFile()
		{
			var data = new byte[] { 0, 1, 1, 2, 3, 5, 8, 13, 21, 34, 55, 89, 144 };

			var unmappedPath = @"X:\foo\bar.txt";
			var mappedPath = Temp.GetTempFileName();
			try
			{
				var mappedClientPath = new Mock<MappedClientPath>();
				mappedClientPath.Setup(o => o.GetMappedPath(unmappedPath)).Returns(mappedPath);
				AssertNotNull(mappedClientPath.Object.GetMappedPath(unmappedPath));
				using (var remoteStream = new RemoteFileSaveStream(unmappedPath, mappedClientPath.Object, 40))
				{
					remoteStream.Write(data, 0, data.Length);
					AssertEquals(false, remoteStream.IsSwitchedToFile);
				}
				System.Threading.Thread.Sleep(1000);

				AssertFileSameAsBytes(mappedPath, data);
			}
			finally
			{
				File.Delete(mappedPath);
			}
		}

		public void TestRemoteFileSaveStreamWithMappedFileAndTempFile()
		{
			var data = new byte[] { 0, 1, 1, 2, 3, 5, 8, 13, 21, 34, 55, 89, 144 };

			var unmappedPath = @"X:\foo\bar.txt";
			var mappedPath = Temp.GetTempFileName();
			try
			{
				var mappedClientPath = new Mock<MappedClientPath>();
				mappedClientPath.Setup(o => o.GetMappedPath(unmappedPath)).Returns(mappedPath);
				AssertNotNull(mappedClientPath.Object.GetMappedPath(unmappedPath));
				using (var remoteStream = new RemoteFileSaveStream(unmappedPath, mappedClientPath.Object, 10))
				{
					remoteStream.Write(data, 0, data.Length);
					AssertEquals(true, remoteStream.IsSwitchedToFile);
				}
				System.Threading.Thread.Sleep(1000);

				AssertFileSameAsBytes(mappedPath, data);
			}
			finally
			{
				File.Delete(mappedPath);
			}
		}

		public void TestSaveStreamWhenDriveMappingStopsWorking()
		{
			// <= RemoteFileDialog.LargeFileSizeDefinition
			Test(344);								// 344B
			Test(678);								// 678B
			Test(1 * 1024);									// 1Kb
			Test(1 * 1024 + 123);							// 1Kb+123B
			Test(2 * 1024);									// 2Kb
			Test(2 * 1024 + 234);							// 2Kb+234B
			Test(766 * 1024);								// 766Kb
			Test(766 * 1024 + 1023);						// 766Kb+1023B
			Test(RemoteFileDialog.LargeFileSizeDefinition);	// 1X

			// > RemoteFileDialog.LargeFileSizeDefinition
			Test(RemoteFileDialog.LargeFileSizeDefinition + 444);
			Test(RemoteFileDialog.LargeFileSizeDefinition + 1 * RemoteFileDialog.FileChunkSize - 1);
			Test(RemoteFileDialog.LargeFileSizeDefinition + 1 * RemoteFileDialog.FileChunkSize);
			Test(RemoteFileDialog.LargeFileSizeDefinition + 1 * RemoteFileDialog.FileChunkSize + 1);
			Test(RemoteFileDialog.LargeFileSizeDefinition + 1 * RemoteFileDialog.FileChunkSize + 123);

			Test(RemoteFileDialog.LargeFileSizeDefinition + 2 * RemoteFileDialog.FileChunkSize - 1);
			Test(RemoteFileDialog.LargeFileSizeDefinition + 2 * RemoteFileDialog.FileChunkSize);
			Test(RemoteFileDialog.LargeFileSizeDefinition + 2 * RemoteFileDialog.FileChunkSize + 1);
			Test(RemoteFileDialog.LargeFileSizeDefinition + 2 * RemoteFileDialog.FileChunkSize + 246);

			Test(RemoteFileDialog.LargeFileSizeDefinition + 3 * RemoteFileDialog.FileChunkSize - 1);

			// > 2 * RemoteFileDialog.LargeFileSizeDefinition
			Test(2 * RemoteFileDialog.LargeFileSizeDefinition + 555);
			Test(2 * RemoteFileDialog.LargeFileSizeDefinition + RemoteFileDialog.FileChunkSize + 666);

			void Test(int dataSize)
			{
				// Arrange
				var sb = new StringBuilder();
				using (var logCollector = new TrackingInfoLoggerCollector(sb))
				using (var destinationFile = TempFile.New())
				{
					var data = GenerateLargeData(dataSize);

					var mappedClientPath = new Mock<MappedClientPath>();
					mappedClientPath.Setup(o => o.GetMappedPath(destinationFile.Filename)).Returns(default(string));

					var remoteStream = new RemoteFileSaveStream(destinationFile.Filename, mappedClientPath.Object, switchToFileLimitInBytes: 1 * 1024 * 1024); // switch limit: 1 Mb
					remoteStream.Write(data, 0, data.Length);

					// Act
					AssertNoExceptionThrown(() => remoteStream.Dispose());

					// Assert
					AssertSequencesEqual(
						$@"dataSize: {dataSize}

Logs:
{sb}",
						data,
						File.ReadAllBytes(destinationFile.Filename));
				}
			}
		}

		public void TestSaveStreamWhenDriveMappingStopsWorkingAndFileExists()
		{
			Test("File exists", CreatePreExistingObsoleteFile);
			Test("Temp file exists", x => CreatePreExistingObsoleteFile($"{x}.tmp")); // temp file

			// both file and temp file
			Test("Both files exist", x =>
			{
				CreatePreExistingObsoleteFile(x);
				CreatePreExistingObsoleteFile($"{x}.tmp");
			});

			void Test(string message, Action<string> createPreExistingFileAction)
			{
				// Arrange
				var sb = new StringBuilder();
				using (var logCollector = new TrackingInfoLoggerCollector(sb))
				using (var destinationFile = TempFile.New())
				{
					createPreExistingFileAction(destinationFile.Filename);

					var data = GenerateLargeData(2 * 1024 * 1024);

					var mappedClientPath = new Mock<MappedClientPath>();
					mappedClientPath.Setup(o => o.GetMappedPath(destinationFile.Filename)).Returns(default(string));

					var remoteStream = new RemoteFileSaveStream(destinationFile.Filename, mappedClientPath.Object, switchToFileLimitInBytes: 1 * 1024 * 1024);
					remoteStream.Write(data, 0, data.Length);

					// Act
					AssertNoExceptionThrown(() => remoteStream.Dispose());

					// Assert
					AssertSequencesEqual($@"Type: {message}

Logs:
{sb}", data, File.ReadAllBytes(destinationFile.Filename));
				}
			}
		}

		static byte[] GenerateLargeData(int dataSizeInByte)
		{
			var gen = RandomNumberGenerator.Create();
			var data = new byte[dataSizeInByte];
			gen.GetBytes(data);
			return data;
		}

		static void CreatePreExistingObsoleteFile(string filePath)
		{
			File.WriteAllText(filePath, "invalid contents which will be overwritten");
		}

		sealed class TrackingInfoLoggerCollector : IDisposable
		{
			readonly StringBuilder sb;

			public TrackingInfoLoggerCollector(StringBuilder sb)
			{
				this.sb = sb;
				TrackingInfoLogger.Instance.OnNewLog += TrackingInfoLogger_OnNewLog;
			}

			void TrackingInfoLogger_OnNewLog(object sender, string e)
			{
				sb.AppendLine(e);
			}

			public void Dispose()
			{
				TrackingInfoLogger.Instance.OnNewLog -= TrackingInfoLogger_OnNewLog;
			}
		}
	}
}
