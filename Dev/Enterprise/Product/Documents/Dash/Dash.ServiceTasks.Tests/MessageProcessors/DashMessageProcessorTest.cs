using System;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Dash.Business;
using Enterprise.Dash.ServiceTasks.MessageProcessors;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.Business.Test;
using Enterprise.DocumentScanning.Integration;
using Enterprise.Integration;
using Enterprise.Messaging.Integration;
using Moq;
using Moq.Protected;

namespace Enterprise.Dash.ServiceTasks.Tests.MessageProcessors
{
	public class DashMessageProcessorTest : TestCaseWithFactory
	{
		public void TestProcessMessages_No_Messages_To_Process()
		{
			// We want ShipamaxEdiMessage to be created automatically when StorageDoc is saved
			using (DocManagerRegistry.Instance.EnableCommercialInvoiceDocumentParsing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				// arrange
				var logCounter = 0;
				var factoryMock = new Mock<IFactory>();
				var loggerMock = new Mock<ILogger>();
				var shipamaxMock = new Mock<IShipamaxService>();
				var dashErrorReporterMock = new Mock<IDashErrorReporter>();
				var dashMessageProcessorMock = new Mock<DashMessageProcessor>(factoryMock.Object, loggerMock.Object, shipamaxMock.Object, dashErrorReporterMock.Object);
				dashMessageProcessorMock.CallBase = true;

				loggerMock.Setup(x => x.Log(LogType.Information, It.IsAny<string>())).Callback(() => logCounter++);

				// act
				dashMessageProcessorMock.Object.ProcessMessages(CancellationToken.None);

				// assert
				loggerMock.Verify(x => x.Log(LogType.Information, "No DashDocumentDataMessage records have been loaded to process."), Times.Once);
				loggerMock.Verify(x => x.Log(LogType.Information, "Total 0 message(s) successfully processed."), Times.Once);

				AssertEquals(2, logCounter);
			}
		}

		public void TestProcessMessages_Processes_Two_Batches_One_Message_Each()
		{
			// We want ShipamaxEdiMessage to be created automatically when StorageDoc is saved
			using (DocManagerRegistry.Instance.EnableCommercialInvoiceDocumentParsing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				// arrange
				var dashDocument1 = Factory.CreateDashDocument();
				var dashDocument2 = Factory.CreateDashDocument();
				var ediMessage1 = Factory.NewWithValidTestData<DashDocumentDataMessage>();
				ediMessage1.EM_LinkUniqueID = dashDocument1.PK.ToGuid();
				var ediMessage2 = Factory.NewWithValidTestData<DashDocumentDataMessage>();
				ediMessage2.EM_LinkUniqueID = dashDocument2.PK.ToGuid();

				var logCounter = 0;

				var factoryMock = new Mock<IFactory>();
				factoryMock.Setup(x => x.Load<DashDocument>(dashDocument1.PK)).Returns(dashDocument1);
				factoryMock.Setup(x => x.Load<DashDocument>(dashDocument2.PK)).Returns(dashDocument2);

				var loggerMock = new Mock<ILogger>();
				var shipamaxService = new Mock<IShipamaxService>();
				var dashErrorReporterMock = new Mock<IDashErrorReporter>();
				var dashMessageProcessorMock = new Mock<DashMessageProcessor>(factoryMock.Object, loggerMock.Object, shipamaxService.Object, dashErrorReporterMock.Object);
				dashMessageProcessorMock.CallBase = true;

				factoryMock.SetupSequence(x => x.Load<DashDocumentDataMessage>(It.IsAny<ZQuery>()))
					.Returns([ediMessage1])
					.Returns([ediMessage2])
					.Returns([]);

				loggerMock.Setup(x => x.Log(LogType.Information, It.IsAny<string>())).Callback(() => logCounter++);

				// act
				dashMessageProcessorMock.Object.ProcessMessages(CancellationToken.None);

				// assert
				loggerMock.Verify(x => x.Log(LogType.Information, "Loaded and started processing 1 of DashDocumentDataMessage records."), Times.Exactly(2));
				loggerMock.Verify(x => x.Log(LogType.Information, "1 message(s) successfully processed in current batch."), Times.Exactly(2));
				loggerMock.Verify(x => x.Log(LogType.Information, "No DashDocumentDataMessage records have been loaded to process."), Times.Once);
				loggerMock.Verify(x => x.Log(LogType.Information, "Total 2 message(s) successfully processed."), Times.Once);

				factoryMock.Verify(x => x.Save(), Times.Exactly(2));

				dashMessageProcessorMock.Protected().Verify("ProcessMessageCore", Times.Once(), factoryMock.Object, dashDocument1, ediMessage1, It.IsAny<CancellationToken>());
				dashMessageProcessorMock.Protected().Verify("ProcessMessageCore", Times.Once(), factoryMock.Object, dashDocument2, ediMessage2, It.IsAny<CancellationToken>());

				AssertEquals(6, logCounter);

				AssertEquals(ediMessage1.EM_Status, EDIMessageStatusList.Codes.ProcessedOK);
				AssertEquals(ediMessage2.EM_Status, EDIMessageStatusList.Codes.ProcessedOK);
			}
		}

