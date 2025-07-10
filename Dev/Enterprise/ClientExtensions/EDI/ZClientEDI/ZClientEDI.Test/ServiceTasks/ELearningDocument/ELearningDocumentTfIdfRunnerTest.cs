using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.Client.EDI.ServiceTasks.ELearningDocument;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ZClientEDI.Business.IncidentManager.ELearningDocument.Business;

namespace Enterprise.Client.EDI.Test
{
	public class ELearningDocumentTfIdfRunnerTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		[StressTest]
		[UseSnapshotProtection(skipTransaction: true)]
		public void RunEndToEnd()
		{
			// Arrange
			var debugLog = new List<string>();
			var warningLog = new List<string>();
			var infoLog = new List<string>();
			var errorLog = new List<string>();

			Db.Connection.ExecuteNonQuery($"DELETE FROM {AutoELearningDocumentTfIdf.Schema.TableName}");
			Db.Connection.ExecuteNonQuery($"DELETE FROM {AutoELearningDocumentDescription.Schema.TableName}");
			Db.Connection.ExecuteNonQuery($"DELETE FROM {StmDataSchema.Constants.SqlSchemaName}.{StmDataSchema.Constants.TableName}");
			Db.Connection.ExecuteNonQuery($"INSERT INTO {StmDataSchema.Constants.SqlSchemaName}.{StmDataSchema.Constants.TableName}" +
				$"([{AutoStmData.Schema.PK}]" +
				$",[{AutoStmData.Schema.SD_Name}]" +
				$",[{AutoStmData.Schema.SD_Owner}]" +
				$",[{AutoStmData.Schema.SD_DepartmentGuid}]" +
				$",[{AutoStmData.Schema.SD_Type}]" +
				$",[{AutoStmData.Schema.SD_IsLogged}]" +
				$",[{AutoStmData.Schema.SD_BinaryValue}]" +
				$",[{AutoStmData.Schema.SD_GuidValue}]" +
				$",[{AutoStmData.Schema.SD_IsCancelled}]" +
				$",[{AutoStmData.Schema.SD_PreserveTestValue}])" +
				"VALUES" +
				"('5C1185A9-1960-488F-9E54-5BF0463B112D'" +
				", 'IncidentSimilarity_LatestVersion'" +
				", null" +
				", null" +
				", '   '" +
				", 0" +
				", 0x01000000" +
				", null" +
				", 0" +
				", 0)");

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
			new ELearningDocumentDescriptionRunner(mockLogger.Object).Run(cancellationTokenSource, new RecentPdfUpdatesApiClient(mockLogger.Object));
			Assert("log info message are added", infoLog.Any());
			Db.Connection.ExecuteNonQuery($"DELETE FROM {AutoELearningDocumentTfIdf.Schema.TableName}");
			infoLog.Clear();
			new ELearningDocumentTfIdfRunner().Run(cancellationTokenSource,
				mockLogger.Object,
				new MyAccountShareFolderClient(mockLogger.Object),
				(ILogger logger) => new PdfTextExtractor(logger),
				new TfIdfTransformer());
			// Assert
			Assert("log info message are added", infoLog.Any());
		}

