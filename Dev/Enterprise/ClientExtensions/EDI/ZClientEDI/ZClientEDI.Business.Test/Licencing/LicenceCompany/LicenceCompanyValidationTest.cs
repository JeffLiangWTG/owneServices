using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	class LicenceCompanyValidationTest : BusinessObjectValidationTestCase
	{
		public void TestLC_CompanyCountry_UnsupportedCountryCannotBeSaved()
		{
			EDIDataRegistry.CreateProductsAndModulesForTest();
			LicenceCompany company = Factory.New<LicenceCompany>();

			company.LC_CompanyCountry = "ER";
			AssertNoErrors(company.LC_CompanyCountryInfo);

			LicenceHeader licence1 = company.GetHeader(company.LicDatabases.AddNew());
			LicenceHeader licence2 = company.GetHeader(company.LicDatabases.AddNew());

			licence1.Database.LD_Product = "AAA";
			licence2.Database.LD_Product = "BBB";

			company.Validation.ValidateLC_CompanyCountry();
			AssertNoErrors(company.LC_CompanyCountryInfo);

			licence1.Database.LD_Product = ProductTypes.Codes.Enterprise;
			company.Validation.ValidateLC_CompanyCountry();
			AssertHasError(company.LC_CompanyCountryInfo, LicenceCompanyValidation.SelectedCountryNotSupported);

			licence1.Database.LD_Product = "AAA";
			licence2.Database.LD_Product = ProductTypes.Codes.Enterprise;
			company.Validation.ValidateLC_CompanyCountry();
			AssertHasError(company.LC_CompanyCountryInfo, LicenceCompanyValidation.SelectedCountryNotSupported);

			licence2.Database.LD_Product = ProductTypes.Codes.CargoWiseOne;
			company.Validation.ValidateLC_CompanyCountry();
			AssertNoErrors(company.LC_CompanyCountryInfo);
			AssertNoWarnings(company.LC_CompanyCountryInfo);

			company.LC_CompanyCountry = "AU";
			AssertNoErrors(company.LC_CompanyCountryInfo);
		}

		public void TestLC_CompanyCountry_DifferentToParentOrgCountry()
		{
			EDIOrgHeader header = Factory.New<EDIOrgHeader>();
			header.OH_Code = "ABCXYZ";
			header.OH_RL_NKClosestPort = "AUSYD";
			AssertEquals("Precondition", "AU", header.UNLOCO.RL_RN_NKCountryCode);
			header.CreateAndLoadLicenceForOrg();

			header.LicCompany.LC_CompanyCountry = "US";
			AssertHasWarning(header.LicCompany.LC_CompanyCountryInfo, "The selected country (US) is different to the country (AU) this organization locates.");

			header.LicCompany.LC_CompanyCountry = "AU";
			AssertNoNotifications("Company Country is valid", header.LicCompany.LC_CompanyCountryInfo);
		}

		public void TestLC_CompanyCode()
		{
			EDIOrgHeader header = Factory.New<EDIOrgHeader>();
			header.OH_Code = "ABCXYZ";
			header.MainAddress.OA_Address1 = "Address 1";
			header.CreateAndLoadLicenceForOrg();
			header.LicenceEnterpriseCode = "ABC";
			header.LicCompany.LC_CompanyCode = "XYZ";

			Factory.Save();
			Assert("Precondition: Header is in database (simulate an already existing company code for validation)", header.IsInDatabase);

			EDIOrgHeader testHeader = Factory.New<EDIOrgHeader>();
			testHeader.OH_Code = "AAAAAA";
			testHeader.MainAddress.OA_Address1 = "Address 1";
			testHeader.CreateAndLoadLicenceForOrg();
			testHeader.LicenceEnterpriseCode = "ABC";

			AssertNoErrors("Company Code is valid", testHeader.LicCompany.LC_CompanyCodeInfo);
			testHeader.LicCompany.LC_CompanyCode = "X";
			AssertHasErrors(testHeader.LicCompany.LC_CompanyCodeInfo);

			testHeader.LicCompany.LC_CompanyCode = "";
			AssertHasErrors(testHeader.LicCompany.LC_CompanyCodeInfo);

			testHeader.LicCompany.LC_CompanyCode = "ABC";
			AssertNoErrors("Company Code is valid", testHeader.LicCompany.LC_CompanyCodeInfo);

			testHeader.LicCompany.LC_CompanyCode = "XYZ";
			AssertHasError(testHeader.LicCompany.LC_CompanyCodeInfo, "The specified code is already in use");

			testHeader.LicCompany.LC_CompanyCode = "_AL";
			AssertHasErrors("must be letters and digits", testHeader.LicCompany.LC_CompanyCodeInfo);

			Factory.Save();
			testHeader.LicCompany.Validation.ValidateLC_CompanyCode();
			AssertNoErrors("invalid code is not an error once in database", testHeader.LicCompany.LC_CompanyCodeInfo);
			AssertHasWarnings("Company Code is not valid", testHeader.LicCompany.LC_CompanyCodeInfo);

			testHeader.LicCompany.LC_CompanyCode = "_ZZ";
			AssertHasErrors("must be letters and digits", testHeader.LicCompany.LC_CompanyCodeInfo);
		}

		public void TestLC_RX_NKCurrency()
		{
			var aud = RefCurrency.LoadFromCurrencyCode(Factory, "AUD");
			aud.RX_IsActive = false;

			var company = Factory.New<LicenceCompany>();
			company.LC_RX_NKCurrency = "AUD";
			company.Validation.ValidateLC_RX_NKCurrency();
			AssertNoErrors(company.LC_RX_NKCurrencyInfo);

			company.LC_RX_NKCurrency = "AAA";
			company.Validation.ValidateLC_RX_NKCurrency();
			AssertHasErrors(company.LC_RX_NKCurrencyInfo);
		}
	}
}