using System.Linq;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

sealed class InventorySealDataProviderTest : BaseArrivalDataProviderTest<InventorySealDataProvider, NctsHeaderArrivalMessageSendingObject>
{
	protected override InventorySealDataProvider CreateDataProvider() => InventorySealDataProvider.NewCollection(NctsHeader.ArrivalHeaderContainers.Cast<NctsArrivalHeaderContainer>().First().SealsForMessaging).First();

	public void TestNewCollection()
	{
		AssertNull("null", InventorySealDataProvider.NewCollection(null));

		var seal1 = Container.Seals.AddNew();
		var seal2 = Container.Seals.AddNew();
		var seal3 = Container.Seals.AddNew();
		var seal4 = Container.Seals.AddNew();
		var seal5 = Container.Seals.AddNew();
		seal1.BK_UnloadingState = NctsUnloadedStateList.Codes.NEW;
		seal2.BK_UnloadingState = NctsUnloadedStateList.Codes.MIS;
		seal3.BK_UnloadingState = NctsUnloadedStateList.Codes.DIF;
		seal4.BK_UnloadingState = NctsUnloadedStateList.Codes.DEC;
		seal5.BK_UnloadingState = NctsUnloadedStateList.Codes.DAM;

		var providers = InventorySealDataProvider.NewCollection(container.SealsForMessaging);

		AssertEquals("NEW", true, providers.Any(x => x.SequenceNumber == seal1.BK_SequenceNumber));
		AssertEquals("MIS", true, providers.Any(x => x.SequenceNumber == seal2.BK_SequenceNumber));
		AssertEquals("DIF", true, providers.Any(x => x.SequenceNumber == seal3.BK_SequenceNumber));
		AssertEquals("DEC", false, providers.Any(x => x.SequenceNumber == seal4.BK_SequenceNumber));
		AssertEquals("DAM", false, providers.Any(x => x.SequenceNumber == seal5.BK_SequenceNumber));
	}

	public void TestProperties() => CombineAssertions(() =>
	{
		Seal.BK_SealNumber = "S01";
		Seal.UnloadingRemarksText = "Unloading Remark 1";

		AssertEquals("SequenceNumber", 1, DataProvider.SequenceNumber);
		AssertEquals("Identifier", "S01", DataProvider.Identifier);
		AssertEquals("UnloadingRemarkText", "Unloading Remark 1", DataProvider.UnloadingRemarkText);
	});

	public void TestPropertiesMIS()
	{
		Seal.BK_UnloadingState = NctsUnloadedStateList.Codes.MIS;
		Seal.BK_SealNumber = "S01";
		Seal.UnloadingRemarksText = "Unloading Remark 1";

		AssertEquals("SequenceNumber", 1, DataProvider.SequenceNumber);
		AssertNull("Identifier", DataProvider.Identifier);
		AssertEquals("UnloadingRemarkText", "Unloading Remark 1", DataProvider.UnloadingRemarkText);
	}

	NctsArrivalHeaderContainer Container => container ?? (container = NctsHeader.ArrivalHeaderContainers.AddNew());
	NctsArrivalHeaderContainer container;

	CusSeal Seal => seal ?? (seal = Container.Seals.AddNew());
	CusSeal seal;
}