		[ExpectException(typeof(InvalidOperationException))]
		[StressTest]
		[UseSnapshotProtection(skipTransaction: true)]
		public void TestELearningDocumentTfIdfRunnerExpectedExceptionDueToNoStmDataVersion()
		{
			// Arrange
			var debugLog = new List<string>();
			var warningLog = new List<string>();
			var infoLog = new List<string>();
			var errorLog = new List<string>();

			Db.Connection.ExecuteNonQuery($"DELETE FROM {AutoELearningDocumentTfIdf.Schema.TableName}");
			Db.Connection.ExecuteNonQuery($"DELETE FROM {AutoELearningDocumentDescription.Schema.TableName}");
			Db.Connection.ExecuteNonQuery($"DELETE FROM {StmDataSchema.Constants.SqlSchemaName}.{StmDataSchema.Constants.TableName}");
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
			var mockMyAccountClient = new Mock<IMyAccountClient>();
			mockMyAccountClient.Setup(m => m.DownloadFile(It.IsAny<string>())).Returns(Assembly
				.GetExecutingAssembly()
				.GetManifestResourceStream("ZClientEDI.Test.ServiceTasks.ELearningDocument.Data.1ACC001.pdf")
				.ToByteArray());
			mockMyAccountClient.Setup(m => m.UrlToNetworkPath(It.IsAny<string>())).Returns("folder\\file.pdf");
			var cancellationTokenSource = new CancellationToken();
			// Act
			new ELearningDocumentDescriptionRunner(mockLogger.Object).Run(cancellationTokenSource, new RecentPdfUpdatesApiClient(mockLogger.Object));
			Assert("log info message are added", infoLog.Any());
			infoLog.Clear();
			new ELearningDocumentTfIdfRunner().Run(cancellationTokenSource,
				mockLogger.Object,
				mockMyAccountClient.Object,
				(ILogger logger) => new PdfTextExtractor(logger),
				new TfIdfTransformer());
			Assert("log info message are added", !infoLog.Any());
			var cmd = Db.Connection.Command($@"SELECT count(1) from {AutoELearningDocumentTfIdf.Schema.TableName}");
			// Assert
			using (var reader = cmd.ExecuteReader())
			{
				reader.Read();
				var actualNumOfRecords = (int)reader.GetValue(0);
				Assert("0 number of records should in ELearningDocumentTfIdf table", 0 == actualNumOfRecords);
			}
		}

		[ExpectNoExceptions]
		[StressTest]
		[UseSnapshotProtection(skipTransaction: true)]
		public void TestELearningDocumentTfIdfRunnernoRecordsInserted()
		{
			// Arrange
			var debugLog = new List<string>();
			var warningLog = new List<string>();
			var infoLog = new List<string>();
			var errorLog = new List<string>();

			Db.Connection.ExecuteNonQuery($"DELETE FROM {AutoELearningDocumentTfIdf.Schema.TableName}");
			Db.Connection.ExecuteNonQuery($"DELETE FROM {AutoELearningDocumentDescription.Schema.TableName}");
			Db.Connection.ExecuteNonQuery($"DELETE FROM {StmDataSchema.Constants.SqlSchemaName}.{StmDataSchema.Constants.TableName}");
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
			new ELearningDocumentTfIdfRunner().Run(cancellationTokenSource,
				mockLogger.Object,
				new MyAccountShareFolderClient(mockLogger.Object),
				(ILogger logger) => new PdfTextExtractor(logger),
				new TfIdfTransformer());
			Assert("log info message are added", !infoLog.Any());
			var cmd = Db.Connection.Command($@"SELECT count(1) from {AutoELearningDocumentTfIdf.Schema.TableName}");
			// Assert
			using (var reader = cmd.ExecuteReader())
			{
				reader.Read();
				var actualNumOfRecords = (int)reader.GetValue(0);
				Assert("0 number of records should in ELearningDocumentTfIdf table", 0 == actualNumOfRecords);
			}
		}

		[ExpectNoExceptions]
		[StressTest]
		[UseSnapshotProtection(skipTransaction: true)]
		public void TestELearningDocumentTfIdfRunnerPassNullLogger()
		{
			// Arrange
			var debugLog = new List<string>();
			var warningLog = new List<string>();
			var infoLog = new List<string>();
			var errorLog = new List<string>();
			Db.Connection.ExecuteNonQuery($"DELETE FROM {AutoELearningDocumentTfIdf.Schema.TableName}");
			Db.Connection.ExecuteNonQuery($"DELETE FROM {AutoELearningDocumentDescription.Schema.TableName}");
			Db.Connection.ExecuteNonQuery($"DELETE FROM {StmDataSchema.Constants.SqlSchemaName}.{StmDataSchema.Constants.TableName}");

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
			new ELearningDocumentTfIdfRunner().Run(cancellationTokenSource,
				null,
				new MyAccountShareFolderClient(mockLogger.Object),
				(ILogger logger) => new PdfTextExtractor(logger),
				new TfIdfTransformer());
			Assert("log info message are added", !infoLog.Any());
			var cmd = Db.Connection.Command($@"SELECT count(1) from {AutoELearningDocumentTfIdf.Schema.TableName}");
			// Assert
			using (var reader = cmd.ExecuteReader())
			{
				reader.Read();
				var actualNumOfRecords = (int)reader.GetValue(0);
				Assert("0 number of records should in ELearningDocumentTfIdf table", 0 == actualNumOfRecords);
			}
		}

