using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class InboundMessageCreatorTest : InboundMessageCreatorBaseTest
{
	public void TestCreateMessagesForInterchange()
	{
		var interchange = Factory.New<EDIInterchange>();
		interchange.EI_InterchangeType = SADConstants.CustomsInterchangeType.IrispX;

		interchange.EI_BodyText = "SAMPLE HEADER TEXT\nSAMPLE BODY TEXT";
		interchange.EI_ApplicationCode = "ITM";

		ProcessInterchange(interchange);

		AssertContainedMessagesCount(interchange, 1);
		CombineAssertions(() => AssertCreatedMessage(interchange));

		var interchange2 = Factory.New<EDIInterchange>();
		interchange2.EI_InterchangeType = "H1";
		interchange2.EI_BodyText = "SAMPLE HEADER TEXT\nSAMPLE BODY TEXT";
		interchange2.EI_ApplicationCode = "ITH";

		ProcessInterchange(interchange2);

		AssertContainedMessagesCount(interchange2, 1);
		CombineAssertions(() => AssertCreatedMessage(interchange2));
	}

	protected override IInboundMessageCreator GetInboundMessageCreator() => new InboundMessageCreator();
}

abstract class InboundMessageCreatorBaseTest : TestCaseWithFactory
{
	public void TestCreateMessagesForInterchangeThrowsException()
	{
		AssertExceptionThrown<ArgumentNullException>(() => ProcessInterchange(null));
	}

	protected void ProcessInterchange(EDIInterchange interchange)
	{
		var messageCreator = GetInboundMessageCreator();
		messageCreator.CreateMessagesForInterchange(interchange);
	}

	protected abstract IInboundMessageCreator GetInboundMessageCreator();

	protected void AssertContainedMessagesCount(EDIInterchange interchange, ZInt expectedCount)
	{
		AssertContainedMessagesCount(string.Empty, interchange, expectedCount);
	}

	protected void AssertContainedMessagesCount(string message, EDIInterchange interchange, ZInt expectedCount)
	{
		var finalMessage = new ZStringBuilder()
			.AppendIfNotEmpty(message)
			.Append("ContainedMessages.Count")
			.ToStringWithDelimiterBetweenAppends(", ");
		AssertEquals(finalMessage, expectedCount, interchange.ContainedMessages.Count);
	}

	protected void AssertCreatedMessage(EDIInterchange interchange)
	{
		var createdMessage = interchange.ContainedMessages[0];
		AssertEquals("EM_EI", interchange.PK, createdMessage.EM_EI);
		Assert("EM_IsTestMessage", createdMessage.EM_IsTestMessage);
		AssertEquals("EM_ApplicationCode", interchange.EI_ApplicationCode, createdMessage.EM_ApplicationCode);
		AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Receive, createdMessage.EM_ReceiveTransmit);
		AssertEquals("EM_Status", EDIMessage.Status.Queued, createdMessage.EM_Status);
		AssertEquals("EM_MessageText", interchange.EI_BodyText, createdMessage.EM_MessageText);
		AssertEquals("EM_MessageType", interchange.EI_InterchangeType, createdMessage.EM_MessageType);
	}
}
