using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Management.Matching;

namespace Enterprise.UniversalDataBuss.Matching.Testing
{
	class OrganizationAddressMatchingComparatorTest : TestCaseWithFactory
	{
		public void TestHashCodeWorksFromContentsNotInstanceID()
		{
			var comparator = new OrganizationAddressMatchingComparator();
			var org1 = GetNewPopulatedOrganizationAddress();
			var org2 = GetNewPopulatedOrganizationAddress();

			AssertEquals("Comparing GetHashCode() with no content differences", comparator.GetHashCode(org1), comparator.GetHashCode(org2));

			org2.Address1 = "SOMETHING ELSE";
			AssertNotEquals("Comparing GetHashCode() with one content difference", comparator.GetHashCode(org1), comparator.GetHashCode(org2));
		}

		public void TestStraightMatch()
		{
			var org1 = GetNewPopulatedOrganizationAddress();
			var org2 = GetNewPopulatedOrganizationAddress();
			AssertEquals("Compare 2 identical OrganizationAddresses", true, new OrganizationAddressMatchingComparator().Equals(org1, org2));
		}

		public void TestDifferentScreeningStatusAddressTypeAndAddressOverrideMakeNoDifference()
		{
			var org1 = GetNewPopulatedOrganizationAddress();
			var org2 = GetNewPopulatedOrganizationAddress();
			org2.ScreeningStatus = new CodeDescriptionPair() { Code = "XOX" };
			org2.AddressType = "SomethingDifferent";
			org2.AddressOverride = true;

			AssertEquals("Compare with different 'Irrelevant' fields", true, new OrganizationAddressMatchingComparator().Equals(org1, org2));

			org2.ScreeningStatus = null;
			org2.AddressType = null;
			org2.AddressOverride = null;

			AssertEquals("Compare with null 'Irrelevant' fields", true, new OrganizationAddressMatchingComparator().Equals(org1, org2));
		}

		public void TestDifferentValidationStatusAndAddressOverrideMakeNoDifference()
		{
			var org1 = GetNewPopulatedOrganizationAddress();
			var org2 = GetNewPopulatedOrganizationAddress();
			org2.ValidationStatus = new CodeDescriptionPair() { Code = "NYV" };
			org2.AddressOverride = true;

			AssertEquals("Compare with different 'Irrelevant' fields", true, new OrganizationAddressMatchingComparator().Equals(org1, org2));

			org2.ValidationStatus = null;
			org2.AddressOverride = null;

			AssertEquals("Compare with null 'Irrelevant' fields", true, new OrganizationAddressMatchingComparator().Equals(org1, org2));
		}

		public void TestDifferentGeoLocationAndAddressOverrideMakeNoDifference()
		{
			var org1 = GetNewPopulatedOrganizationAddress();
			var org2 = GetNewPopulatedOrganizationAddress();
			org2.GeoLocation = GeoLocation.New(CargoWise.Types.ZGeography.CreatePoint(1.23, 4.56));
			org2.AddressOverride = true;

			AssertEquals("Compare with different 'Irrelevant' fields", true, new OrganizationAddressMatchingComparator().Equals(org1, org2));

			org2.ValidationStatus = null;
			org2.AddressOverride = null;

			AssertEquals("Compare with null 'Irrelevant' fields", true, new OrganizationAddressMatchingComparator().Equals(org1, org2));
		}

		public void TestDifferentOrganizationCode()
		{
			var org1 = GetNewPopulatedOrganizationAddress();
			var org2 = GetNewPopulatedOrganizationAddress();
			org2.OrganizationCode = "TIWAS22";
			AssertEquals("Compare with different OrganizationCode", false, new OrganizationAddressMatchingComparator().Equals(org1, org2));

			org2.OrganizationCode = null;
			AssertEquals("Compare with null OrganizationCode", false, new OrganizationAddressMatchingComparator().Equals(org1, org2));
		}

