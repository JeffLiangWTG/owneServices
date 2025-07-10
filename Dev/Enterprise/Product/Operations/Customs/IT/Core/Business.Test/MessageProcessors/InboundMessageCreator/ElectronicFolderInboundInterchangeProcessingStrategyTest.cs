using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class ElectronicFolderInboundInterchangeProcessingStrategyTest : XTradeInboundMessageCreatorAbstractTest
{
	public void TestProcessInterchange()
	{
		var interchange = Factory.New<EDIInterchange>();
		interchange.EI_InterchangeType = "EFQ";
		interchange.EI_BodyText = "<EFQ FROM XTR>";
		interchange.EI_ApplicationCode = "ITH";

		ProcessInterchange(interchange);

		AssertContainedMessagesCount(interchange, 1);
		CombineAssertions("Assert EFR EDI Message", () =>
		{
			var createdMessage = interchange.ContainedMessages[0];
			AssertEquals("EM_EI", interchange.PK, createdMessage.EM_EI);
			Assert("EM_IsTestMessage", createdMessage.EM_IsTestMessage);
			AssertEquals("EM_ApplicationCode", interchange.EI_ApplicationCode, createdMessage.EM_ApplicationCode);
			AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Receive, createdMessage.EM_ReceiveTransmit);
			AssertEquals("EM_Status", EDIMessage.Status.Queued, createdMessage.EM_Status);
			AssertEquals("EM_MessageText", interchange.EI_BodyText, createdMessage.EM_MessageText);
			AssertEquals("EM_MessageType", "EFR", createdMessage.EM_MessageType);
			AssertEquals("EM_MessageSubType", "EFR", createdMessage.EM_MessageSubType);
		});
	}
}
