using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(CommodityCodeDataProvider))]
sealed class CommodityCodeDataProviderTest : BaseDepartureDataProviderTest<CommodityCodeDataProvider, NctsHeaderDepartureMessageSendingObject>
{
	public void TestConstructorEmptyArgument() => AssertNull(CommodityCodeDataProvider.New(ZString.Empty));

	public void TestNationalCustomsTariffNumber()
	{
		DepartureGoodsItem.BY_HarmonisedTariff = "12345678901";
		AssertEquals("not empty", "1234.5678", DataProvider.NationalCustomsTariffNumber);
	}

	public void TestControlCode() => CombineAssertions(() =>
	{
		DepartureGoodsItem.BY_HarmonisedTariff = "12345678901";
		AssertEquals($"BY_HarmonisedTariff.Length='{DepartureGoodsItem.BY_HarmonisedTariff.Length}'", "901", DataProvider.ControlCode);
		ResetDataProvider();
		DepartureGoodsItem.BY_HarmonisedTariff = "12345678";
		AssertNull($"BY_HarmonisedTariff.Length='{DepartureGoodsItem.BY_HarmonisedTariff.Length}'", DataProvider.ControlCode);
	});

	public void TestCustomsFavourCode()
	{
		DepartureGoodsItem.BY_HarmonisedTariff = "12345678901";
		AssertNull("CustomsFavourCode not available", DataProvider.CustomsFavourCode);
	}

	public void TestCustomsRate()
	{
		DepartureGoodsItem.BY_HarmonisedTariff = "12345678901";
		AssertNull("CustomsRate not available", DataProvider.CustomsRate);
	}

	public void TestVatCode()
	{
		DepartureGoodsItem.BY_HarmonisedTariff = "12345678901";
		AssertNull("VatCode not available", DataProvider.VatCode);
	}

	protected override CommodityCodeDataProvider CreateDataProvider() => CommodityCodeDataProvider.New(DepartureGoodsItem.BY_HarmonisedTariff);
}
