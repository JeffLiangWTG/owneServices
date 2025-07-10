using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class MakeAnEnquiryResponseProcessorTest : COLSMessageProcessorAbstractTest
	{
		public void TestProcessMakeAnEnquiryResponseMessage_Success()
		{
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
			var sessionID = ZGuid.NewZGuid();
			var outboundInterchange = Factory.New<EDIInterchange>();
			outboundInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			outboundInterchange.EI_SessionGUID = sessionID;
			var outboundMessage = colsHeader.Messages.AddNew();
			outboundMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.COLS;
			outboundMessage.EM_MessageType = AUCOLSMessageTypeList.Codes.MakeAnEnquiry;
			outboundMessage.EM_EI = outboundInterchange.PK;
			outboundMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outboundMessage.EM_Status = EDIMessage.Status.Sent;

			var inboundInterchange = Factory.New<EDIInterchange>();
			inboundInterchange.EI_SessionGUID = sessionID;
			var inboundMessage = Factory.New<EDIMessage>();
			inboundMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.COLS;
			inboundMessage.EM_MessageType = AUCOLSMessageTypeList.Codes.MakeAnEnquiry;
			inboundMessage.EM_EI = inboundInterchange.PK;
			inboundMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			inboundMessage.EM_Status = EDIMessage.Status.Queued;
			inboundMessage.EM_MessageText = "{ \"lrn\": \"LRN654321\", \"result\": \"Enquiry saved successfully.\", \"validationMessages\": null }";
			colsHeader.QCH_MessageStatus = COLSHeaderStatusList.Codes.AwaitingMakeAnEnquiryWithDocsResponse;
			processor.ProcessMessage(inboundMessage);

			CombineAssertions("Process success make an enquiry response message - AMD", () =>
			{
				AssertEquals(colsHeader.PK, inboundMessage.EM_LinkedObject.PK);
				AssertEquals(COLSHeaderStatusList.Codes.AwaitingAddAttachmentResponse, colsHeader.QCH_MessageStatus);
				AssertEquals(EDIMessage.Status.Queued, outboundAttachmentMessage1.EM_Status);
				AssertEquals("LRN654321", outboundAttachmentMessage1.EM_ApplicationReference);
				AssertEquals(EDIMessage.Status.Pending, outboundAttachmentMessage2.EM_Status);
				AssertEquals("LRN654321", outboundAttachmentMessage2.EM_ApplicationReference);
				AssertEquals("LRN654321", colsHeader.LRN);
				AssertEquals(COLSEntryStatusList.Codes.LrnActive, colsHeader.LRNStatus);
			});

			inboundMessage.EM_Status = EDIMessage.Status.Queued;
			inboundMessage.EM_LinkedObject = null;
			inboundMessage.EM_MessageText = "{ \"lrn\": \"LRN123456\", \"result\": \"SUCCESS\", \"validationMessages\": null }";
			colsHeader.QCH_MessageStatus = COLSHeaderStatusList.Codes.AwaitingMakeAnEnquiryWithoutDocsResponse;
			colsHeader.LRNCusEntryNumber.Delete();
			processor.ProcessMessage(inboundMessage);

			CombineAssertions("Process success make an enquiry response message - AME", () =>
			{
				AssertEquals(colsHeader.PK, inboundMessage.EM_LinkedObject.PK);
				AssertEquals(COLSHeaderStatusList.Codes.SuccessfulMakeAnEnquiryWithoutDocs, colsHeader.QCH_MessageStatus);
				AssertEquals("LRN123456", colsHeader.LRN);
				AssertEquals(COLSEntryStatusList.Codes.LrnInactive, colsHeader.LRNStatus);
			});
		}

		public void TestProcessMakeAnEnquiryResponseMessage_Failure()
		{
			var doc = colsHeader.EDocPivotCollection.AddNew();
			var outboundAttachmentMessage = Factory.New<COLSMessage>();
			outboundAttachmentMessage.EM_LinkedObject = doc;
			outboundAttachmentMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.COLS;
			outboundAttachmentMessage.EM_MessageType = AUCOLSMessageTypeList.Codes.AddAttachment;
			outboundAttachmentMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outboundAttachmentMessage.EM_Status = EDIMessage.Status.Pending;
			var attachment = outboundAttachmentMessage.MessageAttachments.AddNew();
			attachment.EG_FileName = doc.FileName;
			attachment.EG_StorageDocsGuid = doc.Document?.UniqueKey ?? ZGuid.Empty;
			var sessionID = ZGuid.NewZGuid();
			var outboundInterchange = Factory.New<EDIInterchange>();
			outboundInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			outboundInterchange.EI_SessionGUID = sessionID;
			var outboundMessage = colsHeader.Messages.AddNew();
			outboundMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.COLS;
			outboundMessage.EM_MessageType = AUCOLSMessageTypeList.Codes.MakeAnEnquiry;
			outboundMessage.EM_EI = outboundInterchange.PK;
			outboundMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outboundMessage.EM_Status = EDIMessage.Status.Sent;

			var inboundInterchange = Factory.New<EDIInterchange>();
			inboundInterchange.EI_SessionGUID = sessionID;
			var inboundMessage = Factory.New<EDIMessage>();
			inboundMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.COLS;
			inboundMessage.EM_MessageType = AUCOLSMessageTypeList.Codes.MakeAnEnquiry;
			inboundMessage.EM_EI = inboundInterchange.PK;
			inboundMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			inboundMessage.EM_Status = EDIMessage.Status.Queued;
			inboundMessage.EM_MessageText = "{ \"lrn\": null, \"result\": \"VALIDATION FAILED\", \"validationMessages\": \"Validation error\" }";
			colsHeader.QCH_MessageStatus = COLSHeaderStatusList.Codes.AwaitingMakeAnEnquiryWithDocsResponse;
			processor.ProcessMessage(inboundMessage);

			CombineAssertions("Process failed make an enquiry response message - AMD", () =>
			{
				AssertEquals(colsHeader.PK, inboundMessage.EM_LinkedObject.PK);
				AssertEquals(COLSHeaderStatusList.Codes.FailedMakeAnEnquiryWithDocs, colsHeader.QCH_MessageStatus);
				AssertEquals(EDIMessage.Status.Discarded, outboundAttachmentMessage.EM_Status);
				AssertEquals(COLSDocumentStatusList.Codes.Discarded, doc.CSD_MessageStatus);
			});

			inboundMessage.EM_Status = EDIMessage.Status.Queued;
			inboundMessage.EM_LinkedObject = null;
			colsHeader.QCH_MessageStatus = COLSHeaderStatusList.Codes.AwaitingMakeAnEnquiryWithoutDocsResponse;
			processor.ProcessMessage(inboundMessage);

			CombineAssertions("Process failed make an enquiry response message - AME", () =>
			{
				AssertEquals(colsHeader.PK, inboundMessage.EM_LinkedObject.PK);
				AssertEquals(COLSHeaderStatusList.Codes.FailedMakeAnEnquiryWithoutDocs, colsHeader.QCH_MessageStatus);
			});
		}

		protected override COLSMessageProcessor GetMessageProcessor() => new MakeAnEnquiryResponseProcessor(new LoggingInformation());
	}
}
