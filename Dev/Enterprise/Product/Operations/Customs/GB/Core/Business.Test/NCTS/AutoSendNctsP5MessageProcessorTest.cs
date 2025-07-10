using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.GB.Business.Testing.NCTS;

public class AutoSendNctsP5MessageProcessorTest : EU.NCTS.Business.Testing.AutoSendNctsP5MessageProcessorAbstractTest
{
	protected override void PrepareNctsHeaderForArrivalTestCore(EU.NCTS.Business.NctsHeader nctsHeader)
	{
		base.PrepareNctsHeaderForArrivalTestCore(nctsHeader);

		var departureOffice = nctsHeader.ArrivalMovementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfDestinationForArrival);
		departureOffice.CY_Data = "GB000001";
	}

	protected override void AssertEntryAndMessageResultForArrivalEndToEndTest(EU.NCTS.Business.NctsHeader nctsHeader)
	{
		CombineAssertions(() =>
		{
			AssertEquals("SNT", nctsHeader.EffectiveMessageStatus);

			AssertEquals(1, nctsHeader.Messages.Count);

			var message = nctsHeader.Messages[0];
			AssertEquals(nameof(message.EM_ApplicationCode), "GBN", message.EM_ApplicationCode);
			AssertEquals(nameof(message.EM_MessageType), "007", message.EM_MessageType);
			AssertEquals(nameof(message.EM_MessageSubType), ZString.Empty, message.EM_MessageSubType);
			AssertEquals(nameof(message.EM_ApplicationReference), ZString.Empty, message.EM_ApplicationReference);
			AssertEquals(nameof(message.EM_ReceiveTransmit), ReceiveTransmitList.Codes.Transmit, message.EM_ReceiveTransmit);
			AssertEquals(nameof(message.EM_Status), EDIMessageStatusList.Codes.Queued, message.EM_Status);
		});
	}

	protected override void PrepareNctsHeaderForDepartureTestCore(EU.NCTS.Business.NctsHeader nctsHeader)
	{
		base.PrepareNctsHeaderForDepartureTestCore(nctsHeader);

		nctsHeader.MovementHeader.CustomsOffices.RemoveAndDeleteAll();
		var departureOffice = nctsHeader.MovementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture);
		departureOffice.CY_Data = "GB000001";
	}

	protected override void AssertEntryAndMessageResultForDepartureEndToEndTest(EU.NCTS.Business.NctsHeader nctsHeader)
	{
		CombineAssertions(() =>
		{
			AssertEquals("SNT", nctsHeader.EffectiveMessageStatus);

			AssertEquals(1, nctsHeader.MovementHeader.Messages.Count);

			var message = nctsHeader.MovementHeader.Messages[0];
			AssertEquals(nameof(message.EM_ApplicationCode), "GBN", message.EM_ApplicationCode);
			AssertEquals(nameof(message.EM_MessageType), "015", message.EM_MessageType);
			AssertEquals(nameof(message.EM_MessageSubType), ZString.Empty, message.EM_MessageSubType);
			AssertEquals(nameof(message.EM_ApplicationReference), ZString.Empty, message.EM_ApplicationReference);
			AssertEquals(nameof(message.EM_ReceiveTransmit), ReceiveTransmitList.Codes.Transmit, message.EM_ReceiveTransmit);
			AssertEquals(nameof(message.EM_Status), EDIMessageStatusList.Codes.Queued, message.EM_Status);
		});
	}
}
