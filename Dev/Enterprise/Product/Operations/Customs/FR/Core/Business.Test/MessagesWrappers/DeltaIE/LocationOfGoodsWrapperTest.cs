using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE.Testing
{
	class LocationOfGoodsWrapperTest : Customs.Business.Testing.DataProviderTestCase<LocationOfGoodsWrapper>
	{
		protected override LocationOfGoodsWrapper GetProvider()
		{
			var locationGoods = Factory.NewWithValidTestData<CusGoodsLocation>();
			locationGoods.CGL_AdditionalIdentifier = "ID";
			locationGoods.CGL_CustomsOffice = "CUSOFFICE";
			locationGoods.CGL_Qualifier = "1";
			locationGoods.CGL_Type = "A";

			var locationGoodsAddress = locationGoods.Address;
			locationGoodsAddress.FillWithValidTestData();
			locationGoodsAddress.E2_Address1 = "address1";
			locationGoodsAddress.E2_City = "city";
			locationGoodsAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			locationGoodsAddress.E2_Postcode = "24000";
			locationGoodsAddress.E2_GovRegNum = "auth";
			locationGoodsAddress.E2_Latitude = 32m;
			locationGoodsAddress.E2_Longitude = 21m;

			locationGoods.CGL_AdditionalIdentifier = "ID";
			locationGoods.CGL_CustomsOffice = "CUSOFFICE";

			return LocationOfGoodsWrapper.New(locationGoods);
		}

		public void TestAdditionalIdentifier()
		{
			AssertEquals("AdditionalIdentifier should be equal to CGL_AdditionalIdentifier.", "ID", Provider.AdditionalIdentifier);
		}

		public void TestAddress()
		{
			var address = Provider.Address;
			AssertEquals("address.StreetAndNumber should be equal to locationGoodsAddress.E2_Address1.", "address1", address.StreetAndNumber);
			AssertEquals("address.City should be equal to locationGoodsAddress.E2_City.", "city", address.City);
			AssertEquals("address.Country should be equal to locationGoodsAddress.E2_RN_NKCountryCode.", Core.Constants.CountryCodes.France, address.Country);
			AssertEquals("address.Postcode should be equal to locationGoodsAddress.E2_Postcode.", "24000", address.Postcode);
		}

		public void TestAuthorisationNumber()
		{
			AssertEquals("AuthorisationNumber should be equal to locationGoodsAddress.E2_GovRegNum.", "auth", Provider.AuthorisationNumber);
		}

		public void TestCustomsOffice()
		{
			var customsOffice = Provider.CustomsOffice;
			AssertEquals("customsOffice.ReferenceNumber should be equal to CGL_CustomsOffice", "CUSOFFICE", customsOffice.ReferenceNumber);
		}

		public void TestEconomicOperator()
		{
			var economicOperator = Provider.EconomicOperator;
			AssertEquals("economicOperator.IdentificationNumber should be equal to locationGoodsAddress.E2_GovRegNum.", "auth", economicOperator.IdentificationNumber);
		}

		public void TestGNSS()
		{
			var gNSS = Provider.GNSS;
			AssertEquals("gNSS.Latitude should be equal to locationGoodsAddress.E2_Latitude.", "32", gNSS.Latitude);
			AssertEquals("gNSS.Longitude should be equal to locationGoodsAddress.E2_Longitude.", "21", gNSS.Longitude);
		}

		public void TestPostcodeAddress()
		{
			var postcodeAddress = Provider.PostcodeAddress;
			AssertEquals("postcodeAddress.HouseNumber should be equal to locationGoodsAddress.E2_Address1.", "address1", postcodeAddress.HouseNumber);
			AssertEquals("postcodeAddress.Country should be equal to locationGoodsAddress.E2_RN_NKCountryCode.", Core.Constants.CountryCodes.France, postcodeAddress.Country);
			AssertEquals("postcodeAddress.Postcode should be equal to locationGoodsAddress.E2_Postcode.", "24000", postcodeAddress.Postcode);
		}

		public void TestQualifierOfIdentification()
		{
			AssertEquals("QualifierOfIdentification should be equal to CGL_Qualifier.", "1", Provider.QualifierOfIdentification);
		}

		public void TestTypeOfLocation()
		{
			AssertEquals("TypeOfLocation should be equal to CGL_Type.", "A", Provider.TypeOfLocation);
		}

		public void TestUNLOCODE()
		{
			AssertEquals("UNLOCODE should be equal to CGL_AdditionalIdentifier.", "ID", Provider.UNLOCODE);
		}
	}
}
