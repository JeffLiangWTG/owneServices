using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class LodgementStatusResponseProcessorTest : COLSMessageProcessorAbstractTest
	{
		public void TestProcessLodgementStatusResponseMessage_Success()
		{
			var sessionID = ZGuid.NewZGuid();
			var outboundInterchange = Factory.New<EDIInterchange>();
			outboundInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			outboundInterchange.EI_SessionGUID = sessionID;
			var outboundMessage = colsHeader.Messages.AddNew();
			outboundMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.COLS;
			outboundMessage.EM_MessageType = AUCOLSMessageTypeList.Codes.LodgementStatus;
			outboundMessage.EM_EI = outboundInterchange.PK;
			outboundMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outboundMessage.EM_Status = EDIMessage.Status.Sent;

			var inboundInterchange = Factory.New<EDIInterchange>();
			inboundInterchange.EI_SessionGUID = sessionID;
			var inboundMessage = Factory.New<EDIMessage>();
			inboundMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.COLS;
			inboundMessage.EM_MessageType = AUCOLSMessageTypeList.Codes.LodgementStatus;
			inboundMessage.EM_EI = inboundInterchange.PK;
			inboundMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			inboundMessage.EM_Status = EDIMessage.Status.Queued;
			inboundMessage.EM_MessageText = "{ \"status\": \"Escalated\", \"receivedDate\": \"Thu 8 June 2023 at 11:33:35 AEST\", \"type\": \"ASSESSMENT\", \"resultMessage\": \"Assessment request received on Thu 8 June 2023 at 11:33:35 AEST has a status od Escalated\", \"result\": \"SUCCESS\", \"validationMessages\": null }";
			processor.ProcessMessage(inboundMessage);

			CombineAssertions("Process success Lodgement Status response message", () =>
			{
				AssertEquals(colsHeader.PK, inboundMessage.EM_LinkedObject.PK);
				AssertEquals(COLSHeaderStatusList.Codes.SuccessfulGetLodgementStatus, colsHeader.QCH_MessageStatus);
				AssertEquals(COLSLodgementStatusList.Codes.Escalated, colsHeader.QCH_LodgementStatus);
			});
		}

		public void TestProcessLodgementStatusResponseMessage_Failure()
		{
			var sessionID = ZGuid.NewZGuid();
			var outboundInterchange = Factory.New<EDIInterchange>();
			outboundInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			outboundInterchange.EI_SessionGUID = sessionID;
			var outboundMessage = colsHeader.Messages.AddNew();
			outboundMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.COLS;
			outboundMessage.EM_MessageType = AUCOLSMessageTypeList.Codes.LodgementStatus;
			outboundMessage.EM_EI = outboundInterchange.PK;
			outboundMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outboundMessage.EM_Status = EDIMessage.Status.Sent;

			var inboundInterchange = Factory.New<EDIInterchange>();
			inboundInterchange.EI_SessionGUID = sessionID;
			var inboundMessage = Factory.New<EDIMessage>();
			inboundMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.COLS;
			inboundMessage.EM_MessageType = AUCOLSMessageTypeList.Codes.LodgementStatus;
			inboundMessage.EM_EI = inboundInterchange.PK;
			inboundMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			inboundMessage.EM_Status = EDIMessage.Status.Queued;
			inboundMessage.EM_MessageText = "{ \"status\": null, \"receivedDate\": null, \"type\": null, \"resultMessage\": null, \"result\": \"VALIDATION FAILED\", \"validationMessages\": \"Validation error\" }";
			processor.ProcessMessage(inboundMessage);

			CombineAssertions("Process failed Lodgement Status response message", () =>
			{
				AssertEquals(colsHeader.PK, inboundMessage.EM_LinkedObject.PK);
				AssertEquals(COLSHeaderStatusList.Codes.FailedGetLodgementStatus, colsHeader.QCH_MessageStatus);
				AssertEquals(ZString.Empty, colsHeader.QCH_LodgementStatus);
			});
		}

		public void TestResponseStatusDescriptionGeneratesCode()
		{
			var colsHeader = Factory.New<QuarantineColsHeader>();
			AssertEquals("AIR", colsHeader.Lookups.COLSLodgementStatusList.GetCodeFromDescription("Additional Information Requested"));
			AssertEquals("AIP", colsHeader.Lookups.COLSLodgementStatusList.GetCodeFromDescription("Assessment in Progress"));
			AssertEquals("AWA", colsHeader.Lookups.COLSLodgementStatusList.GetCodeFromDescription("Awaiting Assessment"));
			AssertEquals("ARE", colsHeader.Lookups.COLSLodgementStatusList.GetCodeFromDescription("Awaiting Re-assessment"));
			AssertEquals("COM", colsHeader.Lookups.COLSLodgementStatusList.GetCodeFromDescription("Completed"));
			AssertEquals("AEP", colsHeader.Lookups.COLSLodgementStatusList.GetCodeFromDescription("Completed - AEP docs stored"));
			AssertEquals("ESC", colsHeader.Lookups.COLSLodgementStatusList.GetCodeFromDescription("Escalated"));
		}

		public void TestLodgementStatusCodeIsExtracted()
		{
			var sessionID = ZGuid.NewZGuid();
			var outboundInterchange = Factory.New<EDIInterchange>();
			outboundInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			outboundInterchange.EI_SessionGUID = sessionID;
			var outboundMessage = colsHeader.Messages.AddNew();
			outboundMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.COLS;
			outboundMessage.EM_MessageType = AUCOLSMessageTypeList.Codes.LodgementStatus;
			outboundMessage.EM_EI = outboundInterchange.PK;
			outboundMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outboundMessage.EM_Status = EDIMessage.Status.Sent;

			var inboundInterchange = Factory.New<EDIInterchange>();
			inboundInterchange.EI_SessionGUID = sessionID;
			var inboundMessage = Factory.New<EDIMessage>();
			inboundMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.COLS;
			inboundMessage.EM_MessageType = AUCOLSMessageTypeList.Codes.LodgementStatus;
			inboundMessage.EM_EI = inboundInterchange.PK;
			inboundMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			inboundMessage.EM_Status = EDIMessage.Status.Queued;
			inboundMessage.EM_MessageText = "{ \"status\": \"Completed - AEP docs stored\", \"receivedDate\": \"Tue 4 February 2025 at 10:18:05 AEDT\", \"type\": \"AEP\", \"resultMessage\": \"AEP docs received on Tue 4 February 2025 at 10:18:05 AEDT has a status of Completed.\", \"result\": \"SUCCESS\", \"messages\": null }";

			processor.ProcessMessage(inboundMessage);
			AssertEquals(colsHeader.PK, inboundMessage.EM_LinkedObject.PK);
			AssertEquals(COLSHeaderStatusList.Codes.SuccessfulGetLodgementStatus, colsHeader.QCH_MessageStatus);
			AssertEquals(COLSLodgementStatusList.Codes.CompletedAepDocsStored, colsHeader.QCH_LodgementStatus);
		}

		protected override COLSMessageProcessor GetMessageProcessor() => new LodgementStatusResponseProcessor(new LoggingInformation());
	}
}
