using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IE.H7.Business.Messaging.V1.Test
{
	public class GoodsInformation02ProviderTest : DataProviderTestCase<GoodsInformation02Provider>
	{
		public void TestGrossMass()
		{
			AssertEquals(0m, Provider.GrossMass);
			bill.ABL_GrossWeight = 111m;
			bill.ABL_GrossWeightUQ = "KG";
			AssertEquals(111m, Provider.GrossMass);
			bill.ABL_GrossWeightUQ = "G";
			AssertEquals(0.111m, Provider.GrossMass);
		}

		protected override GoodsInformation02Provider GetProvider()
		{
			SetUpTestData();
			return new GoodsInformation02Provider(bill);
		}

		void SetUpTestData()
		{
			if (bill == null)
			{
				bill = Factory.New<AsycudaBill>();
			}
		}
		AsycudaBill bill;
	}
}
