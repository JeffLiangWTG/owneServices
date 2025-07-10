using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	sealed class ProcessedProductsProviderTest : DataProviderTestCase<ProcessedProductsProvider>
	{
		public void TestCommodityCode()
		{
			instruction.ZG_ProcessedProductsCommodityCode = "12";
			AssertEquals("CommodityCode", "12", Provider.CommodityCode);
		}

		public void TestDescriptionOfGoods()
		{
			instruction.ProcessedProductDescription = "Test";
			AssertEquals("DescriptionOfGoods", "Test", Provider.DescriptionOfGoods);
		}

		protected override ProcessedProductsProvider GetProvider() => new ProcessedProductsProvider(instruction);

		protected override void SetUp()
		{
			base.SetUp();
			instruction = Factory.New<CusEntryInstruction>();
		}

		CusEntryInstruction instruction;
	}
}
