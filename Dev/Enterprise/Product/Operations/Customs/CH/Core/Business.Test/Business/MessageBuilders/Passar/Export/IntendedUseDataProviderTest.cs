using static Enterprise.Customs.CH.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.CH.Business.Testing;

class IntendedUseDataProviderTest : BasePassarDataProviderTest<IntendedUseDataProvider>
{
	public void TestNew()
	{
		CombineAssertions(() =>
		{
			AssertNull("Null argument", IntendedUseDataProvider.New(null));
			AssertNotNull("Argument != null", IntendedUseDataProvider.New(EntryInstruction));
		});
	}

	public void TestProvider()
	{
		EntryInstruction.CEI_Style = InputControlCodes.Simplified;
		EntryInstruction.CEI_Procedure = UniversalReferenceConstants.ProcedureCodesPassar.ExportFromFreeCirculation;

		CombineAssertions(() =>
		{
			AssertEquals("Input Control = 1 Simplified", InputControlCodes.Simplified, DataProvider.InputControl);
			AssertEquals("Goods Provision = 20 ExportFromFreeCirculation", UniversalReferenceConstants.ProcedureCodesPassar.ExportFromFreeCirculation, DataProvider.GoodsProvision);
		});
	}

	public void TestReasonTaxExemption() => AssertNull("ReasonTaxExemption not available", CreateDataProvider().ReasonTaxExemption);

	protected override IntendedUseDataProvider CreateDataProvider() => IntendedUseDataProvider.New(EntryInstruction);
}
