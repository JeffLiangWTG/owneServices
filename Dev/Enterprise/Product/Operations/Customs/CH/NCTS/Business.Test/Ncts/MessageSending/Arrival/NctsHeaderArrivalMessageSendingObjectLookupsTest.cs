using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

sealed class NctsHeaderArrivalMessageSendingObjectLookupsTest : TestCaseWithFactory
{
	public void TestMessageTypeList() => CombineAssertions(() =>
	{
		MessageSendingObject.NctsHeader.ArrivalMovementHeader.MultipleMRNIndicator = true;
		AssertEquals("NT007", Lookups.MessageTypeList.CodesAsString);
		AssertEquals($"Description {PassarMessageTypeList.Codes.NT007}", "Arrival Announce", Lookups.MessageTypeList.GetDescriptionFromCode(PassarMessageTypeList.Codes.NT007));

		MessageSendingObject.NctsHeader.ArrivalMovementHeader.MultipleMRNIndicator = false;
		AssertEquals("NT044", Lookups.MessageTypeList.CodesAsString);
		AssertEquals($"Description {PassarMessageTypeList.Codes.NT044}", "Unloading Remarks", Lookups.MessageTypeList.GetDescriptionFromCode(PassarMessageTypeList.Codes.NT044));

		MessageSendingObject.NctsHeader.ArrivalMovementHeader.BM_CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease;
		AssertEquals("Arrival Status with value CL1 should return an emtpy Message Type List", 0, Lookups.MessageTypeList.Count);

		MessageSendingObject.NctsHeader.ArrivalMovementHeader.BM_CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.ClosedPartialRelease;
		AssertEquals("Arrival Status with value CL3 should return an emtpy Message Type List", 0, Lookups.MessageTypeList.Count);

		MessageSendingObject.NctsHeader.ArrivalMovementHeader.BM_CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.CHActive;
		AssertEquals("Arrival Status with value any other than CL1 or CL2 should return a filled Message Type List", 1, Lookups.MessageTypeList.Count);
	});

	NctsHeaderArrivalMessageSendingObject MessageSendingObject => messageSendingObject ?? (messageSendingObject = CreateMessageSendingObject());
	NctsHeaderArrivalMessageSendingObject messageSendingObject;

	NctsHeaderArrivalMessageSendingObjectLookups Lookups => MessageSendingObject.Lookups;

	NctsHeaderArrivalMessageSendingObject CreateMessageSendingObject()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		return new NctsHeaderArrivalMessageSendingObject(nctsHeader);
	}
}
