using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers.Base.Testing
{
	sealed class PostalAddressFormatterTest : TestCaseWithFactory
	{
		public void TestNotSpecifyingLocalCountry()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			organisation.OH_FullName = "ROTTEN DOGS AND HATSTANDS ROTTEN DOGS AND HATSTANDS ROTTEN DOGS AND HATSTANDS";

			OrgAddress mainAddress = organisation.MainAddress;
			mainAddress.OA_Address1 = "MAIN ADDRESS 1";
			mainAddress.OA_Address2 = "MAIN ADDRESS 2";
			mainAddress.OA_City = "ALEXANDRIA";
			mainAddress.OA_RL_NKRelatedPortCode = "NZAKL";
			mainAddress.OA_RN_NKCountryCode = "AU";
			mainAddress.OA_State = "NSW";
			mainAddress.OA_PostCode = "2015";

			OrgAddress otherAddress = organisation.Addresses.AddNew();
			otherAddress.OA_Address1 = "OTHER ADDRESS 1";
			otherAddress.OA_Address2 = "";
			otherAddress.OA_City = "OTHER ALEXANDRIA";
			otherAddress.OA_RL_NKRelatedPortCode = "NZAKL";
			otherAddress.OA_RN_NKCountryCode = null;
			otherAddress.OA_State = "OSW";
			otherAddress.OA_PostCode = "2099";

			AssertMultilineASCIIEquals("formatter.GetPostalAddressFrom(mainAddress)"
				, "ROTTEN DOGS AND HATSTANDS ROTTEN DOGS AND HATSTANDS ROTTEN DOGS AND HATSTANDS\nMAIN ADDRESS 1\nMAIN ADDRESS 2\nALEXANDRIA NSW 2015\nAUSTRALIA"
				, new PostalAddressFormatter(mainAddress).PostalAddress());

			AssertMultilineASCIIEquals("formatter.GetPostalAddressFrom(otherAddress)"
				, "ROTTEN DOGS AND HATSTANDS ROTTEN DOGS AND HATSTANDS ROTTEN DOGS AND HATSTANDS\nOTHER ADDRESS 1\nOTHER ALEXANDRIA 2099\nNEW ZEALAND"
				, new PostalAddressFormatter(otherAddress).PostalAddress());
		}

		/* TODO can remove?
		public void TestSpecifyingLocalCountry()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			organisation.OH_FullName = "ROTTEN DOGS AND HATSTANDS";

			OrgAddress mainAddress = organisation.MainAddress;
			mainAddress.OA_Address1 = "MAIN ADDRESS 1";
			mainAddress.OA_Address2 = "MAIN ADDRESS 2";
			mainAddress.OA_City = "ALEXANDRIA";
			mainAddress.OA_State = "NSW";
			mainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			mainAddress.OA_PostCode = "2015";

			OrgAddress otherAddress = organisation.Addresses.AddNew();
			otherAddress.OA_Address1 = "OTHER ADDRESS 1";
			otherAddress.OA_Address2 = "";
			otherAddress.OA_City = "OTHER ALEXANDRIA";
			otherAddress.OA_State = "OSW";
			otherAddress.OA_RL_NKRelatedPortCode = "NZAKL";
			otherAddress.OA_PostCode = "2099";

			PostalAddressFormatter formatter = new PostalAddressFormatter(Factory, RefCountry.LoadFromCountryCode(Factory, "AU"));

			AssertMultilineASCIIEquals("formatter.GetPostalAddressFrom(mainAddress)"
				, "ROTTEN DOGS AND HATSTANDS\nMAIN ADDRESS 1\nMAIN ADDRESS 2\nALEXANDRIA NSW 2015"
				, formatter.GetPostalAddressFrom(mainAddress));

			AssertMultilineASCIIEquals("formatter.GetPostalAddressFrom(otherAddress)"
				, "ROTTEN DOGS AND HATSTANDS\nOTHER ADDRESS 1\nOTHER ALEXANDRIA 2099\nNEW ZEALAND"
				, formatter.GetPostalAddressFrom(otherAddress));
		}
		 */

		public void TestGetAddressFromJobDocAddress()
		{
			JobDocAddress docAddress = Factory.New<JobDocAddress>();
			docAddress.E2_CompanyName = "ROTTEN DOGS AND HATSTANDS";
			docAddress.E2_Address1 = "MAIN ADDRESS 1";
			docAddress.E2_Address2 = "MAIN ADDRESS 2";
			docAddress.E2_City = "ALEXANDRIA";
			docAddress.E2_State = "NSW";
			docAddress.E2_RN_NKCountryCode = "AU";
			docAddress.E2_Postcode = "2015";

			AssertMultilineASCIIEquals("formatter.GetPostalAddressFrom(organisation)"
				, "ROTTEN DOGS AND HATSTANDS\nMAIN ADDRESS 1\nMAIN ADDRESS 2\nALEXANDRIA NSW 2015\nAUSTRALIA"
				, new PostalAddressFormatter(docAddress).PostalAddress());
		}

		public void TestGetAddressWithAdditionalInfo()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.OH_FullName = "ROTTEN DOGS AND HATSTANDS";

			OrgAddress mainAddress = organisation.MainAddress;
			mainAddress.OA_AdditionalAddressInformation = "MAIN BUILDING";
			mainAddress.OA_Address1 = "MAIN ADDRESS 1";
			mainAddress.OA_Address2 = "MAIN ADDRESS 2";
			mainAddress.OA_City = "ALEXANDRIA";
			mainAddress.OA_RN_NKCountryCode = "AU";
			mainAddress.OA_State = "NSW";
			mainAddress.OA_PostCode = "2015";

			AssertMultilineASCIIEquals("formatter.GetPostalAddressFrom(organisation)"
				, "ROTTEN DOGS AND HATSTANDS\nMAIN BUILDING\nMAIN ADDRESS 1\nMAIN ADDRESS 2\nALEXANDRIA NSW 2015\nAUSTRALIA"
				, new PostalAddressFormatter(mainAddress).PostalAddress());
		}

		public void TestGetAddressWithAdditionalInfo_OverrideAdditionalInfo()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_FullName = "ROTTEN DOGS AND HATSTANDS";

			var mainAddress = orgHeader.MainAddress;
			mainAddress.OA_AdditionalAddressInformation = "MAIN BUILDING";
			mainAddress.OA_Address1 = "MAIN ADDRESS 1";
			mainAddress.OA_Address2 = "MAIN ADDRESS 2";
			mainAddress.OA_City = "ALEXANDRIA";
			mainAddress.OA_RN_NKCountryCode = "AU";
			mainAddress.OA_State = "NSW";
			mainAddress.OA_PostCode = "2015";
			const string testAddress = "ROTTEN DOGS AND HATSTANDS\nMAIN BUILDING\nMAIN ADDRESS 1\nMAIN ADDRESS 2\nALEXANDRIA NSW 2015\nAUSTRALIA";
			AssertMultilineASCIIEquals(testAddress, new PostalAddressFormatter(mainAddress, null).PostalAddress());
			AssertMultilineASCIIEquals(testAddress, new PostalAddressFormatter(mainAddress, string.Empty).PostalAddress());
			AssertMultilineASCIIEquals(testAddress, new PostalAddressFormatter(mainAddress, " ").PostalAddress());
			AssertMultilineASCIIEquals("ROTTEN DOGS AND HATSTANDS\nOVERRIDE ADDITIONAL INFO\nMAIN ADDRESS 1\nMAIN ADDRESS 2\nALEXANDRIA NSW 2015\nAUSTRALIA", new PostalAddressFormatter(mainAddress, "OVERRIDE ADDITIONAL INFO").PostalAddress());
		}
	}
}
