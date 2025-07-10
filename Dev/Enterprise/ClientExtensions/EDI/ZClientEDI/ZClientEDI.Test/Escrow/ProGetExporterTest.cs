using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.IO;
using Enterprise.Client.EDI.Escrow;
using Enterprise.Client.EDI.Escrow.Interfaces;
using Enterprise.Integration;
using Moq;
using NUnit.Framework;

namespace ZClientEDI.Test.Escrow
{
	class ProGetExporterTest : TestCase
	{
		public void TestWrongParamsCall()
		{
			var result = AssertExceptionThrown<ArgumentNullException>(() => _ = new ProGetExporter(null, Mock.Of<IAssetDirectoryClient>()));
			AssertEquals("proGetAssetDirectoryRegistry", result.ParamName);

			result = AssertExceptionThrown<ArgumentNullException>(() => _ = new ProGetExporter(Mock.Of<IProGetAssetDirectoryRegistry>(), null));
			AssertEquals("assetDirectoryClient", result.ParamName);
		}

		[ExpectNoExceptions]
		[TestDate(2024, 8, 21)]
		public void TestCreatesFolderWithYearAndMonth()
		{
			// Arrange
			using var tempDir = new TempDirectory(Temp.GetNewTempSubdirectory());
			var dirMock = Mock.Of<IWorkingDirectory>(d => d.DirectoryName == tempDir.DirectoryName);

			// Act
			proGetExporter.Export(dirMock, loggerMock.Object, CancellationToken.None);

			// Assert
			assetDirectoryClientMock.Verify(x => x.CreateRemoteDirectoryAsync(It.Is<string>(s => s.StartsWith("Escrow202408")), It.IsAny<CancellationToken>()), Times.Once);
		}

		[ExpectNoExceptions]
		[TestDate(2024, 8, 21)]
		public void TestExportCallsDeleteRemoteDirectoryAsync()
		{
			// Arrange
			using var tempDir = new TempDirectory(Temp.GetNewTempSubdirectory());
			var dirMock = Mock.Of<IWorkingDirectory>(d => d.DirectoryName == tempDir.DirectoryName);

			// Act
			proGetExporter.Export(dirMock, loggerMock.Object, CancellationToken.None);

			// Assert
			assetDirectoryClientMock.Verify(x => x.DeleteRemoteDirectoryAsync(It.Is<string>(s => s.StartsWith("Escrow202408")), It.IsAny<CancellationToken>()), Times.Once);
		}

		[ExpectNoExceptions]
		[TestDate(2024, 8, 21)]
		public void TestCopyFilesFromRootFolder()
		{
			// Arrange
			using var tempDir = new TempDirectory(Temp.GetNewTempSubdirectory());
			var dirMock = Mock.Of<IWorkingDirectory>(d => d.DirectoryName == tempDir.DirectoryName);

			var filePaths = new List<string>();
			for (var i = 0; i < 10; i++)
			{
				var path = Path.Combine(tempDir, $"File {i}");
				File.Create(path).Close();
				filePaths.Add(path);
			}

			// Act
			proGetExporter.Export(dirMock, loggerMock.Object, CancellationToken.None);

			// Assert
			foreach (var path in filePaths)
			{
				var remotePath = Path.Combine("Escrow202408", Path.GetFileName(path));
				assetDirectoryClientMock.Verify(x => x.UploadFileAsync(It.IsAny<string>(), remotePath, It.IsAny<CancellationToken>()), Times.Once);
			}
		}