		public void TestProcessMessages_Processes__Message_Processing_Cancelled_When_DashDocument_Record_Not_Found()
		{
			// We want ShipamaxEdiMessage to be created automatically when StorageDoc is saved
			using (DocManagerRegistry.Instance.EnableCommercialInvoiceDocumentParsing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				// arrange
				var dashDocument1 = Factory.CreateDashDocument();
				var ediMessage1 = Factory.NewWithValidTestData<DashDocumentDataMessage>();
				var dashDocumentIdInDataMessage = Guid.NewGuid();
				ediMessage1.EM_LinkUniqueID = dashDocumentIdInDataMessage;

				var logCounter = 0;

				var factoryMock = new Mock<IFactory>();
				factoryMock.Setup(x => x.Load<DashDocument>(dashDocument1.PK)).Returns(dashDocument1);

				var loggerMock = new Mock<ILogger>();
				var shipamaxService = new Mock<IShipamaxService>();
				var dashErrorReporterMock = new Mock<IDashErrorReporter>();
				var dashMessageProcessorMock = new Mock<DashMessageProcessor>(factoryMock.Object, loggerMock.Object, shipamaxService.Object, dashErrorReporterMock.Object);
				dashMessageProcessorMock.CallBase = true;

				factoryMock.SetupSequence(x => x.Load<DashDocumentDataMessage>(It.IsAny<ZQuery>()))
					.Returns([ediMessage1])
					.Returns([]);

				loggerMock.Setup(x => x.Log(It.IsAny<LogType>(), It.IsAny<string>())).Callback(() => logCounter++);

				// act
				dashMessageProcessorMock.Object.ProcessMessages(CancellationToken.None);

				// assert
				loggerMock.Verify(x => x.Log(LogType.Information, "Loaded and started processing 1 of DashDocumentDataMessage records."), Times.Once());
				loggerMock.Verify(x => x.Log(LogType.Error, $"DashDocument record is not found. DashDocument ID stored in DashDocumentDataMessage: {dashDocumentIdInDataMessage}."));
				loggerMock.Verify(x => x.Log(LogType.Information, "1 message(s) successfully processed in current batch."), Times.Once());
				loggerMock.Verify(x => x.Log(LogType.Information, "No DashDocumentDataMessage records have been loaded to process."), Times.Once);
				loggerMock.Verify(x => x.Log(LogType.Information, "Total 1 message(s) successfully processed."), Times.Once);

				factoryMock.Verify(x => x.Save(), Times.Exactly(1));

				dashMessageProcessorMock.Protected().Verify("ProcessMessageCore", Times.Never(), factoryMock.Object, dashDocument1, ediMessage1, It.IsAny<CancellationToken>());

				AssertEquals(5, logCounter);

				AssertEquals(ediMessage1.EM_Status, EDIMessageStatusList.Codes.ProcessedOK);
			}
		}

