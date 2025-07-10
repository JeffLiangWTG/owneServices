namespace Enterprise.Customs.CH.Business.Testing;

class CommodityCodeDataProviderTest : BasePassarDataProviderTest<CommodityCodeDataProvider>
{
	public void TestConstructorNullArgument()
	{
		AssertNull(CommodityCodeDataProvider.New(null));
	}

	public void TestProvider()
	{
		CombineAssertions(() =>
		{
			AssertEquals("NationalCustomsTariffNumber", ".", DataProvider.NationalCustomsTariffNumber);
			AssertEquals("ControlCode", "000", DataProvider.ControlCode);

			EntryLine.CL_AdValoremTariff = "61101100000";
			AssertEquals("NationalCustomsTariffNumber", "6110.1100", DataProvider.NationalCustomsTariffNumber);
			AssertEquals("ControlCode", "000", DataProvider.ControlCode);

			EntryLine.CL_AdValoremTariff = "61101100002";
			AssertEquals("NationalCustomsTariffNumber", "6110.1100", DataProvider.NationalCustomsTariffNumber);
			AssertEquals("ControlCode", "002", DataProvider.ControlCode);
		});
	}

	public void TestCustomsFavourCode() => AssertNull("CustomsFavourCode not available", CreateDataProvider().CustomsFavourCode);

	public void TestCustomsRate() => AssertNull("CustomsRate not available", CreateDataProvider().CustomsRate);

	public void TestVatCode() => AssertNull("VatCode not available", CreateDataProvider().VatCode);

	protected override CommodityCodeDataProvider CreateDataProvider() => CommodityCodeDataProvider.New(EntryLine);
}