		[ExpectNoExceptions]
		[TestDate(2024, 8, 21)]
		public void TestCopySubFoldersWithFiles()
		{
			// Arrange
			using var tempDir = new TempDirectory(Temp.GetNewTempSubdirectory());
			var dirMock = Mock.Of<IWorkingDirectory>(d => d.DirectoryName == tempDir.DirectoryName);
			var folders = new List<string>();
			var files = new List<string>();
			// Create first level of subfolders
			for (var i = 0; i < 2; i++)
			{
				var firstLevelDir = Directory.CreateDirectory(Path.Combine(tempDir.DirectoryName, $"SubFolder{i}"));
				for (var j = 0; j < 2; j++)
				{
					var path = Path.Combine(firstLevelDir.FullName, $"File{j}.txt");
					File.Create(path).Close();
					files.Add(path);
				}

				folders.Add(firstLevelDir.FullName);

				// Create second level of subfolders within each first-level subfolder
				for (var k = 0; k < 2; k++)
				{
					var secondLevelDir =
						Directory.CreateDirectory(Path.Combine(firstLevelDir.FullName, $"SubSubFolder{k}"));
					for (var l = 0; l < 2; l++)
					{
						var path = Path.Combine(secondLevelDir.FullName, $"File{l}.txt");
						File.Create(path).Close();
						files.Add(path);
					}

					folders.Add(secondLevelDir.FullName);
				}
			}

			// Act
			proGetExporter.Export(dirMock, loggerMock.Object, CancellationToken.None);

			// Assert
			assetDirectoryClientMock.Verify(x => x.CreateRemoteDirectoryAsync("Escrow202408", It.IsAny<CancellationToken>()), Times.Once);
			assetDirectoryClientMock.Verify(x => x.CreateRemoteDirectoryAsync(@"Escrow202408\SubFolder0", It.IsAny<CancellationToken>()), Times.Once);
			assetDirectoryClientMock.Verify(x => x.CreateRemoteDirectoryAsync(@"Escrow202408\SubFolder1", It.IsAny<CancellationToken>()), Times.Once);
			assetDirectoryClientMock.Verify(x => x.CreateRemoteDirectoryAsync(@"Escrow202408\SubFolder0\SubSubFolder0", It.IsAny<CancellationToken>()), Times.Once);
			assetDirectoryClientMock.Verify(x => x.CreateRemoteDirectoryAsync(@"Escrow202408\SubFolder0\SubSubFolder1", It.IsAny<CancellationToken>()), Times.Once);
			assetDirectoryClientMock.Verify(x => x.CreateRemoteDirectoryAsync(@"Escrow202408\SubFolder1\SubSubFolder0", It.IsAny<CancellationToken>()), Times.Once);
			assetDirectoryClientMock.Verify(x => x.CreateRemoteDirectoryAsync(@"Escrow202408\SubFolder1\SubSubFolder1", It.IsAny<CancellationToken>()), Times.Once);

			assetDirectoryClientMock.Verify(x => x.UploadFileAsync(It.IsAny<string>(), @"Escrow202408\SubFolder0\File0.txt", It.IsAny<CancellationToken>()), Times.Once);
			assetDirectoryClientMock.Verify(x => x.UploadFileAsync(It.IsAny<string>(), @"Escrow202408\SubFolder0\File1.txt", It.IsAny<CancellationToken>()), Times.Once);
			assetDirectoryClientMock.Verify(x => x.UploadFileAsync(It.IsAny<string>(), @"Escrow202408\SubFolder1\File0.txt", It.IsAny<CancellationToken>()), Times.Once);
			assetDirectoryClientMock.Verify(x => x.UploadFileAsync(It.IsAny<string>(), @"Escrow202408\SubFolder1\File1.txt", It.IsAny<CancellationToken>()), Times.Once);
			assetDirectoryClientMock.Verify(x => x.UploadFileAsync(It.IsAny<string>(), @"Escrow202408\SubFolder0\SubSubFolder0\File0.txt", It.IsAny<CancellationToken>()), Times.Once);
			assetDirectoryClientMock.Verify(x => x.UploadFileAsync(It.IsAny<string>(), @"Escrow202408\SubFolder0\SubSubFolder0\File1.txt", It.IsAny<CancellationToken>()), Times.Once);
			assetDirectoryClientMock.Verify(x => x.UploadFileAsync(It.IsAny<string>(), @"Escrow202408\SubFolder0\SubSubFolder1\File0.txt", It.IsAny<CancellationToken>()), Times.Once);
			assetDirectoryClientMock.Verify(x => x.UploadFileAsync(It.IsAny<string>(), @"Escrow202408\SubFolder0\SubSubFolder1\File1.txt", It.IsAny<CancellationToken>()), Times.Once);
			assetDirectoryClientMock.Verify(x => x.UploadFileAsync(It.IsAny<string>(), @"Escrow202408\SubFolder1\SubSubFolder0\File0.txt", It.IsAny<CancellationToken>()), Times.Once);
			assetDirectoryClientMock.Verify(x => x.UploadFileAsync(It.IsAny<string>(), @"Escrow202408\SubFolder1\SubSubFolder0\File1.txt", It.IsAny<CancellationToken>()), Times.Once);
			assetDirectoryClientMock.Verify(x => x.UploadFileAsync(It.IsAny<string>(), @"Escrow202408\SubFolder1\SubSubFolder1\File0.txt", It.IsAny<CancellationToken>()), Times.Once);
			assetDirectoryClientMock.Verify(x => x.UploadFileAsync(It.IsAny<string>(), @"Escrow202408\SubFolder1\SubSubFolder1\File1.txt", It.IsAny<CancellationToken>()), Times.Once);
		}