		[ExpectNoExceptions]
		[StressTest]
		[UseSnapshotProtection(skipTransaction: true)]
		public void TestELearningDocumentTfIdfRunnerDbConnectionWrapperPassNullLogger()
		{
			// Arrange
			var debugLog = new List<string>();
			var warningLog = new List<string>();
			var infoLog = new List<string>();
			var errorLog = new List<string>();
			Db.Connection.ExecuteNonQuery($"DELETE FROM {AutoELearningDocumentTfIdf.Schema.TableName}");
			Db.Connection.ExecuteNonQuery($"DELETE FROM {AutoELearningDocumentDescription.Schema.TableName}");
			Db.Connection.ExecuteNonQuery($"DELETE FROM {StmDataSchema.Constants.SqlSchemaName}.{StmDataSchema.Constants.TableName}");

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
			new ELearningDocumentTfIdfRunner().Run(cancellationTokenSource,
				null,
				new MyAccountShareFolderClient(mockLogger.Object),
				(ILogger logger) => new PdfTextExtractor(logger),
				new TfIdfTransformer());
			// Assert
			Assert("log info message are added", !infoLog.Any());
			var cmd = Db.Connection.Command($@"SELECT count(1) from {AutoELearningDocumentTfIdf.Schema.TableName}");
			using (var reader = cmd.ExecuteReader())
			{
				reader.Read();
				var actualNumOfRecords = (int)reader.GetValue(0);
				Assert("0 number of records should in ELearningDocumentTfIdf table", 0 == actualNumOfRecords);
			}
		}

		[ExpectNoExceptions]
		[StressTest]
		[UseSnapshotProtection(skipTransaction: true)]
		public void TestELearningDocumentTfIdfRunnerMyAccountShareFolderClientPassNullLogger()
		{
			// Arrange
			Db.Connection.ExecuteNonQuery($"DELETE FROM {AutoELearningDocumentTfIdf.Schema.TableName}");
			Db.Connection.ExecuteNonQuery($"DELETE FROM {AutoELearningDocumentDescription.Schema.TableName}");
			Db.Connection.ExecuteNonQuery($"DELETE FROM {StmDataSchema.Constants.SqlSchemaName}.{StmDataSchema.Constants.TableName}");

			var cancellationTokenSource = new CancellationToken();
			// Act
			new ELearningDocumentTfIdfRunner().Run(cancellationTokenSource,
				null,
				new MyAccountShareFolderClient(null),
				(ILogger logger) => new PdfTextExtractor(logger),
				new TfIdfTransformer());
			var cmd = Db.Connection.Command($@"SELECT count(1) from {AutoELearningDocumentTfIdf.Schema.TableName}");
			// Assert
			using (var reader = cmd.ExecuteReader())
			{
				reader.Read();
				var actualNumOfRecords = (int)reader.GetValue(0);
				Assert("0 number of records should in ELearningDocumentTfIdf table", 0 == actualNumOfRecords);
			}
		}

