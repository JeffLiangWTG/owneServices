using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.ServiceTasks.ELearningDocument;
using Enterprise.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Test
{
	public class ELearningDocumentMyAccountShareFolderClientTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestDownloadFileEmptyPath()
		{
			// Arrange
			var mockLogger = new Mock<ILogger>();
			var client = new MyAccountShareFolderClient(mockLogger.Object);

			// Act
			// Assert
			Assert("Should return null for the empty path", client.DownloadFile(string.Empty) == null);
		}

		[ExpectNoExceptions]
		public void TestDownloadFileNullPath()
		{
			// Arrange
			var mockLogger = new Mock<ILogger>();
			var client = new MyAccountShareFolderClient(mockLogger.Object);

			// Act
			// Assert
			Assert("Should return null for the null path", client.DownloadFile(null) == null);
		}

		[ExpectNoExceptions]
		public void DownloadFileIncorectlyFormattedPathWithoutNetworkShareUserParameter()
		{
			// Arrange
			var mockLogger = new Mock<ILogger>();
			var client = new MyAccountShareFolderClient(mockLogger.Object);

			// Act
			// Assert
			Assert("Should return null for the incorrectly formatted path", client.DownloadFile(Guid.NewGuid().ToString()) == null);
		}

		[ExpectNoExceptions]
		public void DownloadFileSuccessfulDownload()
		{
			// Arrange
			var mockLogger = new Mock<ILogger>();
			var client = new MyAccountShareFolderClient(mockLogger.Object);
			// Act
			var path = client.UrlToNetworkPath(
				"https://myaccount-portal.cargowise.com/my-account/Documents/AccessTest.txt");
			var data = client.DownloadFile(path);
			// Assert
			Assert("Should return not null", data != null);
		}
		[ExpectNoExceptions]
		public void TestDownloadFileWholeFlow()
		{
			// Arrange
			var mockLogger = new Mock<ILogger>();
			var client = new Mock<IMyAccountClient>();
			client.Setup(m => m.DownloadFile(It.IsAny<string>())).Returns(Array.Empty<byte>());
			client.Setup(m => m.UrlToNetworkPath(It.IsAny<string>())).Returns("folder\\file.pdf");
			// Act
			var data = client.Object.DownloadFile(It.IsAny<string>());
			// Assert
			Assert("Should return not null", data.Length == 0);
		}
	}
}
