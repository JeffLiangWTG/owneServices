using System;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IE.H7.Business.Messaging.V1.Test
{
	sealed class GoodsLocationProviderTest : DataProviderTestCase<GoodsLocationProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>("Manifest header missing", () => new GoodsLocationProvider(null));
		}

		public void TestUNLOCODE()
		{
			AssertEquals("UNLOCODE", "AUSYD", Provider.UNLOCODE);
		}

		public void TestAdditionalIdentifier()
		{
			AssertEquals("AdditionalIdentifier", "Test", Provider.AdditionalIdentifier);
		}

		public void TestTypeOfLocation()
		{
			AssertEquals("TypeOfLocation", "A", Provider.TypeOfLocation);
		}

		public void TestQualifierOfIdentification()
		{
			AssertEquals("QualifierOfIdentification", "U", Provider.QualifierOfIdentification);
		}

		public void TestAddress()
		{
			AssertNotNull("Address", Provider.Address);
			var provider = new GoodsLocationProvider(goodsLocation);
			var address = provider.Address;
			CombineAssertions("Address", () =>
			{
				Assert("Address is RepresentativeProvider", address is AddressProvider);
				AssertEquals("Address street and number is empty", string.Empty, address.StreetAndNumber);
				AssertEquals("Address city is empty", string.Empty, address.City);
				AssertEquals("Address postcode is empty", string.Empty, address.Postcode);
				AssertEquals("Address country is Ireland", Core.Constants.CountryCodes.Ireland, address.Country);
				AssertSame("Is cached", address, provider.Address);
			});
		}

		protected override GoodsLocationProvider GetProvider()
		{
			SetUpTestData();
			return new GoodsLocationProvider(goodsLocation);
		}

		void SetUpTestData()
		{
			if (bill == null)
			{
				bill = Factory.New<AsycudaBill>();
				goodsLocation = bill.CusGoodsLocation;
				goodsLocation.CGL_Qualifier = "U";
				goodsLocation.AdditionalIdentifier = "Test";
				goodsLocation.Unlocode = "AUSYD";
				goodsLocation.CGL_Type = "A";
			}
		}

		AsycudaBill bill;
		CusGoodsLocation goodsLocation;
	}
}