		[ExpectNoExceptions]
		[StressTest]
		[UseSnapshotProtection(skipTransaction: true)]
		public void ELearningDocumentTfIdfRunnerProcessSingleRecord()
		{
			// Arrange
			var debugLog = new List<string>();
			var warningLog = new List<string>();
			var infoLog = new List<string>();
			var errorLog = new List<string>();

			Db.Connection.ExecuteNonQuery($"DELETE FROM {AutoELearningDocumentTfIdf.Schema.TableName}");
			Db.Connection.ExecuteNonQuery($"DELETE FROM {AutoELearningDocumentDescription.Schema.TableName}");
			Db.Connection.ExecuteNonQuery($"DELETE FROM {StmDataSchema.Constants.SqlSchemaName}.{StmDataSchema.Constants.TableName}");
			Db.Connection.ExecuteNonQuery($"INSERT INTO {StmDataSchema.Constants.SqlSchemaName}.{StmDataSchema.Constants.TableName}" +
				$"([{AutoStmData.Schema.PK}]" +
				$",[{AutoStmData.Schema.SD_Name}]" +
				$",[{AutoStmData.Schema.SD_Owner}]" +
				$",[{AutoStmData.Schema.SD_DepartmentGuid}]" +
				$",[{AutoStmData.Schema.SD_Type}]" +
				$",[{AutoStmData.Schema.SD_IsLogged}]" +
				$",[{AutoStmData.Schema.SD_BinaryValue}]" +
				$",[{AutoStmData.Schema.SD_GuidValue}]" +
				$",[{AutoStmData.Schema.SD_IsCancelled}]" +
				$",[{AutoStmData.Schema.SD_PreserveTestValue}])" +
				"VALUES" +
				"('5C1185A9-1960-488F-9E54-5BF0463B112D'" +
				", 'IncidentSimilarity_LatestVersion'" +
				", null" +
				", null" +
				", '   '" +
				", 0" +
				", 0x01000000" +
				", null" +
				", 0" +
				", 0)");
			Db.Connection.ExecuteNonQuery($"INSERT INTO[dbo].{AutoELearningDocumentDescription.Schema.TableName}" +
				$"([{AutoELearningDocumentDescription.Schema.PK}]" +
				$",[{AutoELearningDocumentDescription.Schema.ELD_MyAccountDocumentId}]" +
				$",[{AutoELearningDocumentDescription.Schema.ELD_Title}]" +
				$",[{AutoELearningDocumentDescription.Schema.ELD_Url}]" +
				$",[{AutoELearningDocumentDescription.Schema.ELD_DocumentType}]" +
				$",[{AutoELearningDocumentDescription.Schema.ELD_DocumentLastModified}])" +
				"VALUES" +
				"('DDAD92F9-21B6-4A48-B88F-0007B8F1EF9A'" +
				", '6636E51A-251E-471A-B282-168B62AF9A28'" +
				", 'mock1'" +
				", 'http://localhost.com/mock1.pdf'" +
				", 'Workbook'" +
				", '2020-11-10 01:03:31.260')");
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
			var mockMyAccountClient = new Mock<IMyAccountClient>();
			mockMyAccountClient.Setup(m => m.DownloadFile(It.IsAny<string>())).Returns(Assembly
				.GetExecutingAssembly()
				.GetManifestResourceStream("ZClientEDI.Test.ServiceTasks.ELearningDocument.Data.1ACC001.pdf")
				.ToByteArray());
			mockMyAccountClient.Setup(m => m.UrlToNetworkPath(It.IsAny<string>())).Returns("folder\\file.pdf");
			var cancellationTokenSource = new CancellationToken();
			// Act
			new ELearningDocumentTfIdfRunner().Run(cancellationTokenSource,
				mockLogger.Object,
				mockMyAccountClient.Object,
				(ILogger logger) => new PdfTextExtractor(logger),
				new TfIdfTransformer());
			// Assert
			Assert("log info message are added", infoLog.Any());
			var cmd = Db.Connection.Command($@"SELECT count(1) from {AutoELearningDocumentTfIdf.Schema.TableName}");
			using (var reader = cmd.ExecuteReader())
			{
				reader.Read();
				var actualNumOfRecords = (int)reader.GetValue(0);
				Assert($"1 record should  be in ELearningDocumentTfIdf table, actual: {actualNumOfRecords}", 1 == actualNumOfRecords);
			}
		}

