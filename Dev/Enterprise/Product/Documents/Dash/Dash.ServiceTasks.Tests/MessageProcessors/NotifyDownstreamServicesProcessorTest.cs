using System;
using System.Linq.Expressions;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Dash.Business;
using Enterprise.Dash.Business.Services;
using Enterprise.Dash.Integration;
using Enterprise.Dash.ServiceTasks.MessageProcessors;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.Integration;
using Enterprise.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Moq;
using NUnit.Framework;
using SharedConstants = WTG.Shared.Dash.Common.Constants;

namespace Enterprise.Dash.ServiceTasks.Tests.MessageProcessors
{
	[TestedType(typeof(NotifyDownstreamServicesMessageProcessor))]
	public class NotifyDownstreamServicesProcessorTest : TestCaseWithFactory
	{
		const int BatchSize = 5;
		const byte RetryCount = 5;
		const string AccountingIntegrationApplicationCode = "DAI";
		const string AccountingIntegrationMessageType = "API";

		public void TestProcessMessages_Ensure_Correct_DashDocument_Status_Set_And_EdiMessage_For_Accounting_Created_For_APInvoice()
		{
			// We want ShipamaxEdiMessage to be created automatically when StorageDoc is saved
			using (DocManagerRegistry.Instance.EnableAccountsPayableInvoiceDocumentParsing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (GlowRegistry.Instance.GlowServiceUriRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://localhost/Glow"))
			{
				// arrange
				var docId = ZGuid.NewZGuid();
				var docToken = ZGuid.NewZGuid();

				var dashDocument = CreateDashDocument(SharedConstants.ParseType.Code.AccountPayableInvoice, docId, docToken);

				var notifyDownstreamServicesEdiMessage = CreateEdiMessage(SharedConstants.DataProcessingType.Code.NotifyDownstreamServices);

				var accountingIntegrationEdiMessage = Factory.NewWithValidTestData<DashDocumentDataMessage>();
				accountingIntegrationEdiMessage.EM_LinkUniqueID = dashDocument.PK;
				accountingIntegrationEdiMessage.EM_LinkTable = dashDocument.TableName;

				var infoLogCounter = 0;
				var errorLogCounter = 0;
				var (factoryMock, loggerMock, shipamaxServiceMock, dashCompletionServiceMock, dashMessageProcessor) = SetupMessageProcessor();

				// We're self-filtering/batching; our Load mechanism ignores the filtering that we're providing, so it's a tiny deceit, but we're not testing the actual Factory
				factoryMock.SetupSequence(x => x.Load<DashDocumentDataMessage>(It.IsAny<ZQuery>()))
					.Returns([notifyDownstreamServicesEdiMessage])
					.Returns(Array.Empty<DashDocumentDataMessage>());

				factoryMock.Setup(x => x.New(It.IsAny<Type>())).Returns(accountingIntegrationEdiMessage);
				factoryMock.Setup(x => x.Load<DashDocument>(It.IsAny<ZGuid>())).Returns(dashDocument);

				loggerMock.Setup(x => x.Log(LogType.Information, It.IsAny<string>())).Callback(() => infoLogCounter++);
				loggerMock.Setup(x => x.Log(LogType.Error, It.IsAny<string>())).Callback(() => errorLogCounter++);

				// act
				dashMessageProcessor.ProcessMessages(CancellationToken.None);

				// assert
				factoryMock.Verify(x => x.Load<DashDocumentDataMessage>(It.Is(ExpectedQueryFilter)));
				factoryMock.Verify(x => x.Save(), Times.Exactly(1));

				AssertLogs(loggerMock, false, notifyDownstreamServicesEdiMessage.PK, infoLogCounter, errorLogCounter);

				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, notifyDownstreamServicesEdiMessage.EM_Status);
				AssertEquals(SharedConstants.ParseStatus.Code.ReadyForThirdPartyProcessing, dashDocument.DDD_ParseStatus);

				AssertEquals(AccountingIntegrationApplicationCode, accountingIntegrationEdiMessage.EM_ApplicationCode);
				AssertEquals(AccountingIntegrationMessageType, accountingIntegrationEdiMessage.EM_MessageType);
				AssertEquals(dashDocument.PK, accountingIntegrationEdiMessage.EM_LinkUniqueID);
				AssertEquals(dashDocument.TableName, accountingIntegrationEdiMessage.EM_LinkTable);
			}
		}

		public void TestProcessMessage_CallsCompletionService_When_ValidationStatusIsPassed_For_CIV()
		{
			// We want ShipamaxEdiMessage to be created automatically when StorageDoc is saved
			using (DocManagerRegistry.Instance.EnableCommercialInvoiceDocumentParsing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (GlowRegistry.Instance.GlowServiceUriRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://localhost/Glow"))
			{
				// arrange
				var ediMessage = CreateEdiMessage(SharedConstants.DataProcessingType.Code.NotifyDownstreamServices);

				var docId = ZGuid.NewZGuid();
				var docToken = ZGuid.NewZGuid();

				var dashDocument = CreateDashDocument(SharedConstants.ParseType.Code.CommercialInvoice, docId, docToken);

				var infoLogCounter = 0;
				var errorLogCounter = 0;
				var (factoryMock, loggerMock, shipamaxServiceMock, dashCompletionServiceMock, dashMessageProcessor) = SetupMessageProcessor();

				// We're self-filtering/batching; our Load mechanism ignores the filtering that we're providing, so it's a tiny deceit, but we're not testing the actual Factory
				factoryMock.SetupSequence(x => x.Load<DashDocumentDataMessage>(It.IsAny<ZQuery>()))
					.Returns([ediMessage])
					.Returns(Array.Empty<DashDocumentDataMessage>());

				factoryMock.Setup(x => x.Load<DashDocument>(It.IsAny<ZGuid>())).Returns(dashDocument);

				loggerMock.Setup(x => x.Log(LogType.Information, It.IsAny<string>())).Callback(() => infoLogCounter++);
				loggerMock.Setup(x => x.Log(LogType.Error, It.IsAny<string>())).Callback(() => errorLogCounter++);

				// act
				dashMessageProcessor.ProcessMessages(CancellationToken.None);

				// assert
				factoryMock.Verify(x => x.Load<DashDocumentDataMessage>(It.Is(ExpectedQueryFilter)));
				factoryMock.Verify(x => x.Save(), Times.Exactly(1));

				AssertLogs(loggerMock, false, ediMessage.PK, infoLogCounter, errorLogCounter);

				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, ediMessage.EM_Status);
				AssertEquals(SharedConstants.ParseStatus.Code.SubmittedForCompletion, dashDocument.DDD_ParseStatus);

				AssertHandleInvoiceBehavior_For_CIV(
						dashCompletionServiceMock,
						shipamaxServiceMock,
						dashDocument,
						docId,
						docToken,
						SharedConstants.ValidationStatus.Code.Passed);
			}
		}

		public void TestProcessMessage_SetsParseStatusToNeedsReview_When_ValidationStatusIsError_For_CIV() =>
			Assert_HandleCommercialInvoice_SetsParseStatusToNeedsReview(SharedConstants.ValidationStatus.Code.Error);

		public void TestProcessMessage_SetsParseStatusToNeedsReview_When_ValidationStatusIsWarning_For_CIV() =>
			Assert_HandleCommercialInvoice_SetsParseStatusToNeedsReview(SharedConstants.ValidationStatus.Code.Warning);

		public void TestProcessMessage_SetsParseStatusToNeedsReview_When_ValidationStatusIsNone_For_CIV() =>
			Assert_HandleCommercialInvoice_SetsParseStatusToNeedsReview(SharedConstants.ValidationStatus.Code.None);

		public void TestProcessMessage_CatchesException_When_CompletionServiceThrowsException_For_CIV()
		{
			// We want ShipamaxEdiMessage to be created automatically when StorageDoc is saved
			using (DocManagerRegistry.Instance.EnableCommercialInvoiceDocumentParsing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (GlowRegistry.Instance.GlowServiceUriRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://localhost/Glow"))
			{
				// arrange
				var ediMessage = CreateEdiMessage(SharedConstants.DataProcessingType.Code.NotifyDownstreamServices);

				var docId = ZGuid.NewZGuid();
				var docToken = ZGuid.NewZGuid();

				var dashDocument = CreateDashDocument(SharedConstants.ParseType.Code.CommercialInvoice, docId, docToken);

				var infoLogCounter = 0;
				var errorLogCounter = 0;
				var (factoryMock, loggerMock, shipamaxServiceMock, dashCompletionServiceMock, dashMessageProcessor) = SetupMessageProcessor();

				// We're self-filtering/batching; our Load mechanism ignores the filtering that we're providing, so it's a tiny deceit, but we're not testing the actual Factory
				factoryMock.SetupSequence(x => x.Load<DashDocumentDataMessage>(It.IsAny<ZQuery>()))
					.Returns([ediMessage])
					.Returns(Array.Empty<DashDocumentDataMessage>());

				factoryMock.Setup(x => x.Load<DashDocument>(It.IsAny<ZGuid>())).Returns(dashDocument);

				loggerMock.Setup(x => x.Log(LogType.Information, It.IsAny<string>())).Callback(() => infoLogCounter++);
				loggerMock.Setup(x => x.Log(LogType.Error, It.IsAny<string>())).Callback(() => errorLogCounter++);

				dashCompletionServiceMock.Setup(x => x.Complete(dashDocument, null)).Throws(new DashException("Test Exception"));

				// act
				dashMessageProcessor.ProcessMessages(CancellationToken.None);

				// assert
				factoryMock.Verify(x => x.Load<DashDocumentDataMessage>(It.Is(ExpectedQueryFilter)));
				factoryMock.Verify(x => x.Save(), Times.Exactly(1));

				AssertLogs(loggerMock, true, ediMessage.PK, infoLogCounter, errorLogCounter);

				AssertEquals(EDIMessageStatusList.Codes.Queued, ediMessage.EM_Status);
				AssertEquals(SharedConstants.ParseStatus.Code.Error, dashDocument.DDD_ParseStatus);

				AssertHandleInvoiceBehavior_For_CIV(
						dashCompletionServiceMock,
						shipamaxServiceMock,
						dashDocument,
						docId,
						docToken,
						SharedConstants.ValidationStatus.Code.Passed);
			}
		}

		DashDocument CreateDashDocument(string parseType, ZGuid docId, ZGuid docToken)
		{
			var dashDocument = Factory.NewWithValidTestData<DashDocument>();
			dashDocument.DDD_ParseType = parseType;
			dashDocument.DDD_ParseStatus = SharedConstants.ParseStatus.Code.Processing;
			dashDocument.DDD_DocID = docId;
			dashDocument.DDD_DocToken = docToken.ToString();
			dashDocument.DDD_ValidationStatus = SharedConstants.ValidationStatus.Code.Passed;
			return dashDocument;
		}

		void Assert_HandleCommercialInvoice_SetsParseStatusToNeedsReview(string validationStatus)
		{
			// We want ShipamaxEdiMessage to be created automatically when StorageDoc is saved
			using (DocManagerRegistry.Instance.EnableCommercialInvoiceDocumentParsing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (GlowRegistry.Instance.GlowServiceUriRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://localhost/Glow"))
			{
				// arrange
				var ediMessage = CreateEdiMessage(SharedConstants.DataProcessingType.Code.NotifyDownstreamServices);

				var docId = ZGuid.NewZGuid();
				var docToken = ZGuid.NewZGuid();

				var dashDocument = Factory.NewWithValidTestData<DashDocument>();
				dashDocument.DDD_ParseType = SharedConstants.ParseType.Code.CommercialInvoice;
				dashDocument.DDD_ParseStatus = SharedConstants.ParseStatus.Code.Processing;
				dashDocument.DDD_DocID = docId;
				dashDocument.DDD_DocToken = docToken.ToString();
				dashDocument.DDD_ValidationStatus = validationStatus;

				var infoLogCounter = 0;
				var errorLogCounter = 0;
				var factoryMock = new Mock<IFactory>();
				var loggerMock = new Mock<ILogger>();
				var shipamaxServiceMock = new Mock<IShipamaxService>();
				var dashErrorReporterMock = new Mock<IDashErrorReporter>();
				var dashCompletionServiceMock = new Mock<IDashCompletionService>();
				var dashMessageProcessor = new NotifyDownstreamServicesMessageProcessor(
					factoryMock.Object,
					loggerMock.Object,
					shipamaxServiceMock.Object,
					dashErrorReporterMock.Object,
					dashCompletionServiceMock.Object);

				// We're self-filtering/batching; our Load mechanism ignores the filtering that we're providing, so it's a tiny deceit, but we're not testing the actual Factory
				factoryMock.SetupSequence(x => x.Load<DashDocumentDataMessage>(It.IsAny<ZQuery>()))
					.Returns([ediMessage])
					.Returns(Array.Empty<DashDocumentDataMessage>());

				factoryMock.Setup(x => x.Load<DashDocument>(It.IsAny<ZGuid>())).Returns(dashDocument);

				loggerMock.Setup(x => x.Log(LogType.Information, It.IsAny<string>())).Callback(() => infoLogCounter++);
				loggerMock.Setup(x => x.Log(LogType.Error, It.IsAny<string>())).Callback(() => errorLogCounter++);

				// act
				dashMessageProcessor.ProcessMessages(CancellationToken.None);

				// assert
				factoryMock.Verify(x => x.Load<DashDocumentDataMessage>(It.Is(ExpectedQueryFilter)));
				factoryMock.Verify(x => x.Save(), Times.Exactly(1));

				AssertLogs(loggerMock, false, ediMessage.PK, infoLogCounter, errorLogCounter);

				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, ediMessage.EM_Status);
				AssertEquals(SharedConstants.ParseStatus.Code.NeedReview, dashDocument.DDD_ParseStatus);

				AssertHandleInvoiceBehavior_For_CIV(
					dashCompletionServiceMock,
					shipamaxServiceMock,
					dashDocument,
					docId,
					docToken,
					validationStatus);
			}
		}

		void AssertLogs(Mock<ILogger> loggerMock, bool expectError, ZGuid messagePK, int infoLogCounter, int errorLogCounter)
		{
			if (expectError)
			{
				loggerMock.Verify(x => x.Log(LogType.Information, "Loaded and started processing 1 of DashDocumentDataMessage records."), Times.Once);
				loggerMock.Verify(x => x.Log(LogType.Error, $"DashDocumentDataMessage record '{messagePK}' will be re-processed in the next service task run. Failure reason: Test Exception"), Times.Once);
				loggerMock.Verify(x => x.Log(LogType.Information, "0 message(s) successfully processed in current batch."), Times.Once);
				loggerMock.Verify(x => x.Log(LogType.Information, "1 message(s) failed to process in current batch."), Times.Once);
				loggerMock.Verify(x => x.Log(LogType.Information, "No DashDocumentDataMessage records have been loaded to process."), Times.Once);
				loggerMock.Verify(x => x.Log(LogType.Information, "Total 0 message(s) successfully processed."), Times.Once);
				loggerMock.Verify(x => x.Log(LogType.Information, "Total 1 message(s) failed to process."), Times.Once);

				AssertEquals(6, infoLogCounter);
				AssertEquals(1, errorLogCounter);

				AssertEquals($"An exception occurred while processing DashDocumentDataMessage record '{messagePK}'.", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
			else
			{
				loggerMock.Verify(x => x.Log(LogType.Information, "Loaded and started processing 1 of DashDocumentDataMessage records."), Times.Once);
				loggerMock.Verify(x => x.Log(LogType.Information, "1 message(s) successfully processed in current batch."), Times.Once);
				loggerMock.Verify(x => x.Log(LogType.Information, "No DashDocumentDataMessage records have been loaded to process."), Times.Once);
				loggerMock.Verify(x => x.Log(LogType.Information, "Total 1 message(s) successfully processed."), Times.Once);

				AssertEquals(4, infoLogCounter);
				AssertEquals(0, errorLogCounter);
			}
		}

		void AssertHandleInvoiceBehavior_For_CIV(
			Mock<IDashCompletionService> dashCompletionServiceMock,
			Mock<IShipamaxService> shipamaxServiceMock,
			DashDocument dashDocument,
			ZGuid docId,
			ZGuid docToken,
			string validationStatus)
		{
			if (validationStatus is SharedConstants.ValidationStatus.Code.Passed)
			{
				dashCompletionServiceMock.Verify(x => x.Complete(dashDocument, null), Times.Once);
			}
			else
			{
				dashCompletionServiceMock.Verify(x => x.Complete(dashDocument, null), Times.Never);
				shipamaxServiceMock.Verify(x => x.SaveParseResult(docId.ToGuid(), docToken.ToString(), It.Is<ShipamaxParseResult>(y => y.ParseStatus == ShipamaxParseStatus.NeedReview)));
			}
		}

		DashDocumentDataMessage CreateEdiMessage(string messageType)
		{
			var ediMessage = Factory.NewWithValidTestData<DashDocumentDataMessage>();
			ediMessage.EM_MessageType = messageType;
			return ediMessage;
		}

		(
			Mock<IFactory> FactoryMock,
			Mock<ILogger> LoggerMock,
			Mock<IShipamaxService> shipamaxServiceMock,
			Mock<IDashCompletionService> DashCompletionServiceMock,
			NotifyDownstreamServicesMessageProcessor MessageProcessor) SetupMessageProcessor()
		{
			var factoryMock = new Mock<IFactory>();
			var loggerMock = new Mock<ILogger>();
			var shipamaxServiceMock = new Mock<IShipamaxService>();
			var dashErrorReporterMock = new Mock<IDashErrorReporter>();
			var dashCompletionServiceMock = new Mock<IDashCompletionService>();
			var dashMessageProcessor = new NotifyDownstreamServicesMessageProcessor(
				factoryMock.Object,
				loggerMock.Object,
				shipamaxServiceMock.Object,
				dashErrorReporterMock.Object,
				dashCompletionServiceMock.Object);

			return (factoryMock, loggerMock, shipamaxServiceMock, dashCompletionServiceMock, dashMessageProcessor);
		}

		static readonly Expression<Func<ZQuery, bool>> ExpectedQueryFilter = x => x.MaximumRows == BatchSize && x.FilterPartsHashKey.Contains("and EM_MessageType = 'NDS'");
	}
}
