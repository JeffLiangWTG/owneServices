using System.Linq;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

sealed class InventoryTransportEquipmentDataProviderTest : BaseArrivalDataProviderTest<InventoryTransportEquipmentDataProvider, NctsHeaderArrivalMessageSendingObject>
{
	protected override InventoryTransportEquipmentDataProvider CreateDataProvider() => InventoryTransportEquipmentDataProvider.NewCollection(NctsHeader.ArrivalHeaderContainers).First();

	public void TestNew() => CombineAssertions(() =>
	{
		AssertNull("null", InventoryTransportEquipmentDataProvider.NewCollection(null));

		var container1 = NctsHeader.ArrivalHeaderContainers.AddNew();
		var container2 = NctsHeader.ArrivalHeaderContainers.AddNew();
		var container3 = NctsHeader.ArrivalHeaderContainers.AddNew();
		var container4 = NctsHeader.ArrivalHeaderContainers.AddNew();
		container1.BC_UnloadedState = NctsUnloadedStateList.Codes.NEW;
		container2.BC_UnloadedState = NctsUnloadedStateList.Codes.MIS;
		container3.BC_UnloadedState = NctsUnloadedStateList.Codes.DIF;
		container4.BC_UnloadedState = NctsUnloadedStateList.Codes.DEC;

		var providers = InventoryTransportEquipmentDataProvider.NewCollection(NctsHeader.ArrivalHeaderContainers);

		AssertEquals("NEW", true, providers.Any(x => x.SequenceNumber == container1.BC_SequenceNumber));
		AssertEquals("MIS", true, providers.Any(x => x.SequenceNumber == container2.BC_SequenceNumber));
		AssertEquals("DIF", true, providers.Any(x => x.SequenceNumber == container3.BC_SequenceNumber));
		AssertEquals("DEC", false, providers.Any(x => x.SequenceNumber == container4.BC_SequenceNumber));
	});

	public void TestSeals()
	{
		var containers = NctsHeader.ArrivalHeaderContainers;
		var container = containers.AddNew();
		container.BC_UnloadedState = NctsUnloadedStateList.Codes.DIF;

		var seal1 = container.Seals.AddNew();
		var seal2 = container.Seals.AddNew();
		var seal3 = container.Seals.AddNew();
		var seal4 = container.Seals.AddNew();
		seal1.BK_UnloadingState = NctsUnloadedStateList.Codes.NEW;
		seal2.BK_UnloadingState = NctsUnloadedStateList.Codes.MIS;
		seal3.BK_UnloadingState = NctsUnloadedStateList.Codes.DIF;
		seal4.BK_UnloadingState = NctsUnloadedStateList.Codes.DEC;

		AssertEquals("NEW", true, DataProvider.Seals.Any(x => x.SequenceNumber == seal1.BK_SequenceNumber));
		AssertEquals("MIS", true, DataProvider.Seals.Any(x => x.SequenceNumber == seal2.BK_SequenceNumber));
		AssertEquals("DIF", true, DataProvider.Seals.Any(x => x.SequenceNumber == seal3.BK_SequenceNumber));
		AssertEquals("DEC", false, DataProvider.Seals.Any(x => x.SequenceNumber == seal4.BK_SequenceNumber));
	}

	public void TestNumberOfSeals() => CombineAssertions(() =>
	{
		var containers = NctsHeader.ArrivalHeaderContainers;
		var container = containers.AddNew();
		container.BC_UnloadedState = NctsUnloadedStateList.Codes.DIF;

		AssertEquals("No seals", null, DataProvider.NumberOfSeals);
		ResetDataProvider();

		container.Seals.AddNew().BK_UnloadingState = NctsUnloadedStateList.Codes.DIF;
		AssertEquals("single DIF not considered", null, DataProvider.NumberOfSeals);
		ResetDataProvider();

		container.Seals[0].BK_UnloadingState = NctsUnloadedStateList.Codes.DAM;
		AssertEquals("Single DAM not considered", null, DataProvider.NumberOfSeals);
		ResetDataProvider();

		container.Seals[0].BK_UnloadingState = NctsUnloadedStateList.Codes.DEC;
		AssertEquals("Single DEC not considered", null, DataProvider.NumberOfSeals);
		ResetDataProvider();

		container.Seals.AddNew().BK_UnloadingState = NctsUnloadedStateList.Codes.DAM;
		AssertEquals("DEC + DAM not considered", null, DataProvider.NumberOfSeals);

		container.Seals.AddNew().BK_UnloadingState = NctsUnloadedStateList.Codes.NEW;
		AssertEquals("DEC + DAM + NEW -> counted DEC + DAM + NEW", 3, DataProvider.NumberOfSeals);
		ResetDataProvider();

		container.Seals.AddNew().BK_UnloadingState = NctsUnloadedStateList.Codes.MIS;
		AssertEquals("DEC + DAM + NEW + MIS -> counted DEC + DAM + NEW", 3, DataProvider.NumberOfSeals);
		ResetDataProvider();

		container.Seals.AddNew().BK_UnloadingState = NctsUnloadedStateList.Codes.DIF;
		AssertEquals("DEC + DAM + NEW + MIS + DIF -> counted DEC + DAM + NEW + DIF", 4, DataProvider.NumberOfSeals);
		ResetDataProvider();

		container.Seals[0].BK_UnloadingState = NctsUnloadedStateList.Codes.DIF;
		container.Seals[1].BK_UnloadingState = NctsUnloadedStateList.Codes.DIF;
		container.Seals[2].BK_UnloadingState = NctsUnloadedStateList.Codes.DIF;
		container.Seals[3].BK_UnloadingState = NctsUnloadedStateList.Codes.DIF;
		AssertEquals("All DIF not considered", null, DataProvider.NumberOfSeals);
		ResetDataProvider();

		container.Seals[0].BK_UnloadingState = NctsUnloadedStateList.Codes.DEC;
		AssertEquals("DEC + DIF not considered", null, DataProvider.NumberOfSeals);
		ResetDataProvider();

		container.Seals[0].BK_UnloadingState = NctsUnloadedStateList.Codes.DAM;
		AssertEquals("DAM + DIF not considered", null, DataProvider.NumberOfSeals);
		ResetDataProvider();

		container.Seals[2].BK_UnloadingState = NctsUnloadedStateList.Codes.DEC;
		AssertEquals("DEC + DAM + DIF not considered", null, DataProvider.NumberOfSeals);
		ResetDataProvider();
	});
}