		[ExpectNoExceptions]
		[StressTest]
		[UseSnapshotProtection(skipTransaction: true)]
		public void ELearningDocumentTfIdfRunnerProcessSingleRecordLoggerIsNull()
		{
			// Arrange
			Db.Connection.ExecuteNonQuery($"DELETE FROM {AutoELearningDocumentTfIdf.Schema.TableName}");
			Db.Connection.ExecuteNonQuery($"DELETE FROM {AutoELearningDocumentDescription.Schema.TableName}");
			Db.Connection.ExecuteNonQuery($"DELETE FROM {StmDataSchema.Constants.SqlSchemaName}.{StmDataSchema.Constants.TableName}");
			Db.Connection.ExecuteNonQuery($"INSERT INTO {StmDataSchema.Constants.SqlSchemaName}.{StmDataSchema.Constants.TableName}" +
				$"([{AutoStmData.Schema.PK}]" +
				$",[{AutoStmData.Schema.SD_Name}]" +
				$",[{AutoStmData.Schema.SD_Owner}]" +
				$",[{AutoStmData.Schema.SD_DepartmentGuid}]" +
				$",[{AutoStmData.Schema.SD_Type}]" +
				$",[{AutoStmData.Schema.SD_IsLogged}]" +
				$",[{AutoStmData.Schema.SD_BinaryValue}]" +
				$",[{AutoStmData.Schema.SD_GuidValue}]" +
				$",[{AutoStmData.Schema.SD_IsCancelled}]" +
				$",[{AutoStmData.Schema.SD_PreserveTestValue}])" +
				"VALUES" +
				"('5C1185A9-1960-488F-9E54-5BF0463B112D'" +
				", 'IncidentSimilarity_LatestVersion'" +
				", null" +
				", null" +
				", '   '" +
				", 0" +
				", 0x01000000" +
				", null" +
				", 0" +
				", 0)");
			Db.Connection.ExecuteNonQuery($"INSERT INTO[dbo].{AutoELearningDocumentDescription.Schema.TableName}" +
				$"([{AutoELearningDocumentDescription.Schema.PK}]" +
				$",[{AutoELearningDocumentDescription.Schema.ELD_MyAccountDocumentId}]" +
				$",[{AutoELearningDocumentDescription.Schema.ELD_Title}]" +
				$",[{AutoELearningDocumentDescription.Schema.ELD_Url}]" +
				$",[{AutoELearningDocumentDescription.Schema.ELD_DocumentType}]" +
				$",[{AutoELearningDocumentDescription.Schema.ELD_DocumentLastModified}])" +
				"VALUES" +
				"('DDAD92F9-21B6-4A48-B88F-0007B8F1EF9A'" +
				", '6636E51A-251E-471A-B282-168B62AF9A28'" +
				", 'mock1'" +
				", 'https://localhost.com/mock1.pdf'" +
				", 'Workbook'" +
				", '2020-11-10 01:03:31.260')");
			var mockMyAccountClient = new Mock<IMyAccountClient>();
			mockMyAccountClient.Setup(m => m.DownloadFile(It.IsAny<string>())).Returns(Assembly
				.GetExecutingAssembly()
				.GetManifestResourceStream("ZClientEDI.Test.ServiceTasks.ELearningDocument.Data.1ACC001.pdf")
				.ToByteArray());
			mockMyAccountClient.Setup(m => m.UrlToNetworkPath(It.IsAny<string>())).Returns("file.pdf");
			var cancellationTokenSource = new CancellationToken();
			// Act
			new ELearningDocumentTfIdfRunner().Run(cancellationTokenSource,
				null,
				mockMyAccountClient.Object,
				(ILogger logger) => new PdfTextExtractor(logger),
				new TfIdfTransformer());
			var cmd = Db.Connection.Command($@"SELECT count(1) from {AutoELearningDocumentTfIdf.Schema.TableName}");
			// Assert
			using (var reader = cmd.ExecuteReader())
			{
				reader.Read();
				var actualNumOfRecords = (int)reader.GetValue(0);
				Assert("1 record should  be in ELearningDocumentTfIdf table", 1 == actualNumOfRecords);
			}
		}

