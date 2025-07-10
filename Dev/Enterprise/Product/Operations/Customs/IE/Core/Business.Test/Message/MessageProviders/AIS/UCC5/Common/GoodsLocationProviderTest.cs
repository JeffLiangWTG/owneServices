using CargoWise.Customs.IE.MessageContracts.Interfaces;
using Enterprise.Customs.Business.Testing;
using IGoodsLocation = CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces.IGoodsLocation;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	class GoodsLocationProviderTest : DataProviderTestCase<GoodsLocationProvider>
	{
		public void TestIGoodsLocation()
		{
			Assert("Should implement IGoodsLocation", Provider is IGoodsLocation);
		}

		public void TestQualifier()
		{
			goodsLocation.CGL_Qualifier = "U";
			AssertEquals("Qualifier=>CGL_Qualifier", "U", Provider.Qualifier);
		}

		public void TestType()
		{
			goodsLocation.CGL_Type = "A";
			AssertEquals("Type=>CGL_Type", "A", Provider.Type);
		}

		public void TestAdditionalIdentifier()
		{
			AssertEquals("CGL_AdditionalIdentifier is blank", string.Empty, Provider.AdditionalIdentifier);

			goodsLocation.CGL_AdditionalIdentifier = "additional id";
			AssertEquals("CGL_AdditionalIdentifier is set, should still return empty string", "additional id", GetProvider().AdditionalIdentifier);
		}

		public void TestID()
		{
			goodsLocation.CGL_CustomsOffice = "UC213";
			AssertEquals("UC213", Provider.ID);
		}

		public void TestAddress()
		{
			Assert(Provider.Address is IAddress);
		}

		protected override GoodsLocationProvider GetProvider() => new GoodsLocationProvider(goodsLocation);

		protected override void SetUp()
		{
			goodsLocation = Factory.New<CusGoodsLocation>();
		}
		CusGoodsLocation goodsLocation;
	}
}
