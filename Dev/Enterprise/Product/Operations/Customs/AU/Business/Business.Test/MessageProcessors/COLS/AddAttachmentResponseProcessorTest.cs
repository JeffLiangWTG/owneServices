using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(AddAttachmentResponseProcessor))]
	sealed class AddAttachmentResponseProcessorTest : COLSMessageProcessorAbstractTest
	{
		public void TestProcessAddAttachmentResponseMessage_Success()
		{
			var entryNumber = CusEntryNumber.New<CusEntryNumber>(colsHeader, CusEntryNumberTypes.Standard.LocalReferenceNumber, "AU");
			entryNumber.CE_Category = "CUS";
			entryNumber.CE_EntryNum = "123456";
			entryNumber.CE_EntryStatus = COLSEntryStatusList.Codes.LrnActive;
			Factory.Save();
			var sessionID = ZGuid.NewZGuid();
			var outboundInterchange = Factory.New<EDIInterchange>();
			outboundInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			outboundInterchange.EI_SessionGUID = sessionID;

			var cusStorageDocPivot1 = colsHeader.EDocPivotCollection.AddNew();
			cusStorageDocPivot1.CSD_ParentID = colsHeader.PK;
			cusStorageDocPivot1.CSD_MessageStatus = COLSDocumentStatusList.Codes.AwaitingDocumentSentResponse;
			var outboundAttachmentMessage = colsHeader.Messages.AddNew();
			outboundAttachmentMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.COLS;
			outboundAttachmentMessage.EM_MessageType = AUCOLSMessageTypeList.Codes.AddAttachment;
			outboundAttachmentMessage.EM_EI = outboundInterchange.PK;
			outboundAttachmentMessage.EM_MessageSubType = AUCOLSMessageSubTypeList.Codes.NormalAttachment;
			outboundAttachmentMessage.EM_LinkedObject = cusStorageDocPivot1;
			outboundAttachmentMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outboundAttachmentMessage.EM_Status = EDIMessage.Status.Sent;

			var cusStorageDocPivot2 = colsHeader.EDocPivotCollection.AddNew();
			cusStorageDocPivot2.CSD_ParentID = colsHeader.PK;
			cusStorageDocPivot2.CSD_MessageStatus = COLSDocumentStatusList.Codes.AwaitingDocumentSentResponse;

			var outboundAnotherAttachmentMessage = colsHeader.Messages.AddNew();
			outboundAnotherAttachmentMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.COLS;
			outboundAnotherAttachmentMessage.EM_MessageType = AUCOLSMessageTypeList.Codes.AddAttachment;
			outboundAnotherAttachmentMessage.EM_MessageSubType = AUCOLSMessageSubTypeList.Codes.NormalAttachment;
			outboundAnotherAttachmentMessage.EM_LinkedObject = cusStorageDocPivot2;
			outboundAnotherAttachmentMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outboundAnotherAttachmentMessage.EM_Status = EDIMessage.Status.Pending;

			var inboundInterchange = Factory.New<EDIInterchange>();
			inboundInterchange.EI_SessionGUID = sessionID;
			var inboundNormalAttachmentMessage = Factory.New<EDIMessage>();
			inboundNormalAttachmentMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.COLS;
			inboundNormalAttachmentMessage.EM_MessageType = AUCOLSMessageTypeList.Codes.AddAttachment;
			inboundNormalAttachmentMessage.EM_MessageSubType = AUCOLSMessageSubTypeList.Codes.NormalAttachment;
			inboundNormalAttachmentMessage.EM_EI = inboundInterchange.PK;
			inboundNormalAttachmentMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			inboundNormalAttachmentMessage.EM_Status = EDIMessage.Status.Queued;
			inboundNormalAttachmentMessage.EM_MessageText = "{ \"result\": \"SUCCESS\", \"validationMessages\" : null}";
			processor.ProcessMessage(inboundNormalAttachmentMessage);

			CombineAssertions("Process success normal attachment response message", () =>
			{
				AssertEquals(COLSDocumentStatusList.Codes.SuccessfulDocumentSent, cusStorageDocPivot1.CSD_MessageStatus);
				AssertEquals(EDIMessage.Status.Queued, outboundAnotherAttachmentMessage.EM_Status);
				AssertEquals(EDIMessage.Status.Received, inboundNormalAttachmentMessage.EM_Status);
				AssertEquals(COLSEntryStatusList.Codes.LrnActive, entryNumber.CE_EntryStatus);
			});

			var lodgementStatusMessages = GetLodgementStatusMessages();
			AssertEquals("Does not create a LodgementStatus message", 0, lodgementStatusMessages.Length);
			AssertEquals("QCH_MessageStatus", COLSHeaderStatusList.Codes.AwaitingAddAttachmentResponse, colsHeader.QCH_MessageStatus);
		}

		public void TestProcessAddLastAttachmentResponseMessage_Success()
		{
			var entryNumber = CusEntryNumber.New<CusEntryNumber>(colsHeader, CusEntryNumberTypes.Standard.LocalReferenceNumber, "AU");
			entryNumber.CE_Category = "CUS";
			entryNumber.CE_EntryNum = "123456";
			Factory.Save();
			var sessionID = ZGuid.NewZGuid();
			var outboundInterchange = Factory.New<EDIInterchange>();
			outboundInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			outboundInterchange.EI_SessionGUID = sessionID;

			var cusStorageDocPivot1 = colsHeader.EDocPivotCollection.AddNew();
			cusStorageDocPivot1.CSD_MessageStatus = COLSDocumentStatusList.Codes.AwaitingLastdocSentResponse;
			var outboundAttachmentMessage = colsHeader.Messages.AddNew();
			outboundAttachmentMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.COLS;
			outboundAttachmentMessage.EM_MessageType = AUCOLSMessageTypeList.Codes.AddAttachment;
			outboundAttachmentMessage.EM_EI = outboundInterchange.PK;
			outboundAttachmentMessage.EM_MessageSubType = AUCOLSMessageSubTypeList.Codes.LastdocAttachment;
			outboundAttachmentMessage.EM_LinkedObject = cusStorageDocPivot1;
			outboundAttachmentMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outboundAttachmentMessage.EM_Status = EDIMessage.Status.Sent;

			var inboundInterchange = Factory.New<EDIInterchange>();
			inboundInterchange.EI_SessionGUID = sessionID;
			var inboundLastAttachmentMessage = Factory.New<EDIMessage>();
			inboundLastAttachmentMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.COLS;
			inboundLastAttachmentMessage.EM_MessageType = AUCOLSMessageTypeList.Codes.AddAttachment;
			inboundLastAttachmentMessage.EM_MessageSubType = AUCOLSMessageSubTypeList.Codes.LastdocAttachment;
			inboundLastAttachmentMessage.EM_EI = inboundInterchange.PK;
			inboundLastAttachmentMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			inboundLastAttachmentMessage.EM_Status = EDIMessage.Status.Queued;
			inboundLastAttachmentMessage.EM_MessageText = "{ \"result\": \"SUCCESS\", \"validationMessages\" : null}";
			processor.ProcessMessage(inboundLastAttachmentMessage);

			CombineAssertions("Process success last attachment response message", () =>
			{
				AssertEquals(COLSEntryStatusList.Codes.LrnInactive, entryNumber.CE_EntryStatus);
				AssertEquals(COLSDocumentStatusList.Codes.SucessfulLastdocSent, cusStorageDocPivot1.CSD_MessageStatus);
				AssertEquals(EDIMessage.Status.Received, inboundLastAttachmentMessage.EM_Status);
			});

			var lodgementStatusMessages = GetLodgementStatusMessages();
			AssertEquals("Created one LodgementStatus message", 1, lodgementStatusMessages.Length);

			var lodgementStatusMessage = lodgementStatusMessages[0];
			AssertEquals("LodgementStatus message is Queued", EDIMessage.Status.Queued, lodgementStatusMessage.EM_Status);
			AssertEquals("LodgementStatus message is Transmit", EDIMessage.Direction.Transmit, lodgementStatusMessage.EM_ReceiveTransmit);

			AssertEquals("QCH_MessageStatus", COLSHeaderStatusList.Codes.AwaitingGetLodgementStatusResponse, colsHeader.QCH_MessageStatus);
		}

		public void TestProcessAddAttachmentResponseMessage_Failure()
		{
			var sessionID = ZGuid.NewZGuid();
			var outboundInterchange = Factory.New<EDIInterchange>();
			outboundInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			outboundInterchange.EI_SessionGUID = sessionID;

			var cusStorageDocPivot1 = colsHeader.EDocPivotCollection.AddNew();
			cusStorageDocPivot1.CSD_ParentID = colsHeader.PK;
			cusStorageDocPivot1.CSD_MessageStatus = COLSDocumentStatusList.Codes.AwaitingDocumentSentResponse;
			var outboundAttachmentMessage = colsHeader.Messages.AddNew();
			outboundAttachmentMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.COLS;
			outboundAttachmentMessage.EM_MessageType = AUCOLSMessageTypeList.Codes.AddAttachment;
			outboundAttachmentMessage.EM_EI = outboundInterchange.PK;
			outboundAttachmentMessage.EM_MessageSubType = AUCOLSMessageSubTypeList.Codes.NormalAttachment;
			outboundAttachmentMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outboundAttachmentMessage.EM_LinkedObject = cusStorageDocPivot1;
			outboundAttachmentMessage.EM_Status = EDIMessage.Status.Sent;

			var cusStorageDocPivot2 = colsHeader.EDocPivotCollection.AddNew();
			cusStorageDocPivot2.CSD_ParentID = colsHeader.PK;
			cusStorageDocPivot2.CSD_MessageStatus = COLSDocumentStatusList.Codes.AwaitingDocumentSentResponse;

			var outboundAnotherAttachmentMessage = colsHeader.Messages.AddNew();
			outboundAnotherAttachmentMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.COLS;
			outboundAnotherAttachmentMessage.EM_MessageType = AUCOLSMessageTypeList.Codes.AddAttachment;
			outboundAnotherAttachmentMessage.EM_MessageSubType = AUCOLSMessageSubTypeList.Codes.NormalAttachment;
			outboundAnotherAttachmentMessage.EM_LinkedObject = cusStorageDocPivot2;
			outboundAnotherAttachmentMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outboundAnotherAttachmentMessage.EM_Status = EDIMessage.Status.Pending;

			var inboundInterchange = Factory.New<EDIInterchange>();
			inboundInterchange.EI_SessionGUID = sessionID;
			var inboundNormalAttachmentMessage = Factory.New<EDIMessage>();
			inboundNormalAttachmentMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.COLS;
			inboundNormalAttachmentMessage.EM_MessageType = AUCOLSMessageTypeList.Codes.AddAttachment;
			inboundNormalAttachmentMessage.EM_MessageSubType = AUCOLSMessageSubTypeList.Codes.NormalAttachment;
			inboundNormalAttachmentMessage.EM_EI = inboundInterchange.PK;
			inboundNormalAttachmentMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			inboundNormalAttachmentMessage.EM_Status = EDIMessage.Status.Queued;
			inboundNormalAttachmentMessage.EM_MessageText = "{ \"result\": \"VALIDATION FAILED\", \"validationMessages\": [{ \"messageCode\": \"EM.46\",\"messageText\": \"as DocumentEngine must be attached to the LRN.\"}]}";

			processor.ProcessMessage(inboundNormalAttachmentMessage);

			CombineAssertions("Process failed normal attachment response message", () =>
			{
				AssertEquals(COLSDocumentStatusList.Codes.FailedDocumentSentResponse, cusStorageDocPivot1.CSD_MessageStatus);
				AssertEquals(EDIMessage.Status.Received, inboundNormalAttachmentMessage.EM_Status);
				AssertEquals(EDIMessage.Status.Queued, outboundAnotherAttachmentMessage.EM_Status);
			});

			var lodgementStatusMessages = GetLodgementStatusMessages();
			AssertEquals("Does not create a LodgementStatus message", 0, lodgementStatusMessages.Length);
			AssertEquals("QCH_MessageStatus", COLSHeaderStatusList.Codes.AwaitingAddAttachmentResponse, colsHeader.QCH_MessageStatus);
		}

		public void TestProcessAddLastAttachmentResponseMessage_Failure()
		{
			var sessionID = ZGuid.NewZGuid();
			var outboundInterchange = Factory.New<EDIInterchange>();
			outboundInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			outboundInterchange.EI_SessionGUID = sessionID;

			var cusStorageDocPivot1 = colsHeader.EDocPivotCollection.AddNew();
			cusStorageDocPivot1.CSD_ParentID = colsHeader.PK;
			cusStorageDocPivot1.CSD_MessageStatus = COLSDocumentStatusList.Codes.AwaitingLastdocSentResponse;
			var outboundAttachmentMessage = colsHeader.Messages.AddNew();
			outboundAttachmentMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.COLS;
			outboundAttachmentMessage.EM_MessageType = AUCOLSMessageTypeList.Codes.AddAttachment;
			outboundAttachmentMessage.EM_EI = outboundInterchange.PK;
			outboundAttachmentMessage.EM_MessageSubType = AUCOLSMessageSubTypeList.Codes.LastdocAttachment;
			outboundAttachmentMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outboundAttachmentMessage.EM_LinkedObject = cusStorageDocPivot1;
			outboundAttachmentMessage.EM_Status = EDIMessage.Status.Sent;

			var inboundInterchange = Factory.New<EDIInterchange>();
			inboundInterchange.EI_SessionGUID = sessionID;
			var inboundLastAttachmentMessage = Factory.New<EDIMessage>();
			inboundLastAttachmentMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.COLS;
			inboundLastAttachmentMessage.EM_MessageType = AUCOLSMessageTypeList.Codes.AddAttachment;
			inboundLastAttachmentMessage.EM_MessageSubType = AUCOLSMessageSubTypeList.Codes.LastdocAttachment;
			inboundLastAttachmentMessage.EM_EI = inboundInterchange.PK;
			inboundLastAttachmentMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			inboundLastAttachmentMessage.EM_Status = EDIMessage.Status.Queued;

			inboundLastAttachmentMessage.EM_MessageText = "{ \"result\": \"VALIDATION FAILED\" , \"validationMessages\": [{ \"messageCode\": \"EM.46\",\"messageText\": \"as DocumentEngine must be attached to the LRN.\"}]}";

			processor.ProcessMessage(inboundLastAttachmentMessage);

			CombineAssertions("Process failed last attachment response message", () =>
			{
				AssertEquals(COLSDocumentStatusList.Codes.FailedLastdocSentResponse, cusStorageDocPivot1.CSD_MessageStatus);
				AssertEquals(EDIMessage.Status.Received, inboundLastAttachmentMessage.EM_Status);
			});

			var lodgementStatusMessages = GetLodgementStatusMessages();
			AssertEquals("Does not create a LodgementStatus message", 0, lodgementStatusMessages.Length);
			AssertEquals("QCH_MessageStatus", COLSHeaderStatusList.Codes.FailedAddAttachment, colsHeader.QCH_MessageStatus);
		}

		public void TestProcessAddLastAttachmentResponseMessageWithoutValidationMessages_Failure()
		{
			var sessionID = ZGuid.NewZGuid();
			var outboundInterchange = Factory.New<EDIInterchange>();
			outboundInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			outboundInterchange.EI_SessionGUID = sessionID;

			var cusStorageDocPivot1 = colsHeader.EDocPivotCollection.AddNew();
			cusStorageDocPivot1.CSD_ParentID = colsHeader.PK;
			cusStorageDocPivot1.CSD_MessageStatus = COLSDocumentStatusList.Codes.AwaitingLastdocSentResponse;

			var outboundAttachmentMessage = colsHeader.Messages.AddNew();
			outboundAttachmentMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.COLS;
			outboundAttachmentMessage.EM_MessageType = AUCOLSMessageTypeList.Codes.AddAttachment;
			outboundAttachmentMessage.EM_MessageSubType = AUCOLSMessageSubTypeList.Codes.LastdocAttachment;
			outboundAttachmentMessage.EM_EI = outboundInterchange.PK;
			outboundAttachmentMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outboundAttachmentMessage.EM_LinkedObject = cusStorageDocPivot1;
			outboundAttachmentMessage.EM_Status = EDIMessage.Status.Sent;

			var inboundInterchange = Factory.New<EDIInterchange>();
			inboundInterchange.EI_SessionGUID = sessionID;
			var inboundLastAttachmentMessage = Factory.New<EDIMessage>();
			inboundLastAttachmentMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.COLS;
			inboundLastAttachmentMessage.EM_MessageType = AUCOLSMessageTypeList.Codes.AddAttachment;
			inboundLastAttachmentMessage.EM_MessageSubType = AUCOLSMessageSubTypeList.Codes.LastdocAttachment;
			inboundLastAttachmentMessage.EM_EI = inboundInterchange.PK;
			inboundLastAttachmentMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			inboundLastAttachmentMessage.EM_Status = EDIMessage.Status.Queued;
			inboundLastAttachmentMessage.EM_MessageText = "{\"result\": \"FAILED\", \"validationMessages\": null}";

			processor.ProcessMessage(inboundLastAttachmentMessage);

			AssertEquals("QCH_MessageStatus", COLSHeaderStatusList.Codes.FailedAddAttachment, colsHeader.QCH_MessageStatus);
		}

		public void TestLinkedObject()
		{
			var cusStorageDocPivot1 = colsHeader.EDocPivotCollection.AddNew();

			var outboundAttachmentMessage = Factory.New<EDIMessage>();
			outboundAttachmentMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.COLS;
			outboundAttachmentMessage.EM_MessageType = AUCOLSMessageTypeList.Codes.AddAttachment;
			outboundAttachmentMessage.EM_LinkUniqueID = cusStorageDocPivot1.PK;
			outboundAttachmentMessage.EM_LinkTable = cusStorageDocPivot1.TableName;

			AssertSame(cusStorageDocPivot1, outboundAttachmentMessage.EM_LinkedObject);
		}

		EDIMessage[] GetLodgementStatusMessages()
		{
			var requestLodgementStatusQuery = new ZQuery();
			requestLodgementStatusQuery.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.COLS);
			requestLodgementStatusQuery.AddToFilter(EDIMessageSchema.EM_LinkTable, colsHeader.TableName);
			requestLodgementStatusQuery.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, colsHeader.PK);
			requestLodgementStatusQuery.AddToFilter(EDIMessageSchema.EM_MessageType, AUCOLSMessageTypeList.Codes.LodgementStatus);
			return Factory.Load<EDIMessage>(requestLodgementStatusQuery);
		}

		protected override COLSMessageProcessor GetMessageProcessor() => new AddAttachmentResponseProcessor(new LoggingInformation());
	}
}
