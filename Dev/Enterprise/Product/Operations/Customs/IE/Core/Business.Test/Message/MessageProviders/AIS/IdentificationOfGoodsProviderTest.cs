using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	sealed class IdentificationOfGoodsProviderTest : DataProviderTestCase<IdentificationOfGoodsProvider>
	{
		public void TestRateOfYield()
		{
			instruction.ZG_RateOfYield = "10KG";
			AssertEquals("RateOfYield", "10KG", Provider.RateOfYield);
		}

		public void TestProcessedProducts()
		{
			AssertType<ProcessedProductsProvider>(Provider.ProcessedProducts);
		}

		public void TestIdentificationOfGoods()
		{
			AssertType<CodeDetailsProvider>(Provider.IdentificationOfGoods);
		}

		protected override IdentificationOfGoodsProvider GetProvider() => new IdentificationOfGoodsProvider(instruction);

		protected override void SetUp()
		{
			base.SetUp();
			instruction = Factory.New<CusEntryInstruction>();
		}

		CusEntryInstruction instruction;
	}
}
