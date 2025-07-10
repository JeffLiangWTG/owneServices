using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
//using System.Threading;
using CargoWise.Common;
using CargoWise.IO;
using Enterprise.Client.EDI.Escrow;
using Enterprise.Client.EDI.Escrow.Interfaces;
using Enterprise.Integration;
using Moq;
using NUnit.Framework;

namespace ZClientEDI.Test.Escrow
{
	class GitAdapterTest : TestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			loggerMock = new Mock<ILogger>();
			gitFolder = new TempDirectory(Temp.GetNewTempSubdirectory());
			gitDirectoryMock = new Mock<IWorkingDirectory>();
			gitDirectoryMock.Setup(directory => directory.DirectoryName).Returns(gitFolder.DirectoryName);
			gitAdapter = new GitAdapter();
			gitAdapter.GitDirectory = gitDirectoryMock.Object;
		}

		protected override void TearDown()
		{
			gitFolder.Dispose();
			base.TearDown();
		}

		string ExtractRepo()
		{
			using var embeddedResourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);

			var repoZip = embeddedResourceRetriever.SaveResourceToFile("ZClientEDI.Test.Escrow.repo.zip");
			var destinationDirectoryName = Temp.GetNewTempSubdirectory();
			ZipFile.ExtractToDirectory(repoZip, destinationDirectoryName);
			return destinationDirectoryName;
		}

		public void TestClonesRepositoryToFolder()
		{
			// Arrange
			var gitDownloader = new GitDownloader(Mock.Of<IProGetAssetDirectoryPathUrlRegistry>(registry => registry.AssetPathUrl == "https://proget.wtg.zone/endpoints/EscrowExport"));
			var gitZip = gitDownloader.Download(gitDirectoryMock.Object, CancellationToken.None);
			gitDownloader.UnzipPortableGit(gitZip, gitDirectoryMock.Object);

			using var tempFolder = new TempDirectory(Path.Combine(Temp.TempPath, Path.GetRandomFileName()));
			using var repoFolder = new TempDirectory(ExtractRepo());

			var tempFolderLength = tempFolder.DirectoryName.Length;
			var repoFolderLength = Path.GetDirectoryName(repoFolder.DirectoryName).Length;
			var expectedFiles = Directory
				.EnumerateFiles(repoFolder, "*.*", SearchOption.AllDirectories)
				.Select(s => s.Substring(repoFolderLength))
				.ToList();

			// Act
			gitAdapter.Clone(repoFolder, tempFolder, loggerMock.Object, string.Empty);

			// Assert
			var result = Directory
				.EnumerateFiles(tempFolder, "*.*", SearchOption.AllDirectories)
				.Select(s => s.Substring(tempFolderLength));
			AssertGreaterThan(expectedFiles.Count, 0);
			AssertContainsExactElementsInAnyOrder(expectedFiles, result);
		}

		public void TestThrowsOnNoFolder()
		{
			// Arrange
			var gitDownloader = new GitDownloader(Mock.Of<IProGetAssetDirectoryPathUrlRegistry>(registry => registry.AssetPathUrl == "https://proget.wtg.zone/endpoints/EscrowExport"));
			var gitZip = gitDownloader.Download(gitDirectoryMock.Object, CancellationToken.None);
			gitDownloader.UnzipPortableGit(gitZip, gitDirectoryMock.Object);

			using var repoFolder = new TempDirectory(ExtractRepo());
			var tempFolder = Path.Combine(Temp.TempPath, nameof(TestThrowsOnNoFolder));

			// Act
			var result = AssertExceptionThrown<GitAdapterException>(() => gitAdapter.Clone(repoFolder, tempFolder, loggerMock.Object, string.Empty));

			// Assert
			AssertContains(tempFolder, result.Message, true);
		}

		public void TestLogsCloneProcess()
		{
			// Arrange
			var gitDownloader = new GitDownloader(Mock.Of<IProGetAssetDirectoryPathUrlRegistry>(registry => registry.AssetPathUrl == "https://proget.wtg.zone/endpoints/EscrowExport"));
			var gitZip = gitDownloader.Download(gitDirectoryMock.Object, CancellationToken.None);
			gitDownloader.UnzipPortableGit(gitZip, gitDirectoryMock.Object);

			using var tempFolder = new TempDirectory(Path.Combine(Temp.TempPath, Path.GetRandomFileName()));
			using var repoFolder = new TempDirectory(ExtractRepo());
			var logs = new List<string>();
			loggerMock
				.Setup(logger => logger.Log(It.IsAny<LogType>(), It.IsAny<string>()))
				.Callback<LogType, string>((type, s) => logs.Add($"{type:G}|{s}"));

			// Act
			gitAdapter.Clone($"file:///{repoFolder.DirectoryName}", tempFolder, loggerMock.Object, string.Empty);

			// Assert
			AssertEquals(logs[0], $"Debug|Cloning into '{Path.GetFileName(repoFolder.DirectoryName)}'...");
			AssertCollectionContains("Debug|remote: Total 114 (delta 16), reused 62 (delta 5), pack-reused 0        ", logs);
		}

		public void TestGitExeNotFoundThrowsGitExecutableFileException()
		{
			// Arrange
			using var tempFolder = new TempDirectory(Path.Combine(Temp.TempPath, Path.GetRandomFileName()));
			using var repoFolder = new TempDirectory(ExtractRepo());
			using var emptyGitFolder = new TempDirectory(Temp.GetNewTempSubdirectory());
			var localGitDirectoryMock = Mock.Of<IWorkingDirectory>(directory => directory.DirectoryName == emptyGitFolder.DirectoryName);

			var localGitAdapter = new GitAdapter();

			localGitAdapter.GitDirectory = localGitDirectoryMock;

			// Act
			var exception = AssertExceptionThrown<GitExecutableFileException>(() => localGitAdapter.Clone(repoFolder, tempFolder, loggerMock.Object, string.Empty));

			// Assert
			AssertContains("System exception encountered trying to run process: git", exception.Message);
			AssertContains("The system cannot find the file specified", exception.Message);
			Assert(exception.InnerException is Win32Exception);
		}

		public void TestGitFromPathIsNotUsed()
		{
			// Arrange
			var gitDownloader = new GitDownloader(Mock.Of<IProGetAssetDirectoryPathUrlRegistry>(registry => registry.AssetPathUrl == "https://proget.wtg.zone/endpoints/EscrowExport"));
			var gitZip = gitDownloader.Download(gitDirectoryMock.Object, CancellationToken.None);
			gitDownloader.UnzipPortableGit(gitZip, gitDirectoryMock.Object);

			var backupPath = System.Environment.GetEnvironmentVariable("PATH");
			using (new DisposableAction(() => System.Environment.SetEnvironmentVariable("PATH", backupPath)))
			{
				System.Environment.SetEnvironmentVariable("PATH", "c:\\git");

				using var tempFolder = new TempDirectory(Path.Combine(Temp.TempPath, Path.GetRandomFileName()));
				using var repoFolder = new TempDirectory(ExtractRepo());

				// Act
				// Assert
				AssertNoExceptionThrown(() => gitAdapter.Clone(repoFolder, tempFolder, loggerMock.Object, string.Empty));
			}
		}

		public void TestWrongParamsCall()
		{
			var result = AssertExceptionThrown<ArgumentNullException>(() => gitAdapter.Clone(string.Empty, string.Empty, null, string.Empty));
			AssertEquals("logger", result.ParamName);
		}

		GitAdapter gitAdapter;
		Mock<ILogger> loggerMock;
		Mock<IWorkingDirectory> gitDirectoryMock;
		TempDirectory gitFolder;
	}
}
