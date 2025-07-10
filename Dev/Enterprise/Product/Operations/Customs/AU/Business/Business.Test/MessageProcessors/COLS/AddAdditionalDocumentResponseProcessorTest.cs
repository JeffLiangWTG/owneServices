using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AddAdditionalDocumentResponseProcessorTest : COLSMessageProcessorAbstractTest
	{
		public void TestProcessAddAdditionalDocumentResponseMessage_WithoutAttachments_Success()
		{
			var entryNumber = CusEntryNumber.New<CusEntryNumber>(colsHeader, CusEntryNumberTypes.Standard.LocalReferenceNumber, Core.Constants.CountryCodes.Australia);
			entryNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			var sessionID = ZGuid.NewZGuid();
			var outboundInterchange = Factory.New<EDIInterchange>();
			outboundInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			outboundInterchange.EI_SessionGUID = sessionID;
			var outboundMessage = colsHeader.Messages.AddNew();
			outboundMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.COLS;
			outboundMessage.EM_MessageType = AUCOLSMessageTypeList.Codes.AddAdditionalDocument;
			outboundMessage.EM_EI = outboundInterchange.PK;
			outboundMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outboundMessage.EM_Status = EDIMessage.Status.Sent;

			var inboundInterchange = Factory.New<EDIInterchange>();
			inboundInterchange.EI_SessionGUID = sessionID;
			var inboundMessage = Factory.New<EDIMessage>();
			inboundMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.COLS;
			inboundMessage.EM_MessageType = AUCOLSMessageTypeList.Codes.AddAdditionalDocument;
			inboundMessage.EM_EI = inboundInterchange.PK;
			inboundMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			inboundMessage.EM_Status = EDIMessage.Status.Queued;
			inboundMessage.EM_MessageText = "{ \"result\": \"SUCCESS\", \"validationMessages\": null }";
			processor.ProcessMessage(inboundMessage);

			CombineAssertions("Process success additional document response message", () =>
			{
				AssertEquals(colsHeader.PK, inboundMessage.EM_LinkedObject.PK);
				AssertEquals(COLSEntryStatusList.Codes.LrnActive, colsHeader.LRNStatus);
				AssertEquals(COLSHeaderStatusList.Codes.SuccessfulAddDocument, colsHeader.QCH_MessageStatus);
			});
		}

		public void TestProcessAddAdditionalDocumentResponseMessage_WithAttachments_Success()
		{
			var entryNumber = CusEntryNumber.New<CusEntryNumber>(colsHeader, CusEntryNumberTypes.Standard.LocalReferenceNumber, Core.Constants.CountryCodes.Australia);
			entryNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			var sessionID = ZGuid.NewZGuid();
			var outboundInterchange = Factory.New<EDIInterchange>();
			outboundInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			outboundInterchange.EI_SessionGUID = sessionID;
			var outboundMessage = CreateColsMessage(colsHeader);
			outboundMessage.EM_MessageType = AUCOLSMessageTypeList.Codes.AddAdditionalDocument;
			outboundMessage.EM_EI = outboundInterchange.PK;
			outboundMessage.EM_Status = EDIMessage.Status.Sent;

			var doc1 = colsHeader.EDocPivotCollection.AddNew();
			var outboundAttachmentMessage1 = Factory.New<COLSMessage>();
			outboundAttachmentMessage1.EM_LinkedObject = doc1;
			outboundAttachmentMessage1.EM_ApplicationCode = EDIMessage.ApplicationCodes.COLS;
			outboundAttachmentMessage1.EM_MessageType = AUCOLSMessageTypeList.Codes.AddAttachment;
			outboundAttachmentMessage1.EM_MessageSubType = AUCOLSMessageSubTypeList.Codes.NormalAttachment;
			outboundAttachmentMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outboundAttachmentMessage1.EM_Status = EDIMessage.Status.Pending;
			var attachment1 = outboundAttachmentMessage1.MessageAttachments.AddNew();
			attachment1.EG_FileName = doc1.FileName;
			attachment1.EG_StorageDocsGuid = doc1.Document?.UniqueKey ?? ZGuid.Empty;
			var doc2 = colsHeader.EDocPivotCollection.AddNew();
			var outboundAttachmentMessage2 = Factory.New<COLSMessage>();
			outboundAttachmentMessage2.EM_LinkedObject = doc2;
			outboundAttachmentMessage2.EM_ApplicationCode = EDIMessage.ApplicationCodes.COLS;
			outboundAttachmentMessage2.EM_MessageType = AUCOLSMessageTypeList.Codes.AddAttachment;
			outboundAttachmentMessage2.EM_MessageSubType = AUCOLSMessageSubTypeList.Codes.LastdocAttachment;
			outboundAttachmentMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outboundAttachmentMessage2.EM_Status = EDIMessage.Status.Pending;
			var attachment2 = outboundAttachmentMessage2.MessageAttachments.AddNew();
			attachment2.EG_FileName = doc2.FileName;
			attachment2.EG_StorageDocsGuid = doc2.Document?.UniqueKey ?? ZGuid.Empty;

			var inboundInterchange = Factory.New<EDIInterchange>();
			inboundInterchange.EI_SessionGUID = sessionID;
			var inboundMessage = Factory.New<EDIMessage>();
			inboundMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.COLS;
			inboundMessage.EM_MessageType = AUCOLSMessageTypeList.Codes.AddAdditionalDocument;
			inboundMessage.EM_EI = inboundInterchange.PK;
			inboundMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			inboundMessage.EM_Status = EDIMessage.Status.Queued;
			inboundMessage.EM_MessageText = "{ \"result\": \"SUCCESS\", \"validationMessages\": null }";
			processor.ProcessMessage(inboundMessage);

			CombineAssertions("Process success additional document response message", () =>
			{
				AssertEquals(colsHeader.PK, inboundMessage.EM_LinkedObject.PK);
				AssertEquals(COLSEntryStatusList.Codes.LrnActive, colsHeader.LRNStatus);
				AssertEquals(COLSHeaderStatusList.Codes.AwaitingAddAttachmentResponse, colsHeader.QCH_MessageStatus);

				AssertEquals("outboundAttachmentMessage1.EM_Status", EDIMessage.Status.Queued, outboundAttachmentMessage1.EM_Status);
				AssertEquals("outboundAttachmentMessage2.EM_Status", EDIMessage.Status.Pending, outboundAttachmentMessage2.EM_Status);
			});
		}

		public void TestProcessAddAdditionalDocumentResponseMessage_Failure()
		{
			var sessionID = ZGuid.NewZGuid();
			var outboundInterchange = Factory.New<EDIInterchange>();
			outboundInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			outboundInterchange.EI_SessionGUID = sessionID;
			var outboundMessage = CreateColsMessage(colsHeader);
			outboundMessage.EM_MessageType = AUCOLSMessageTypeList.Codes.AddAdditionalDocument;
			outboundMessage.EM_EI = outboundInterchange.PK;
			outboundMessage.EM_Status = EDIMessage.Status.Sent;

			var doc1 = colsHeader.EDocPivotCollection.AddNew();
			var outboundAttachmentMessage1 = Factory.New<COLSMessage>();
			outboundAttachmentMessage1.EM_LinkedObject = doc1;
			outboundAttachmentMessage1.EM_ApplicationCode = EDIMessage.ApplicationCodes.COLS;
			outboundAttachmentMessage1.EM_MessageType = AUCOLSMessageTypeList.Codes.AddAttachment;
			outboundAttachmentMessage1.EM_MessageSubType = AUCOLSMessageSubTypeList.Codes.NormalAttachment;
			outboundAttachmentMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outboundAttachmentMessage1.EM_Status = EDIMessage.Status.Pending;
			var attachment1 = outboundAttachmentMessage1.MessageAttachments.AddNew();
			attachment1.EG_FileName = doc1.FileName;
			attachment1.EG_StorageDocsGuid = doc1.Document?.UniqueKey ?? ZGuid.Empty;
			var doc2 = colsHeader.EDocPivotCollection.AddNew();
			var outboundAttachmentMessage2 = Factory.New<COLSMessage>();
			outboundAttachmentMessage2.EM_LinkedObject = doc2;
			outboundAttachmentMessage2.EM_ApplicationCode = EDIMessage.ApplicationCodes.COLS;
			outboundAttachmentMessage2.EM_MessageType = AUCOLSMessageTypeList.Codes.AddAttachment;
			outboundAttachmentMessage2.EM_MessageSubType = AUCOLSMessageSubTypeList.Codes.LastdocAttachment;
			outboundAttachmentMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outboundAttachmentMessage2.EM_Status = EDIMessage.Status.Pending;
			var attachment2 = outboundAttachmentMessage2.MessageAttachments.AddNew();
			attachment2.EG_FileName = doc2.FileName;
			attachment2.EG_StorageDocsGuid = doc2.Document?.UniqueKey ?? ZGuid.Empty;

			var inboundInterchange = Factory.New<EDIInterchange>();
			inboundInterchange.EI_SessionGUID = sessionID;
			var inboundMessage = Factory.New<EDIMessage>();
			inboundMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.COLS;
			inboundMessage.EM_MessageType = AUCOLSMessageTypeList.Codes.AddAdditionalDocument;
			inboundMessage.EM_EI = inboundInterchange.PK;
			inboundMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			inboundMessage.EM_Status = EDIMessage.Status.Queued;
			inboundMessage.EM_MessageText = "{ \"result\": \"VALIDATION FAILED\", \"validationMessages\": [ { \"messageCode\": \"EM.58\", \"messageText\": \"Error information\" } ] }";
			processor.ProcessMessage(inboundMessage);

			CombineAssertions("Process failed additional document response message", () =>
			{
				AssertEquals(colsHeader.PK, inboundMessage.EM_LinkedObject.PK);
				AssertEquals(COLSHeaderStatusList.Codes.FailedAddDocument, colsHeader.QCH_MessageStatus);

				AssertEquals("outboundAttachmentMessage1.EM_Status", EDIMessage.Status.Discarded, outboundAttachmentMessage1.EM_Status);
				AssertEquals("outboundAttachmentMessage2.EM_Status", EDIMessage.Status.Discarded, outboundAttachmentMessage2.EM_Status);
				AssertEquals("doc1.CSD_MessageStatus", COLSDocumentStatusList.Codes.Discarded, doc1.CSD_MessageStatus);
				AssertEquals("doc2.CSD_MessageStatus", COLSDocumentStatusList.Codes.Discarded, doc2.CSD_MessageStatus);
			});
		}

		protected override COLSMessageProcessor GetMessageProcessor() => new AddAdditionalDocumentResponseProcessor(new LoggingInformation());
	}
}
