using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.IO;
using Enterprise.Client.EDI.Escrow;
using Enterprise.Integration;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace ZClientEDI.Test.Escrow
{
	class BinaryProcessorTest : TransactionedTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			httpClientFactoryMock = new Mock<WTG.Foundation.Http.IHttpClientFactory>();
		}

		public void TestWrongParamsCall()
		{
			var result = AssertExceptionThrown<ArgumentNullException>(() => new BinaryProcessor(null));
			AssertEquals("httpClientFactory", result.ParamName);

			var binaryProcessor = new BinaryProcessor(httpClientFactoryMock.Object);
			result = AssertExceptionThrown<ArgumentNullException>(() => binaryProcessor.FindAndCopy(null, Mock.Of<IReadOnlyCollection<IRepository>>(), Mock.Of<ILogger>()));
			AssertEquals("outputDirectory", result.ParamName);

			result = AssertExceptionThrown<ArgumentNullException>(() => binaryProcessor.FindAndCopy(Mock.Of<IWorkingDirectory>(), null, Mock.Of<ILogger>()));
			AssertEquals("repositories", result.ParamName);

			result = AssertExceptionThrown<ArgumentNullException>(() => binaryProcessor.FindAndCopy(Mock.Of<IWorkingDirectory>(), Mock.Of<IReadOnlyCollection<IRepository>>(), null));
			AssertEquals("logger", result.ParamName);
		}

		[ExpectNoExceptions]
		public void TestCreatesAndDisposesHttpClient()
		{
			// Arrange
			var httpClientMock = new Mock<HttpClient>();
			httpClientFactoryMock
				.Setup(factory => factory.CreateNew(It.IsAny<HttpMessageHandler>()))
				.Returns(httpClientMock.Object);
			var binaryProcessor = new BinaryProcessor(httpClientFactoryMock.Object);

			using var tempFolder = new TempDirectory(Temp.GetNewTempSubdirectory());
			var workingDirectoryMock = Mock.Of<IWorkingDirectory>(directory => directory.DirectoryName == tempFolder.DirectoryName);

			// Act
			binaryProcessor.FindAndCopy(workingDirectoryMock, Array.Empty<IRepository>(), Mock.Of<ILogger>());

			// Assert
			httpClientFactoryMock.Verify(
				factory => factory.CreateNew(It.Is<HttpClientHandler>(handler =>
					handler.Proxy == null)),
				Times.Once);
			httpClientMock
				.Protected()
				.Verify(nameof(HttpClient.Dispose), Times.Once(), new object[] { true });
			httpClientFactoryMock.VerifyNoOtherCalls();
		}

		[ExpectNoExceptions]
		public void TestCallsCorrectRepositoryUrls()
		{
			Test(new[]
				{
					Mock.Of<IRepository>(repository => repository.Repository == "https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev" && repository.Path == "/"),
					Mock.Of<IRepository>(repository => repository.Repository == "https://devops.wisetechglobal.com/wtg/IdentityAndSecurity/_git/IdentityAndSecurity" && repository.Path == "/AWSCertificateIntegration"),
				},
				new[]
				{
					@"http://crikey.wtg.zone/api/BuildArtifactRepository/latestBuild?repository=https%3A%2F%2Fdevops.wisetechglobal.com%2Fwtg%2FCargoWise%2F_git%2FDev&branch=master&path=%2F",
					@"http://crikey.wtg.zone/api/BuildArtifactRepository/latestBuild?repository=https%3A%2F%2Fdevops.wisetechglobal.com%2Fwtg%2FIdentityAndSecurity%2F_git%2FIdentityAndSecurity&branch=master&path=%2FAWSCertificateIntegration",
				});
			Test(new[]
				{
					Mock.Of<IRepository>(repository => repository.Repository == "https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev" && repository.Path == "/"),
					Mock.Of<IRepository>(repository => repository.Repository == "https://devops.wisetechglobal.com/wtg/CargoWise/_git/Shared" && repository.Path == "/ServiceManager.Common"),
				},
				new[]
				{
					@"http://crikey.wtg.zone/api/BuildArtifactRepository/latestBuild?repository=https%3A%2F%2Fdevops.wisetechglobal.com%2Fwtg%2FCargoWise%2F_git%2FShared&branch=master&path=%2FServiceManager.Common",
					@"http://crikey.wtg.zone/api/BuildArtifactRepository/latestBuild?repository=https%3A%2F%2Fdevops.wisetechglobal.com%2Fwtg%2FCargoWise%2F_git%2FDev&branch=master&path=%2F",
				});

			Test(new[]
				{
					Mock.Of<IRepository>(repository => repository.Repository == "https://github.com/WiseTechGlobal/WTG.WindowMonitoring" && repository.Path == "/"),
				},
				new[]
				{
					@"http://crikey.wtg.zone/api/BuildArtifactRepository/latestBuild?repository=https%3A%2F%2Fgithub.com%2FWiseTechGlobal%2FWTG.WindowMonitoring&branch=master",
				});

			void Test(IRepository[] repositories, string[] expectedRequestUris)
			{
				// Arrange
				var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
				httpMessageHandlerMock
					.Protected()
					.Setup<Task<HttpResponseMessage>>(
						nameof(HttpClient.SendAsync),
						ItExpr.IsAny<HttpRequestMessage>(),
						ItExpr.IsAny<CancellationToken>())
					.ReturnsAsync((HttpRequestMessage message, CancellationToken _) =>
						new HttpResponseMessage
						{
							Content = new StreamContent(Stream.Null),
						});
				var httpClient = new HttpClient(httpMessageHandlerMock.Object);
				httpClientFactoryMock
					.Setup(factory => factory.CreateNew(It.IsAny<HttpMessageHandler>()))
					.Returns(httpClient);

				using var tempFolder = new TempDirectory(Temp.GetNewTempSubdirectory());
				var workingDirectoryMock = Mock.Of<IWorkingDirectory>(directory => directory.DirectoryName == tempFolder.DirectoryName);

				var binaryProcessor = new BinaryProcessor(httpClientFactoryMock.Object);

				// Act
				binaryProcessor.FindAndCopy(workingDirectoryMock, repositories, Mock.Of<ILogger>());

				// Assert
				foreach (var requestUri in expectedRequestUris)
				{
					httpMessageHandlerMock
						.Protected()
						.Verify<Task<HttpResponseMessage>>(
							nameof(HttpClient.SendAsync),
							Times.Once(),
							ItExpr.Is<HttpRequestMessage>(message => string.Equals(requestUri, message.RequestUri.AbsoluteUri, StringComparison.OrdinalIgnoreCase)),
							ItExpr.IsAny<CancellationToken>());
				}

				httpMessageHandlerMock
					.Protected()
					.Verify<Task<HttpResponseMessage>>(
						nameof(HttpClient.SendAsync),
						Times.Exactly(expectedRequestUris.Length),
						ItExpr.IsAny<HttpRequestMessage>(),
						ItExpr.IsAny<CancellationToken>());
			}
		}

		public void TestDownloadsCorrectlyToCorrectLocations()
		{
			Test(new[]
				{
					Mock.Of<IRepository>(repository => repository.Repository == "https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev" && repository.Path == "/"),
					Mock.Of<IRepository>(repository => repository.Repository == "https://devops.wisetechglobal.com/wtg/IdentityAndSecurity/_git/IdentityAndSecurity" && repository.Path == "/AWSCertificateIntegration"),
				},
				new (string fileName, string content)[]
				{
					(@"\binary\CargoWise\Dev\Release.zip", @"http://crikey.wtg.zone/api/BuildArtifactRepository/latestBuild?repository=https%3A%2F%2Fdevops.wisetechglobal.com%2Fwtg%2FCargoWise%2F_git%2FDev&branch=master&path=%2F"),
					(@"\binary\IdentityAndSecurity\IdentityAndSecurity\AWSCertificateIntegration\Release.zip", @"http://crikey.wtg.zone/api/BuildArtifactRepository/latestBuild?repository=https%3A%2F%2Fdevops.wisetechglobal.com%2Fwtg%2FIdentityAndSecurity%2F_git%2FIdentityAndSecurity&branch=master&path=%2FAWSCertificateIntegration"),
				});
			Test(new[]
				{
					Mock.Of<IRepository>(repository => repository.Repository == "https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev" && repository.Path == "/"),
					Mock.Of<IRepository>(repository => repository.Repository == "https://devops.wisetechglobal.com/wtg/CargoWise/_git/Shared" && repository.Path == "/ServiceManager.Common"),
				},
				new (string fileName, string content)[]
				{
					(@"\binary\CargoWise\Shared\ServiceManager.Common\Release.zip", @"http://crikey.wtg.zone/api/BuildArtifactRepository/latestBuild?repository=https%3A%2F%2Fdevops.wisetechglobal.com%2Fwtg%2FCargoWise%2F_git%2FShared&branch=master&path=%2FServiceManager.Common"),
					(@"\binary\CargoWise\Dev\Release.zip", @"http://crikey.wtg.zone/api/BuildArtifactRepository/latestBuild?repository=https%3A%2F%2Fdevops.wisetechglobal.com%2Fwtg%2FCargoWise%2F_git%2FDev&branch=master&path=%2F"),
				});

			Test(new[]
				{
					Mock.Of<IRepository>(repository => repository.Repository == "https://github.com/WiseTechGlobal/WTG.WindowMonitoring" && repository.Path == "/"),
				},
				new[]
				{
					(@"\binary\WiseTechGlobal\WTG.WindowMonitoring\Release.zip", @"http://crikey.wtg.zone/api/BuildArtifactRepository/latestBuild?repository=https%3A%2F%2Fgithub.com%2FWiseTechGlobal%2FWTG.WindowMonitoring&branch=master"),
				});

			void Test(IRepository[] repositories, (string fileName, string content)[] expectedFiles)
			{
				// Arrange
				var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
				httpMessageHandlerMock
					.Protected()
					.Setup<Task<HttpResponseMessage>>(
						nameof(HttpClient.SendAsync),
						ItExpr.IsAny<HttpRequestMessage>(),
						ItExpr.IsAny<CancellationToken>())
					.ReturnsAsync((HttpRequestMessage message, CancellationToken _) =>
						new HttpResponseMessage
						{
							Content = new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes(message.RequestUri.AbsoluteUri))),
						});

				var httpClient = new HttpClient(httpMessageHandlerMock.Object);
				httpClientFactoryMock
					.Setup(factory => factory.CreateNew(It.IsAny<HttpMessageHandler>()))
					.Returns(httpClient);

				using var tempFolder = new TempDirectory(Temp.GetNewTempSubdirectory());
				var workingDirectoryMock = Mock.Of<IWorkingDirectory>(directory => directory.DirectoryName == tempFolder.DirectoryName);

				var binaryProcessor = new BinaryProcessor(httpClientFactoryMock.Object);

				// Act
				binaryProcessor.FindAndCopy(workingDirectoryMock, repositories, Mock.Of<ILogger>());

				// Assert
				var result = Directory
					.GetFiles(tempFolder.DirectoryName, "*", SearchOption.AllDirectories)
					.Select(s => (
							fileName: s.Replace(tempFolder.DirectoryName, string.Empty),
							content: File.ReadAllText(s)))
					.ToArray();
				AssertContainsExactElementsInAnyOrder(expectedFiles, result);
			}
		}

		public void TestLogging()
		{
			Test(new[]
				{
					Mock.Of<IRepository>(repository => repository.Repository == "https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev" && repository.Path == "/"),
					Mock.Of<IRepository>(repository => repository.Repository == "https://devops.wisetechglobal.com/wtg/IdentityAndSecurity/_git/IdentityAndSecurity" && repository.Path == "/AWSCertificateIntegration"),
				},
				new[]
				{
					(LogType.Information, @"> Downloading binaries for repository \[1\/2, https:\/\/devops\.wisetechglobal\.com\/wtg\/CargoWise\/_git\/Dev\, \/]\.\.\."),
					(LogType.Information, @"> Downloading binaries for repository \[1\/2, https:\/\/devops\.wisetechglobal\.com\/wtg\/CargoWise\/_git\/Dev\, \/] finished in \d+:\d{2}:\d{2}:\d{2}\.\d+\."),
					(LogType.Information, @"> Downloading binaries for repository \[2\/2, https:\/\/devops\.wisetechglobal\.com\/wtg\/IdentityAndSecurity\/_git\/IdentityAndSecurity\, \/AWSCertificateIntegration]\.\.\."),
					(LogType.Information, @"> Downloading binaries for repository \[2\/2, https:\/\/devops\.wisetechglobal\.com\/wtg\/IdentityAndSecurity\/_git\/IdentityAndSecurity\, \/AWSCertificateIntegration] finished in \d+:\d{2}:\d{2}:\d{2}\.\d+\."),
				});
			Test(new[]
				{
					Mock.Of<IRepository>(repository => repository.Repository == "https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev" && repository.Path == "/"),
					Mock.Of<IRepository>(repository => repository.Repository == "https://devops.wisetechglobal.com/wtg/CargoWise/_git/Shared" && repository.Path == "/ServiceManager.Common"),
				},
				new[]
				{
					(LogType.Information, @"> Downloading binaries for repository \[1\/2, https:\/\/devops\.wisetechglobal\.com\/wtg\/CargoWise\/_git\/Dev\, \/]\.\.\."),
					(LogType.Information, @"> Downloading binaries for repository \[1\/2, https:\/\/devops\.wisetechglobal\.com\/wtg\/CargoWise\/_git\/Dev\, \/] finished in \d+:\d{2}:\d{2}:\d{2}\.\d+\."),
					(LogType.Information, @"> Downloading binaries for repository \[2\/2, https:\/\/devops\.wisetechglobal\.com\/wtg\/CargoWise\/_git\/Shared\, \/ServiceManager.Common]\.\.\."),
					(LogType.Information, @"> Downloading binaries for repository \[2\/2, https:\/\/devops\.wisetechglobal\.com\/wtg\/CargoWise\/_git\/Shared\, \/ServiceManager.Common] finished in \d+:\d{2}:\d{2}:\d{2}\.\d+\."),
				});

			Test(new[]
				{
					Mock.Of<IRepository>(repository => repository.Repository == "https://github.com/WiseTechGlobal/WTG.WindowMonitoring" && repository.Path == "/"),
				},
				new[]
				{
					(LogType.Information, @"> Downloading binaries for repository \[1\/1, https:\/\/github\.com\/WiseTechGlobal\/WTG\.WindowMonitoring\, \/]\.\.\."),
					(LogType.Information, @"> Downloading binaries for repository \[1\/1, https:\/\/github\.com\/WiseTechGlobal\/WTG\.WindowMonitoring\, \/] finished in \d+:\d{2}:\d{2}:\d{2}\.\d+\."),
				});

			void Test(IRepository[] repositories, (LogType logType, string pattern)[] expected)
			{
				// Arrange
				var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
				httpMessageHandlerMock
					.Protected()
					.Setup<Task<HttpResponseMessage>>(
						nameof(HttpClient.SendAsync),
						ItExpr.IsAny<HttpRequestMessage>(),
						ItExpr.IsAny<CancellationToken>())
					.ReturnsAsync(() =>
						new HttpResponseMessage
						{
							Content = new StreamContent(Stream.Null),
						});

				var httpClient = new HttpClient(httpMessageHandlerMock.Object);
				httpClientFactoryMock
					.Setup(factory => factory.CreateNew(It.IsAny<HttpMessageHandler>()))
					.Returns(httpClient);

				using var tempFolder = new TempDirectory(Temp.GetNewTempSubdirectory());
				var workingDirectoryMock = Mock.Of<IWorkingDirectory>(directory => directory.DirectoryName == tempFolder.DirectoryName);

				var binaryProcessor = new BinaryProcessor(httpClientFactoryMock.Object);

				var logTypes = new List<LogType>();
				var logMessages = new List<string>();
				var loggerMock = new Mock<ILogger>();
				loggerMock
					.Setup(logger => logger.Log(Moq.Capture.In(logTypes), Moq.Capture.In(logMessages)));

				// Act
				binaryProcessor.FindAndCopy(workingDirectoryMock, repositories, loggerMock.Object);

				// Assert
				var result = logTypes.Zip(logMessages, (type, message) => (type, message)).ToArray();
				AssertContainsExactElementsInExactOrder(new LogRecordComparer(), expected, result);
				loggerMock.Verify(logger => logger.Log(It.IsAny<LogType>(), It.IsAny<string>()), Times.Exactly(expected.Length));
				loggerMock.VerifyNoOtherCalls();
			}
		}

		public void TestLoggingOnFailedRetrieve()
		{
			// Arrange
			IRepository[] repositories = {
				Mock.Of<IRepository>(repository => repository.Repository == "https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev" && repository.Path == "/"),
				Mock.Of<IRepository>(repository => repository.Repository == "https://devops.wisetechglobal.com/wtg/IdentityAndSecurity/_git/IdentityAndSecurity" && repository.Path == "/AWSCertificateIntegration"),
			};
			var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
			httpMessageHandlerMock
				.Protected()
				.Setup<Task<HttpResponseMessage>>(
					nameof(HttpClient.SendAsync),
					ItExpr.IsAny<HttpRequestMessage>(),
					ItExpr.IsAny<CancellationToken>())
				.Throws<HttpRequestException>();

			var httpClient = new HttpClient(httpMessageHandlerMock.Object);
			httpClientFactoryMock
				.Setup(factory => factory.CreateNew(It.IsAny<HttpMessageHandler>()))
				.Returns(httpClient);

			using var tempFolder = new TempDirectory(Temp.GetNewTempSubdirectory());
			var workingDirectoryMock = Mock.Of<IWorkingDirectory>(directory => directory.DirectoryName == tempFolder.DirectoryName);

			var binaryProcessor = new BinaryProcessor(httpClientFactoryMock.Object);

			var logTypes = new List<LogType>();
			var logMessages = new List<string>();
			var loggerMock = new Mock<ILogger>();
			loggerMock
				.Setup(logger => logger.Log(Moq.Capture.In(logTypes), Moq.Capture.In(logMessages)));

			// Act
			AssertExceptionThrown<Exception>(() => binaryProcessor.FindAndCopy(workingDirectoryMock, repositories, loggerMock.Object));

			// Assert
			var result = logTypes.Zip(logMessages, (type, message) => (type, message)).ToArray();
			AssertContainsExactElementsInExactOrder(new LogRecordComparer(),
				new[]
				{
					(LogType.Information, @"> Downloading binaries for repository \[1\/2, https:\/\/devops\.wisetechglobal\.com\/wtg\/CargoWise\/_git\/Dev\, \/]\.\.\."),
				},
				result);
			loggerMock.Verify(logger => logger.Log(It.IsAny<LogType>(), It.IsAny<string>()), Times.Once);
			loggerMock.VerifyNoOtherCalls();
		}

		[DeveloperOnlyTest]
		public void TestDownloadsBinaryOfDevOpsRepository()
		{
			// Arrange
			IRepository[] repositories = { Mock.Of<IRepository>(repository => repository.Repository == "https://devops.wisetechglobal.com/wtg/CargoWise/_git/Shared" && repository.Path == "/ServiceManager.Common"), };

			using var tempFolder = new TempDirectory(Temp.GetNewTempSubdirectory());
			var workingDirectoryMock = Mock.Of<IWorkingDirectory>(directory => directory.DirectoryName == tempFolder.DirectoryName);

			var binaryProcessor = new BinaryProcessor(new WTG.Foundation.Http.HttpClientFactory(() => new HttpClientHandler()));

			// Act
			binaryProcessor.FindAndCopy(workingDirectoryMock, repositories, Mock.Of<ILogger>());

			// Assert
			var result = Directory
				.GetFiles(tempFolder.DirectoryName, "*", SearchOption.AllDirectories)
				.Select(s => s.Replace(tempFolder.DirectoryName, string.Empty));
			AssertContainsExactElementsInAnyOrder(new[] { @"\binary\CargoWise\Shared\ServiceManager.Common\Release.zip" }, result);
		}

		[DeveloperOnlyTest]
		public void TestDownloadsBinaryOfGitHubRepository()
		{
			// Arrange
			IRepository[] repositories = { Mock.Of<IRepository>(repository => repository.Repository == "https://github.com/WiseTechGlobal/WTG.WindowMonitoring" && repository.Path == "/"), };

			using var tempFolder = new TempDirectory(Temp.GetNewTempSubdirectory());
			var workingDirectoryMock = Mock.Of<IWorkingDirectory>(directory => directory.DirectoryName == tempFolder.DirectoryName);

			var binaryProcessor = new BinaryProcessor(new WTG.Foundation.Http.HttpClientFactory(() => new HttpClientHandler()));

			// Act
			binaryProcessor.FindAndCopy(workingDirectoryMock, repositories, Mock.Of<ILogger>());

			// Assert
			var result = Directory
				.GetFiles(tempFolder.DirectoryName, "*", SearchOption.AllDirectories)
				.Select(s => s.Replace(tempFolder.DirectoryName, string.Empty));
			AssertContainsExactElementsInAnyOrder(new[] { @"\binary\WiseTechGlobal\WTG.WindowMonitoring\Release.zip" }, result);
		}

		Mock<WTG.Foundation.Http.IHttpClientFactory> httpClientFactoryMock;

		class LogRecordComparer : IEqualityComparer<(LogType logType, string)>
		{
			public bool Equals((LogType logType, string) x, (LogType logType, string) y)
			{
				return x.logType == y.logType
						&& Regex.IsMatch(y.Item2, x.Item2);
			}

			public int GetHashCode((LogType logType, string) obj)
			{
				throw new NotImplementedException();
			}
		}
	}
}
