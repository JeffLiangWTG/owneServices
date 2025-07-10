using System;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Dash.Business;
using Enterprise.Dash.Integration;
using Enterprise.Dash.ServiceTasks.MessageProcessors;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.Integration;
using Enterprise.Integration;
using Enterprise.Messaging.Integration;
using Moq;
using SharedConstants = WTG.Shared.Dash.Common.Constants;

namespace Enterprise.Dash.ServiceTasks.Tests.MessageProcessors
{
	public class ValidationProcessorTests : TestCaseWithFactory
	{
		public void TestValidationProcessor_ProcessMessages_Sets_ValidationStatus_To_ERR_And_ValidationMessage_When_Validator_Returns_ErrorMessage_For_CIV()
		{
			// We want ShipamaxEdiMessage to be created automatically when StorageDoc is saved
			using (DocManagerRegistry.Instance.EnableCommercialInvoiceDocumentParsing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				// arrange
				const string errorMessage = "validation message";
				var dashDocument = Factory.CreateDashDocument();
				var dashCommercialInvoice = Factory.NewWithValidTestData<DashCommercialInvoice>();
				dashCommercialInvoice.DCI_DDD_DashDocID = dashDocument.PK;
				var ediMessage = CreateEdiMessage(SharedConstants.DataProcessingType.Code.Validation, dashDocument.PK.ToGuid());

				Factory.Save();

				var logCounter = 0;
				var factoryMock = new Mock<IFactory>();
				factoryMock.Setup(x => x.Load<DashDocument>(dashDocument.PK)).Returns(dashDocument);
				var loggerMock = new Mock<ILogger>();
				var shipamaxServiceMock = new Mock<IShipamaxService>();
				var dashErrorReporterMock = new Mock<IDashErrorReporter>();
				var validatorMock = new Mock<IValidator<DashCommercialInvoice>>();
				validatorMock.Setup(x => x.Validate(It.IsAny<DashCommercialInvoice>())).Returns(errorMessage);
				var serviceTaskBusinessObjectFactory = new BusinessObjectFactory();
				var validationProcessor = new ValidationProcessor(serviceTaskBusinessObjectFactory, loggerMock.Object, shipamaxServiceMock.Object, dashErrorReporterMock.Object, validatorMock.Object);

				loggerMock.Setup(x => x.Log(LogType.Information, It.IsAny<string>())).Callback(() => logCounter++);

				// act
				validationProcessor.ProcessMessages(CancellationToken.None);

				// assert
				loggerMock.Verify(x => x.Log(LogType.Information, "Loaded and started processing 1 of DashDocumentDataMessage records."), Times.Once);
				loggerMock.Verify(x => x.Log(LogType.Information, "1 message(s) successfully processed in current batch."), Times.Once);
				loggerMock.Verify(x => x.Log(LogType.Information, "No DashDocumentDataMessage records have been loaded to process."), Times.Once);
				loggerMock.Verify(x => x.Log(LogType.Information, "Total 1 message(s) successfully processed."), Times.Once);

				AssertEquals(4, logCounter);

				var newBusinessObjectFactory = new BusinessObjectFactory();

				var loadedEdiMessage = newBusinessObjectFactory.Load<DashDocumentDataMessage>(ediMessage.PK);
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, loadedEdiMessage.EM_Status);

				var loadedDashDocument = newBusinessObjectFactory.Load<DashDocument>(dashDocument.PK);
				AssertEquals(SharedConstants.ValidationStatus.Code.Error, loadedDashDocument.DDD_ValidationStatus);
				AssertEquals(errorMessage, loadedDashDocument.DDD_ValidationMessage);
			}
		}

