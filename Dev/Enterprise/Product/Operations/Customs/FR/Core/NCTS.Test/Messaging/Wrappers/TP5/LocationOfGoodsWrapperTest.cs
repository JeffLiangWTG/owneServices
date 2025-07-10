namespace Enterprise.Customs.FR.NCTS.Messaging.TP5.Testing
{
	class LocationOfGoodsWrapperTest : Customs.Business.Testing.DataProviderTestCase<LocationOfGoodsWrapper>
	{
		public void TestTypeOfLocation()
		{
			AssertEquals("TypeOfLocation should be equal to CGL_Type.", "A", Provider.TypeOfLocation);
		}

		public void TestQualifierofIdentification()
		{
			AssertEquals("QualifierofIdentification should be equal to CGL_Qualifier.", "1", Provider.QualifierofIdentification);
		}

		public void TestAuthorisationNumber()
		{
			GetProvider();
			var locationGoodsAddress = locationGoods.Address;
			locationGoodsAddress.E2_GovRegNum = "FR12345678";
			var provider = LocationOfGoodsWrapper.New(locationGoods);
			AssertEquals("AuthorisationNumber should be equal to location GoodsAddress.E2_GovRegNum.", "FR12345678", provider.AuthorisationNumber);
		}

		public void TestAdditionalIdentifier()
		{
			AssertEquals("AdditionalIdentifier should be equal to CGL_AdditionalIdentifier.", "AdditionalId", Provider.AdditionalIdentifier);
		}

		public void TestUNLoCode()
		{
			GetProvider();
			locationGoods.CGL_AdditionalIdentifier = "FRPAR";
			var provider = LocationOfGoodsWrapper.New(locationGoods);
			AssertEquals("UNLoCode should be equal to CGL_AdditionalIdentifier.", "FRPAR", provider.UNLoCode);
		}

		public void TestCustomsOffice()
		{
			GetProvider();
			locationGoods.CGL_CustomsOffice = "FR000040";
			var provider = LocationOfGoodsWrapper.New(locationGoods);
			AssertEquals("customsOffice.ReferenceNumber should be equal to CGL_CustomsOffice.", "FR000040", provider.CustomsOffice.ReferenceNumber);
		}

		public void TestGNSS()
		{
			GetProvider();
			var locationGoodsAddress = locationGoods.Address;
			locationGoodsAddress.E2_Latitude = 44.9108009m;
			locationGoodsAddress.E2_Longitude = 0.4889892m;
			var provider = LocationOfGoodsWrapper.New(locationGoods);
			var gNSS = provider.GNSS;
			AssertEquals("GNSS should be mapped to location address latitude and longitude.", "44.9108009, 0.4889892", $"{gNSS.Latitude}, {gNSS.Longitude}");
		}

		public void TestEconomicOperator()
		{
			GetProvider();
			var locationGoodsAddress = locationGoods.Address;
			locationGoodsAddress.E2_GovRegNum = "FR12345678";
			var provider = LocationOfGoodsWrapper.New(locationGoods);
			AssertEquals("EconomicOperator.IdentificationNumber should be mapped to locationGoodsAddress.E2_GovRegNum.", "FR12345678", provider.EconomicOperator.IdentificationNumber);
		}

		public void TestAddress()
		{
			GetProvider();
			var locationGoodsAddress = locationGoods.Address;
			locationGoodsAddress.E2_Address1 = "177 Impasse Jane Poupelet";
			locationGoodsAddress.E2_City = "MAURENS";
			locationGoodsAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			locationGoodsAddress.E2_Postcode = "24140";
			locationGoodsAddress.E2_AdditionalAddressInformation = "6ter";
			var provider = LocationOfGoodsWrapper.New(locationGoods);
			var address = provider.Address;
			AssertEquals("Address should be mapped to organisation Main Address.", "177 Impasse Jane Poupelet, 24140, MAURENS, FR", $"{address.StreetAndNumber}, {address.PostCode}, {address.City}, {address.Country}");
		}

		public void TestPostCodeAddress()
		{
			GetProvider();
			var locationGoodsAddress = locationGoods.Address;
			locationGoodsAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			locationGoodsAddress.E2_Postcode = "24140";
			locationGoodsAddress.E2_AdditionalAddressInformation = "6ter";
			var provider = LocationOfGoodsWrapper.New(locationGoods);
			var postcodeAddress = provider.PostCodeAddress;
			AssertEquals("PostCodeAddress should be mapped to LocationOfGoods.Address.", "6ter, 24140, FR", $"{postcodeAddress.HouseNumber}, {postcodeAddress.PostCode}, {postcodeAddress.Country}");
		}

		public void TestContactPerson()
		{
			AssertEquals("ContactPerson is not yet mapped.", null, Provider.ContactPerson);
		}

		protected override LocationOfGoodsWrapper GetProvider()
		{
			locationGoods = Factory.NewWithValidTestData<EU.NCTS.Business.CusGoodsLocation>();
			locationGoods.CGL_Qualifier = "1";
			locationGoods.CGL_Type = "A";
			locationGoods.CGL_AdditionalIdentifier = "AdditionalId";

			return LocationOfGoodsWrapper.New(locationGoods);
		}

		EU.NCTS.Business.CusGoodsLocation locationGoods;
	}
}
