using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class TradeChainPartnerValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCA_Address()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ABC";
			var address1 = org.Addresses.AddNew();
			address1.OA_Address1 = "Test Address AU";
			address1.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;

			var address2 = org.Addresses.AddNew();
			address2.OA_Address1 = "Test Address US";
			address2.OA_PostCode = "M4B";
			address2.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;

			var address3 = org.Addresses.AddNew();
			address3.OA_Address1 = "Test Address CA";
			address3.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;

			var address4 = org.Addresses.AddNew();
			address4.OA_Address1 = "Test Address CA";
			address4.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			address4.OA_State = ZString.Empty;

			var address5 = org.Addresses.AddNew();
			address5.OA_Address1 = "Test Address CA";
			address5.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			address5.OA_PostCode = "M4B1B3";
			address5.OA_State = "ON";

			Factory.Save();

			var impAddInfo = OrgImpAddInfo.Get(org);

			var tcp = impAddInfo.TradeChainPartners.AddNew();
			tcp.CA_Address = address1.PK;
			tcp.CA_Type = TradeChainPartnersTypeList.Codes.V;
			tcp.Validation.ValidateCA_Address();
			AssertHasMessageError(tcp.CA_AddressInfo, "Country/Region for Vendor Address should be United States or Mexico.");

			tcp.CA_Address = address2.PK;
			tcp.Validation.ValidateCA_Address();
			AssertNoMessageError(tcp.CA_AddressInfo, "Country/Region for Vendor Address should be United States or Mexico.");

			tcp.CA_Type = TradeChainPartnersTypeList.Codes.C;
			tcp.Validation.ValidateCA_Address();
			AssertHasMessageError(tcp.CA_AddressInfo, "Country/Region for Consignee Address should be Canada.");

			tcp.CA_Address = address3.PK;
			tcp.Validation.ValidateCA_Address();
			AssertNoMessageError(tcp.CA_AddressInfo, "Country/Region for Consignee Address should be Canada.");

			tcp.CA_Address = address4.PK;
			tcp.CA_Type = TradeChainPartnersTypeList.Codes.C;
			AssertHasMessageError(tcp.CA_AddressInfo, "A province/state is required when the country/region is Canada or United States. Please add a province/state or go to the organization and add to organization address details.");
			AssertHasMessageError(tcp.CA_AddressInfo, "A postal/zip code is required when the country/region is Canada or United States. Please add a postal/zip code or go to the organization and add to organization address details.");

			tcp.CA_Address = address5.PK;
			tcp.CA_Type = TradeChainPartnersTypeList.Codes.C;
			tcp.Validation.ValidateCA_Address();
			AssertNoMessageError(tcp.CA_AddressInfo, "A province/state is required when the country/region is Canada or United States. Please add a province/state or go to the organization and add to organization address details.");
			AssertNoMessageError(tcp.CA_AddressInfo, "A postal/zip code is required when the country/region is Canada or United States. Please add a postal/zip code or go to the organization and add to organization address details.");

			tcp.CA_Address = address2.PK;
			tcp.CA_Type = TradeChainPartnersTypeList.Codes.V;
			tcp.Validation.ValidateCA_Address();
			AssertHasMessageError(tcp.CA_AddressInfo, "A province/state is required when the country/region is Canada or United States. Please add a province/state or go to the organization and add to organization address details.");
			AssertHasMessageError(tcp.CA_AddressInfo, "This US zip code is invalid. A US Zip code should be in the following format: 99999 or 99999-9999, where 9 is a digit.");
		}
	}
}
