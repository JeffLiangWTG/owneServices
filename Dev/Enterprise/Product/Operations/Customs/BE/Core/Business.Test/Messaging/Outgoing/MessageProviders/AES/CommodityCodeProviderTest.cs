using Enterprise.Customs.BE.Business.Declaration;

namespace Enterprise.Customs.BE.Business.Testing;

sealed class CommodityCodeProviderTest : Customs.Business.Testing.DataProviderTestCase<CommodityCodeProvider>
{
	public void TestCombinedNomenclatureCode()
	{
		invoiceLine.JI_Tariff = "1122334455";
		AssertEquals("Should be 44", "44", provider.CombinedNomenclatureCode);
	}

	public void TestHarmonizedSystemSubHeadingCode()
	{
		invoiceLine.JI_Tariff = "1122334455";
		AssertEquals("Should be 112233", "112233", provider.HarmonizedSystemSubHeadingCode);
	}

	public void TestTARICAdditionalCodes()
	{
		var supplementaryCode = invoiceLine.AdditionalSupplementaryCodes.AddNew();
		supplementaryCode.CY_Code = "ABC123";
		AssertEquals(1, provider.TARICAdditionalCodes.Count);
	}

	public void TestNationalAdditionalCodes()
	{
		var supplementaryCode = invoiceLine.AdditionalSupplementaryCodes.AddNew();
		supplementaryCode.CY_Code = "1ABC123";
		AssertEquals(1, provider.NationalAdditionalCodes.Count);
	}

	public void TestTARICCode()
	{
		invoiceLine.JI_Tariff = "1122334455";
		AssertEquals("Should be 55", "55", provider.TARICCode);
	}

	protected override CommodityCodeProvider GetProvider() => provider;

	protected override void SetUp()
	{
		base.SetUp();
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();

		invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		provider = new CommodityCodeProvider(invoiceLine);
	}

	JobComInvoiceLine invoiceLine;
	CommodityCodeProvider provider;
}
