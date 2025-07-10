namespace Enterprise.Customs.BE.Business.Testing;

sealed class CalculationOfTaxesProviderTest : Customs.Business.Testing.DataProviderTestCase<CalculationOfTaxesProvider>
{
	public void TestDutiesAndTaxes()
	{
		cusEntryLine.Fees.AddNew();
		AssertEquals(1, Provider.DutiesAndTaxes.Count);
	}

	public void TestTotalDutiesAndTaxesAmount()
	{
		var fee1 = cusEntryLine.Fees.AddNew();
		var fee2 = cusEntryLine.Fees.AddNew();
		fee1.CF_ChargeAmount = 1108;
		fee2.CF_ChargeAmount = 22;

		AssertEquals(1130m, Provider.TotalDutiesAndTaxesAmount);
	}

	public void TestPreference()
	{
		var invoiceLine = cusEntryLine.InvoiceLines.AddNew();
		invoiceLine.JI_PrimaryPreference = "AVC";
		AssertEquals("AVC", Provider.Preference);
	}

	protected override CalculationOfTaxesProvider GetProvider() => provider;

	protected override void SetUp()
	{
		base.SetUp();
		cusEntryLine = Factory.NewWithValidTestData<EU.Business.Declaration.CusEntryLine>();
		provider = new CalculationOfTaxesProvider(cusEntryLine);
	}

	EU.Business.Declaration.CusEntryLine cusEntryLine;

	CalculationOfTaxesProvider provider;
}