		[ExpectException(typeof(ArgumentNullException))]
		[StressTest]
		[UseSnapshotProtection(skipTransaction: true)]
		public void TestELearningDocumentTfIdfRunnerProcessNoRecordsinsertedIMyAccountClientIsNull()
		{
			// Arrange
			Db.Connection.ExecuteNonQuery($"DELETE FROM {AutoELearningDocumentTfIdf.Schema.TableName}");
			Db.Connection.ExecuteNonQuery($"DELETE FROM {AutoELearningDocumentDescription.Schema.TableName}");
			Db.Connection.ExecuteNonQuery($"DELETE FROM {StmDataSchema.Constants.SqlSchemaName}.{StmDataSchema.Constants.TableName}");
			Db.Connection.ExecuteNonQuery($"INSERT INTO {StmDataSchema.Constants.SqlSchemaName}.{StmDataSchema.Constants.TableName}" +
				$"([{AutoStmData.Schema.PK}]" +
				$",[{AutoStmData.Schema.SD_Name}]" +
				$",[{AutoStmData.Schema.SD_Owner}]" +
				$",[{AutoStmData.Schema.SD_DepartmentGuid}]" +
				$",[{AutoStmData.Schema.SD_Type}]" +
				$",[{AutoStmData.Schema.SD_IsLogged}]" +
				$",[{AutoStmData.Schema.SD_BinaryValue}]" +
				$",[{AutoStmData.Schema.SD_GuidValue}]" +
				$",[{AutoStmData.Schema.SD_IsCancelled}]" +
				$",[{AutoStmData.Schema.SD_PreserveTestValue}])" +
				"VALUES" +
				"('5C1185A9-1960-488F-9E54-5BF0463B112D'" +
				", 'IncidentSimilarity_LatestVersion'" +
				", null" +
				", null" +
				", '   '" +
				", 0" +
				", 0x01000000" +
				", null" +
				", 0" +
				", 0)");
			Db.Connection.ExecuteNonQuery($"INSERT INTO[dbo].{AutoELearningDocumentDescription.Schema.TableName}" +
				$"([{AutoELearningDocumentDescription.Schema.PK}]" +
				$",[{AutoELearningDocumentDescription.Schema.ELD_MyAccountDocumentId}]" +
				$",[{AutoELearningDocumentDescription.Schema.ELD_Title}]" +
				$",[{AutoELearningDocumentDescription.Schema.ELD_Url}]" +
				$",[{AutoELearningDocumentDescription.Schema.ELD_DocumentType}]" +
				$",[{AutoELearningDocumentDescription.Schema.ELD_DocumentLastModified}])" +
				"VALUES" +
				"('DDAD92F9-21B6-4A48-B88F-0007B8F1EF9A'" +
				", '6636E51A-251E-471A-B282-168B62AF9A28'" +
				", 'mock1'" +
				", 'mock1.pdf'" +
				", 'Workbook'" +
				", '2020-11-10 01:03:31.260')");
			var cancellationTokenSource = new CancellationToken();
			// Act
			new ELearningDocumentTfIdfRunner().Run(cancellationTokenSource,
				null,
				null,
				(ILogger logger) => new PdfTextExtractor(logger),
				new TfIdfTransformer());
			//Assert
			Assert("Should have DeveloperNotificationException", ErrorReporter.LastMessageReported.Length > 0);
			AssertEquals("myAccount cannot be null, class:ELearningDocumentTfIdfRunner, method:Run", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		[ExpectException(typeof(ArgumentNullException))]
		[StressTest]
		[UseSnapshotProtection(skipTransaction: true)]
		public void TestELearningDocumentTfIdfRunnerProcessNoRecordsinsertedITfIdfTransformerIsNull()
		{
			// Arrange
			Db.Connection.ExecuteNonQuery($"DELETE FROM {AutoELearningDocumentTfIdf.Schema.TableName}");
			Db.Connection.ExecuteNonQuery($"DELETE FROM {AutoELearningDocumentDescription.Schema.TableName}");
			Db.Connection.ExecuteNonQuery($"DELETE FROM {StmDataSchema.Constants.SqlSchemaName}.{StmDataSchema.Constants.TableName}");
			Db.Connection.ExecuteNonQuery($"INSERT INTO {StmDataSchema.Constants.SqlSchemaName}.{StmDataSchema.Constants.TableName}" +
				$"([{AutoStmData.Schema.PK}]" +
				$",[{AutoStmData.Schema.SD_Name}]" +
				$",[{AutoStmData.Schema.SD_Owner}]" +
				$",[{AutoStmData.Schema.SD_DepartmentGuid}]" +
				$",[{AutoStmData.Schema.SD_Type}]" +
				$",[{AutoStmData.Schema.SD_IsLogged}]" +
				$",[{AutoStmData.Schema.SD_BinaryValue}]" +
				$",[{AutoStmData.Schema.SD_GuidValue}]" +
				$",[{AutoStmData.Schema.SD_IsCancelled}]" +
				$",[{AutoStmData.Schema.SD_PreserveTestValue}])" +
				"VALUES" +
				"('5C1185A9-1960-488F-9E54-5BF0463B112D'" +
				", 'IncidentSimilarity_LatestVersion'" +
				", null" +
				", null" +
				", '   '" +
				", 0" +
				", 0x01000000" +
				", null" +
				", 0" +
				", 0)");
			Db.Connection.ExecuteNonQuery($"INSERT INTO[dbo].{AutoELearningDocumentDescription.Schema.TableName}" +
				$"([{AutoELearningDocumentDescription.Schema.PK}]" +
				$",[{AutoELearningDocumentDescription.Schema.ELD_MyAccountDocumentId}]" +
				$",[{AutoELearningDocumentDescription.Schema.ELD_Title}]" +
				$",[{AutoELearningDocumentDescription.Schema.ELD_Url}]" +
				$",[{AutoELearningDocumentDescription.Schema.ELD_DocumentType}]" +
				$",[{AutoELearningDocumentDescription.Schema.ELD_DocumentLastModified}])" +
				"VALUES" +
				"('DDAD92F9-21B6-4A48-B88F-0007B8F1EF9A'" +
				", '6636E51A-251E-471A-B282-168B62AF9A28'" +
				", 'mock1'" +
				", 'mock1.pdf'" +
				", 'Workbook'" +
				", '2020-11-10 01:03:31.260')");
			var mockMyAccountClient = new Mock<IMyAccountClient>();
			mockMyAccountClient.Setup(m => m.DownloadFile(It.IsAny<string>())).Returns(Assembly
				.GetExecutingAssembly()
				.GetManifestResourceStream("ZClientEDI.Test.ServiceTasks.ELearningDocument.Data.1ACC001.pdf")
				.ToByteArray());
			mockMyAccountClient.Setup(m => m.UrlToNetworkPath(It.IsAny<string>())).Returns("folder\\file.pdf");
			var cancellationTokenSource = new CancellationToken();
			// Acts
			new ELearningDocumentTfIdfRunner().Run(cancellationTokenSource,
				null,
				mockMyAccountClient.Object,
				(ILogger logger) => new PdfTextExtractor(logger),
				null);

			//Assert
			Assert("Should have DeveloperNotificationException", ErrorReporter.LastMessageReported.Length > 0);
			AssertEquals("tfIdfTransformer cannot be null, class:ELearningDocumentTfIdfRunner, method:Run", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		[ExpectException(typeof(ArgumentNullException))]
		[StressTest]
		[UseSnapshotProtection(skipTransaction: true)]
		public void TestELearningDocumentTfIdfRunnerProcessNoRecordsInsertedCreatePdfExtractorIsNull()
		{
			// Arrange
			Db.Connection.ExecuteNonQuery($"DELETE FROM {AutoELearningDocumentTfIdf.Schema.TableName}");
			Db.Connection.ExecuteNonQuery($"DELETE FROM {AutoELearningDocumentDescription.Schema.TableName}");
			Db.Connection.ExecuteNonQuery($"DELETE FROM {StmDataSchema.Constants.SqlSchemaName}.{StmDataSchema.Constants.TableName}");
			Db.Connection.ExecuteNonQuery($"INSERT INTO {StmDataSchema.Constants.SqlSchemaName}.{StmDataSchema.Constants.TableName}" +
				$"([{AutoStmData.Schema.PK}]" +
				$",[{AutoStmData.Schema.SD_Name}]" +
				$",[{AutoStmData.Schema.SD_Owner}]" +
				$",[{AutoStmData.Schema.SD_DepartmentGuid}]" +
				$",[{AutoStmData.Schema.SD_Type}]" +
				$",[{AutoStmData.Schema.SD_IsLogged}]" +
				$",[{AutoStmData.Schema.SD_BinaryValue}]" +
				$",[{AutoStmData.Schema.SD_GuidValue}]" +
				$",[{AutoStmData.Schema.SD_IsCancelled}]" +
				$",[{AutoStmData.Schema.SD_PreserveTestValue}])" +
				"VALUES" +
				"('5C1185A9-1960-488F-9E54-5BF0463B112D'" +
				", 'IncidentSimilarity_LatestVersion'" +
				", null" +
				", null" +
				", '   '" +
				", 0" +
				", 0x01000000" +
				", null" +
				", 0" +
				", 0)");
			Db.Connection.ExecuteNonQuery($"INSERT INTO[dbo].{AutoELearningDocumentDescription.Schema.TableName}" +
				$"([{AutoELearningDocumentDescription.Schema.PK}]" +
				$",[{AutoELearningDocumentDescription.Schema.ELD_MyAccountDocumentId}]" +
				$",[{AutoELearningDocumentDescription.Schema.ELD_Title}]" +
				$",[{AutoELearningDocumentDescription.Schema.ELD_Url}]" +
				$",[{AutoELearningDocumentDescription.Schema.ELD_DocumentType}]" +
				$",[{AutoELearningDocumentDescription.Schema.ELD_DocumentLastModified}])" +
				"VALUES" +
				"('DDAD92F9-21B6-4A48-B88F-0007B8F1EF9A'" +
				", '6636E51A-251E-471A-B282-168B62AF9A28'" +
				", 'mock1'" +
				", 'mock1.pdf'" +
				", 'Workbook'" +
				", '2020-11-10 01:03:31.260')");
			var mockMyAccountClient = new Mock<IMyAccountClient>();
			mockMyAccountClient.Setup(m => m.DownloadFile(It.IsAny<string>())).Returns(Assembly
				.GetExecutingAssembly()
				.GetManifestResourceStream("ZClientEDI.Test.ServiceTasks.ELearningDocument.Data.1ACC001.pdf")
				.ToByteArray());
			mockMyAccountClient.Setup(m => m.UrlToNetworkPath(It.IsAny<string>())).Returns("folder\\file.pdf");
			var cancellationTokenSource = new CancellationToken();
			// Act
			new ELearningDocumentTfIdfRunner().Run(cancellationTokenSource,
				null,
				mockMyAccountClient.Object,
				null,
				new TfIdfTransformer());
			//Assert
			Assert("Should have DeveloperNotificationException", ErrorReporter.LastMessageReported.Length > 0);
			AssertEquals("createPdfExtractor cannot be null, class:ELearningDocumentTfIdfRunner, method:Run", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}
	}
}
