using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	public class FRPNTSInboundInterchangeProcessorTest : TestCaseWithFactory
	{
		public void TestEDIMessageCreated()
		{
			var intchg = Factory.NewWithValidTestData<EDIInterchange>();
			intchg.EI_From = "FRCUS";
			intchg.EI_To = "TEST";
			intchg.EI_ApplicationCode = ApplicationCodeList.Codes.GenericMessageDelivery;
			intchg.EI_InterchangeType = GenericMessageDeliveryInterchangeTypeList.Codes.FRCustomsPNTS;
			intchg.EI_InterchangeNum = "0000000000000000001";
			intchg.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			intchg.EI_Status = EDIInterchange.Status.Queued;
			intchg.EI_IsActive = true;
			intchg.EI_GB = GlbBranch.CurrentBranch.PK;
			intchg.EI_BodyText = getMessage();
			Factory.Save();

			var processor = new FRPNTSInboundInterchangeProcessor(new BatchProcessor.LoggingInformation());
			processor.ExecuteBatch(new System.Threading.CancellationToken());

			intchg.Reload();
			AssertEquals(EDIInterchangeStatusList.Codes.Received, intchg.EI_Status);

			var msg = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, intchg.PK));
			AssertEquals(1, msg.Length);

			AssertEquals(1, intchg.ContainedMessages.Count);

			AssertEquals(msg[0].PK, intchg.ContainedMessages[0].PK);
			AssertEquals("message linked to interchange", intchg.PK, msg[0].EM_EI);
			AssertEquals("EM_ApplicationCode", FREDIMessage.ApplicationCodes.FRCustomsMessage, msg[0].EM_ApplicationCode);
			AssertEquals("EM_MessageType", MessageTypeList.Codes.STO, msg[0].EM_MessageType);
			AssertEquals("EM_MessageSubType", "016", msg[0].EM_MessageSubType);
			AssertEquals("EM_ReceiveTransmit", ReceiveTransmitList.Codes.Receive, msg[0].EM_ReceiveTransmit);
			AssertEquals("EM_MessageNum", "0000000000000000001", msg[0].EM_MessageNum);
			AssertEquals("EM_ApplicationReference", ZString.Empty, msg[0].EM_ApplicationReference);
			AssertEquals("EM_Status", "QUE", msg[0].EM_Status);
			AssertEquals("EM_HeldUntilDate", ZDateTime.Empty, msg[0].EM_HeldUntilDate);
			AssertEquals("EM_MessageText", getMessage(), msg[0].EM_MessageText);
			AssertEquals("EM_LinkTable", ZString.Empty, msg[0].EM_LinkTable);
			AssertEquals("EM_LinkUniqueID", ZGuid.Empty, msg[0].EM_LinkUniqueID);
		}

		string getMessage()
		{
			return @"<IETS016></IETS016>";
		}
	}
}
