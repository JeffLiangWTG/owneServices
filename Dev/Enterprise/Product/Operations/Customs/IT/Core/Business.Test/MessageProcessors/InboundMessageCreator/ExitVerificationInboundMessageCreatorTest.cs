using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class ExitVerificationInboundMessageCreatorTest : InboundMessageCreatorBaseTest
{
	public void TestCreateMessagesForInterchangeFilledPan()
	{
		var interchange = GetNewInterchange(pan: "123456");
		ProcessInterchange(interchange);

		AssertContainedMessagesCount(interchange, 1);
		CombineAssertions(() =>
		{
			AssertCreatedMessage(interchange);
			AssertExitVerificationSpecificData(interchange.ContainedMessages[0], "123456");
		});
	}

	public void TestCreateMessagesForInterchangeEmptyPan()
	{
		var interchange = GetNewInterchange(pan: ZString.Empty);

		CombineAssertions(() =>
		{
			AssertExceptionThrown<UnableToInterpretInterchangeException>(() => ProcessInterchange(interchange));
			AssertContainedMessagesCount(interchange, 0);
		});
	}

	EDIInterchange GetNewInterchange(ZString pan)
	{
		var interchange = Factory.New<EDIInterchange>();
		interchange.EI_HeaderText = $"<ITMessage><PAN>{pan}</PAN></ITMessage>";
		return interchange;
	}

	protected override IInboundMessageCreator GetInboundMessageCreator() => new ExitVerificationInboundMessageCreator();

	void AssertExitVerificationSpecificData(EDIMessage createdMessage, ZString expectedMessageNum)
	{
		AssertEquals("EM_MessageNum", expectedMessageNum, createdMessage.EM_MessageNum);
	}
}