		public void TestDifferentAddressShortCode()
		{
			var org1 = GetNewPopulatedOrganizationAddress();
			var org2 = GetNewPopulatedOrganizationAddress();
			org2.AddressShortCode = "THATS IT, WHAT A SHOT";
			AssertEquals("Compare with different AddressShortCode", false, new OrganizationAddressMatchingComparator().Equals(org1, org2));

			org2.AddressShortCode = null;
			AssertEquals("Compare with null AddressShortCode", false, new OrganizationAddressMatchingComparator().Equals(org1, org2));
		}

		public void TestDifferentCompanyName()
		{
			var org1 = GetNewPopulatedOrganizationAddress();
			var org2 = GetNewPopulatedOrganizationAddress();
			org2.CompanyName = "THATS IT";
			AssertEquals("Compare with different CompanyName", false, new OrganizationAddressMatchingComparator().Equals(org1, org2));

			org2.CompanyName = null;
			AssertEquals("Compare with null CompanyName", false, new OrganizationAddressMatchingComparator().Equals(org1, org2));
		}

		public void TestDifferentAddress1()
		{
			var org1 = GetNewPopulatedOrganizationAddress();
			var org2 = GetNewPopulatedOrganizationAddress();
			org2.Address1 = "WHAT A SHOT";
			AssertEquals("Compare with different Address1", false, new OrganizationAddressMatchingComparator().Equals(org1, org2));

			org2.Address1 = null;
			AssertEquals("Compare with null Address1", false, new OrganizationAddressMatchingComparator().Equals(org1, org2));
		}

		public void TestDifferentAddress2()
		{
			var org1 = GetNewPopulatedOrganizationAddress();
			var org2 = GetNewPopulatedOrganizationAddress();
			org2.Address2 = "HERE WE ARE";
			AssertEquals("Compare with different Address2", false, new OrganizationAddressMatchingComparator().Equals(org1, org2));

			org2.Address2 = null;
			AssertEquals("Compare with null Address2", false, new OrganizationAddressMatchingComparator().Equals(org1, org2));
		}

		public void TestDifferentCity()
		{
			var org1 = GetNewPopulatedOrganizationAddress();
			var org2 = GetNewPopulatedOrganizationAddress();
			org2.City = "SWAGGERSTON";
			AssertEquals("Compare with different City", false, new OrganizationAddressMatchingComparator().Equals(org1, org2));

			org2.City = null;
			AssertEquals("Compare with null City", false, new OrganizationAddressMatchingComparator().Equals(org1, org2));
		}

		public void TestDifferentState()
		{
			var org1 = GetNewPopulatedOrganizationAddress();
			var org2 = GetNewPopulatedOrganizationAddress();
			org2.State = "TAS";
			AssertEquals("Compare with different State", false, new OrganizationAddressMatchingComparator().Equals(org1, org2));

			org2.State = null;
			AssertEquals("Compare with null State", false, new OrganizationAddressMatchingComparator().Equals(org1, org2));
		}

		public void TestDifferentPostcode()
		{
			var org1 = GetNewPopulatedOrganizationAddress();
			var org2 = GetNewPopulatedOrganizationAddress();
			org2.Postcode = "3289";
			AssertEquals("Compare with different Postcode", false, new OrganizationAddressMatchingComparator().Equals(org1, org2));

			org2.Postcode = null;
			AssertEquals("Compare with null Postcode", false, new OrganizationAddressMatchingComparator().Equals(org1, org2));
		}

		public void TestDifferentPort()
		{
			var org1 = GetNewPopulatedOrganizationAddress();
			var org2 = GetNewPopulatedOrganizationAddress();

			org2.Port = new UNLOCO() { Code = "AUMEL" };
			AssertEquals("Compare with same values in new Port", true, new OrganizationAddressMatchingComparator().Equals(org1, org2));

			org2.Port = new UNLOCO() { Code = "AUBKK" };
			AssertEquals("Compare with different Port", false, new OrganizationAddressMatchingComparator().Equals(org1, org2));

			org2.Port = null;
			AssertEquals("Compare with null Port", false, new OrganizationAddressMatchingComparator().Equals(org1, org2));
		}

