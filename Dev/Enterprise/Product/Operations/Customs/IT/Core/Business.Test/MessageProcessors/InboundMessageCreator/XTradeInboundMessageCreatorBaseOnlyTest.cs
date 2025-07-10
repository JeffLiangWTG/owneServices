using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class XTradeInboundMessageCreatorBaseOnlyTest : XTradeInboundMessageCreatorAbstractTest
{
	public void TestDetectDuplicateInterchange()
	{
		var sessionGuid = ZGuid.NewZGuid();

		var interchange = CreateInterchange(sessionGuid);
		interchange.EI_Status = "QUE";

		ProcessInterchange(interchange);
		AssertContainedMessagesCount(interchange, 1);

		interchange.EI_Status = "RCV";
		var duplicatedInterchange = CreateInterchange(sessionGuid);
		duplicatedInterchange.EI_Status = "QUE";
		AssertExceptionThrown<CustomsMessageProcessorException>("Exception expected when process a duplicated interchange", () => ProcessInterchange(duplicatedInterchange));
		AssertContainedMessagesCount(duplicatedInterchange, 0);

		var otherInterchange = CreateInterchange(ZGuid.NewZGuid());
		otherInterchange.EI_Status = "QUE";
		ProcessInterchange(otherInterchange);
		AssertContainedMessagesCount(otherInterchange, 1);
	}

	EDIInterchange CreateInterchange(ZGuid sessionGuid)
	{
		var interchange = Factory.New<EDIInterchange>();
		interchange.EI_InterchangeType = "RES";
		interchange.EI_ApplicationCode = "ITH";
		interchange.EI_ReceiveTransmit = "RCV";
		interchange.EI_From = "DIT";
		interchange.EI_To = "ITCustoms";
		interchange.EI_BodyText = "<H1 FROM XTR>";
		interchange.EI_SessionGUID = sessionGuid;

		return interchange;
	}
}
