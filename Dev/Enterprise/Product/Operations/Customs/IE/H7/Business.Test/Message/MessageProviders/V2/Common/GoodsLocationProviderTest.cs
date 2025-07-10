using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IE.H7.Business.Test
{
	public class GoodsLocationProviderTest : DataProviderTestCase<GoodsLocationProvider>
	{
		public void TestIGoodsLocation()
		{
			Assert("Should implement IGoodsLocation", Provider is IGoodsLocation);
		}

		public void TestTypeOfLocation()
		{
			SetUpTestData();
			goodsLocation.CGL_Type = "A";
			AssertEquals("A", Provider.TypeOfLocation);
		}

		public void TestQualifierOfIdentification()
		{
			SetUpTestData();
			goodsLocation.CGL_Qualifier = "T";
			AssertEquals("T", Provider.QualifierOfIdentification);
		}

		public void TestAuthorisationNumber()
		{
			AssertNull("Should return CGL_Authorisation when available", Provider.AuthorisationNumber);
		}

		public void TestAdditionalIdentifier()
		{
			SetUpTestData();
			goodsLocation.CGL_AdditionalIdentifier = "AID";
			AssertEquals("AID", Provider.AdditionalIdentifier);
		}

		public void TestUNLOCODE()
		{
			SetUpTestData();
			goodsLocation.Unlocode = "IEDUB";
			AssertEquals("IEDUB", Provider.UNLOCODE);
		}

		public void TestCustomsOffice()
		{
			SetUpTestData();
			header.AMA_CustomsOffice = "TestOffice";
			AssertEquals("TestOffice", Provider.CustomsOffice);
		}

		public void TestGNSS()
		{
			AssertNull("When CGL_Qualifier is not W", Provider.GNSS);

			header = null;
			var provider = GetProvider();
			goodsLocation.CGL_Qualifier = "W";
			var address = goodsLocation.Address;
			address.E2_Latitude = 52.3142599m;
			address.E2_Longitude = -66.9437914m;
			var gnss = provider.GNSS;
			CombineAssertions("When CGL_Qualifier is W", () =>
			{
				AssertEquals("Latitude", "52.3142599", gnss.Latitude);
				AssertEquals("Longitude", "-66.9437914", gnss.Longitude);
				AssertSame("Cached", gnss, provider.GNSS);
			});
		}

		public void TestEconomicOperator()
		{
			AssertNull("When CGL_Qualifier is not X", Provider.GNSS);

			header = null;
			var provider = GetProvider();
			goodsLocation.CGL_Qualifier = "X";
			goodsLocation.Address.E2_GovRegNum = "AEOC";
			var economicOperator = provider.EconomicOperator;
			CombineAssertions("When CGL_Qualifier is X", () =>
			{
				AssertEquals("Id", "AEOC", economicOperator);
				AssertSame("Cached", economicOperator, provider.EconomicOperator);
			});
		}

		public void TestAddress()
		{
			AssertNull("When CGL_Qualifier is not Z", Provider.Address);

			header = null;
			var provider = GetProvider();
			goodsLocation.CGL_Qualifier = "Z";
			goodsLocation.Address.E2_Postcode = "D18";
			var address = provider.Address;
			CombineAssertions("When CGL_Qualifier is Z", () =>
			{
				AssertEquals("Has been constructed with goodsLocation.Address", "D18", address.Postcode);
				AssertSame("Cached", address, provider.Address);
			});
		}

		public void TestPostcodeAddress()
		{
			AssertNull("When CGL_Qualifier is not T", Provider.Address);

			header = null;
			var provider = GetProvider();
			goodsLocation.CGL_Qualifier = "T";
			goodsLocation.CGL_AdditionalIdentifier = "AID";
			goodsLocation.Address.E2_RN_NKCountryCode = "IE";
			goodsLocation.Address.E2_Postcode = "D18";
			var postcodeAddress = provider.PostcodeAddress;
			CombineAssertions("When CGL_Qualifier is T", () =>
			{
				AssertEquals("HouseNumber", "AID", postcodeAddress.HouseNumber);
				AssertEquals("Postcode", "D18", postcodeAddress.Postcode);
				AssertEquals("Country", "IE", postcodeAddress.Country);
				AssertSame("Cached", postcodeAddress, provider.PostcodeAddress);
			});
		}

		protected override GoodsLocationProvider GetProvider()
		{
			SetUpTestData();
			return new GoodsLocationProvider(goodsLocation);
		}

		void SetUpTestData()
		{
			if (header == null)
			{
				header = Factory.New<AsycudaManifestHeader>();
				bill = header.Bills.AddNew();
				goodsLocation = bill.CusGoodsLocation;
			}
		}

		AsycudaManifestHeader header;
		AsycudaBill bill;
		CusGoodsLocation goodsLocation;
	}
}
