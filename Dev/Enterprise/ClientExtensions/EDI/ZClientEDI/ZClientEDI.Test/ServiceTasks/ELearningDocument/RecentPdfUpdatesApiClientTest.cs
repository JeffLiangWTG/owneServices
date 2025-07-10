using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Client.EDI.ServiceTasks.ELearningDocument;
using Enterprise.Integration;
using Moq;
using NUnit.Framework;

namespace ZClientEDI.Test.ServiceTasks.ELearningDocument
{
	public class RecentPdfUpdatesApiClientTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestHappyPath()
		{
			// Arrange
			var debugLog = new List<string>();
			var warningLog = new List<string>();
			var infoLog = new List<string>();
			var errorLog = new List<string>();
			var mockLogger = new Mock<ILogger>();
			mockLogger.Setup(m => m.Log(It.IsAny<LogType>(), It.IsAny<string>()))
				.Callback((LogType logType, string s) =>
				{
					switch (logType)
					{
						case LogType.Debug:
							debugLog.Add(s);
							break;
						case LogType.Warning:
							warningLog.Add(s);
							break;
						case LogType.Information:
							infoLog.Add(s);
							break;
						case LogType.Error:
							errorLog.Add(s);
							break;
					}
				});

			var jsonRsp = System.Text.Encoding.Default.GetString(Assembly.GetExecutingAssembly()
					.GetManifestResourceStream(
						"ZClientEDI.Test.ServiceTasks.ELearningDocument.Data.ElearningDocumentDescriptionBigPayload.json")
					.ToByteArray());
			var client = CreateClientForTest(mockLogger.Object, jsonRsp);
			// Act
			var datetime = new ZDateTime(2000, 1, 1);
			var result = client.DownloadListOfChanges(datetime);
			// Assert
			Assert("log info message are added", infoLog.Any());
			Assert("log error message should be empty", !errorLog.Any());
			Assert("log warningLog message should be empty", !warningLog.Any());
			Assert("log debugLog message should be empty", !debugLog.Any());
			Assert("eLearning document description should exists", result.Any());
		}

		[ExpectNoExceptions]
		public void TestHappyPathPassNullLogsHandlingCorrectly()
		{
			// Arrange
			var recentPdfUpdatesApiClient = CreateClientForTest(null, "[{\"id\":\"1\"}]");
			// Act
			var datetime = new ZDateTime(2000, 1, 1);
			var result = recentPdfUpdatesApiClient.DownloadListOfChanges(datetime);
			// Assert
			Assert("eLearning document description should exists", result.Any());
		}

		[ExpectNoExceptions]
		public void TestHappyPathPassNullLogsTimeFromFutureHandlingCorrectly()
		{
			// Arrange
			var recentPdfUpdatesApiClient = CreateClientForTest(null, "[]");
			// Act
			var datetime = new ZDateTime(DateTime.Now.Year + 1, 1, 1);
			var result = recentPdfUpdatesApiClient.DownloadListOfChanges(datetime);
			// Assert
			Assert("eLearning document description should not exists", !result.Any());
		}

		static IRecentPdfUpdatesApiClient CreateClientForTest(ILogger logger, string jsonResponse)
		{
			var mockHttpWebResponse = new Mock<HttpWebResponse>();
			mockHttpWebResponse.Setup(o => o.StatusCode).Returns(HttpStatusCode.OK);
			mockHttpWebResponse.Setup(o => o.ToString()).Returns("mockHttpWebResponse");
			mockHttpWebResponse.Setup(o => o.GetResponseStream()).Returns(new MemoryStream(Encoding.UTF8.GetBytes(jsonResponse)));

			// Set private field behind CharacterSet property
			var prop = typeof(HttpWebResponse).GetField("m_CharacterSet", BindingFlags.NonPublic | BindingFlags.Instance);
			prop.SetValue(mockHttpWebResponse.Object, "utf-8");

			// Set private field used in CharacterSet getter
			prop = typeof(HttpWebResponse).GetField("m_HttpResponseHeaders", BindingFlags.NonPublic | BindingFlags.Instance);
			prop.SetValue(mockHttpWebResponse.Object, new WebHeaderCollection());

			var mockHttpWebRequest = new Mock<HttpWebRequest>();
			mockHttpWebRequest.Setup(o => o.GetResponse()).Returns(mockHttpWebResponse.Object);

			var mockRecentPdfUpdatesApiClient = new Mock<RecentPdfUpdatesApiClient>(logger);
			mockRecentPdfUpdatesApiClient.Setup(o => o.CreateHttpWebRequest(It.IsAny<string>())).Returns(mockHttpWebRequest.Object);

			return mockRecentPdfUpdatesApiClient.Object;
		}

		[ExpectNoExceptions]
		public void TestDownloadOtherElearningDocumentChangesNullPath()
		{
			// Arrange
			var recentPdfUpdatesApiClient = new RecentPdfUpdatesApiClient(null);
			// Act
			var datetime = new ZDateTime(DateTime.Now.Year + 1, 1, 1);
			var result = recentPdfUpdatesApiClient.DownloadOtherElearningDocumentChanges(datetime, null);
			// Assert
			Assert("result should be empty", !result.Any());
		}

		[ExpectNoExceptions]
		public void TestDownloadOtherElearningDocumentChangesEmptyPath()
		{
			// Arrange
			var recentPdfUpdatesApiClient = new RecentPdfUpdatesApiClient(null);
			// Act
			var datetime = new ZDateTime(DateTime.Now.Year + 1, 1, 1);
			var result = recentPdfUpdatesApiClient.DownloadOtherElearningDocumentChanges(datetime, string.Empty);
			// Assert
			Assert("result should be empty", !result.Any());
		}
	}
}