		public void TestProcessMessages_Processes__Message_Processing_Cancelled_When_DashDocument_ParseType_Is_Disabled_ForCIV()
		{
			// We want DocumentParsing to be enabled but the required parsing type to be disabled for this test.
			using (DocManagerRegistry.Instance.EnableAccountsPayableInvoiceDocumentParsing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (DocManagerRegistry.Instance.EnableCommercialInvoiceDocumentParsing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				TestProcessMessages_Processes__Message_Processing_Cancelled_When_DashDocument_ParseType_Is_Disabled("CIV");
			}
		}

		public void TestProcessMessages_Processes__Message_Processing_Cancelled_When_DashDocument_ParseType_Is_Disabled_ForPIN()
		{
			// We want DocumentParsing to be enabled but the required parsing type to be disabled for this test.
			using (DocManagerRegistry.Instance.EnableCommercialInvoiceDocumentParsing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (DocManagerRegistry.Instance.EnableAccountsPayableInvoiceDocumentParsing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				TestProcessMessages_Processes__Message_Processing_Cancelled_When_DashDocument_ParseType_Is_Disabled("PIN");
			}
		}

		public void TestProcessMessages_Processes__Message_Processing_Cancelled_When_DashDocument_ParseType_Is_Disabled(string parseType)
		{
			// We want ShipamaxEdiMessage to be created automatically when StorageDoc is saved
			// arrange
			var dashDocument1 = Factory.CreateDashDocument(parseType);
			var ediMessage1 = Factory.NewWithValidTestData<DashDocumentDataMessage>();
			ediMessage1.EM_LinkUniqueID = dashDocument1.PK.ToGuid();

			var logCounter = 0;

			var factoryMock = new Mock<IFactory>();
			factoryMock.Setup(x => x.Load<DashDocument>(dashDocument1.PK)).Returns(dashDocument1);

			var loggerMock = new Mock<ILogger>();
			var shipamaxService = new Mock<IShipamaxService>();
			var dashErrorReporterMock = new Mock<IDashErrorReporter>();
			var dashMessageProcessorMock = new Mock<DashMessageProcessor>(factoryMock.Object, loggerMock.Object, shipamaxService.Object, dashErrorReporterMock.Object);
			dashMessageProcessorMock.CallBase = true;

			factoryMock.SetupSequence(x => x.Load<DashDocumentDataMessage>(It.IsAny<ZQuery>()))
				.Returns([ediMessage1])
				.Returns([]);

			loggerMock.Setup(x => x.Log(It.IsAny<LogType>(), It.IsAny<string>())).Callback(() => logCounter++);

			// act
			dashMessageProcessorMock.Object.ProcessMessages(CancellationToken.None);

			// assert
			loggerMock.Verify(x => x.Log(LogType.Information, "Loaded and started processing 1 of DashDocumentDataMessage records."), Times.Once());
			loggerMock.Verify(x => x.Log(LogType.Warning, $"DashDocument's parse type is not enabled for parsing. Parse Type: {parseType}. DashDocument ID: {dashDocument1.PK}."));
			loggerMock.Verify(x => x.Log(LogType.Information, "1 message(s) successfully processed in current batch."), Times.Once());
			loggerMock.Verify(x => x.Log(LogType.Information, "No DashDocumentDataMessage records have been loaded to process."), Times.Once);
			loggerMock.Verify(x => x.Log(LogType.Information, "Total 1 message(s) successfully processed."), Times.Once);

			factoryMock.Verify(x => x.Save(), Times.Exactly(1));

			dashMessageProcessorMock.Protected().Verify("ProcessMessageCore", Times.Never(), factoryMock.Object, dashDocument1, ediMessage1, It.IsAny<CancellationToken>());

			AssertEquals(5, logCounter);

			AssertEquals(ediMessage1.EM_Status, EDIMessageStatusList.Codes.ProcessedOK);
		}

		public void TestProcessMessages_Processes__Message_Processing_Cancelled_When_DashDocument_Is_Obsolete()
		{
			// We want ShipamaxEdiMessage to be created automatically when StorageDoc is saved
			using (DocManagerRegistry.Instance.EnableCommercialInvoiceDocumentParsing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				// arrange
				var dashDocument1 = Factory.CreateDashDocument();
				dashDocument1.DDD_IsObsolete = true;
				var ediMessage1 = Factory.NewWithValidTestData<DashDocumentDataMessage>();
				ediMessage1.EM_LinkUniqueID = dashDocument1.PK.ToGuid();

				var logCounter = 0;

				var factoryMock = new Mock<IFactory>();
				factoryMock.Setup(x => x.Load<DashDocument>(dashDocument1.PK)).Returns(dashDocument1);

				var loggerMock = new Mock<ILogger>();
				var shipamaxService = new Mock<IShipamaxService>();
				var dashErrorReporterMock = new Mock<IDashErrorReporter>();
				var dashMessageProcessorMock = new Mock<DashMessageProcessor>(factoryMock.Object, loggerMock.Object, shipamaxService.Object, dashErrorReporterMock.Object);
				dashMessageProcessorMock.CallBase = true;

				factoryMock.SetupSequence(x => x.Load<DashDocumentDataMessage>(It.IsAny<ZQuery>()))
					.Returns([ediMessage1])
					.Returns([]);

				loggerMock.Setup(x => x.Log(It.IsAny<LogType>(), It.IsAny<string>())).Callback(() => logCounter++);

				// act
				dashMessageProcessorMock.Object.ProcessMessages(CancellationToken.None);

				// assert
				loggerMock.Verify(x => x.Log(LogType.Information, "Loaded and started processing 1 of DashDocumentDataMessage records."), Times.Once());
				loggerMock.Verify(x => x.Log(LogType.Warning, $"DashDocument record is obsolete. DashDocument ID: {dashDocument1.PK}."));
				loggerMock.Verify(x => x.Log(LogType.Information, "1 message(s) successfully processed in current batch."), Times.Once());
				loggerMock.Verify(x => x.Log(LogType.Information, "No DashDocumentDataMessage records have been loaded to process."), Times.Once);
				loggerMock.Verify(x => x.Log(LogType.Information, "Total 1 message(s) successfully processed."), Times.Once);

				factoryMock.Verify(x => x.Save(), Times.Exactly(1));

				dashMessageProcessorMock.Protected().Verify("ProcessMessageCore", Times.Never(), factoryMock.Object, dashDocument1, ediMessage1, It.IsAny<CancellationToken>());

				AssertEquals(5, logCounter);

				AssertEquals(ediMessage1.EM_Status, EDIMessageStatusList.Codes.ProcessedOK);
			}
		}

		public void TestProcessMessages_Processes__Message_Processing_Cancelled_When_StorageDocID_Is_Empty()
		{
			// We want ShipamaxEdiMessage to be created automatically when StorageDoc is saved
			using (DocManagerRegistry.Instance.EnableCommercialInvoiceDocumentParsing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				// arrange
				var dashDocument1 = Factory.CreateDashDocument();
				dashDocument1.DDD_DocID = ZGuid.Empty;
				var ediMessage1 = Factory.NewWithValidTestData<DashDocumentDataMessage>();
				ediMessage1.EM_LinkUniqueID = dashDocument1.PK.ToGuid();

				var logCounter = 0;

				var factoryMock = new Mock<IFactory>();
				factoryMock.Setup(x => x.Load<DashDocument>(dashDocument1.PK)).Returns(dashDocument1);

				var loggerMock = new Mock<ILogger>();
				var shipamaxService = new Mock<IShipamaxService>();
				var dashErrorReporterMock  = new Mock<IDashErrorReporter>();
				var dashMessageProcessorMock = new Mock<DashMessageProcessor>(factoryMock.Object, loggerMock.Object, shipamaxService.Object, dashErrorReporterMock.Object);
				dashMessageProcessorMock.CallBase = true;

				factoryMock.SetupSequence(x => x.Load<DashDocumentDataMessage>(It.IsAny<ZQuery>()))
					.Returns([ediMessage1])
					.Returns([]);

				loggerMock.Setup(x => x.Log(It.IsAny<LogType>(), It.IsAny<string>())).Callback(() => logCounter++);

				// act
				dashMessageProcessorMock.Object.ProcessMessages(CancellationToken.None);

				// assert
				loggerMock.Verify(x => x.Log(LogType.Information, "Loaded and started processing 1 of DashDocumentDataMessage records."), Times.Once());
				loggerMock.Verify(x => x.Log(LogType.Warning, $"StorageDoc ID must not be empty. DashDocument ID: {dashDocument1.PK}."));
				loggerMock.Verify(x => x.Log(LogType.Information, "1 message(s) successfully processed in current batch."), Times.Once());
				loggerMock.Verify(x => x.Log(LogType.Information, "No DashDocumentDataMessage records have been loaded to process."), Times.Once);
				loggerMock.Verify(x => x.Log(LogType.Information, "Total 1 message(s) successfully processed."), Times.Once);

				factoryMock.Verify(x => x.Save(), Times.Exactly(1));

				dashMessageProcessorMock.Protected().Verify("ProcessMessageCore", Times.Never(), factoryMock.Object, dashDocument1, ediMessage1, It.IsAny<CancellationToken>());

				AssertEquals(5, logCounter);

				AssertEquals(ediMessage1.EM_Status, EDIMessageStatusList.Codes.ProcessedOK);
			}
		}

		public void TestProcessMessages_Processes__Message_Processing_Cancelled_When_Document_Has_Changed()
		{
			// We want ShipamaxEdiMessage to be created automatically when StorageDoc is saved
			using (DocManagerRegistry.Instance.EnableCommercialInvoiceDocumentParsing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				// arrange
				var dashDocument1 = Factory.CreateDashDocument();
				dashDocument1.DDD_DocToken = Guid.NewGuid().ToString();
				var ediMessage1 = Factory.NewWithValidTestData<DashDocumentDataMessage>();
				ediMessage1.EM_LinkUniqueID = dashDocument1.PK.ToGuid();

				var logCounter = 0;

				var factoryMock = new Mock<IFactory>();
				factoryMock.Setup(x => x.Load<DashDocument>(dashDocument1.PK)).Returns(dashDocument1);

				var loggerMock = new Mock<ILogger>();
				var shipamaxService = new Mock<IShipamaxService>();
				var dashErrorReporterMock = new Mock<IDashErrorReporter>();
				var shipamaxEDocChange = new ShipamaxEDocsChange()
				{
					Field = ShipamaxEDocsChangeType.DocFormat,
					OldValue = "OldValue",
					NewValue = "NewValue"
				};
				shipamaxService.Setup(x => x.CheckEDocsChanges(dashDocument1.DDD_DocID.ToGuid(), dashDocument1.DDD_DocToken.ToString())).Returns([shipamaxEDocChange]);
				var dashMessageProcessorMock = new Mock<DashMessageProcessor>(factoryMock.Object, loggerMock.Object, shipamaxService.Object, dashErrorReporterMock.Object);
				dashMessageProcessorMock.CallBase = true;

				factoryMock.SetupSequence(x => x.Load<DashDocumentDataMessage>(It.IsAny<ZQuery>()))
					.Returns([ediMessage1])
					.Returns([]);

				loggerMock.Setup(x => x.Log(It.IsAny<LogType>(), It.IsAny<string>())).Callback(() => logCounter++);

				// act
				dashMessageProcessorMock.Object.ProcessMessages(CancellationToken.None);

				// assert
				loggerMock.Verify(x => x.Log(LogType.Information, "Loaded and started processing 1 of DashDocumentDataMessage records."), Times.Once());
				loggerMock.Verify(x => x.Log(LogType.Information, $"Document related to DashDocument with ID {dashDocument1.PK} has changed. There are the following changes: Change Type: DocFormat. Old value: OldValue, New Value: NewValue."));
				loggerMock.Verify(x => x.Log(LogType.Information, "1 message(s) successfully processed in current batch."), Times.Once());
				loggerMock.Verify(x => x.Log(LogType.Information, "No DashDocumentDataMessage records have been loaded to process."), Times.Once);
				loggerMock.Verify(x => x.Log(LogType.Information, "Total 1 message(s) successfully processed."), Times.Once);

				factoryMock.Verify(x => x.Save(), Times.Exactly(1));

				dashMessageProcessorMock.Protected().Verify("ProcessMessageCore", Times.Never(), factoryMock.Object, dashDocument1, ediMessage1, It.IsAny<CancellationToken>());

				AssertEquals(5, logCounter);

				AssertEquals(ediMessage1.EM_Status, EDIMessageStatusList.Codes.ProcessedOK);
			}
		}

		public void TestProcessMessages_Reports_Error_When_ProcessMessage_Throws_Exception()
		{
			// We want ShipamaxEdiMessage to be created automatically when StorageDoc is saved
			using (DocManagerRegistry.Instance.EnableCommercialInvoiceDocumentParsing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				// arrange
				const string errorMessage = "Message processing failed";
				var dashDocument1 = Factory.CreateDashDocument();
				var dashDocument2 = Factory.CreateDashDocument();
				var ediMessage1 = Factory.NewWithValidTestData<DashDocumentDataMessage>();
				ediMessage1.EM_LinkUniqueID = dashDocument1.PK.ToGuid();
				var ediMessage2 = Factory.NewWithValidTestData<DashDocumentDataMessage>();
				ediMessage2.EM_LinkUniqueID = dashDocument2.PK.ToGuid();

				var logCounter = 0;
				var factoryMock = new Mock<IFactory>();
				factoryMock.Setup(x => x.Load<DashDocument>(dashDocument1.PK)).Returns(dashDocument1);
				factoryMock.Setup(x => x.Load<DashDocument>(dashDocument2.PK)).Returns(dashDocument2);
				var loggerMock = new Mock<ILogger>();
				var shipamaxMock = new Mock<IShipamaxService>();
				var errorReporterMock = new Mock<IDashErrorReporter>();
				errorReporterMock.Setup(r => r.GatherAdditionalInformation(ediMessage1))
					.Returns(Mock.Of<IDisposable>()).Verifiable();
				var dashMessageProcessorMock = new Mock<DashMessageProcessor>(factoryMock.Object, loggerMock.Object, shipamaxMock.Object, errorReporterMock.Object);
				dashMessageProcessorMock.CallBase = true;
				dashMessageProcessorMock.Protected().SetupGet<byte>("MaxRetryCount").Returns(5);
				dashMessageProcessorMock.Protected().Setup("ProcessMessageCore", factoryMock.Object, dashDocument1, ediMessage1, It.IsAny<CancellationToken>())
					.Throws(() => new InvalidOperationException(errorMessage));

				factoryMock.SetupSequence(x => x.Load<DashDocumentDataMessage>(It.IsAny<ZQuery>()))
					.Returns([ediMessage1])
					.Returns([ediMessage2])
					.Returns([]);

				loggerMock.Setup(x => x.Log(It.IsAny<LogType>(), It.IsAny<string>())).Callback(() => logCounter++);

				// act
				dashMessageProcessorMock.Object.ProcessMessages(CancellationToken.None);

				// assert
				AssertEquals($"An exception occurred while processing DashDocumentDataMessage record '{ediMessage1.PK}'.", ErrorReporter.LastMessageReported);

				ErrorReporter.Clear();

				loggerMock.Verify(x => x.Log(LogType.Information, "Loaded and started processing 1 of DashDocumentDataMessage records."), Times.Exactly(2));
				loggerMock.Verify(x => x.Log(LogType.Information, "1 message(s) successfully processed in current batch."), Times.Once);
				loggerMock.Verify(x => x.Log(LogType.Information, "0 message(s) successfully processed in current batch."), Times.Once);
				loggerMock.Verify(x => x.Log(LogType.Information, "1 message(s) failed to process in current batch."), Times.Once);
				loggerMock.Verify(x => x.Log(LogType.Error, $"DashDocumentDataMessage record '{ediMessage1.PK}' will be re-processed in the next service task run. Failure reason: {errorMessage}"), Times.Once);
				loggerMock.Verify(x => x.Log(LogType.Information, "No DashDocumentDataMessage records have been loaded to process."), Times.Once);
				loggerMock.Verify(x => x.Log(LogType.Information, "Total 1 message(s) successfully processed."), Times.Once);
				loggerMock.Verify(x => x.Log(LogType.Information, "Total 1 message(s) failed to process."), Times.Once);

				errorReporterMock.Verify(r => r.GatherAdditionalInformation(ediMessage1), Times.Once);

				factoryMock.Verify(x => x.Save(), Times.Exactly(2));

				dashMessageProcessorMock.Protected().Verify("ProcessMessageCore", Times.Once(), factoryMock.Object, dashDocument1, ediMessage1, It.IsAny<CancellationToken>());

				AssertEquals(9, logCounter);

				AssertEquals(ediMessage1.EM_Status, EDIMessageStatusList.Codes.Queued);
			}
		}

		public void TestProcessMessages_And_Create_New_Message()
		{
			// We want ShipamaxEdiMessage to be created automatically when StorageDoc is saved
			using (DocManagerRegistry.Instance.EnableCommercialInvoiceDocumentParsing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				// arrange
				var existingEdiMessage = Factory.NewWithValidTestData<DashDocumentDataMessage>();
				var newEdiMessage = Factory.NewWithValidTestData<DashDocumentDataMessage>();

				var logCounter = 0;
				var factoryMock = new Mock<IFactory>();
				var loggerMock = new Mock<ILogger>();
				var shipamaxServiceMock = new Mock<IShipamaxService>();
				var dashErrorReporterMock = new Mock<IDashErrorReporter>();
				var dashMessageProcessorMock = new Mock<DashMessageProcessor>(factoryMock.Object, loggerMock.Object, shipamaxServiceMock.Object, dashErrorReporterMock.Object);
				dashMessageProcessorMock.CallBase = true;

				factoryMock.SetupSequence(x => x.Load<DashDocumentDataMessage>(It.IsAny<ZQuery>()))
					.Returns(new[] { existingEdiMessage })
					.Returns(Array.Empty<DashDocumentDataMessage>());

				factoryMock.Setup(x => x.New(It.Is<Type>(x => x == typeof(DashDocumentDataMessage))))
					.Returns(newEdiMessage);

				loggerMock.Setup(x => x.Log(LogType.Information, It.IsAny<string>())).Callback(() => logCounter++);

				existingEdiMessage.EM_MessageType = WTG.Shared.Dash.Common.Constants.DataProcessingType.Code.OrganizationMatching;
				existingEdiMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Internal;
				existingEdiMessage.MessageData = new DashDocumentDataMessageData { DataProcessingSteps = [WTG.Shared.Dash.Common.Constants.DataProcessingType.Code.OrganizationMatching, WTG.Shared.Dash.Common.Constants.DataProcessingType.Code.NotifyDownstreamServices], CurrentDataProcessingStep = 0 };

				// act
				dashMessageProcessorMock.Object.ProcessMessages(CancellationToken.None);

				// assert
				loggerMock.Verify(x => x.Log(LogType.Information, "Loaded and started processing 1 of DashDocumentDataMessage records."), Times.Exactly(1));
				loggerMock.Verify(x => x.Log(LogType.Information, "1 message(s) successfully processed in current batch."), Times.Exactly(1));
				loggerMock.Verify(x => x.Log(LogType.Information, "No DashDocumentDataMessage records have been loaded to process."), Times.Once);
				loggerMock.Verify(x => x.Log(LogType.Information, "Total 1 message(s) successfully processed."), Times.Once);

				factoryMock.Verify(x => x.Save(), Times.Exactly(1));
				factoryMock.Verify(x => x.New(It.Is<Type>(x => x == typeof(DashDocumentDataMessage))), Times.Once());

				dashMessageProcessorMock.Protected().Verify("ProcessMessage", Times.Once(), factoryMock.Object, existingEdiMessage, It.IsAny<CancellationToken>());

				AssertEquals(4, logCounter);

				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, existingEdiMessage.EM_Status);
				AssertEquals(EDIMessageStatusList.Codes.Queued, newEdiMessage.EM_Status);
				AssertEquals(WTG.Shared.Dash.Common.Constants.DataProcessingType.Code.NotifyDownstreamServices, newEdiMessage.EM_MessageType);
				AssertEquals(existingEdiMessage.EM_GB, newEdiMessage.EM_GB);
				AssertEquals(existingEdiMessage.EM_ApplicationCode, newEdiMessage.EM_ApplicationCode);
				AssertEquals(existingEdiMessage.EM_ReceiveTransmit, newEdiMessage.EM_ReceiveTransmit);
				AssertEquals(existingEdiMessage.EM_MessageSubType, newEdiMessage.EM_MessageSubType);
				AssertEquals(existingEdiMessage.EM_LinkTable, newEdiMessage.EM_LinkTable);
				AssertEquals(existingEdiMessage.EM_LinkUniqueID, newEdiMessage.EM_LinkUniqueID);
				AssertEquals(true, newEdiMessage.EM_IsActive);
			}
		}

		public void TestProcessMessages_PreConditions_NotMet()
		{
			using (DocManagerRegistry.Instance.SetTemporaryDocParsingRegistryValues(false))
			{
				// arrange
				var existingEdiMessage = Factory.NewWithValidTestData<DashDocumentDataMessage>();
				var newEdiMessage = Factory.NewWithValidTestData<DashDocumentDataMessage>();

				var logCounter = 0;
				var factoryMock = new Mock<IFactory>();
				var loggerMock = new Mock<ILogger>();
				var shipamaxServiceMock = new Mock<IShipamaxService>();
				var dashErrorReporterMock = new Mock<IDashErrorReporter>();
				var dashMessageProcessorMock = new Mock<DashMessageProcessor>(factoryMock.Object, loggerMock.Object, shipamaxServiceMock.Object, dashErrorReporterMock.Object);
				dashMessageProcessorMock.CallBase = true;

				factoryMock.SetupSequence(x => x.Load<DashDocumentDataMessage>(It.IsAny<ZQuery>()))
					.Returns([existingEdiMessage])
					.Returns(Array.Empty<DashDocumentDataMessage>());

				factoryMock.Setup(x => x.New(It.Is<Type>(x => x == typeof(DashDocumentDataMessage))))
					.Returns(newEdiMessage);

				loggerMock.Setup(x => x.Log(It.IsAny<LogType>(), It.IsAny<string>())).Callback(() => logCounter++);

				// act
				dashMessageProcessorMock.Object.ProcessMessages(CancellationToken.None);

				// assert
				loggerMock.Verify(x => x.Log(LogType.Error, "Failed to run service task. Please check if registry for at least one of the parsing types is enabled at System -> DocManager -> Document Ingestion -> Parse Types."), Times.Exactly(1));
				AssertEquals(1, logCounter);

				factoryMock.Verify(x => x.Load<DashDocumentDataMessage>(It.IsAny<ZQuery>()), Times.Never());
				factoryMock.Verify(x => x.Save(), Times.Never());
				factoryMock.Verify(x => x.New(It.Is<Type>(x => x == typeof(DashDocumentDataMessage))), Times.Never());

				dashMessageProcessorMock.Protected().Verify(
					"ProcessMessage",
					Times.Never(),
					ItExpr.IsAny<IFactory>(),
					ItExpr.IsAny<DashDocumentDataMessage>(),
					ItExpr.IsAny<CancellationToken>());
			}
		}
	}
}
