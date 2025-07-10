using System;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class UBMREQEMessageProcessorTest : CMRMessageResponseProcessorTest
	{
		public void TestLastUnderbondRequestCancelled()
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			CusHAWB hAWB = mAWB.ChildBills.AddNew();
			hAWB.CS_MessageReference = "A00000003";
			hAWB.CS_IsResponsePending = true;
			hAWB.CS_IsPrealerted = false;
			hAWB.RequestLogs.AddNew();
			outgoingMessage = (CMRUBMREQMessage)hAWB.Messages.AddNew(typeof(CMRUBMREQMessage));
			outgoingMessage.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			outgoingMessage.EM_Status = EDIMessage.Status.Sent;
			UBMREQEMessageProcessor processor = new UBMREQEMessageProcessor(logger);

			CMRUBMREQEMessage incomingMessage = Factory.New<CMRUBMREQEMessage>();
			incomingMessage.EM_MessageText = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::UBMREQE+36IG 37E9 DI05:001+11'NAD+MR+AAA374M:110:95'RFF+ACW:UBMREQ'RFF+AFM:9'RFF+ABO:A00000003/1::001'DTM+310:20050121043422:204'ERP+1'ERC+ADVICE:80:95'ERC+XX9999:6:95'FTX+AAO+++THIS TRANSACTION WAS REJECTED.'ERP+1'ERC+ERROR:80:95'ERC+CG0919:6:95'FTX+AAO+++ESTABLISHMENT ID IS INVALID'ERP+1'ERC+ERROR:80:95'ERC+CG0920:6:95'FTX+AAO+++DESTINATION ESTABLISHMENT ID IS INVALID'ERP+1'ERC+ERROR:80:95'ERC+CG1015:6:95'FTX+AAO+++UNDERBOND MOVEMENT REQUEST REJECTED - ERRORS REPORTED IN HEADER'CNT+55:003'UNT+25+000001'";

			processor.ProcessMessage(incomingMessage);
			Assert("All Requests Cancelled", hAWB.RequestLogs.Count == 0);
		}

		protected override ZString GetExpectedMessageCode() => CMRMessage.CMRMessageTypes.UBMREQE;

		protected override ZString GetExpectedMessageName() => "Underbond Movement Request Error Response - (UBMREQE)";

		protected override CMRMessageResponseProcessor GetMessageProcessor() => new UBMREQEMessageProcessor(logger);

		protected override Type IncomingMessageType => typeof(CMRUBMREQEMessage);
	}
}