		[ExpectNoExceptions]
		[TestDate(2024, 8, 21)]
		public void TestLogsAreCreatedAsExpected()
		{
			// Arrange
			proGetAssetDirectoryRegistryMock.SetupGet(x => x.AssetPathUrl).Returns("https://a.com/b/c/");
			using var tempDir = new TempDirectory(Temp.GetNewTempSubdirectory());
			var dirMock = Mock.Of<IWorkingDirectory>(d => d.DirectoryName == tempDir.DirectoryName);
			var folders = new List<string>();
			var files = new List<string>();
			// Create first level of subfolders
			for (var i = 0; i < 2; i++)
			{
				var firstLevelDir = Directory.CreateDirectory(Path.Combine(tempDir.DirectoryName, $"SubFolder{i}"));
				for (var j = 0; j < 2; j++)
				{
					var path = Path.Combine(firstLevelDir.FullName, $"File{j}.txt");
					File.Create(path).Close();
					files.Add(path);
				}

				folders.Add(firstLevelDir.FullName);

				// Create second level of subfolders within each first-level subfolder
				for (var k = 0; k < 2; k++)
				{
					var secondLevelDir =
						Directory.CreateDirectory(Path.Combine(firstLevelDir.FullName, $"SubSubFolder{k}"));
					for (var l = 0; l < 2; l++)
					{
						var path = Path.Combine(secondLevelDir.FullName, $"File{l}.txt");
						File.Create(path).Close();
						files.Add(path);
					}

					folders.Add(secondLevelDir.FullName);
				}
			}

			// Act
			proGetExporter.Export(dirMock, loggerMock.Object, CancellationToken.None);

			// Assert
			loggerMock.Verify(x => x.Log(LogType.Information, "> Creating remote folders..."), Times.Once);
			loggerMock.Verify(x => x.Log(LogType.Information, "-> Delete root remote folder [https://a.com/b/c/Escrow202408]"), Times.Once);
			loggerMock.Verify(x => x.Log(LogType.Information, "-> Create root remote folder [https://a.com/b/c/Escrow202408]"), Times.Once);
			loggerMock.Verify(x => x.Log(LogType.Information, "-> Create remote folder [1/6, https://a.com/b/c/Escrow202408/SubFolder0]"), Times.Once);
			loggerMock.Verify(x => x.Log(LogType.Information, "-> Create remote folder [2/6, https://a.com/b/c/Escrow202408/SubFolder1]"), Times.Once);
			loggerMock.Verify(x => x.Log(LogType.Information, "-> Create remote folder [3/6, https://a.com/b/c/Escrow202408/SubFolder0/SubSubFolder0]"), Times.Once);
			loggerMock.Verify(x => x.Log(LogType.Information, "-> Create remote folder [4/6, https://a.com/b/c/Escrow202408/SubFolder0/SubSubFolder1]"), Times.Once);
			loggerMock.Verify(x => x.Log(LogType.Information, "-> Create remote folder [5/6, https://a.com/b/c/Escrow202408/SubFolder1/SubSubFolder0]"), Times.Once);
			loggerMock.Verify(x => x.Log(LogType.Information, "-> Create remote folder [6/6, https://a.com/b/c/Escrow202408/SubFolder1/SubSubFolder1]"), Times.Once);
			loggerMock.Verify(x => x.Log(LogType.Information, It.Is<string>(s => s.StartsWith("> Creating remote folders finished in"))), Times.Once);

			var sepChar = Path.DirectorySeparatorChar;
			loggerMock.Verify(x => x.Log(LogType.Information, "> Uploading files to [https://a.com/b/c/Escrow202408]..."), Times.Exactly(1));

			loggerMock.Verify(x => x.Log(LogType.Information, It.Is<string>(s => s.Contains($".{sepChar}SubFolder0{sepChar}File0.txt]..."))), Times.Exactly(1));
			loggerMock.Verify(x => x.Log(LogType.Information, It.Is<string>(s => s.Contains($".{sepChar}SubFolder0{sepChar}File0.txt] finished in"))), Times.Exactly(1));
			loggerMock.Verify(x => x.Log(LogType.Information, It.Is<string>(s => s.Contains($".{sepChar}SubFolder0{sepChar}File1.txt]..."))), Times.Exactly(1));
			loggerMock.Verify(x => x.Log(LogType.Information, It.Is<string>(s => s.Contains($".{sepChar}SubFolder0{sepChar}File1.txt] finished in"))), Times.Exactly(1));
			loggerMock.Verify(x => x.Log(LogType.Information, It.Is<string>(s => s.Contains($".{sepChar}SubFolder0{sepChar}SubSubFolder0{sepChar}File0.txt]..."))), Times.Exactly(1));
			loggerMock.Verify(x => x.Log(LogType.Information, It.Is<string>(s => s.Contains($".{sepChar}SubFolder0{sepChar}SubSubFolder0{sepChar}File0.txt] finished in"))), Times.Exactly(1));
			loggerMock.Verify(x => x.Log(LogType.Information, It.Is<string>(s => s.Contains($".{sepChar}SubFolder0{sepChar}SubSubFolder0{sepChar}File1.txt]..."))), Times.Exactly(1));
			loggerMock.Verify(x => x.Log(LogType.Information, It.Is<string>(s => s.Contains($".{sepChar}SubFolder0{sepChar}SubSubFolder0{sepChar}File1.txt] finished in"))), Times.Exactly(1));
			loggerMock.Verify(x => x.Log(LogType.Information, It.Is<string>(s => s.Contains($".{sepChar}SubFolder0{sepChar}SubSubFolder1{sepChar}File0.txt]..."))), Times.Exactly(1));
			loggerMock.Verify(x => x.Log(LogType.Information, It.Is<string>(s => s.Contains($".{sepChar}SubFolder0{sepChar}SubSubFolder1{sepChar}File0.txt] finished in"))), Times.Exactly(1));
			loggerMock.Verify(x => x.Log(LogType.Information, It.Is<string>(s => s.Contains($".{sepChar}SubFolder0{sepChar}SubSubFolder1{sepChar}File1.txt]..."))), Times.Exactly(1));
			loggerMock.Verify(x => x.Log(LogType.Information, It.Is<string>(s => s.Contains($".{sepChar}SubFolder0{sepChar}SubSubFolder1{sepChar}File1.txt] finished in"))), Times.Exactly(1));
			loggerMock.Verify(x => x.Log(LogType.Information, It.Is<string>(s => s.Contains($".{sepChar}SubFolder1{sepChar}File0.txt]..."))), Times.Exactly(1));
			loggerMock.Verify(x => x.Log(LogType.Information, It.Is<string>(s => s.Contains($".{sepChar}SubFolder1{sepChar}File0.txt] finished in"))), Times.Exactly(1));
			loggerMock.Verify(x => x.Log(LogType.Information, It.Is<string>(s => s.Contains($".{sepChar}SubFolder1{sepChar}File1.txt]..."))), Times.Exactly(1));
			loggerMock.Verify(x => x.Log(LogType.Information, It.Is<string>(s => s.Contains($".{sepChar}SubFolder1{sepChar}File1.txt] finished in"))), Times.Exactly(1));
			loggerMock.Verify(x => x.Log(LogType.Information, It.Is<string>(s => s.Contains($".{sepChar}SubFolder1{sepChar}SubSubFolder0{sepChar}File0.txt]..."))), Times.Exactly(1));
			loggerMock.Verify(x => x.Log(LogType.Information, It.Is<string>(s => s.Contains($".{sepChar}SubFolder1{sepChar}SubSubFolder0{sepChar}File0.txt] finished in"))), Times.Exactly(1));
			loggerMock.Verify(x => x.Log(LogType.Information, It.Is<string>(s => s.Contains($".{sepChar}SubFolder1{sepChar}SubSubFolder0{sepChar}File1.txt]..."))), Times.Exactly(1));
			loggerMock.Verify(x => x.Log(LogType.Information, It.Is<string>(s => s.Contains($".{sepChar}SubFolder1{sepChar}SubSubFolder0{sepChar}File1.txt] finished in"))), Times.Exactly(1));
			loggerMock.Verify(x => x.Log(LogType.Information, It.Is<string>(s => s.Contains($".{sepChar}SubFolder1{sepChar}SubSubFolder1{sepChar}File0.txt]..."))), Times.Exactly(1));
			loggerMock.Verify(x => x.Log(LogType.Information, It.Is<string>(s => s.Contains($".{sepChar}SubFolder1{sepChar}SubSubFolder1{sepChar}File0.txt] finished in"))), Times.Exactly(1));
			loggerMock.Verify(x => x.Log(LogType.Information, It.Is<string>(s => s.Contains($".{sepChar}SubFolder1{sepChar}SubSubFolder1{sepChar}File1.txt]..."))), Times.Exactly(1));
			loggerMock.Verify(x => x.Log(LogType.Information, It.Is<string>(s => s.Contains($".{sepChar}SubFolder1{sepChar}SubSubFolder1{sepChar}File1.txt] finished in"))), Times.Exactly(1));

			for (var i = 1; i <= 12; i++)
			{
				loggerMock.Verify(
					x => x.Log(LogType.Information, It.Is<string>(s => s.StartsWith($"-> Uploading file [{i}/12,"))),
					Times.Exactly(2));
			}

			loggerMock.Verify(x => x.Log(LogType.Information, It.Is<string>(s => s.StartsWith("> Uploading files to [https://a.com/b/c/Escrow202408] finished in"))), Times.Exactly(1));
		}

