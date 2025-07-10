using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class SwitchAepLodgementResponseProcessorTest : COLSMessageProcessorAbstractTest
	{
		public void TestProcessSwitchAepLodgementResponseMessage_Success()
		{
			var entryNumObject = Factory.New<CusEntryNumber>();
			entryNumObject.CE_ParentID = colsHeader.PK;
			entryNumObject.CE_EntryType = CusEntryNumberTypes.Standard.LocalReferenceNumber;
			entryNumObject.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			entryNumObject.CE_EntryStatus = COLSEntryStatusList.Codes.LrnActive;
			entryNumObject.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			entryNumObject.CE_EntryNum = "LRN1";

			var sessionID = ZGuid.NewZGuid();
			var outboundInterchange = Factory.New<EDIInterchange>();
			outboundInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			outboundInterchange.EI_SessionGUID = sessionID;
			var outboundMessage = colsHeader.Messages.AddNew();
			outboundMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.COLS;
			outboundMessage.EM_MessageType = AUCOLSMessageTypeList.Codes.SwitchAepLodgement;
			outboundMessage.EM_EI = outboundInterchange.PK;
			outboundMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outboundMessage.EM_Status = EDIMessage.Status.Sent;

			var inboundInterchange = Factory.New<EDIInterchange>();
			inboundInterchange.EI_SessionGUID = sessionID;
			var inboundMessage = Factory.New<EDIMessage>();
			inboundMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.COLS;
			inboundMessage.EM_MessageType = AUCOLSMessageTypeList.Codes.SwitchAepLodgement;
			inboundMessage.EM_EI = inboundInterchange.PK;
			inboundMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			inboundMessage.EM_Status = EDIMessage.Status.Queued;
			inboundMessage.EM_MessageText = "{ \"result\": \"SUCCESS\", \"lrn\": \"LRN123456\", \"validationMessages\": null }";
			processor.ProcessMessage(inboundMessage);

			CombineAssertions("Process success Switch Aep Lodgement Response message", () =>
			{
				AssertEquals(colsHeader.PK, inboundMessage.EM_LinkedObject.PK);
				AssertEquals("LRN123456", colsHeader.LRN);
				AssertEquals(CusEntryNumberTypes.Australia.InactiveLodgmentReferenceNumber, entryNumObject.CE_EntryType);
				AssertEquals(COLSEntryStatusList.Codes.LrnInactive, colsHeader.LRNStatus);
				AssertEquals(COLSHeaderStatusList.Codes.SuccessfulSwitchAepRequest, colsHeader.QCH_MessageStatus);
			});
		}

		public void TestProcessSwitchAepLodgementResponseMessage_Failure()
		{
			var sessionID = ZGuid.NewZGuid();
			var outboundInterchange = Factory.New<EDIInterchange>();
			outboundInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			outboundInterchange.EI_SessionGUID = sessionID;
			var outboundMessage = colsHeader.Messages.AddNew();
			outboundMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.COLS;
			outboundMessage.EM_MessageType = AUCOLSMessageTypeList.Codes.SwitchAepLodgement;
			outboundMessage.EM_EI = outboundInterchange.PK;
			outboundMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outboundMessage.EM_Status = EDIMessage.Status.Sent;

			var inboundInterchange = Factory.New<EDIInterchange>();
			inboundInterchange.EI_SessionGUID = sessionID;
			var inboundMessage = Factory.New<EDIMessage>();
			inboundMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.COLS;
			inboundMessage.EM_MessageType = AUCOLSMessageTypeList.Codes.SwitchAepLodgement;
			inboundMessage.EM_EI = inboundInterchange.PK;
			inboundMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			inboundMessage.EM_Status = EDIMessage.Status.Queued;
			inboundMessage.EM_MessageText = "{ \"result\": \"VALIDATION FAILED\", \"lrn\": null, \"validationMessages\": [ { \"messageCode\": \"EM.61\", \"messageText\": \"The LRN is not an AEPCOMM/NCCC entry and cannot be processed.\" } ] }";
			processor.ProcessMessage(inboundMessage);

			CombineAssertions("Process failed Switch Aep Lodgement Response message", () =>
			{
				AssertEquals(colsHeader.PK, inboundMessage.EM_LinkedObject.PK);
				AssertEquals(COLSHeaderStatusList.Codes.FailedSwitchAepRequest, colsHeader.QCH_MessageStatus);
			});
		}

		protected override COLSMessageProcessor GetMessageProcessor() => new SwitchAepLodgementResponseProcessor(new LoggingInformation());
	}
}