		public void TestDifferentCountry()
		{
			var org1 = GetNewPopulatedOrganizationAddress();
			var org2 = GetNewPopulatedOrganizationAddress();
			org2.Country = new Country() { Code = "AU" };
			AssertEquals("Compare with same values in a new Country", true, new OrganizationAddressMatchingComparator().Equals(org1, org2));

			org2.Country = new Country() { Code = "NZ" };
			AssertEquals("Compare with different Country", false, new OrganizationAddressMatchingComparator().Equals(org1, org2));

			org2.Country = null;
			AssertEquals("Compare with null Country", false, new OrganizationAddressMatchingComparator().Equals(org1, org2));
		}

		public void TestDifferentContact()
		{
			var org1 = GetNewPopulatedOrganizationAddress();
			var org2 = GetNewPopulatedOrganizationAddress();
			org2.Contact = "PHIL MCRACKEN";
			AssertEquals("Compare with different Contact", false, new OrganizationAddressMatchingComparator().Equals(org1, org2));

			org2.Contact = null;
			AssertEquals("Compare with null Contact", false, new OrganizationAddressMatchingComparator().Equals(org1, org2));
		}

		public void TestDifferentMobile()
		{
			var org1 = GetNewPopulatedOrganizationAddress();
			var org2 = GetNewPopulatedOrganizationAddress();
			org2.Mobile = "0434 888 777";
			AssertEquals("Compare with different Mobile", false, new OrganizationAddressMatchingComparator().Equals(org1, org2));

			org2.Mobile = null;
			AssertEquals("Compare with null Mobile", false, new OrganizationAddressMatchingComparator().Equals(org1, org2));
		}

		public void TestDifferentEmail()
		{
			var org1 = GetNewPopulatedOrganizationAddress();
			var org2 = GetNewPopulatedOrganizationAddress();
			org2.Email = "phil.burton@crackerjackpromotions.com.au";
			AssertEquals("Compare with different Email", false, new OrganizationAddressMatchingComparator().Equals(org1, org2));

			org2.Email = null;
			AssertEquals("Compare with null Email", false, new OrganizationAddressMatchingComparator().Equals(org1, org2));
		}

		public void TestDifferentPhone()
		{
			var org1 = GetNewPopulatedOrganizationAddress();
			var org2 = GetNewPopulatedOrganizationAddress();
			org2.Phone = "61 3 4392 4333";
			AssertEquals("Compare with different Phone", false, new OrganizationAddressMatchingComparator().Equals(org1, org2));

			org2.Phone = null;
			AssertEquals("Compare with null Phone", false, new OrganizationAddressMatchingComparator().Equals(org1, org2));
		}

		public void TestDifferentFax()
		{
			var org1 = GetNewPopulatedOrganizationAddress();
			var org2 = GetNewPopulatedOrganizationAddress();
			org2.Fax = "61 3 4328 2891";
			AssertEquals("Compare with different Fax", false, new OrganizationAddressMatchingComparator().Equals(org1, org2));

			org2.Fax = null;
			AssertEquals("Compare with null Fax", false, new OrganizationAddressMatchingComparator().Equals(org1, org2));
		}

		public void TestDifferentGovRegNum()
		{
			var org1 = GetNewPopulatedOrganizationAddress();
			var org2 = GetNewPopulatedOrganizationAddress();
			org2.GovRegNum = "829 439 217";
			AssertEquals("Compare with different GovRegNum", false, new OrganizationAddressMatchingComparator().Equals(org1, org2));

			org2.GovRegNum = null;
			AssertEquals("Compare with null GovRegNum", false, new OrganizationAddressMatchingComparator().Equals(org1, org2));
		}

		public void TestDifferentGovRegNumType()
		{
			var org1 = GetNewPopulatedOrganizationAddress();
			var org2 = GetNewPopulatedOrganizationAddress();
			org2.GovRegNumType = new RegistrationNumberType() { Code = "GST" };
			AssertEquals("Compare with same values in a new GovRegNumType", true, new OrganizationAddressMatchingComparator().Equals(org1, org2));

			org2.GovRegNumType = new RegistrationNumberType() { Code = "GDD" };
			AssertEquals("Compare with different GovRegNumType", false, new OrganizationAddressMatchingComparator().Equals(org1, org2));

			org2.GovRegNumType = null;
			AssertEquals("Compare with null GovRegNumType", false, new OrganizationAddressMatchingComparator().Equals(org1, org2));
		}

