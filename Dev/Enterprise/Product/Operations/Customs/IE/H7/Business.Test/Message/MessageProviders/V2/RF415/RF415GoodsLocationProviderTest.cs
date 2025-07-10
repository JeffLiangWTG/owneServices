using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IE.H7.Business.Test
{
	sealed class RF415GoodsLocationProviderTest : DataProviderTestCase<RF415GoodsLocationProvider>
	{
		public void TestAdditionalIdentifier()
		{
			cusGoodsLocation.CGL_AdditionalIdentifier = "Something";
			AssertEquals("AdditionalIdentifier", "Something", Provider.AdditionalIdentifier);
		}

		public void TestQualifierIdentification()
		{
			cusGoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.EoriNumber;
			AssertEquals("QualifierIdentification", CusGoodsLocationQualifierList.Codes.EoriNumber, Provider.QualifierIdentification);
		}

		public void TestLocationTypeCode()
		{
			cusGoodsLocation.CGL_Type = "B";
			AssertEquals("LocationTypeCode", "B", Provider.LocationTypeCode);
		}

		public void TestIdentification()
		{
			AssertNull("Identification", Provider.Identification);
		}

		public void TestCity()
		{
			AssertNull("City", Provider.City);
		}

		public void TestCountry()
		{
			AssertNull("Country", Provider.Country);
		}

		public void TestStreetAndNumber()
		{
			AssertNull("StreetAndNumber", Provider.StreetAndNumber);
		}

		public void TestPostcode()
		{
			AssertNull("Postcode", Provider.Postcode);
		}

		protected sealed override RF415GoodsLocationProvider GetProvider()
		{
			return new RF415GoodsLocationProvider(cusGoodsLocation);
		}

		protected override void SetUp()
		{
			base.SetUp();

			bill = Factory.New<AsycudaBill>();
			cusGoodsLocation = bill.CusGoodsLocation;
		}

		AsycudaBill bill;
		CusGoodsLocation cusGoodsLocation;
	}
}

