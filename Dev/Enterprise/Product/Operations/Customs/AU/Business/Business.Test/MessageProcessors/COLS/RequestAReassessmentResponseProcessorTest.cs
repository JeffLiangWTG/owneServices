using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class RequestAReassessmentResponseProcessorTest : COLSMessageProcessorAbstractTest
	{
		public void TestProcessRequestAReassessmentResponseMessage_ARN_Success()
		{
			var entryNumber = CusEntryNumber.New<CusEntryNumber>(colsHeader, CusEntryNumberTypes.Standard.LocalReferenceNumber, Core.Constants.CountryCodes.Australia);
			entryNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			entryNumber.CE_EntryNum = "OLDLRN1234";
			colsHeader.QCH_MessageStatus = COLSHeaderStatusList.Codes.AwaitingReassessmentWithoutDocsResponse;
			Factory.Save();

			var sessionID = ZGuid.NewZGuid();
			var outboundInterchange = Factory.New<EDIInterchange>();
			outboundInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			outboundInterchange.EI_SessionGUID = sessionID;
			outboundInterchange.EI_From = "AAA";
			outboundInterchange.EI_To = "AUCustoms";
			var outboundMessage = CreateColsMessage(colsHeader);
			outboundMessage.EM_MessageType = AUCOLSMessageTypeList.Codes.RequestAReassessment;
			outboundMessage.EM_EI = outboundInterchange.PK;
			outboundMessage.EM_Status = EDIMessage.Status.Sent;

			var inboundInterchange = Factory.New<EDIInterchange>();
			inboundInterchange.EI_SessionGUID = sessionID;
			inboundInterchange.EI_From = "AUCustoms";
			inboundInterchange.EI_To = "AAA";
			var inboundMessage = Factory.New<EDIMessage>();
			inboundMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.COLS;
			inboundMessage.EM_MessageType = AUCOLSMessageTypeList.Codes.RequestAReassessment;
			inboundMessage.EM_EI = inboundInterchange.PK;
			inboundMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			inboundMessage.EM_Status = EDIMessage.Status.Queued;
			inboundMessage.EM_MessageText = "{ \"generatedLrn\": \"LRN123456\", \"result\": \"SUCCESS\", \"messages\": null }";
			processor.ProcessMessage(inboundMessage);

			CombineAssertions("Process success Request A Reassessment response message when QCH_MessageStatus = ARN", () =>
			{
				AssertEquals(colsHeader.PK, inboundMessage.EM_LinkedObject.PK);
				AssertEquals(COLSHeaderStatusList.Codes.SuccessfulReassessmentWithoutDocs, colsHeader.QCH_MessageStatus);
				AssertEquals("LRN123456", colsHeader.LRN);
				AssertEquals(COLSEntryStatusList.Codes.LrnInactive, colsHeader.LRNStatus);
				AssertEquals(CusEntryNumberTypes.Australia.InactiveLodgmentReferenceNumber, entryNumber.CE_EntryType);
			});
		}

		public void TestProcessRequestAReassessmentResponseMessage_ARW_Success()
		{
			var entryNumber = CusEntryNumber.New<CusEntryNumber>(colsHeader, CusEntryNumberTypes.Standard.LocalReferenceNumber, Core.Constants.CountryCodes.Australia);
			entryNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			entryNumber.CE_EntryNum = "OLDLRN1234";
			colsHeader.QCH_MessageStatus = COLSHeaderStatusList.Codes.AwaitingReassessmentWithDocsResponse;
			Factory.Save();

			var sessionID = ZGuid.NewZGuid();
			var outboundInterchange = Factory.New<EDIInterchange>();
			outboundInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			outboundInterchange.EI_SessionGUID = sessionID;
			outboundInterchange.EI_From = "AAA";
			outboundInterchange.EI_To = "AUCustoms";
			var outboundMessage = CreateColsMessage(colsHeader);
			outboundMessage.EM_MessageType = AUCOLSMessageTypeList.Codes.RequestAReassessment;
			outboundMessage.EM_EI = outboundInterchange.PK;
			outboundMessage.EM_Status = EDIMessage.Status.Sent;

			var attachment1 = colsHeader.EDocPivotCollection.AddNew();
			var attachment2 = colsHeader.EDocPivotCollection.AddNew();
			var outboundAttachmentMessage = CreateColsAttachmentMessage(attachment1);
			var outboundLastAttachmentMessage = CreateColsAttachmentMessage(attachment2);
			outboundLastAttachmentMessage.EM_MessageSubType = AUCOLSMessageSubTypeList.Codes.LastdocAttachment;

			var inboundInterchange = Factory.New<EDIInterchange>();
			inboundInterchange.EI_SessionGUID = sessionID;
			inboundInterchange.EI_From = "AUCustoms";
			inboundInterchange.EI_To = "AAA";
			var inboundMessage = Factory.New<EDIMessage>();
			inboundMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.COLS;
			inboundMessage.EM_MessageType = AUCOLSMessageTypeList.Codes.RequestAReassessment;
			inboundMessage.EM_EI = inboundInterchange.PK;
			inboundMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			inboundMessage.EM_Status = EDIMessage.Status.Queued;
			inboundMessage.EM_MessageText = "{ \"generatedLrn\": \"LRN123456\", \"result\": \"SUCCESS\", \"messages\": null }";
			processor.ProcessMessage(inboundMessage);

			CombineAssertions("Process success Request A Reassessment response message when QCH_MessageStatus = ARW", () =>
			{
				AssertEquals(colsHeader.PK, inboundMessage.EM_LinkedObject.PK);
				AssertEquals(COLSHeaderStatusList.Codes.AwaitingAddAttachmentResponse, colsHeader.QCH_MessageStatus);
				AssertEquals("LRN123456", colsHeader.LRN);
				AssertEquals(COLSEntryStatusList.Codes.LrnActive, colsHeader.LRNStatus);
				AssertEquals(CusEntryNumberTypes.Australia.InactiveLodgmentReferenceNumber, entryNumber.CE_EntryType);
				AssertEquals(EDIMessage.Status.Queued, outboundAttachmentMessage.EM_Status);
				AssertEquals("EM_ApplicationReference of outboundAttachmentMessage", "LRN123456", outboundAttachmentMessage.EM_ApplicationReference);
				AssertEquals(EDIMessage.Status.Pending, outboundLastAttachmentMessage.EM_Status);
				AssertEquals("EM_ApplicationReference of outboundLastAttachmentMessage", "LRN123456", outboundLastAttachmentMessage.EM_ApplicationReference);
			});
		}

		public void TestProcessRequestAReassessmentResponseMessage_ARN_Failure()
		{
			var sessionID = ZGuid.NewZGuid();
			var outboundInterchange = Factory.New<EDIInterchange>();
			outboundInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			outboundInterchange.EI_SessionGUID = sessionID;
			var outboundMessage = CreateColsMessage(colsHeader);
			outboundMessage.EM_MessageType = AUCOLSMessageTypeList.Codes.RequestAReassessment;
			outboundMessage.EM_EI = outboundInterchange.PK;
			outboundMessage.EM_Status = EDIMessage.Status.Sent;

			var inboundInterchange = Factory.New<EDIInterchange>();
			inboundInterchange.EI_SessionGUID = sessionID;

			var inboundMessage = Factory.New<EDIMessage>();
			inboundMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.COLS;
			inboundMessage.EM_MessageType = AUCOLSMessageTypeList.Codes.RequestAReassessment;
			inboundMessage.EM_EI = inboundInterchange.PK;
			inboundMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			inboundMessage.EM_Status = EDIMessage.Status.Queued;
			inboundMessage.EM_MessageText = "{ \"generatedLrn\": null, \"result\": \"VALIDATION FAILED\", \"messages\": [ { \"messageCode\": \"EM.58\", \"messageText\": \"Error information\" } ] }";
			colsHeader.QCH_MessageStatus = COLSHeaderStatusList.Codes.AwaitingReassessmentWithoutDocsResponse;
			processor.ProcessMessage(inboundMessage);

			CombineAssertions("Process failure Request A Reassessment response message when QCH_MessageStatus = ARN", () =>
			{
				AssertEquals(colsHeader.PK, inboundMessage.EM_LinkedObject.PK);
				AssertEquals(COLSHeaderStatusList.Codes.FailedReassessmentWithoutDocs, colsHeader.QCH_MessageStatus);
			});
		}

		public void TestProcessRequestAReassessmentResponseMessage_ARW_Failure()
		{
			var sessionID = ZGuid.NewZGuid();
			var outboundInterchange = Factory.New<EDIInterchange>();
			outboundInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			outboundInterchange.EI_SessionGUID = sessionID;
			var outboundMessage = CreateColsMessage(colsHeader);
			outboundMessage.EM_MessageType = AUCOLSMessageTypeList.Codes.RequestAReassessment;
			outboundMessage.EM_EI = outboundInterchange.PK;
			outboundMessage.EM_Status = EDIMessage.Status.Sent;

			var attachment1 = colsHeader.EDocPivotCollection.AddNew();
			var attachment2 = colsHeader.EDocPivotCollection.AddNew();
			var outboundAttachmentMessage = CreateColsAttachmentMessage(attachment1);
			var outboundLastAttachmentMessage = CreateColsAttachmentMessage(attachment2);
			outboundLastAttachmentMessage.EM_MessageSubType = AUCOLSMessageSubTypeList.Codes.LastdocAttachment;

			var inboundInterchange = Factory.New<EDIInterchange>();
			inboundInterchange.EI_SessionGUID = sessionID;

			var inboundMessage = Factory.New<EDIMessage>();
			inboundMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.COLS;
			inboundMessage.EM_MessageType = AUCOLSMessageTypeList.Codes.RequestAReassessment;
			inboundMessage.EM_EI = inboundInterchange.PK;
			inboundMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			inboundMessage.EM_Status = EDIMessage.Status.Queued;
			inboundMessage.EM_MessageText = "{ \"generatedLrn\": null, \"result\": \"VALIDATION FAILED\", \"messages\": [ { \"messageCode\": \"EM.58\", \"messageText\": \"Error information\" } ] }";
			colsHeader.QCH_MessageStatus = COLSHeaderStatusList.Codes.AwaitingReassessmentWithDocsResponse;
			processor.ProcessMessage(inboundMessage);

			CombineAssertions("Process failure Request A Reassessment response message when QCH_MessageStatus = ARN", () =>
			{
				AssertEquals(colsHeader.PK, inboundMessage.EM_LinkedObject.PK);
				AssertEquals(COLSHeaderStatusList.Codes.FailedReassessmentWithDocs, colsHeader.QCH_MessageStatus);
				AssertEquals(EDIMessage.Status.Discarded, outboundAttachmentMessage.EM_Status);
				AssertEquals(EDIMessage.Status.Discarded, outboundLastAttachmentMessage.EM_Status);
			});
		}

		public void TestProcessRequestAReassessmentResponseMessage_SuccessResultResponse()
		{
			var entryNumber = CusEntryNumber.New<CusEntryNumber>(colsHeader, CusEntryNumberTypes.Standard.LocalReferenceNumber, Core.Constants.CountryCodes.Australia);
			entryNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			entryNumber.CE_EntryNum = "OLDLRN1234";
			colsHeader.QCH_MessageStatus = COLSHeaderStatusList.Codes.AwaitingReassessmentWithoutDocsResponse;
			Factory.Save();

			var sessionID = ZGuid.NewZGuid();
			var outboundInterchange = Factory.New<EDIInterchange>();
			outboundInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			outboundInterchange.EI_SessionGUID = sessionID;
			outboundInterchange.EI_From = "AAA";
			outboundInterchange.EI_To = "AUCustoms";
			var outboundMessage = CreateColsMessage(colsHeader);
			outboundMessage.EM_MessageType = AUCOLSMessageTypeList.Codes.RequestAReassessment;
			outboundMessage.EM_EI = outboundInterchange.PK;
			outboundMessage.EM_Status = EDIMessage.Status.Sent;

			var inboundInterchange = Factory.New<EDIInterchange>();
			inboundInterchange.EI_SessionGUID = sessionID;
			inboundInterchange.EI_From = "AUCustoms";
			inboundInterchange.EI_To = "AAA";
			var inboundMessage = Factory.New<EDIMessage>();
			inboundMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.COLS;
			inboundMessage.EM_MessageType = AUCOLSMessageTypeList.Codes.RequestAReassessment;
			inboundMessage.EM_EI = inboundInterchange.PK;
			inboundMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			inboundMessage.EM_Status = EDIMessage.Status.Queued;
			inboundMessage.EM_MessageText = "{ \"generatedLrn\": \"LRN123456\", \"result\": \"Reassessment request saved succesfully.\", \"messages\": null }";
			processor.ProcessMessage(inboundMessage);

			CombineAssertions("Process success Request A Reassessment response message when QCH_MessageStatus = ARN", () =>
			{
				AssertEquals(colsHeader.PK, inboundMessage.EM_LinkedObject.PK);
				AssertEquals(COLSHeaderStatusList.Codes.SuccessfulReassessmentWithoutDocs, colsHeader.QCH_MessageStatus);
				AssertEquals("LRN123456", colsHeader.LRN);
				AssertEquals(COLSEntryStatusList.Codes.LrnInactive, colsHeader.LRNStatus);
				AssertEquals(CusEntryNumberTypes.Australia.InactiveLodgmentReferenceNumber, entryNumber.CE_EntryType);
			});
		}

		protected override COLSMessageProcessor GetMessageProcessor() => new RequestAReassessmentResponseProcessor(new LoggingInformation());
	}
}
