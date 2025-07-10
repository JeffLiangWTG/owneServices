using System;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IE.Business.AIS.CusTempStorage.Testing
{
	sealed class LocationOfGoodsProviderTest : DataProviderTestCase<LocationOfGoodsProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>("Should throw ArgumentException with null parameter.", () => LocationOfGoodsProvider.New(null));
		}

		public void TestUNLOCODE()
		{
			AssertEquals("UNLOCODE", "BBB", Provider.UNLOCODE);
		}

		public void TestAdditionalIdentifier()
		{
			AssertEquals("AdditionalIdentifier", "BBB", Provider.AdditionalIdentifier);
		}

		public void TestTypeOfLocation()
		{
			AssertEquals("TypeOfLocation", "CC", Provider.TypeOfLocation);
		}

		public void TestGNSS()
		{
			goodsLocation.CGL_Qualifier = "W";
			goodsLocation.Address.E2_AddressOverride = true;
			goodsLocation.Address.E2_Latitude = 10.5m;
			goodsLocation.Address.E2_Longitude = 20.5m;
			AssertEquals("GNSS.Latitude", "10.5", Provider.GNSS.Latitude);
			AssertEquals("GNSS.Longitude", "20.5", Provider.GNSS.Longitude);

			goodsLocation.CGL_Qualifier = "!";
			AssertNull("GNSS", LocationOfGoodsProvider.New(goodsLocation).GNSS);
		}

		public void TestQualifierOfIdentification()
		{
			AssertEquals("QualifierOfIdentification", "A", Provider.QualifierOfIdentification);
		}

		public void TestAuthorisationNumber()
		{
			AssertEquals("AuthorisationNumber", "BBB", Provider.AuthorisationNumber);
		}

		public void TestAddress()
		{
			goodsLocation.CGL_Qualifier = "Z";
			goodsLocation.Address.E2_AddressOverride = true;
			goodsLocation.Address.E2_Address1AndE2_Address2 = "Address1 Address2";
			goodsLocation.Address.E2_Postcode = "12345";
			goodsLocation.Address.E2_City = "Dublin";
			goodsLocation.Address.E2_RN_NKCountryCode = "IE";
			AssertEquals("Address.StreetAndNumber", "Address1 Address2", Provider.Address.StreetAndNumber);
			AssertEquals("Address.Postcode", "12345", Provider.Address.Postcode);
			AssertEquals("Address.City", "Dublin", Provider.Address.City);
			AssertEquals("Address.Country", "IE", Provider.Address.Country);

			goodsLocation.CGL_Qualifier = "!";
			AssertNull("Address", LocationOfGoodsProvider.New(goodsLocation).Address);
		}

		public void TestPostcodeAddress()
		{
			goodsLocation.CGL_Qualifier = "T";
			goodsLocation.CGL_AdditionalIdentifier = "BBB";
			goodsLocation.Address.E2_AddressOverride = true;
			goodsLocation.Address.E2_Postcode = "12345";
			goodsLocation.Address.E2_RN_NKCountryCode = "IE";
			AssertEquals("PostcodeAddress.HouseNumber", "BBB", Provider.PostcodeAddress.HouseNumber);
			AssertEquals("PostcodeAddress.Postcode", "12345", Provider.PostcodeAddress.Postcode);
			AssertEquals("PostcodeAddress.Country", "IE", Provider.PostcodeAddress.Country);

			goodsLocation.CGL_Qualifier = "!";
			AssertNull("PostcodeAddress", LocationOfGoodsProvider.New(goodsLocation).PostcodeAddress);
		}

		public void TestEconomicOperator()
		{
			goodsLocation.CGL_Qualifier = "X";
			goodsLocation.Address.E2_AddressOverride = true;
			goodsLocation.Address.E2_GovRegNum = "REG12345";
			AssertEquals("EconomicOperator.Id", "REG12345", Provider.EconomicOperator);

			goodsLocation.CGL_Qualifier = "!";
			AssertNullOrEmpty("EconomicOperator", LocationOfGoodsProvider.New(goodsLocation).EconomicOperator);
		}

		public void TestCustomsOffice()
		{
			goodsLocation.CGL_Qualifier = "V";
			goodsLocation.CGL_CustomsOffice = "DDD";
			AssertEquals("CustomsOffice.ReferenceNumber", "DDD", Provider.CustomsOffice);

			goodsLocation.CGL_Qualifier = "!";
			AssertNullOrEmpty("CustomsOffice", LocationOfGoodsProvider.New(goodsLocation).CustomsOffice);
		}

		protected override LocationOfGoodsProvider GetProvider()
		{
			var provider = LocationOfGoodsProvider.New(goodsLocation);
			return provider;
		}

		protected override void SetUp()
		{
			base.SetUp();
			goodsLocation = Factory.New<CusGoodsLocation>();
			goodsLocation.CGL_Qualifier = "A";
			goodsLocation.CGL_AdditionalIdentifier = "BBB";
			goodsLocation.CGL_Type = "CC";
		}
		CusGoodsLocation goodsLocation;
	}
}
