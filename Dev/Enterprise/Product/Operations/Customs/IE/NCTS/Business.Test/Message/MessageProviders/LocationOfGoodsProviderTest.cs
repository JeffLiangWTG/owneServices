using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	class DepartureLocationOfGoodsProviderTest : LocationOfGoodsProviderTest
	{
		protected override string MovementType => NctsMovementType.Codes.Departure;

		protected override EU.Business.CusGoodsLocation GetCusGoodsLocation() => nctsHeader.MovementHeader.GoodsLocation;

		protected override LocationOfGoodsProvider GetProvider() => new LocationOfGoodsProvider(nctsHeader.MovementHeader);
	}

	class ArrivalLocationOfGoodsProviderTest : LocationOfGoodsProviderTest
	{
		protected override string MovementType => NctsMovementType.Codes.Arrival;

		protected override EU.Business.CusGoodsLocation GetCusGoodsLocation() => nctsHeader.ArrivalMovementHeader.GoodsLocation;

		protected override LocationOfGoodsProvider GetProvider() => new LocationOfGoodsProvider(nctsHeader.ArrivalMovementHeader);
	}

	abstract class LocationOfGoodsProviderTest : Customs.Business.Testing.DataProviderTestCase<LocationOfGoodsProvider>
	{
		public void TestLocationCodeType()
		{
			goodsLocation.CGL_Type = "A";
			AssertEquals("A", Provider.LocationCodeType);
			goodsLocation.CGL_Type = "B";
			AssertEquals("B", Provider.LocationCodeType);
		}

		public void TestQualifierOfIdentification()
		{
			goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.UnLocode;
			AssertEquals("U", Provider.QualifierOfIdentification);
		}

		public void TestAuthorisationNumber()
		{
			goodsLocation.Address.AuthorisationNumber = "AUTH1234";
			AssertEquals("AUTH1234", Provider.AuthorisationNumber);
		}

		public void TestAdditionalIdentifier()
		{
			goodsLocation.CGL_AdditionalIdentifier = "AB12";
			AssertEquals("AB12", Provider.AdditionalIdentifier);
		}

		public void TestUNLocode()
		{
			goodsLocation.Unlocode = "UNL12345";
			AssertEquals("UNL12345", Provider.UNLocode);
		}

		public void TestCustomsOffice()
		{
			goodsLocation.CGL_CustomsOffice = "IE001";
			AssertEquals("IE001", Provider.CustomsOffice);
		}

		public void TestGNSS()
		{
			var address = goodsLocation.Address;
			address.E2_Longitude = 53.33;
			address.E2_Latitude = -8.75;
			CombineAssertions(() =>
			{
				AssertEquals("GNSS.Latitude", "-8.75", Provider.GNSS.Latitude);
				AssertEquals("GNSS.Longitude", "53.33", Provider.GNSS.Longitude);
			});
		}

		public void TestEconomicOperator()
		{
			goodsLocation.Address.E2_AddressOverride = true;
			goodsLocation.CGL_Qualifier = "X";
			goodsLocation.Address.E2_GovRegNum = "TEST999";
			AssertEquals("TEST999", Provider.EconomicOperator);
		}

		public void TestEconomicOperator_FallbackToOrganisation()
		{
			goodsLocation.Address.E2_AddressOverride = true;
			goodsLocation.CGL_Qualifier = "X";
			goodsLocation.Address.Organisation.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "0123456789001");
			goodsLocation.Address.E2_GovRegNum = string.Empty;
			AssertEquals("IE0123456789001", Provider.EconomicOperator);
		}

		public void TestAddress()
		{
			SetGoodsLocationAddress("0123456789000");
			CombineAssertions(() =>
			{
				AssertEquals("Address.Line", "123 Test Street", Provider.Address.StreetAndNumber);
				AssertEquals("Address.PostcodeID", "A12B3C4", Provider.Address.Postcode);
				AssertEquals("Address.CityName", "City", Provider.Address.City);
				AssertEquals("Address.CountryCode", "IE", Provider.Address.Country);
			});
		}

		public void TestPostcodeAddress()
		{
			SetGoodsLocationAddress("");
			CombineAssertions(() =>
			{
				AssertEquals("PostcodeAddress.Line", "123 Test Street", Provider.PostcodeAddress.StreetAndNumber);
				AssertEquals("PostcodeAddress.PostcodeID", "A12B3C4", Provider.PostcodeAddress.Postcode);
				AssertEquals("PostcodeAddress.CountryCode", "IE", Provider.PostcodeAddress.Country);
			});
		}

		public void TestContactPerson()
		{
			SetGoodsLocationAddressContact(goodsLocation.Address);
			CombineAssertions(() =>
			{
				AssertEquals("Contact", "Joe Bloggs", Provider.ContactPerson.Name);
				AssertEquals("Phone", "5551234", Provider.ContactPerson.PhoneNumber);
				AssertEquals("Email", "test@example.com", Provider.ContactPerson.EmailAddress);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(MovementType);
			goodsLocation = GetCusGoodsLocation();
		}
		EU.Business.CusGoodsLocation goodsLocation;
		protected NctsHeader nctsHeader;

		protected abstract string MovementType { get; }

		protected abstract EU.Business.CusGoodsLocation GetCusGoodsLocation();

		void SetGoodsLocationAddress(string id)
		{
			NCTSTestHelper.CreateJobDocAddressForTest(Factory, "PC1", goodsLocation.Address, string.Empty, "Test Company Limited", "123 Test Street", "A12B3C4", "City", "IEXX", "IE", id, "TIR123");
		}

		void SetGoodsLocationAddressContact(EU.Business.CusGoodsLocationAddress address)
		{
			address.E2_Email = "test@example.com";
			address.E2_Phone = "5551234";
			address.E2_Contact = "Joe Bloggs";
		}
	}
}
