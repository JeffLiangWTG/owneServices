using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AddNewLodgementResponseProcessorTest : COLSMessageProcessorAbstractTest
	{
		public void TestProcessAddNewLodgementResponseMessageWithoutAttachments()
		{
			var sessionID = ZGuid.NewZGuid();
			var outboundInterchange = Factory.New<EDIInterchange>();
			outboundInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			outboundInterchange.EI_SessionGUID = sessionID;
			var outboundLodgmentMessage = colsHeader.Messages.AddNew();
			outboundLodgmentMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.COLS;
			outboundLodgmentMessage.EM_MessageType = AUCOLSMessageTypeList.Codes.AddNewLodgement;
			outboundLodgmentMessage.EM_EI = outboundInterchange.PK;
			outboundLodgmentMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outboundLodgmentMessage.EM_Status = EDIMessage.Status.Sent;

			var inboundInterchange = Factory.New<EDIInterchange>();
			inboundInterchange.EI_SessionGUID = sessionID;
			var inboundLodgmentMessage = Factory.New<EDIMessage>();
			inboundLodgmentMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.COLS;
			inboundLodgmentMessage.EM_MessageType = AUCOLSMessageTypeList.Codes.AddNewLodgement;
			inboundLodgmentMessage.EM_EI = inboundInterchange.PK;
			inboundLodgmentMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			inboundLodgmentMessage.EM_Status = EDIMessage.Status.Queued;
			inboundLodgmentMessage.EM_MessageText = "{ \"lrn\": \"LRN123456\", \"result\": \"Lodgement saved successfully\" }";
			processor.ProcessMessage(inboundLodgmentMessage);

			CombineAssertions(() =>
			{
				AssertEquals(colsHeader.PK, inboundLodgmentMessage.EM_LinkedObject.PK);
				AssertEquals("LRN123456", colsHeader.LRN);
				AssertEquals(COLSEntryStatusList.Codes.LrnActive, colsHeader.LRNStatus);
				AssertEquals(COLSHeaderStatusList.Codes.SuccessfulAddLodgement, colsHeader.QCH_MessageStatus);
			});
		}

		public void TestProcessAddNewLodgementResponseMessage_Success()
		{
			var sessionID = ZGuid.NewZGuid();
			var outboundInterchange = Factory.New<EDIInterchange>();
			outboundInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			outboundInterchange.EI_SessionGUID = sessionID;

			var outboundLodgmentMessage = CreateColsMessage(colsHeader);
			outboundLodgmentMessage.EM_MessageType = AUCOLSMessageTypeList.Codes.AddNewLodgement;
			outboundLodgmentMessage.EM_EI = outboundInterchange.PK;
			outboundLodgmentMessage.EM_Status = EDIMessage.Status.Sent;

			var attachment1 = colsHeader.EDocPivotCollection.AddNew();
			var attachment2 = colsHeader.EDocPivotCollection.AddNew();
			var outboundAttachmentMessage = CreateColsAttachmentMessage(attachment1);
			var outboundLastAttachmentMessage = CreateColsAttachmentMessage(attachment2);
			outboundLastAttachmentMessage.EM_MessageSubType = AUCOLSMessageSubTypeList.Codes.LastdocAttachment;

			var inboundInterchange = Factory.New<EDIInterchange>();
			inboundInterchange.EI_SessionGUID = sessionID;
			var inboundLodgmentMessage = Factory.New<EDIMessage>();
			inboundLodgmentMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.COLS;
			inboundLodgmentMessage.EM_MessageType = AUCOLSMessageTypeList.Codes.AddNewLodgement;
			inboundLodgmentMessage.EM_EI = inboundInterchange.PK;
			inboundLodgmentMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			inboundLodgmentMessage.EM_Status = EDIMessage.Status.Queued;
			inboundLodgmentMessage.EM_MessageText = "{ \"lrn\": \"LRN123456\", \"result\": \"Lodgement saved successfully\" }";
			processor.ProcessMessage(inboundLodgmentMessage);

			CombineAssertions("Process success lodgement response message", () =>
			{
				AssertEquals(colsHeader.PK, inboundLodgmentMessage.EM_LinkedObject.PK);
				AssertEquals("LRN123456", colsHeader.LRN);
				AssertEquals(COLSEntryStatusList.Codes.LrnActive, colsHeader.LRNStatus);
				AssertEquals(COLSHeaderStatusList.Codes.AwaitingAddAttachmentResponse, colsHeader.QCH_MessageStatus);
				AssertEquals(EDIMessage.Status.Queued, outboundAttachmentMessage.EM_Status);
				AssertEquals("EM_ApplicationReference of outboundAttachmentMessage", "LRN123456", outboundAttachmentMessage.EM_ApplicationReference);
				AssertEquals(EDIMessage.Status.Pending, outboundLastAttachmentMessage.EM_Status);
				AssertEquals("EM_ApplicationReference of outboundLastAttachmentMessage", "LRN123456", outboundLastAttachmentMessage.EM_ApplicationReference);
			});
		}

		public void TestProcessAddNewLodgementResponseMessage_Failure()
		{
			var fileContent = Encoding.UTF8.GetBytes("Test Content");
			var doc1 = declaration.DocManagerInfo.AddFileOrDocument(fileContent, "File1.txt", "txt");
			var doc2 = declaration.DocManagerInfo.AddFileOrDocument(fileContent, "File2.txt", "txt");
			var doc3 = declaration.DocManagerInfo.AddFileOrDocument(fileContent, "File3.txt", "txt");

			var attachment1 = colsHeader.EDocPivotCollection.AddNew();
			attachment1.CSD_StorageDocReference = doc1.UniqueKey;
			attachment1.CSD_DocType = "TXT";
			var attachment2 = colsHeader.EDocPivotCollection.AddNew();
			attachment2.CSD_StorageDocReference = doc2.UniqueKey;
			attachment2.CSD_DocType = "TXT";
			var attachment3 = colsHeader.EDocPivotCollection.AddNew();
			attachment3.CSD_StorageDocReference = doc3.UniqueKey;
			attachment3.CSD_DocType = "TXT";
			var sessionID = ZGuid.NewZGuid();

			var outboundInterchange = Factory.New<EDIInterchange>();
			outboundInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			outboundInterchange.EI_SessionGUID = sessionID;
			outboundInterchange.EI_InterchangeNum = "I111";
			var outboundLodgmentMessage = CreateColsMessage(colsHeader);
			outboundLodgmentMessage.EM_MessageType = AUCOLSMessageTypeList.Codes.AddNewLodgement;
			outboundLodgmentMessage.EM_EI = outboundInterchange.PK;
			outboundLodgmentMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outboundLodgmentMessage.EM_Status = EDIMessage.Status.Sent;

			var outboundAttachmentMessage1 = CreateColsAttachmentMessage(attachment1);
			outboundAttachmentMessage1.EM_Status = EDIMessage.Status.Sent;
			attachment1.CSD_MessageStatus = COLSDocumentStatusList.Codes.SuccessfulDocumentSent;

			var outboundAttachmentMessage2 = CreateColsAttachmentMessage(attachment2);
			outboundAttachmentMessage2.EM_Status = EDIMessage.Status.Pending;

			var outboundLastAttachmentMessage = CreateColsAttachmentMessage(attachment3);
			outboundLastAttachmentMessage.EM_MessageSubType = AUCOLSMessageSubTypeList.Codes.LastdocAttachment;
			outboundLastAttachmentMessage.EM_Status = EDIMessage.Status.Pending;
			Factory.Save();

			var otherFactory = new BusinessObjectFactory();

			var inboundInterchange = otherFactory.New<EDIInterchange>();
			inboundInterchange.EI_SessionGUID = sessionID;
			inboundInterchange.EI_InterchangeNum = "I112";
			var inboundLodgmentMessage = otherFactory.New<COLSMessage>();
			inboundLodgmentMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.COLS;
			inboundLodgmentMessage.EM_MessageType = AUCOLSMessageTypeList.Codes.AddNewLodgement;
			inboundLodgmentMessage.EM_EI = inboundInterchange.PK;
			inboundLodgmentMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			inboundLodgmentMessage.EM_Status = EDIMessage.Status.Queued;
			inboundLodgmentMessage.EM_MessageText = "{ \"result\": \"VALIDATION FAILED\" }";

			processor.ProcessMessage(inboundLodgmentMessage);
			otherFactory.Save();

			CombineAssertions("Process failed lodgement response message", () =>
			{
				AssertEquals(colsHeader.PK, inboundLodgmentMessage.EM_LinkedObject.PK);

				colsHeader.Reload();
				AssertEquals(COLSHeaderStatusList.Codes.FailedAddLodgement, colsHeader.QCH_MessageStatus);

				outboundAttachmentMessage1.Reload();
				AssertEquals("Sent attachment message unaffected by failure", EDIMessage.Status.Sent, outboundAttachmentMessage1.EM_Status);

				outboundAttachmentMessage2.Reload();
				AssertEquals("Pending attachment message 2 discarded", EDIMessage.Status.Discarded, outboundAttachmentMessage2.EM_Status);

				outboundLastAttachmentMessage.Reload();
				AssertEquals("Pending attachment message 3 discarded", EDIMessage.Status.Discarded, outboundLastAttachmentMessage.EM_Status);

				attachment1.Reload();
				AssertEquals("Sent attachment status unaffected by failure", COLSDocumentStatusList.Codes.SuccessfulDocumentSent, attachment1.CSD_MessageStatus);

				attachment2.Reload();
				AssertEquals("Pending attachment 2 status discarded", COLSDocumentStatusList.Codes.Discarded, attachment2.CSD_MessageStatus);

				attachment3.Reload();
				AssertEquals("Pending attachment 3 status discarded", COLSDocumentStatusList.Codes.Discarded, attachment3.CSD_MessageStatus);
			});
		}

		public void TestLRNReceivedEventIsCreated()
		{
			var sessionID = ZGuid.NewZGuid();
			var outboundInterchange = Factory.New<EDIInterchange>();
			outboundInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			outboundInterchange.EI_SessionGUID = sessionID;
			var outboundLodgmentMessage = colsHeader.Messages.AddNew();
			outboundLodgmentMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.COLS;
			outboundLodgmentMessage.EM_MessageType = AUCOLSMessageTypeList.Codes.AddNewLodgement;
			outboundLodgmentMessage.EM_EI = outboundInterchange.PK;
			outboundLodgmentMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outboundLodgmentMessage.EM_Status = EDIMessage.Status.Sent;

			var inboundInterchange = Factory.New<EDIInterchange>();
			inboundInterchange.EI_SessionGUID = sessionID;
			var inboundLodgmentMessage = Factory.New<EDIMessage>();
			inboundLodgmentMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.COLS;
			inboundLodgmentMessage.EM_MessageType = AUCOLSMessageTypeList.Codes.AddNewLodgement;
			inboundLodgmentMessage.EM_EI = inboundInterchange.PK;
			inboundLodgmentMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			inboundLodgmentMessage.EM_Status = EDIMessage.Status.Queued;
			inboundLodgmentMessage.EM_MessageText = "{ \"lrn\": \"LRN123456\", \"result\": \"Lodgement saved successfully\" }";
			processor.ProcessMessage(inboundLodgmentMessage);

			var lrnLog = colsHeader.Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(AutoEvents.StatusChange).First();
			AssertEquals("Status change event", "STC", lrnLog.SL_SE_NKEvent);
			AssertEquals("|MST=COLS|NEW=LRNRCV", lrnLog.SL_Reference);
		}

		protected override COLSMessageProcessor GetMessageProcessor() => new AddNewLodgementResponseProcessor(new LoggingInformation());
	}
}