		public void TestValidationProcessor_ProcessMessages_Sets_ValidationStatus_To_PSD_And_No_ValidationMessage_When_Validator_Returns_No_ErrorMessage_For_CIV()
		{
			// We want ShipamaxEdiMessage to be created automatically when StorageDoc is saved
			using (DocManagerRegistry.Instance.EnableCommercialInvoiceDocumentParsing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				// arrange
				var dashDocument = Factory.CreateDashDocument();
				var dashCommercialInvoice = Factory.NewWithValidTestData<DashCommercialInvoice>();
				dashCommercialInvoice.DCI_DDD_DashDocID = dashDocument.PK;
				var ediMessage = CreateEdiMessage(SharedConstants.DataProcessingType.Code.Validation, dashDocument.PK.ToGuid());

				Factory.Save();

				var logCounter = 0;
				var factoryMock = new Mock<IFactory>();
				factoryMock.Setup(x => x.Load<DashDocument>(dashDocument.PK)).Returns(dashDocument);
				var loggerMock = new Mock<ILogger>();
				var shipamaxServiceMock = new Mock<IShipamaxService>();
				var validatorMock = new Mock<IValidator<DashCommercialInvoice>>();
				var dashErrorReporterMock = new Mock<IDashErrorReporter>();
				validatorMock.Setup(x => x.Validate(It.IsAny<DashCommercialInvoice>())).Returns(string.Empty);
				var serviceTaskBusinessObjectFactory = new BusinessObjectFactory();
				var validationProcessor = new ValidationProcessor(serviceTaskBusinessObjectFactory, loggerMock.Object, shipamaxServiceMock.Object, dashErrorReporterMock.Object, validatorMock.Object);

				loggerMock.Setup(x => x.Log(LogType.Information, It.IsAny<string>())).Callback(() => logCounter++);

				// act
				validationProcessor.ProcessMessages(CancellationToken.None);

				// assert
				loggerMock.Verify(x => x.Log(LogType.Information, "Loaded and started processing 1 of DashDocumentDataMessage records."), Times.Once);
				loggerMock.Verify(x => x.Log(LogType.Information, "1 message(s) successfully processed in current batch."), Times.Once);
				loggerMock.Verify(x => x.Log(LogType.Information, "No DashDocumentDataMessage records have been loaded to process."), Times.Once);
				loggerMock.Verify(x => x.Log(LogType.Information, "Total 1 message(s) successfully processed."), Times.Once);

				AssertEquals(4, logCounter);

				var newBusinessObjectFactory = new BusinessObjectFactory();

				var loadedEdiMessage = newBusinessObjectFactory.Load<DashDocumentDataMessage>(ediMessage.PK);
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, loadedEdiMessage.EM_Status);

				var loadedDashDocument = newBusinessObjectFactory.Load<DashDocument>(dashDocument.PK);
				AssertEquals(SharedConstants.ValidationStatus.Code.Passed, loadedDashDocument.DDD_ValidationStatus);
				AssertEquals(string.Empty, loadedDashDocument.DDD_ValidationMessage);
			}
		}

		public void TestValidationProcessor_ProcessMessages_Does_Not_Run_Validation_For_PIN()
		{
			// We want ShipamaxEdiMessage to be created automatically when StorageDoc is saved
			using (DocManagerRegistry.Instance.EnableAccountsPayableInvoiceDocumentParsing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				// arrange
				var dashDocument = Factory.CreateDashDocument(parseType: SharedConstants.ParseType.Code.AccountPayableInvoice);
				var dashCommercialInvoice = Factory.NewWithValidTestData<DashCommercialInvoice>();
				dashCommercialInvoice.DCI_DDD_DashDocID = dashDocument.PK;
				var ediMessage = CreateEdiMessage(SharedConstants.DataProcessingType.Code.Validation, dashDocument.PK.ToGuid());

				Factory.Save();

				var logCounter = 0;
				var factoryMock = new Mock<IFactory>();
				factoryMock.Setup(x => x.Load<DashDocument>(dashDocument.PK)).Returns(dashDocument);
				var loggerMock = new Mock<ILogger>();
				var shipamaxServiceMock = new Mock<IShipamaxService>();
				var validatorMock = new Mock<IValidator<DashCommercialInvoice>>();
				var dashErrorReporter = new Mock<IDashErrorReporter>();
				validatorMock.Setup(x => x.Validate(It.IsAny<DashCommercialInvoice>())).Returns(string.Empty);
				var serviceTaskBusinessObjectFactory = new BusinessObjectFactory();
				var validationProcessor = new ValidationProcessor(serviceTaskBusinessObjectFactory, loggerMock.Object, shipamaxServiceMock.Object, dashErrorReporter.Object, validatorMock.Object);

				loggerMock.Setup(x => x.Log(LogType.Information, It.IsAny<string>())).Callback(() => logCounter++);

				// act
				validationProcessor.ProcessMessages(CancellationToken.None);

				// assert
				loggerMock.Verify(x => x.Log(LogType.Information, "Loaded and started processing 1 of DashDocumentDataMessage records."), Times.Once);
				loggerMock.Verify(x => x.Log(LogType.Information, "1 message(s) successfully processed in current batch."), Times.Once);
				loggerMock.Verify(x => x.Log(LogType.Information, "No DashDocumentDataMessage records have been loaded to process."), Times.Once);
				loggerMock.Verify(x => x.Log(LogType.Information, "Total 1 message(s) successfully processed."), Times.Once);

				AssertEquals(4, logCounter);

				var newBusinessObjectFactory = new BusinessObjectFactory();

				var loadedEdiMessage = newBusinessObjectFactory.Load<DashDocumentDataMessage>(ediMessage.PK);
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, loadedEdiMessage.EM_Status);

				var loadedDashDocument = newBusinessObjectFactory.Load<DashDocument>(dashDocument.PK);
				AssertEquals(SharedConstants.ValidationStatus.Code.None, loadedDashDocument.DDD_ValidationStatus);
				AssertEquals(string.Empty, loadedDashDocument.DDD_ValidationMessage);
			}
		}

		DashDocumentDataMessage CreateEdiMessage(string messageType, Guid documentId)
		{
			var ediMessage = Factory.NewWithValidTestData<DashDocumentDataMessage>();
			ediMessage.EM_MessageType = messageType;
			ediMessage.EM_ReceiveTransmit = "INT";
			ediMessage.EM_LinkTable = DashDocument.Schema.TableName;
			ediMessage.EM_LinkUniqueID = documentId;
			return ediMessage;
		}
	}
}
