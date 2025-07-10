using System;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.AE.Business.Testing;

sealed class XTTERRDataProviderTest : Customs.Business.Testing.DataProviderTestCase<XTTERRDataProvider, IXTTERRDataProvider>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("The input parameter cannot be null.", () => new XTTERRDataProvider(null));

		var message = Factory.New<EDIMessage>();
		message.EM_Status = EDIMessageStatusList.Codes.PreProcessedOK;

		AssertNoExceptionThrown(() => new XTTERRDataProvider(message));
	}

	public void TestOutgoingAccessReference()
	{
		var (inboundMessage, outboundInterchange) = CreatePreparedData();
		var provider = new XTTERRDataProvider(inboundMessage);

		AssertEquals(outboundInterchange.EI_InterchangeNum, provider.OutgoingAccessReference);
	}

	protected override XTTERRDataProvider GetProvider()
	{
		var (inboundMessage, _) = CreatePreparedData();
		return new XTTERRDataProvider(inboundMessage);
	}

	(AEEDIMessage inboundMessage, EDIInterchange outboundInterchange) CreatePreparedData()
	{
		var sessionId = ZGuid.NewZGuid();
		var outboundMessageLinkedObject = Factory.New<LinkedObjectForTest>();

		var outboundInterchange = Factory.New<EDIInterchange>();
		outboundInterchange.EI_ApplicationCode = ApplicationCodeList.Codes.UAECustoms;
		outboundInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
		outboundInterchange.EI_Status = EDIInterchangeStatusList.Codes.Sent;
		outboundInterchange.EI_InterchangeNum = "Interchange1";
		outboundInterchange.EI_SessionGUID = sessionId;

		var outboundMessage = Factory.New<AEEDIMessage>();
		outboundMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Transmit;
		outboundMessage.EM_LinkedObject = outboundMessageLinkedObject;
		outboundInterchange.ContainedMessages.Add(outboundMessage);

		var inboundInterchange = Factory.New<EDIInterchange>();
		inboundInterchange.EI_ApplicationCode = ApplicationCodeList.Codes.UAECustoms;
		inboundInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
		inboundInterchange.EI_InterchangeNum = "Interchange2";
		inboundInterchange.EI_SessionGUID = sessionId;

		var inboundMessage = Factory.New<AEEDIMessage>();
		inboundMessage.EM_Status = EDIMessageStatusList.Codes.Queued;
		inboundInterchange.ContainedMessages.Add(inboundMessage);

		return (inboundMessage, outboundInterchange);
	}
}
