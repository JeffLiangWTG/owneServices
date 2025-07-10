using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.FR.NCTS.Messaging.Testing;

sealed class AutoSendNctsP5MessageProcessorTest : EU.NCTS.Business.Testing.AutoSendNctsP5MessageProcessorAbstractTest
{
	protected override void PrepareNctsHeaderForArrivalTestCore(NctsHeader nctsHeader)
	{
		base.PrepareNctsHeaderForArrivalTestCore(nctsHeader);

		nctsHeader.ArrivalMovementHeader.BM_ArrivalDate = ZDateTime.Today;
		nctsHeader.ArrivalMovementHeader.CustomsOffices.RemoveAndDeleteAll();
		var departureOffice = nctsHeader.ArrivalMovementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfDestinationForArrival);
		departureOffice.CY_Data = "FR000001";
	}

	protected override void AssertEntryAndMessageResultForArrivalEndToEndTest(NctsHeader nctsHeader)
	{
		CombineAssertions(() =>
		{
			AssertEquals(ZString.Empty, nctsHeader.EffectiveMessageStatus);

			AssertEquals(1, nctsHeader.Messages.Count);

			var message = nctsHeader.Messages[0];
			AssertEquals(nameof(message.EM_ApplicationCode), "FRC", message.EM_ApplicationCode);
			AssertEquals(nameof(message.EM_MessageSubType), "007", message.EM_MessageSubType);
			AssertEquals(nameof(message.EM_ApplicationReference), ZString.Empty, message.EM_ApplicationReference);
			AssertEquals(nameof(message.EM_ReceiveTransmit), ReceiveTransmitList.Codes.Transmit, message.EM_ReceiveTransmit);
			AssertEquals(nameof(message.EM_Status), EDIMessageStatusList.Codes.Queued, message.EM_Status);
		});
	}

	protected override void PrepareNctsHeaderForDepartureTestCore(NctsHeader nctsHeader)
	{
		base.PrepareNctsHeaderForDepartureTestCore(nctsHeader);

		nctsHeader.MovementHeader.CustomsOffices.RemoveAndDeleteAll();
		var departureOffice = nctsHeader.MovementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture);
		departureOffice.CY_Data = "FR000001";
	}

	protected override void AssertEntryAndMessageResultForDepartureEndToEndTest(NctsHeader nctsHeader)
	{
		CombineAssertions(() =>
		{
			AssertEquals(ZString.Empty, nctsHeader.EffectiveMessageStatus);

			AssertEquals(1, nctsHeader.MovementHeader.Messages.Count);

			var message = nctsHeader.MovementHeader.Messages[0];
			AssertEquals(nameof(message.EM_ApplicationCode), "FRC", message.EM_ApplicationCode);
			AssertEquals(nameof(message.EM_MessageSubType), "015", message.EM_MessageSubType);
			AssertEquals(nameof(message.EM_ApplicationReference), ZString.Empty, message.EM_ApplicationReference);
			AssertEquals(nameof(message.EM_ReceiveTransmit), ReceiveTransmitList.Codes.Transmit, message.EM_ReceiveTransmit);
			AssertEquals(nameof(message.EM_Status), EDIMessageStatusList.Codes.Queued, message.EM_Status);
		});
	}
}