		public void TestExportRespondsToCancellation()
		{
			// Arrange
			var cancellationToken = new CancellationToken(canceled: true);

			// Act Assert
			var exception = AssertExceptionThrown<AggregateException>(() => proGetExporter.Export(Mock.Of<IWorkingDirectory>(), loggerMock.Object, cancellationToken));
			AssertType<TaskCanceledException>(exception.InnerException);
		}

		public void TestExportShouldRespondToCancellationWhileRunning()
		{
			// Arrange
			using var cancellationTokenSource = new CancellationTokenSource();
			using var manualResetEvent = new ManualResetEvent(false);
			using var tempDir = new TempDirectory(Temp.GetNewTempSubdirectory());
			var dirMock = Mock.Of<IWorkingDirectory>(d => d.DirectoryName == tempDir.DirectoryName);

			assetDirectoryClientMock
				.Setup(client => client.CreateRemoteDirectoryAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.Callback((string path, CancellationToken token) =>
				{
					manualResetEvent.WaitOne();
					token.ThrowIfCancellationRequested();
				});

			var exportTask = Task.Run(() =>
			{
				// Act
				proGetExporter.Export(dirMock, loggerMock.Object, cancellationTokenSource.Token);
			});

			Thread.Sleep(TimeSpan.FromMilliseconds(1000));
			cancellationTokenSource.Cancel();
			manualResetEvent.Set();

			// Assert
			var exception = AssertExceptionThrown<AggregateException>(() => exportTask.Wait());
			AssertType<TaskCanceledException>(exception.InnerException.InnerException);
			loggerMock.Verify(x => x.Log(LogType.Error, "The operation was canceled."), Times.Once);
		}

		public void TestExportLogsExceptions()
		{
			// Arrange
			var exception = new Exception("Test exception");
			assetDirectoryClientMock
				.Setup(x => x.DeleteRemoteDirectoryAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.Throws(exception);

			AssertExceptionThrown<Exception>(() =>
			{
				// Act
				proGetExporter.Export(Mock.Of<IWorkingDirectory>(), loggerMock.Object, CancellationToken.None);
			});

			// Assert
			loggerMock.Verify(x => x.Log(LogType.Error, exception.Message), Times.Once);
		}

		protected override void SetUp()
		{
			base.SetUp();
			proGetAssetDirectoryRegistryMock = new Mock<IProGetAssetDirectoryRegistry>();
			proGetAssetDirectoryRegistryMock.SetupGet(x => x.AssetPathUrl).Returns("https://a.com/b/c");
			loggerMock = new Mock<ILogger>();
			assetDirectoryClientMock = new Mock<IAssetDirectoryClient>();
			assetDirectoryClientMock
				.Setup(x => x.UploadFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.Returns(Task.CompletedTask);
			proGetExporter = new ProGetExporter(proGetAssetDirectoryRegistryMock.Object, assetDirectoryClientMock.Object);
		}

		Mock<IProGetAssetDirectoryRegistry> proGetAssetDirectoryRegistryMock;
		Mock<ILogger> loggerMock;
		Mock<IAssetDirectoryClient> assetDirectoryClientMock;
		ProGetExporter proGetExporter;
	}
}
