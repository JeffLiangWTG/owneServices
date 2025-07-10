using CargoWise.Customs.DE.MessageContracts;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business.Testing
{
	class GoodsLoadingPlaceProviderTest : Customs.Business.Testing.DataProviderTestCase<GoodsLoadingPlaceProvider>
	{
		public void TestNew()
		{
			CombineAssertions(() =>
			{
				AssertNull("Both arguments == null", GoodsLoadingPlaceProvider.NewOrNull(null, null));
				AssertNotNull("OrgAddress == null", GoodsLoadingPlaceProvider.NewOrNull(entryInstruction, null));
				AssertNotNull("EntryHeader == null", GoodsLoadingPlaceProvider.NewOrNull(null, testOrgAddress));
			});
		}

		public void TestLoadingPlaceCode()
		{
			AssertEquals("BB00", dataProvider.LoadingPlaceCode);
		}

		public void TestAddress()
		{
			AssertEquals("TESTSTRASSE 1", dataProvider.Address);
		}

		public void TestCity()
		{
			AssertEquals("MAINZ", dataProvider.City);
		}

		public void TestPostcode()
		{
			AssertEquals("55126", dataProvider.Postcode);
		}

		public void TestCountry()
		{
			AssertEquals(string.Empty, dataProvider.Country);
		}

		public void TestAdditionalAddressInfo()
		{
			AssertEquals("COMPLEMENT", dataProvider.AdditionalAddressInfo);
		}

		public void TestTypeOfLocation()
		{
			AssertNullOrEmpty(dataProvider.TypeOfLocation);
		}

		public void TestQualifierOfIdentification()
		{
			AssertNullOrEmpty(dataProvider.QualifierOfIdentification);
		}

		public void TestAuthorisationNumber()
		{
			AssertNullOrEmpty(dataProvider.AuthorisationNumber);
		}

		public void TestAdditionalIdentifier()
		{
			AssertNullOrEmpty(dataProvider.AdditionalIdentifier);
		}

		public void TestUNLocode()
		{
			AssertNullOrEmpty(dataProvider.UNLocode);
		}

		public void TestCustomsOfficeReferenceNumber()
		{
			AssertNullOrEmpty(dataProvider.CustomsOfficeReferenceNumber);
		}

		public void TestGNSSLatitude()
		{
			AssertNullOrEmpty(dataProvider.GNSSLatitude);
		}

		public void TestGNSSLongitude()
		{
			AssertNullOrEmpty(dataProvider.GNSSLongitude);
		}

		public void TestEconomicOperatorIdentificationNumber()
		{
			AssertNullOrEmpty(dataProvider.EconomicOperatorIdentificationNumber);
		}

		public void TestPostcodeAddressHouseNumber()
		{
			AssertNullOrEmpty(dataProvider.PostcodeAddressHouseNumber);
		}

		public void TestPostcodeAddressPostcode()
		{
			AssertNullOrEmpty(dataProvider.PostcodeAddressPostcode);
		}

		public void TestPostcodeAddressCountry()
		{
			AssertNullOrEmpty(dataProvider.PostcodeAddressCountry);
		}

		public void TestContactPerson()
		{
			AssertNull(dataProvider.ContactPerson);
		}

		public void TestAddressComplementOfInformation()
		{
			AssertNullOrEmpty(dataProvider.AddressComplementOfInformation);
		}

		protected override void SetUp()
		{
			base.SetUp();
			entryInstruction = Factory.New<CusEntryInstruction>();
			entryInstruction.GoodsLocation.LoadingPlace = "BB00";
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			testOrgAddress = orgHeader.Addresses.AddNew();
			testOrgAddress.OA_Address1 = "TESTSTRASSE 1";
			testOrgAddress.OA_City = "MAINZ";
			testOrgAddress.OA_PostCode = "55126";
			testOrgAddress.PrimaryOrgAddressAdditionalInfoDetail = "COMPLEMENT";
			dataProvider = GoodsLoadingPlaceProvider.NewOrNull(entryInstruction, testOrgAddress);
		}
		CusEntryInstruction entryInstruction;
		OrgAddress testOrgAddress;
		IGoodsLoadingPlace dataProvider;

		protected override GoodsLoadingPlaceProvider GetProvider() => (GoodsLoadingPlaceProvider)dataProvider;
	}
}
