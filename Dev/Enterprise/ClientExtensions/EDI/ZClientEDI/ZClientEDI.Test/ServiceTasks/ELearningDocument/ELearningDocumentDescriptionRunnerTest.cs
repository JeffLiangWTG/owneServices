using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Client.EDI.ServiceTasks.ELearningDocument;
using Enterprise.Integration;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using ZClientEDI.Business.IncidentManager.ELearningDocument.Business;

namespace Enterprise.Client.EDI.Test
{
	public class ELearningDocumentDescriptionRunnerTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		[UseSnapshotProtection(skipTransaction: true)]
		public void TestsRunEndToEnd()
		{
			// Arrange
			var debugLog = new List<string>();
			var warningLog = new List<string>();
			var infoLog = new List<string>();
			var errorLog = new List<string>();

			Db.Connection.ExecuteNonQuery($"DELETE FROM {AutoELearningDocumentTfIdf.Schema.TableName}");
			Db.Connection.ExecuteNonQuery($"DELETE FROM {AutoELearningDocumentDescription.Schema.TableName}");

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
			var cancellationTokenSource = new CancellationToken();
			// Act
			new ELearningDocumentDescriptionRunner(mockLogger.Object).Run(cancellationTokenSource, new RecentPdfUpdatesApiClientForTest(mockLogger.Object));
			// Assert
			Assert("log info message are added", infoLog.Any());
		}

		[ExpectNoExceptions]
		[StressTest]
		[UseSnapshotProtection(skipTransaction: true)]
		public void TestRunTestSamplePayloadSuccessfully()
		{
			// Arrange
			var debugLog = new List<string>();
			var warningLog = new List<string>();
			var infoLog = new List<string>();
			var errorLog = new List<string>();

			Db.Connection.ExecuteNonQuery($"DELETE FROM {AutoELearningDocumentTfIdf.Schema.TableName}");
			Db.Connection.ExecuteNonQuery($"DELETE FROM {AutoELearningDocumentDescription.Schema.TableName}");
			var mockRecentPdfUpdatesApiClient = new Mock<IRecentPdfUpdatesApiClient>();
			mockRecentPdfUpdatesApiClient.Setup(m => m.DownloadListOfChanges(It.IsAny<ZDateTime>())).Returns(
				JsonConvert.DeserializeObject<List<ELearningDocumentFileDescription>>(System.Text.Encoding.Default.GetString(Assembly.GetExecutingAssembly()
					.GetManifestResourceStream(
						"ZClientEDI.Test.ServiceTasks.ELearningDocument.Data.ElearningDocumentDescriptionBigPayload.json")
					.ToByteArray())));

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
			var cancellationTokenSource = new CancellationToken();
			// Act
			new ELearningDocumentDescriptionRunner(mockLogger.Object).Run(cancellationTokenSource, mockRecentPdfUpdatesApiClient.Object);
			// Assert
			//Assert("log info message are added", infoLog.Any());
			var cmd = Db.Connection.Command($@"SELECT count(1) from {AutoELearningDocumentDescription.Schema.TableName}");
			using (var reader = cmd.ExecuteReader())
			{
				reader.Read();
				var actualNumOfRecords = (int)reader.GetValue(0);
				Assert($"2299 records should  be in {AutoELearningDocumentDescription.Schema.TableName} table", 2299 == actualNumOfRecords);
			}
		}

