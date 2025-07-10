using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Business.Testing;

sealed class BusinessObjectFindHelperTest : TestCaseWithFactory
{
	[ExpectNoExceptions]
	public void TestTestGetOutboundMessage_EmptyOutboundInterchangeNumber()
	{
		NUnit.Framework.Assert.That(Factory.GetOutboundMessage(ZString.Empty), Is.Null);
	}

	public void TestGetOutboundMessage_ValidOutboundInterchangeNumber()
	{
		var (outboundInterchange, outboundMessage) = CreateMessageAndInterchangeMessage();
		outboundInterchange.EI_InterchangeNum = "Interchange1";
		outboundInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;

		AssertEquals(outboundMessage, Factory.GetOutboundMessage("Interchange1"));
	}

	public void TestGetOutboundInterchangeBySessionGUID_ValidSessionGUID()
	{
		var sessionID = ZGuid.NewZGuid();
		var (outboundInterchange, outboundMessage) = CreateMessageAndInterchangeMessage();
		outboundInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
		outboundInterchange.EI_SessionGUID = sessionID;
		outboundMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Transmit;

		var (inboundInterchange, inboundMessage) = CreateMessageAndInterchangeMessage();
		inboundInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
		inboundInterchange.EI_SessionGUID = sessionID;
		inboundMessage.EM_Status = EDIMessageStatusList.Codes.PreProcessedOK;

		AssertEquals(outboundInterchange, Factory.GetOutboundInterchangeBySessionGUID(inboundInterchange.EI_SessionGUID));
	}

	public void TestGetOutboundMessageFromOutboundInterchange()
	{
		var (outboundInterchange, outboundMessage) = CreateMessageAndInterchangeMessage();

		AssertEquals(outboundMessage, Factory.GetOutboundMessageFromOutboundInterchange(outboundInterchange));
	}

	public void TestGetMessageAttachee()
	{
		var messageAttachee1 = Factory.New<LinkedObjectForTest>();
		messageAttachee1.Z0_Code = "ABC";

		var messageAttachee2 = Factory.New<LinkedObjectForTest>();
		messageAttachee2.Z0_Code = "XYZ";

		CombineAssertions(() =>
		{
			AssertEquals("Message Attachee loaded", messageAttachee1, Factory.GetMessageAttachee<LinkedObjectForTest>(DummyBusinessObjectSchema.Z0_Code, "ABC", getLatest: false));
			NUnit.Framework.Assert.That(Factory.GetMessageAttachee<LinkedObjectForTest>(DummyBusinessObjectSchema.Z0_Code, ZString.Empty), Is.Null);
		});
	}

	(EDIInterchange interchangeMessage, EDIMessage message) CreateMessageAndInterchangeMessage()
	{
		var interchangeMessage = Factory.New<EDIInterchange>();
		interchangeMessage.EI_ApplicationCode = ApplicationCodeList.Codes.UAECustoms;
		interchangeMessage.EI_Status = EDIInterchangeStatusList.Codes.Sent;
		var message = interchangeMessage.ContainedMessages.AddNew();

		return (interchangeMessage, message);
	}
}