		public void TestDifferentRegistrationNumberCollection()
		{
			var org1 = GetNewPopulatedOrganizationAddress();
			var org2 = GetNewPopulatedOrganizationAddress();
			org2.SetRegistrationNumberCollection(() => new List<RegistrationNumber>() { new RegistrationNumber() { Type = new RegistrationNumberType() { Code = "ABN" }, Value = "11 428 320 321", } });
			AssertEquals("Compare with same values in new RegistrationNumberCollection", true, new OrganizationAddressMatchingComparator().Equals(org1, org2));

			org2.SetRegistrationNumberCollection(() => new List<RegistrationNumber>() { new RegistrationNumber() { Type = new RegistrationNumberType() { Code = "AGM" }, Value = "423789087324", } });
			AssertEquals("Compare with different RegistrationNumberCollection", false, new OrganizationAddressMatchingComparator().Equals(org1, org2));

			org2.SetRegistrationNumberCollection(() => null);
			AssertEquals("Compare with null RegistrationNumberCollection", false, new OrganizationAddressMatchingComparator().Equals(org1, org2));
		}

		public void TestDifferentUniversalNettingCode()
		{
			var org1 = GetNewPopulatedOrganizationAddress();
			var org2 = GetNewPopulatedOrganizationAddress();
			org2.UniversalNettingCode = "MANDY";
			AssertEquals("Compare with different UniversalNettingCode", false, new OrganizationAddressMatchingComparator().Equals(org1, org2));

			org2.UniversalNettingCode = null;
			AssertEquals("Compare with null UniversalNettingCode", false, new OrganizationAddressMatchingComparator().Equals(org1, org2));
		}

		public void TestDifferentUniversalOfficeCode()
		{
			var org1 = GetNewPopulatedOrganizationAddress();
			var org2 = GetNewPopulatedOrganizationAddress();
			org2.UniversalOfficeCode = "MANDY";
			AssertEquals("Compare with different UniversalOfficeCode", false, new OrganizationAddressMatchingComparator().Equals(org1, org2));

			org2.UniversalOfficeCode = null;
			AssertEquals("Compare with null UniversalOfficeCode", false, new OrganizationAddressMatchingComparator().Equals(org1, org2));
		}

		static OrganizationAddress GetNewPopulatedOrganizationAddress()
		{
			var result = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = "ConsignorDocumentaryAddress",
				AddressOverride = false,
				OrganizationCode = "ABC123",
				AddressShortCode = "145 OCCUPANCY LANE, THOMASTOWN",
				CompanyName = "CRACKERJACK PROMOTIONS",
				Address1 = "145 OCCUPANCY LANE",
				Address2 = "",
				City = "THOMASTOWN",
				State = "VIC",
				Postcode = "3381",
				Port = new UNLOCO() { Code = "AUMEL" },
				Country = new Country() { Code = "AU" },
				Contact = "JOHN THOMPSON",
				Mobile = "0434 228 439",
				Email = "john.thompson@crackerjackpromotions.com.au",
				Phone = "61 3 4392 4239",
				Fax = "61 3 4328 3133",
				GovRegNum = "392 290 332",
				GovRegNumType = new RegistrationNumberType() { Code = "GST" },

				ScreeningStatus = new CodeDescriptionPair() { Code = "LAL" },
				ValidationStatus = new CodeDescriptionPair() { Code = "NYV" },
				GeoLocation = GeoLocation.New(CargoWise.Types.ZGeography.Empty),

				UniversalNettingCode = "MILLY",
				UniversalOfficeCode = "MOLLY",
			};
			result.SetRegistrationNumberCollection(() => new List<RegistrationNumber>()
				{
					new RegistrationNumber()
					{
						Type = new RegistrationNumberType() { Code = "ABN" },
						Value = "11 428 320 321",
					}
				});
			return result;
		}
	}
}
