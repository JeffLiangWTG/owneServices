using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.DE.NCTS.Business.Testing;

public class AutoSendNctsP5MessageProcessorTest : EU.NCTS.Business.Testing.AutoSendNctsP5MessageProcessorAbstractTest
{
	protected override void AssertEntryAndMessageResultForArrivalEndToEndTest(EU.NCTS.Business.NctsHeader nctsHeader)
	{
		CombineAssertions(() =>
		{
			AssertEquals("SNT", nctsHeader.EffectiveMessageStatus);

			AssertEquals(1, nctsHeader.Messages.Count);

			var message = nctsHeader.Messages[0];
			AssertEquals(nameof(message.EM_ApplicationCode), "DEA", message.EM_ApplicationCode);
			AssertEquals(nameof(message.EM_MessageSubType), "DES", message.EM_MessageSubType);
			AssertEquals(nameof(message.EM_ApplicationReference), "DETSNF", message.EM_ApplicationReference);
			AssertEquals(ReceiveTransmitList.Codes.Transmit, message.EM_ReceiveTransmit);
			AssertEquals(EDIMessageStatusList.Codes.Queued, message.EM_Status);
		});
	}

	protected override void AssertEntryAndMessageResultForDepartureEndToEndTest(EU.NCTS.Business.NctsHeader nctsHeader)
	{
		CombineAssertions(() =>
		{
			AssertEquals("SNT", nctsHeader.EffectiveMessageStatus);

			AssertEquals(1, nctsHeader.MovementHeader.Messages.Count);

			var message = nctsHeader.MovementHeader.Messages[0];
			AssertEquals(nameof(message.EM_ApplicationCode), "DEA", message.EM_ApplicationCode);
			AssertEquals(nameof(message.EM_MessageSubType), "DEP", message.EM_MessageSubType);
			AssertEquals(nameof(message.EM_ApplicationReference), "DETPDD", message.EM_ApplicationReference);
			AssertEquals(ReceiveTransmitList.Codes.Transmit, message.EM_ReceiveTransmit);
			AssertEquals(EDIMessageStatusList.Codes.Queued, message.EM_Status);
		});
	}
}
