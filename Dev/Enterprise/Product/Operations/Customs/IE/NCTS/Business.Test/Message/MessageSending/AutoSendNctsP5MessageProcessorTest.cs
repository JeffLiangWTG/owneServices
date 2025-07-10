
using CargoWise.Types;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.IE.NCTS.Business.Testing;

sealed class AutoSendNctsP5MessageProcessorTest : EU.NCTS.Business.Testing.AutoSendNctsP5MessageProcessorAbstractTest
{
	protected override void AssertEntryAndMessageResultForDepartureEndToEndTest(EU.NCTS.Business.NctsHeader nctsHeader)
	{
		CombineAssertions(() =>
		{
			AssertEquals("SNT", nctsHeader.EffectiveMessageStatus);

			AssertEquals(1, nctsHeader.MovementHeader.Messages.Count);

			var message = nctsHeader.MovementHeader.Messages[0];
			AssertEquals(nameof(message.EM_ApplicationCode), "IEN", message.EM_ApplicationCode);
			AssertEquals(nameof(message.EM_MessageType), "015", message.EM_MessageType);
			AssertEquals(nameof(message.EM_MessageSubType), ZString.Empty, message.EM_MessageSubType);
			AssertEquals(nameof(message.EM_ApplicationReference), ZString.Empty, message.EM_ApplicationReference);
			AssertEquals(ReceiveTransmitList.Codes.Transmit, message.EM_ReceiveTransmit);
			AssertEquals(EDIMessageStatusList.Codes.Queued, message.EM_Status);
		});
	}

	protected override void PrepareNctsHeaderForArrivalTestCore(EU.NCTS.Business.NctsHeader nctsHeader)
	{
		base.PrepareNctsHeaderForArrivalTestCore(nctsHeader);

		nctsHeader.ArrivalMovementHeader.AuthorizationNumber = "ABCD1234";
	}

	protected override void AssertEntryAndMessageResultForArrivalEndToEndTest(EU.NCTS.Business.NctsHeader nctsHeader)
	{
		CombineAssertions(() =>
		{
			AssertEquals("SNT", nctsHeader.EffectiveMessageStatus);

			AssertEquals(1, nctsHeader.Messages.Count);

			var message = nctsHeader.Messages[0];
			AssertEquals(nameof(message.EM_ApplicationCode), "IEN", message.EM_ApplicationCode);
			AssertEquals(nameof(message.EM_MessageType), "007", message.EM_MessageType);
			AssertEquals(nameof(message.EM_MessageSubType), ZString.Empty, message.EM_MessageSubType);
			AssertEquals(nameof(message.EM_ApplicationReference), ZString.Empty, message.EM_ApplicationReference);
			AssertEquals(ReceiveTransmitList.Codes.Transmit, message.EM_ReceiveTransmit);
			AssertEquals(EDIMessageStatusList.Codes.Queued, message.EM_Status);
		});
	}
}
