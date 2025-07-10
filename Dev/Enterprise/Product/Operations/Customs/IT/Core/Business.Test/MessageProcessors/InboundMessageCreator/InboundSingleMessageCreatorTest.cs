using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class InboundSingleMessageCreatorTest : TestCaseWithFactory
{
	public void TestGuardClause()
	{
		AssertExceptionThrown<ArgumentNullException>(
			() => new InboundSingleMessageCreator().CreateMessageForInterchange(null));
	}

	public void TestCreateMessageForInterchange()
	{
		var interchange = Factory.New<EDIInterchange>();
		interchange.EI_ApplicationCode = "ITH";
		interchange.EI_BodyText = "BODY TEXT";
		interchange.EI_InterchangeType = "H1";

		var singleMessageCreator = new InboundSingleMessageCreator();
		var createdMessage = singleMessageCreator.CreateMessageForInterchange(interchange);

		AssertNotNull("Created Message", createdMessage);
		AssertEquals("ContainedMessages Count", 1, interchange.ContainedMessages.Count);

		CombineAssertions("Created Message Fields", () =>
		{
			AssertEquals("EM_EI", interchange.PK, createdMessage.EM_EI);
			AssertEquals("EM_IsTestMessage", true, createdMessage.EM_IsTestMessage);
			AssertEquals("EM_ApplicationCode", "ITH", createdMessage.EM_ApplicationCode);
			AssertEquals("EM_ReceiveTransmit", "RCV", createdMessage.EM_ReceiveTransmit);
			AssertEquals("EM_Status", "QUE", createdMessage.EM_Status);
			AssertEquals("EM_MessageText", "BODY TEXT", createdMessage.EM_MessageText);
			AssertEquals("EM_MessageType", "H1", createdMessage.EM_MessageType);
		});
	}
}
