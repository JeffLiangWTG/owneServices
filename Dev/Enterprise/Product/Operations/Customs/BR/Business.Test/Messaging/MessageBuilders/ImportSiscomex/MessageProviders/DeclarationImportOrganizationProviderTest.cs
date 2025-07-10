using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business.ImportSiscomex.Testing
{
	class DeclarationImportOrganizationProviderTest : TestCaseWithFactory
	{
		public void TestNew_OrganizationIsNull()
		{
			AssertEquals("DeclarationImportOrganizationProvider", null, DeclarationImportOrganizationProvider.New(null));
		}

		public void TestProperties_OrganizationIsNotNull()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType(RefCusMapTypeList.Codes.Country, "BTH", "Country Codes Mapping", true);
			helper.CreateCusMap(RefCusMapTypeList.Codes.Country, "BR", "105", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), Core.Constants.CountryCodes.Brazil);
			Factory.Save();

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_FullName = "IMPORTER SISCOMEX";
			orgHeader.PrimaryRegistrationNumber.Number = "58.500.398/0001-05";
			orgHeader.MainAddress.OA_PostCode = "02430-000";
			orgHeader.MainAddress.UnrestrictedAdditionalAddressInformation = "COMPLEMENTARY ADDRESS";
			orgHeader.MainAddress.OA_Address1 = "MAIN ADDRESS";
			orgHeader.MainAddress.OA_City = "CAMPINAS";
			orgHeader.MainAddress.OA_State = "SP";
			orgHeader.MainAddress.OA_Phone = "55 11-35856000";

			var dataProvider = new DeclarationImportOrganizationProvider(orgHeader);

			CombineAssertions(() =>
			{
				AssertEquals("ID", "58500398000105", dataProvider.ID);
				AssertEquals("Name", "IMPORTER SISCOMEX", dataProvider.Name);
				AssertEquals("ZipCode", "02430000", dataProvider.ZipCode);
				AssertEquals("CountryCode", "105", dataProvider.CountryCode);
				AssertEquals("AddressComplementary", "COMPLEMENTARY ADDRESS", dataProvider.AddressComplementary);
				AssertEquals("Address", "MAIN ADDRESS", dataProvider.Address);
				AssertEquals("CityName", "CAMPINAS", dataProvider.CityName);
				AssertEquals("AddressNumber", ZString.Empty, dataProvider.AddressNumber);
				AssertEquals("AddressStateCode", "SP", dataProvider.AddressStateCode);
				AssertEquals("PhoneNumber", "551135856000", dataProvider.PhoneNumber);
			});
		}
	}
}
