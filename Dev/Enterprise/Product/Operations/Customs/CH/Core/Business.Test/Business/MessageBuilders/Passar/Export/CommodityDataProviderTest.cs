using System.Linq;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.Business.Testing;

sealed class CommodityDataProviderTest : BasePassarDataProviderTest<CommodityDataProvider>
{
	public void TestConstructorNullArgument()
	{
		AssertNull(CommodityDataProvider.New(null));
	}

	public void TestProvider() => CombineAssertions(() =>
	{
		const string goodsDescription = "description";

		EntryLine.CL_LineNumber = 1;
		EntryLine.CL_Description = goodsDescription;

		AssertNull("CUSCode", DataProvider.CUSCode);
		AssertEquals("DescriptionOfGoods", goodsDescription, DataProvider.DescriptionOfGoods);
	});

	public void TestCommodityCode() => CombineAssertions(() =>
	{
		EntryLine.CL_AdValoremTariff = "61101100002";

		EntryLine.Header.EntryInstruction.CEI_Style = UniversalReferenceConstants.InputControlCodes.Simplified;
		AssertNull("null is simplified", DataProvider.CommodityCode);

		EntryLine.Header.EntryInstruction.CEI_Style = UniversalReferenceConstants.InputControlCodes.Ordinary;
		AssertType<CommodityCodeDataProvider>(DataProvider.CommodityCode);
		AssertSame("cached", DataProvider.CommodityCode, DataProvider.CommodityCode);
		AssertEquals("NationalCustomsTariffNumber", "6110.1100", DataProvider.CommodityCode.NationalCustomsTariffNumber);
	});

	public void TestCommoditySpecification() => CombineAssertions(() =>
	{
		InvoiceLine.JI_NonTradingGoods = true;
		AssertType<CommoditySpecificationDataProvider>(DataProvider.CommoditySpecification);
		AssertSame("cached", DataProvider.CommoditySpecification, DataProvider.CommoditySpecification);
		AssertEquals("NonTradingGoods", true, DataProvider.CommoditySpecification.NonTradingGoods);
	});

	public void TestDangerousGoods() => CombineAssertions(() =>
	{
		var undgSubstance = Factory.New<UNDGSubstance>();
		undgSubstance.DG_UNNO = "1234";

		AssertEquals("No UNDGs", 0, DataProvider.DangerousGoods.Count);

		ResetDataProvider();
		EntryLine.RandomLine.UNDGs.AddNew().DI_DG = undgSubstance.PK;
		AssertEquals("Count", 1, DataProvider.DangerousGoods.Count);
		AssertEquals("SequenceNumber", 1, DataProvider.DangerousGoods.FirstOrDefault()?.SequenceNumber);
		AssertEquals("UNNumber", "1234", DataProvider.DangerousGoods.FirstOrDefault()?.UNNumber);

		AssertSame("cached", DataProvider.DangerousGoods, DataProvider.DangerousGoods);
	});

	public void TestGoodsMeasure() => CombineAssertions(() =>
	{
		AssertType<GoodsMeasureDataProvider>(DataProvider.GoodsMeasure);
		AssertSame("cached", DataProvider.GoodsMeasure, DataProvider.GoodsMeasure);
	});

	protected override CommodityDataProvider CreateDataProvider() => CommodityDataProvider.New(EntryLine);
}
