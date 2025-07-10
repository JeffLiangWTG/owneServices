using System;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.EventMessageProcessor;
using Enterprise.Accounting.ElectronicMessaging.Testing.Romania;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using static Enterprise.Core.Constants;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Accounting.ElectronicMessaging.Romania.Testing
{
	public class RomaniaEInvoicingEventMessageProcessorTest : TestCaseWithFactory
	{
		public void TestIAKMessage_WhenBatchStatusIsSent()
		{
			var pivot = AssertAfterProcessIAKEventMessage
			(
				EInvoicingPivotState.Sent, EInvoicingBatchState.Sent,
				EInvoicingPivotState.Delivered, EInvoicingBatchState.Ready
			);
			AssertEquals(new ZDateTime(2024, 1, 16, 09, 30, 10), pivot.AIP_LastResponseReceivedUtc);
		}

		public void TestIAKMessage_WhenBatchStatusIsDiscarded()
		{
			var pivot = AssertAfterProcessIAKEventMessage
			(
				EInvoicingPivotState.Sent, EInvoicingBatchState.Discarded,
				EInvoicingPivotState.Sent, EInvoicingBatchState.Discarded
			);
			AssertEquals(ZDateTime.Empty, pivot.AIP_LastResponseReceivedUtc);
		}

		public void TestAuthorisationRecordMapping()
		{
			var ediMsg = EDIMessageTestFactory.New(Factory);
			using var xueMsgForDelivery = CreateIAKUniversalEvent(ediMsg, EInvoicingPivotState.Delivered, "123456");
			using var xueMsgForSuccess = CreateIAKUniversalEvent(ediMsg, EInvoicingPivotState.Succeed, "123456");

			var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("00001", TestObjectCreator.AUD, 0.5m, TestObjectCreator.Debtor);
			invoice.IsManuallySetTransactionNumber_ForTestOnly = true;
			var submitInvoiceBatch = CreateSubmitPivotAndBatch(invoice);
			Factory.Save();

			var logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			var countryFactory = new RomaniaEInvoicingObjectFactory();

			var authorizationRecord = Factory.LoadTop1<AccTransactionHeaderAuthorisationRecord>(new ZQuery(AccTransactionHeaderAuthorisationRecordSchema.AHF_ParentId, invoice.PK));
			AssertNull(authorizationRecord);

			var eventMessageProcessorData = new EventMessageProcessorData("ANY", "***", logger, ediMsg, xueMsgForDelivery, submitInvoiceBatch);
			new RomaniaEInvoicingEventMessageProcessor(eventMessageProcessorData, countryFactory).Process();
			Factory.Save();

			authorizationRecord = Factory.LoadTop1<AccTransactionHeaderAuthorisationRecord>(new ZQuery(AccTransactionHeaderAuthorisationRecordSchema.AHF_ParentId, invoice.PK));
			AssertEquals("ROA", authorizationRecord.AHF_RecordType);
			AssertEquals("123456", authorizationRecord.AHF_Number);
			AssertEquals("TYP", authorizationRecord.AHF_IDType);
			AssertEquals("AHF_Counter should has an default value", RomaniaConstants.DefaultAuthorizationCounter, authorizationRecord.AHF_Counter);

			eventMessageProcessorData = new EventMessageProcessorData("ANY", "***", logger, ediMsg, xueMsgForSuccess, submitInvoiceBatch);
			new RomaniaEInvoicingEventMessageProcessor(eventMessageProcessorData, countryFactory).Process();
			Factory.Save();

			authorizationRecord = Factory.LoadTop1<AccTransactionHeaderAuthorisationRecord>(new ZQuery(AccTransactionHeaderAuthorisationRecordSchema.AHF_ParentId, invoice.PK));
			AssertEquals("123456", authorizationRecord.AHF_Number);
			AssertEquals("AHF_Counter should has an default value", RomaniaConstants.DefaultAuthorizationCounter, authorizationRecord.AHF_Counter);
		}

		public void TestGEQMessage_IAK_NormalCase()
		{
			TestGEQMessage(true, "1000000000", "1000000000.xml", "semnatura_1000000000.xml", "");
		}

		public void TestGEQMessage_IAK_EmptyEInvNumber()
		{
			TestGEQMessage(true, "", "1000000000.xml", "semnatura_1000000000.xml", "");
		}

		public void TestGEQMessage_IAK_MissingInvoiceFile()
		{
			TestGEQMessage(true, "1000000000", "", "semnatura_1000000000.xml", "");
		}

		public void TestGEQMessage_IAK_MissingSemnaturaFile()
		{
			TestGEQMessage(true, "1000000000", "1000000000.xml", "", "");
		}

		public void TestGEQMessage_IAK_EmptyZipFile()
		{
			TestGEQMessage(true, "1000000000", "", "", "");
		}

		public void TestGEQMessage_IRJ_SingleError()
		{
			var expectedError = "E: validari globale eroare:  Nu a fost identificat cif-ul cumparatorului. In cazul in care cumparatorul este din exteriorul Romaniei(nu are CUI sau NIF), completati parametrul EXTERN=DA la upload-ul fisierului.";
			TestGEQMessage(false, "1000000001", "1000000001.xml", "semnatura_1000000001.xml", expectedError);
		}

		public void TestGEQMessage_IRJ_MultiErrors()
		{
			var expectedError = "E: validari globale SCHEMATRON  eroare: [BR-AE-02]-An Invoice that contains an Invoice line (BG-25) where the Invoiced item VAT category code (BT-151) is ...Notification email with detail error info cannot be sent, as registry 'E-Reporting Error Notification Group' has not been setup for company EDI";
			TestGEQMessage(false, "1000000002", "1000000002.xml", "semnatura_1000000002.xml", expectedError);
		}

		void TestGEQMessage(bool isIAK, string eInvNumber, string invoiceFileName, string semnaturaFileName, string expectedErrorMessage)
		{
			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("00000001", TestObjectCreator.EUR, 1m, TestObjectCreator.Debtor);
			arInvoice.Company.GC_RN_NKCountryCode = "RO";
			var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(arInvoice, status: EInvoicingPivotState.Delivered);
			var batch = TestObjectCreator.CreateEInvoicingBatchForPivot(pivot, 001, EInvoicingBatchState.Sent);
			var record = TestObjectCreator.CreateTransactionHeaderAuthorisationRecord(arInvoice);
			if (!eInvNumber.IsNullOrEmpty())
			{
				record.AHF_Number = eInvNumber;
			}

			Factory.Save();

			var xmlFileNames = new string[] { invoiceFileName, semnaturaFileName }.Where(s => !s.IsNullOrEmpty()).ToArray();
			using (var zipFileStream = CreateZipStreamFromFiles(xmlFileNames))
			{
				var encodedZipFile = Convert.ToBase64String(zipFileStream.ToByteArray());
				var xueFileName = isIAK ? "IAK_GEQ_XUETemplate.xml" : "IRJ_GEQ_XUETemplate.xml";

				var universalEvent = GetUniversalEvent(xueFileName, encodedZipFile);
				IXmlSessionTracker logger = new XmlSessionTracker(new ServiceTaskLogForTesting());

				var eventMessageProcessorData = new EventMessageProcessorData("ANY", "***", logger, EDIMessage, universalEvent, batch);
				var processor = new RomaniaEInvoicingEventMessageProcessor(eventMessageProcessorData, new RomaniaEInvoicingObjectFactory());

				processor.Process();

				if (isIAK)
				{
					AssertEquals("PostCondition: AuthorisationData", zipFileStream.ConvertToByteArrayAndCloseStream(), record.GetAHF_AuthorisationDataReader().ConvertToByteArrayAndCloseStream());
				}

				if (eInvNumber.IsNullOrEmpty())
				{
					AssertEquals("PostCondition: logger", true, logger.Logs.Any(x => x.Message == $"E-Invoicing Number is empty. The decompression process terminated."));
				}

				if (invoiceFileName.IsNullOrEmpty())
				{
					AssertEquals("PostCondition: logger", true, logger.Logs.Any(x => x.Message == $"Invoice File {eInvNumber}.xml does not exist in the returned zip file, or the filename does not match"));
				}

				if (semnaturaFileName.IsNullOrEmpty())
				{
					AssertEquals("PostCondition: logger", true, logger.Logs.Any(x => x.Message == $"Signature File semnatura_{eInvNumber}.xml does not exist in the returned zip file, or the filename does not match"));
				}

				if (!eInvNumber.IsNullOrEmpty() && !invoiceFileName.IsNullOrEmpty() )
				{
					AssertEquals("PostCondition: Error Message", expectedErrorMessage, pivot.AIP_ErrorDescription);
				}

				if (!eInvNumber.IsNullOrEmpty() && !semnaturaFileName.IsNullOrEmpty())
				{ 
					AssertEquals("PostCondition: EDocs", 1, arInvoice.DocManagerInfo.AllEDocs.Count);
					AssertEquals("PostCondition: EDocs", isIAK ? "ACC" : "ERL", arInvoice.DocManagerInfo.AllEDocs[0].DocType);
					AssertEquals("PostCondition: EDocs", isIAK ? "Signed Electronic Invoice" : "XML Failure Reasons", arInvoice.DocManagerInfo.AllEDocs[0].Description);
					using (var semnaturaFileStream = RomaniaEInvoiceTestHelper.GetEmbeddedZippedXmlFileAsStream(semnaturaFileName))
					{
						AssertEquals("PostCondition: EDocs", new ZBlob(semnaturaFileStream.ToByteArray()), arInvoice.DocManagerInfo.AllEDocs[0].ImageData);
					}
				}
			}
		}

		AccEInvoicingTransactionPivot AssertAfterProcessIAKEventMessage(string pivotStatus, string batchStatus, string expectedPivotStatus, string expectedBatchStatus)
		{
			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("00000001", TestObjectCreator.EUR, 1m, TestObjectCreator.Debtor);
			var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(arInvoice, status: pivotStatus);
			var batch = TestObjectCreator.CreateEInvoicingBatchForPivot(pivot, 001, batchStatus);

			var universalEvent = GetUniversalEvent("IAK_WithPivotStatusDLV.xml");
			var logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			var eventMessageProcessorData = new EventMessageProcessorData("ANY", "***", logger, EDIMessage, universalEvent, batch);
			var processor = new RomaniaEInvoicingEventMessageProcessor(eventMessageProcessorData, new RomaniaEInvoicingObjectFactory());

			AssertEquals("Precondition", ZDateTime.Empty, pivot.AIP_LastResponseReceivedUtc);

			processor.Process();

			AssertEquals(expectedPivotStatus, pivot.AIP_Status);
			AssertEquals(expectedBatchStatus, batch.AIB_Status);
			AssertEquals(false, logger.HasWarnings);

			return pivot;
		}

		AccEInvoicingBatch CreateSubmitPivotAndBatch(ARInvoice invoice)
		{
			var submitPivot = invoice.CreateNewEInvoicingPivot(Factory, EInvoicingPivotActionType.Submit);
			var submitBatch = TestObjectCreator.CreateEInvoicingBatch(1, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
			submitPivot.AIP_AIB = submitBatch.PK;
			return submitBatch;
		}

		UniversalEvent CreateIAKUniversalEvent(EDIMessage ediMsg, string pivotStatus, string uploadId)
		{
			ediMsg.EM_MessageText = string.Format(RomaniaEInvoiceTestHelper.GetEmbeddedResourceAsString("IAK_GEN_XUETemplate.xml"), pivotStatus, uploadId);

			var messageNumberStrategyMock = new Mock<IMessageNumberStrategy>();
			messageNumberStrategyMock.Setup(x => x.GetMessageReferenceNumber()).Returns("00000010");
			ediMsg.MessageNumberStrategy = messageNumberStrategyMock.Object;
			return ediMsg.GetEM_MessageTextReader().Parse<UniversalEvent>();
		}

		UniversalEvent GetUniversalEvent(string resourceName, params string[] values)
		{
			ediMessage = null;
			EDIMessage.EM_MessageText = string.Format(RomaniaEInvoiceTestHelper.GetEmbeddedResourceAsString(resourceName), values);

			return EDIMessage.GetEM_MessageTextReader().Parse<UniversalEvent>();
		}

		Stream CreateZipStreamFromFiles(string[] fileNames)
		{
			var outputStream = new MemoryStream();

			var zipStreamList = fileNames.Select(fileName =>
			{
				var fileStream = RomaniaEInvoiceTestHelper.GetEmbeddedZippedXmlFileAsStream(fileName);
				return new ZipStream(fileName, fileStream);
			}).ToList();

			try
			{
				var zipCreator = new ZipCreator();
				zipCreator.ZipStream(zipStreamList, outputStream);
			}
			finally
			{
				zipStreamList.ForEach(zipStream => zipStream.Stream.Dispose());
			}

			outputStream.Position = 0;
			return outputStream;
		}

		EDIMessage EDIMessage => ediMessage ?? (ediMessage = EDIMessageTestFactory.New(Factory));
		EDIMessage ediMessage;

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
