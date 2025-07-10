using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class DefaultInboundInterchangeProcessingStrategyTest : XTradeInboundMessageCreatorAbstractTest
{
	public void TestProcessInterchange()
	{
		var interchange = Factory.New<EDIInterchange>();
		interchange.EI_InterchangeType = "RES";
		interchange.EI_BodyText = "<H1 FROM XTR>";
		interchange.EI_ApplicationCode = "ITH";

		ProcessInterchange(interchange);

		AssertContainedMessagesCount(interchange, 1);
		CombineAssertions(() => AssertCreatedMessage(interchange));
	}
}
