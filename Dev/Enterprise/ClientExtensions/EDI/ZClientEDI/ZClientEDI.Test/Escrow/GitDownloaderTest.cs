using System;
using System.IO;
using System.Threading;
using CargoWise.IO;
using Enterprise.Client.EDI.Escrow;
using Enterprise.Client.EDI.Escrow.Interfaces;
using Moq;
using NUnit.Framework;

namespace ZClientEDI.Test.Escrow
{
	class GitDownloaderTest : TestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			proGetAssetDirectoryPathUrlRegistryMock = Mock.Of<IProGetAssetDirectoryPathUrlRegistry>(
				registry => registry.AssetPathUrl == "https://proget.wtg.zone/endpoints/EscrowExport");
			gitDownloader = new GitDownloader(proGetAssetDirectoryPathUrlRegistryMock);
			tempDirectory = new TempDirectory(Temp.GetNewTempSubdirectory());
			gitDirectoryMock = new Mock<IWorkingDirectory>();
			gitDirectoryMock.Setup(directory => directory.DirectoryName).Returns(tempDirectory.DirectoryName);
		}

		protected override void TearDown()
		{
			tempDirectory.Dispose();
			base.TearDown();
		}

		public void TestDownloads()
		{
			// Arrange

			// Act
			var result = gitDownloader.Download(gitDirectoryMock.Object, CancellationToken.None);

			// Assert
			AssertEquals(true, File.Exists(result));
		}

		public void TestExceptionOnWrongUrl()
		{
			// Arrange
			const string wrongUrl = "some.wrong.site.com";
			Mock.Get(proGetAssetDirectoryPathUrlRegistryMock)
				.SetupGet(registry => registry.AssetPathUrl)
				.Returns($"https://{wrongUrl}");

			// Act
			// Assert
			var result = AssertExceptionThrown<AggregateException>(() => gitDownloader.Download(gitDirectoryMock.Object, CancellationToken.None))
				.Flatten();

			var innerException = result.InnerException;

			AssertType<PortableGitNotFoundException>(innerException);
			AssertContains(wrongUrl, innerException.Message);
		}

		public void TestWrongParamsCall()
		{
			var result = AssertExceptionThrown<ArgumentNullException>(() => _ = new GitDownloader(null));
			AssertEquals("proGetAssetDirectoryPathUrlRegistry", result.ParamName);
		}

		public void TestExistingFileWithAssetsURLThrowsException()
		{
			// Arrange
			const string url = "https://proget.wtg.zone/assets/EscrowExport";
			Mock.Get(proGetAssetDirectoryPathUrlRegistryMock)
				.SetupGet(registry => registry.AssetPathUrl)
				.Returns(url);

			// Act
			var ex = AssertExceptionThrown<AggregateException>(() => gitDownloader.Download(gitDirectoryMock.Object, CancellationToken.None));

			// Assert
			var innerException = ex.InnerException;

			AssertType<PortableGitNotFoundException>(innerException);
			AssertContains(url, innerException.Message);
		}

		public void TestNonExistingFileWithEndpointsURLThrowsException()
		{
			// Arrange
			const string url = "https://proget.wtg.zone/endpoints/NonExisting";
			Mock.Get(proGetAssetDirectoryPathUrlRegistryMock)
				.SetupGet(registry => registry.AssetPathUrl)
				.Returns(url);

			// Act
			var ex = AssertExceptionThrown<AggregateException>(() => gitDownloader.Download(gitDirectoryMock.Object, CancellationToken.None));

			// Assert
			var innerException = ex.InnerException;

			AssertType<PortableGitNotFoundException>(innerException);
			AssertContains(url, innerException.Message);
		}

		public void TestFailedZipExtractionThrowsException()
		{
			// Arrange
			using var tempFolder = new TempDirectory(Path.Combine(Temp.TempPath, Path.GetRandomFileName()));
			var tempFile = Path.Combine(tempFolder.DirectoryName, Path.GetRandomFileName());
			File.WriteAllText(tempFile, "not a zip file");

			// Act
			var ex = AssertExceptionThrown<PortableGitExtractionException>(() =>
			{
				gitDownloader.UnzipPortableGit(tempFile, gitDirectoryMock.Object);
			});

			// Assert
			AssertContains(tempFile, ex.Message);
		}

		GitDownloader gitDownloader;
		IProGetAssetDirectoryPathUrlRegistry proGetAssetDirectoryPathUrlRegistryMock;
		TempDirectory tempDirectory;
		Mock<IWorkingDirectory> gitDirectoryMock;
	}
}
