using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.NCTS.Messaging;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	class GuaranteeMessageSenderTest : TestCaseWithFactory
	{
		public void TestSendIE224()
		{
			var header = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			header.CPH_StartDate = ZDate.Today.AddYears(-1);
			header.CPH_EndDate = ZDate.Today.AddYears(20);
			var sendingAction = new GuaranteeVoucherSoldSendingAction(header);
			sendingAction.MessageType = NCTSOutgoingMessageTypeList.Codes.GuaranteeVoucherSold;
			var orgHeader = NCTSTestHelper.CreateOrgHeaderForTest(Factory);
			sendingAction.HolderOfTransitProcedure = orgHeader.MainAddress.Header.OH_Code;

			var sender = new GuaranteeMessageSender(sendingAction);
			sender.Send();

			AssertEquals("Message count", 1, header.Messages.Count);
			CombineAssertions(() =>
			{
				AssertEquals("Application code", "IEN", header.Messages[0].EM_ApplicationCode);
				AssertEquals("Message Type", "224", header.Messages[0].EM_MessageType);
				AssertEquals("Direction", "TRX", header.Messages[0].EM_ReceiveTransmit);
				AssertEquals("Status", "QUE", header.Messages[0].EM_Status);
				AssertStartsWith("Message Text", "<q1:CC224C xmlns:q1=\"http://ncts.dgtaxud.ec\">", header.Messages[0].EM_MessageText);
			});
		}
	}
}