		[ExpectException(typeof(ArgumentNullException))]
		[StressTest]
		[UseSnapshotProtection(skipTransaction: true)]
		public void TestRunTestNoRecordsCreatedprdUpdatesClientIsNull()
		{
			// Arrange
			var debugLog = new List<string>();
			var warningLog = new List<string>();
			var infoLog = new List<string>();
			var errorLog = new List<string>();

			Db.Connection.ExecuteNonQuery($"DELETE FROM {AutoELearningDocumentTfIdf.Schema.TableName}");
			Db.Connection.ExecuteNonQuery($"DELETE FROM {AutoELearningDocumentDescription.Schema.TableName}");
			var mockRecentPdfUpdatesApiClient = new Mock<IRecentPdfUpdatesApiClient>();
			mockRecentPdfUpdatesApiClient.Setup(m => m.DownloadListOfChanges(It.IsAny<ZDateTime>())).Returns(
				JsonConvert.DeserializeObject<List<ELearningDocumentFileDescription>>(System.Text.Encoding.Default.GetString(Assembly.GetExecutingAssembly()
					.GetManifestResourceStream(
						"ZClientEDI.Test.ServiceTasks.ELearningDocument.Data.ElearningDocumentDescriptionBigPayload.json")
					.ToByteArray())));

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
			var cancellationTokenSource = new CancellationToken();
			// Act
			new ELearningDocumentDescriptionRunner(mockLogger.Object).Run(cancellationTokenSource, null);
		}

		[ExpectNoExceptions]
		[StressTest]
		[UseSnapshotProtection(skipTransaction: true)]
		public void TestRunTestSamplePayloadSuccessfullyLogIsNull()
		{
			// Arrange
			Db.Connection.ExecuteNonQuery($"DELETE FROM {AutoELearningDocumentTfIdf.Schema.TableName}");
			Db.Connection.ExecuteNonQuery($"DELETE FROM {AutoELearningDocumentDescription.Schema.TableName}");
			var mockRecentPdfUpdatesApiClient = new Mock<IRecentPdfUpdatesApiClient>();
			mockRecentPdfUpdatesApiClient.Setup(m => m.DownloadListOfChanges(It.IsAny<ZDateTime>())).Returns(
				JsonConvert.DeserializeObject<List<ELearningDocumentFileDescription>>(System.Text.Encoding.Default.GetString(Assembly.GetExecutingAssembly()
					.GetManifestResourceStream(
						"ZClientEDI.Test.ServiceTasks.ELearningDocument.Data.ElearningDocumentDescriptionBigPayload.json")
					.ToByteArray())));
			var cancellationTokenSource = new CancellationToken();
			// Act
			new ELearningDocumentDescriptionRunner(null).Run(cancellationTokenSource, mockRecentPdfUpdatesApiClient.Object);
			var cmd = Db.Connection.Command($@"SELECT count(1) from {AutoELearningDocumentDescription.Schema.TableName}");
			// Assert
			using (var reader = cmd.ExecuteReader())
			{
				reader.Read();
				var actualNumOfRecords = (int)reader.GetValue(0);
				Assert($"2299 records should  be in {AutoELearningDocumentDescription.Schema.TableName} table", 2299 == actualNumOfRecords);
			}
		}

		class RecentPdfUpdatesApiClientForTest : RecentPdfUpdatesApiClient
		{
			public RecentPdfUpdatesApiClientForTest(ILogger logger) : base(logger)
			{
			}

			public override HttpWebRequest CreateHttpWebRequest(string requestUriString)
			{
				var mockRequest = new Mock<HttpWebRequest>();
				mockRequest.Setup(req => req.GetResponse()).Returns(() =>
				{
					return CreateMockHttpWebResponse();
				});

				return mockRequest.Object;
			}

			HttpWebResponse CreateMockHttpWebResponse()
			{
				var responseStream = new MemoryStream(Encoding.UTF8.GetBytes((System.Text.Encoding.UTF8.GetString(Assembly.GetExecutingAssembly()
					.GetManifestResourceStream(
						"ZClientEDI.Test.ServiceTasks.ELearningDocument.Data.ElearningDocumentDescriptionBigPayload.json")
					.ToByteArray()))));

				var mockResponse = new Mock<HttpWebResponse>();
				mockResponse.Setup(res => res.GetResponseStream()).Returns(responseStream);
				mockResponse.Setup(res => res.StatusCode).Returns(HttpStatusCode.OK);
				var result = mockResponse.Object;
				typeof(HttpWebResponse).GetField("m_CharacterSet", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(result, "UTF-8");
				typeof(HttpWebResponse).GetField("m_HttpResponseHeaders", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(result, new WebHeaderCollection());
				return result;
			}
		}
	}
}
