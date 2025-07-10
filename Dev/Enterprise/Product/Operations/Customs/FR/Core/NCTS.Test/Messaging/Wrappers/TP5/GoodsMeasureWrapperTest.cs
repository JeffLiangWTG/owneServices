using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5.Testing
{
	class GoodsMeasureWrapperTest : Customs.Business.Testing.DataProviderTestCase<GoodsMeasureWrapper>
	{
		public void TestGrossMass()
		{
			AssertEquals("GrossMass should equal BY_GrossWeight", 2.99m, Provider.GrossMass);
		}

		public void TestNetMass()
		{
			AssertEquals("NetMass should equal BY_NetWeight", 1.99m, Provider.NetMass);
		}

		public void TestSupplementaryUnits()
		{
			AssertEquals("SupplementaryUnits should equal BY_CustomsSecondQuantity", 3.99m, Provider.SupplementaryUnits);
		}

		protected override GoodsMeasureWrapper GetProvider()
		{
			var item = Factory.New<NctsDepartureCargoDesc>();
			item.BY_GrossWeight = 2.99m;
			item.BY_NetWeight = 1.99m;
			item.BY_CustomsSecondQuantity = 3.99m;
			return GoodsMeasureWrapper.New(item);
		}
	}
}
