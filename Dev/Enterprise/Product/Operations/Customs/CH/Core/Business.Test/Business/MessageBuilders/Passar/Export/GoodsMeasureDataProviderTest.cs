using static Enterprise.Customs.CH.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.CH.Business.Testing;

sealed class GoodsMeasureDataProviderTest : BasePassarDataProviderTest<GoodsMeasureDataProvider>
{
	public void TestNew()
	{
		AssertNull(GoodsMeasureDataProvider.New(null));
	}

	public void TestProperties()
	{
		EntryLine.InvoiceLines[0].JI_CustomsQuantity = 10;
		EntryLine.InvoiceLines[0].JI_CustomsSecondQuantity = 20;
		EntryLine.InvoiceLines[0].JI_CustomsThirdQuantity = 30;

		AssertEquals("Prerequisite", 10m, EntryLine.CalcGrossWeight);
		AssertEquals("GrossMass", 10m, DataProvider.GrossMass);

		AssertEquals("Prerequisite", 20m, EntryLine.CalcNetWeight);
		AssertEquals("NetMass", 20m, DataProvider.NetMass);

		AssertEquals("Prerequisite", 30m, EntryLine.CalcAdditionalQty);
		AssertEquals("SupplementaryUnits", 30m, DataProvider.SupplementaryUnits);
	}

	public void TestNetMass() => CombineAssertions(() =>
	{
		EntryInstruction.CEI_Style = UniversalReferenceConstants.InputControlCodes.Ordinary;
		EntryLine.InvoiceLines[0].JI_NetWeight = 0;
		AssertNull("When empty", DataProvider.NetMass);

		EntryLine.InvoiceLines[0].JI_NetWeight = 1;
		AssertNotNull("When not empty", DataProvider.NetMass);

		EntryInstruction.CEI_Style = UniversalReferenceConstants.InputControlCodes.Simplified;
		AssertNull("When simplified", DataProvider.NetMass);
	});

	public void TestSupplementaryUnits()
	{
		EntryLine.InvoiceLines[0].JI_CustomsThirdQuantity = 30;
		EntryInstruction.CEI_Style = InputControlCodes.Ordinary;
		AssertEquals("CEI_Style not Simplified", 30m, DataProvider.SupplementaryUnits);
		EntryInstruction.CEI_Style = InputControlCodes.Simplified;
		AssertNull("CEI_Style is Simplified", DataProvider.SupplementaryUnits);
	}

	protected override GoodsMeasureDataProvider CreateDataProvider() => GoodsMeasureDataProvider.New(EntryLine);
}
