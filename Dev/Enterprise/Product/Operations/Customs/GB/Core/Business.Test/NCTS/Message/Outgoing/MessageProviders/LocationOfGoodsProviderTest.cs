using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.GB.Business.NCTS.Testing
{
	sealed class LocationOfGoodsProviderTest : DataProviderTestCase<LocationOfGoodsProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new LocationOfGoodsProvider(null));
		}

		public void TestInitialState()
		{
			AssertEquals(ZString.Empty, Provider.QualifierOfIdentification);
			AssertEquals(ZString.Empty, Provider.TypeOfLocation);
			AssertNull(Provider.AuthorisationNumber);
			AssertNull(Provider.AdditionalIdentifier);
			AssertNull(Provider.UNLocode);
			AssertNull(Provider.ContactPerson);
			AssertNull(Provider.CustomsOfficeReferenceNumber);
			AssertNull(Provider.EconomicOperatorIdentificationNumber);
			AssertNull(Provider.PostCodeAddress);
			AssertNull(Provider.GNSSLatitude);
			AssertNull(Provider.GNSSLongitude);
			AssertNull(Provider.Address);
		}

		public void TestTypeOfLocation()
		{
			movementHeader.GoodsLocation.CGL_Type = "A";
			AssertEquals("A", Provider.TypeOfLocation);
		}

		public void TestAuthorisationNumber()
		{
			movementHeader.GoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
			movementHeader.GoodsLocation.Address.E2_GovRegNum = "AUTH0001";
			AssertEquals("AUTH0001", Provider.AuthorisationNumber);
		}

		public void TestAdditionalIdentifier()
		{
			movementHeader.GoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
			movementHeader.GoodsLocation.CGL_AdditionalIdentifier = "AUTH INFO";
			AssertEquals("AUTH INFO", Provider.AdditionalIdentifier);

			movementHeader.GoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.EoriNumber;
			movementHeader.GoodsLocation.CGL_AdditionalIdentifier = "EORI INFO";
			AssertEquals("EORI INFO", Provider.AdditionalIdentifier);
		}

		public void TestQualifierOfIdentification()
		{
			movementHeader.GoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier;
			AssertEquals(CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier, Provider.QualifierOfIdentification);
		}

		public void TestUNLocode()
		{
			movementHeader.GoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.UnLocode;
			movementHeader.GoodsLocation.CGL_AdditionalIdentifier = "LocID";
			AssertEquals("LocID", Provider.UNLocode);
		}

		public void TestContactPerson()
		{
			var location = movementHeader.GoodsLocation;
			foreach (var qualifier in new CusGoodsLocationQualifierList().GetAllCodes())
			{
				location.CGL_Qualifier = qualifier;
				location.Address.E2_Contact = "Name";

				var provider = GetProvider();
				if (qualifier == CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier)
				{
					AssertNull(provider.ContactPerson);
				}
				else
				{
					AssertEquals("Name", provider.ContactPerson.Name);
				}
			}
		}

		public void TestCustomsOfficeReferenceNumber()
		{
			movementHeader.GoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier;
			movementHeader.GoodsLocation.CGL_CustomsOffice = "LocID";
			AssertEquals("LocID", Provider.CustomsOfficeReferenceNumber);
		}

		public void TestEconomicOperatorIdentificationNumber()
		{
			movementHeader.GoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.EoriNumber;
			movementHeader.GoodsLocation.Address.E2_GovRegNum = "EORI0001";
			AssertEquals("EORI0001", Provider.EconomicOperatorIdentificationNumber);
		}

		public void TestPostCodeAddress()
		{
			movementHeader.GoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.PostcodeAddress;
			movementHeader.GoodsLocation.CGL_AdditionalIdentifier = "H001";
			movementHeader.GoodsLocation.Address.E2_Postcode = "PC0001";
			movementHeader.GoodsLocation.Address.E2_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
			AssertEquals("H001", Provider.PostCodeAddress.HouseNumber);
			AssertEquals("PC0001", Provider.PostCodeAddress.Postcode);
			AssertEquals(Core.Constants.CountryCodes.UnitedKingdom, Provider.PostCodeAddress.Country);
		}

		public void TestGNSSLatitude()
		{
			movementHeader.GoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.GnssCoordinates;
			movementHeader.GoodsLocation.Address.E2_Latitude = 23.4567;
			AssertEquals("23.4567", Provider.GNSSLatitude);
		}

		public void TestGNSSLongitude()
		{
			movementHeader.GoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.GnssCoordinates;
			movementHeader.GoodsLocation.Address.E2_Longitude = 12.3456;
			AssertEquals("12.3456", Provider.GNSSLongitude);
		}

		public void TestAddress()
		{
			movementHeader.GoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.Address;
			movementHeader.GoodsLocation.Address.E2_Address1 = "Address 1";
			movementHeader.GoodsLocation.Address.E2_Postcode = "PC0001";
			movementHeader.GoodsLocation.Address.E2_City = "London";
			movementHeader.GoodsLocation.Address.E2_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
			AssertEquals("Address 1", Provider.Address.StreetAndNumber);
			AssertEquals("PC0001", Provider.Address.Postcode);
			AssertEquals("London", Provider.Address.City);
			AssertEquals(Core.Constants.CountryCodes.UnitedKingdom, Provider.Address.Country);
		}

		protected override void SetUp()
		{
			header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			movementHeader = header.ArrivalMovementHeader;
		}

		protected override LocationOfGoodsProvider GetProvider() => new LocationOfGoodsProvider(header);

		NctsHeader header;
		NctsArrivalMovementHeader movementHeader;
	}
}
