using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.Accounting.TaxFramework.Business.Testing
{
	public class OrganisationTaxRateImportDataProviderTest : TestCaseWithFactory
	{
		public void TestDictionaryContainExpectedValues()
		{
			PrepareSimpleTestData();
			var dictionary = GetRegNumberToImport();

			var expectedCUIT = "CUIT";
			var expectedOrgName = "MY ARORG 1";
			var expectedOrgCode = "ARCOD";
			var expectedOrgTaxConfigPK = OrgTaxConfiguration.PK;

			AssertEquals(1, dictionary.Count);
			Assert(dictionary.ContainsKey(expectedCUIT));

			AssertEquals(expectedOrgCode, dictionary[expectedCUIT][0].OrgCode);
			AssertEquals(expectedOrgName, dictionary[expectedCUIT][0].OrgName);
			AssertEquals(expectedOrgTaxConfigPK, dictionary[expectedCUIT][0].AccOrgTaxConfigurationPK);
		}

		public void TestDictionaryContainExpectedValuesFromMultipleOrgs()
		{
			PrepareMultipleTestData();
			var dictionary = GetRegNumberToImport();

			var expectedCUIT = "CUIT";
			var expectedValues = new[]
			{
				new { expectedOrgName = "MY ARORG 1", expectedOrgCode = "ARCOD", expectedOrgTaxConfigPK = OrgTaxConfiguration.PK },
				new { expectedOrgName = "MY ARORG 2", expectedOrgCode = "ARCO2", expectedOrgTaxConfigPK = OrgTaxConfiguration2.PK }
			};

			Assert(dictionary.ContainsKey(expectedCUIT));
			AssertEquals(1, dictionary.Count);
			AssertEquals("There should be two organisations with the same CUIT.", 2, dictionary[expectedCUIT].Count);

			foreach (var currentOrg in expectedValues)
			{
				var (orgCode, orgName, accOrgTaxConfigurationPK) = dictionary[expectedCUIT].FirstOrDefault(x => x.OrgCode == currentOrg.expectedOrgCode);

				AssertEquals(currentOrg.expectedOrgName, orgName);
				AssertEquals(currentOrg.expectedOrgTaxConfigPK, accOrgTaxConfigurationPK);
			}
		}

		public void TestDisabledOrg()
		{
			PrepareSimpleTestData();
			var dictionary = GetRegNumberToImport();
			AssertEquals("Precondition: Dictionary must be have only one key element.", 1, dictionary.Count);

			OrgHeader.OH_IsActive = false;
			Factory.Save();
			dictionary = GetRegNumberToImport();
			AssertEquals(0, dictionary.Count);
		}

		public void TestDisabledOrgTaxConfiguration()
		{
			PrepareSimpleTestData();
			var dictionary = GetRegNumberToImport();
			AssertEquals("Precondition: Dictionary must be have only one key element.", 1, dictionary.Count);

			OrgTaxConfiguration.OTC_IsActive = false;
			Factory.Save();
			dictionary = GetRegNumberToImport();
			AssertEquals(0, dictionary.Count);
		}

		public void TestCustomsRegNoIsNotArgentina()
		{
			PrepareSimpleTestData();
			var dictionary = GetRegNumberToImport();
			AssertEquals("Precondition: Dictionary must be have only one key element.", 1, dictionary.Count);

			OrgHeader.CustomsCodes[0].OK_RN_NKCodeCountry = Core.Constants.CountryCodes.AntiguaAndBarbuda;
			Factory.Save();
			dictionary = GetRegNumberToImport();
			AssertEquals(0, dictionary.Count);
		}

		public void TestCustomsRegNoIsNotCUIT()
		{
			PrepareSimpleTestData();
			var dictionary = GetRegNumberToImport();
			AssertEquals("Precondition: Dictionary must be have only one key element.", 1, dictionary.Count);

			OrgHeader.CustomsCodes[0].OK_CodeType = BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ;
			Factory.Save();
			dictionary = GetRegNumberToImport();
			AssertEquals(0, dictionary.Count);
		}

		public void TestManyTaxConfigurations()
		{
			var taxConfig = PrepareTestDataAndGetTaxConfigurationPK();
			var expectedCode = "CUIT";

			AssertEquals("Precondition: Number of AP OrgTaxConfigurations", 1, OrgTaxConfiguration.CompanyData.APOrgTaxConfigurations.Count);
			AssertEquals("Precondition: Number of AR OrgTaxConfigurations", 4, OrgTaxConfiguration.CompanyData.AROrgTaxConfigurations.Count);

			var dictionary = GetRegNumberToImport(taxConfig);

			AssertEquals("Dictionary should return only one element.", 1, dictionary.Count);
			AssertEquals(OrgTaxConfiguration.PK, dictionary[expectedCode][0].AccOrgTaxConfigurationPK);
		}

		public void TestManyOrganisations()
		{
			var expectedValues = new[]
			{
				new { idx = 0, expectedResultCUIT = "CUIT_ONE",   expectedOrgName = "MY ORGANISATION 1", expectedOrgCode = "ARCO1" },
				new { idx = 0, expectedResultCUIT = "CUIT_TWO",   expectedOrgName = "MY ORGANISATION 2", expectedOrgCode = "ARCO2" },
				new { idx = 1, expectedResultCUIT = "CUIT_TWO",   expectedOrgName = "MY ORGANISATION 3", expectedOrgCode = "ARCO3" },
				new { idx = 0, expectedResultCUIT = "CUIT_THREE", expectedOrgName = "MY ORGANISATION 4", expectedOrgCode = "ARCO4" },
			};

			var anyCompany = Factory.NewWithValidTestData<GlbCompany>();
			anyCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Argentina;

			var orgHeader1 = SetOrgHeader("ARCO1", "MY ORGANISATION 1", expectedValues[0].expectedResultCUIT);
			var orgHeader2 = SetOrgHeader("ARCO2", "MY ORGANISATION 2", expectedValues[1].expectedResultCUIT);
			var orgHeader3 = SetOrgHeader("ARCO3", "MY ORGANISATION 3", expectedValues[2].expectedResultCUIT);
			var orgHeader4 = SetOrgHeader("ARCO4", "MY ORGANISATION 4", expectedValues[3].expectedResultCUIT);

			var companyData1 = orgHeader1.GetCompanyDataForGlbCompany(anyCompany);
			var companyData2 = orgHeader2.GetCompanyDataForGlbCompany(anyCompany);
			var companyData3 = orgHeader3.GetCompanyDataForGlbCompany(anyCompany);
			var companyData4 = orgHeader4.GetCompanyDataForGlbCompany(anyCompany);

			var taxConfiguration = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(parent: anyCompany, active: true);

			Factory.Save();

			var orgTaxConfiguration1 = SetTaxConfiguration(companyData1, taxConfiguration);
			var orgTaxConfiguration2 = SetTaxConfiguration(companyData2, taxConfiguration);
			var orgTaxConfiguration3 = SetTaxConfiguration(companyData3, taxConfiguration);
			var orgTaxConfiguration4 = SetTaxConfiguration(companyData4, taxConfiguration);

			Factory.Save();

			var expectedResultOrgTaxConfigPKList = new[] { orgTaxConfiguration1.PK, orgTaxConfiguration2.PK, orgTaxConfiguration3.PK, orgTaxConfiguration4.PK };

			var dictionary = GetRegNumberToImport(taxConfiguration.PK);

			AssertEquals(3, dictionary.Count);

			AssertEquals("Only one Organization has CUIT_ONE as OrgCusCode.", 1, dictionary["CUIT_ONE"].Count);
			AssertEquals("Two organizations have CUIT_TWO as OrgCustCode.", 2, dictionary["CUIT_TWO"].Count);
			AssertEquals("Only one Organization has CUIT_THREE as OrgCusCode.", 1, dictionary["CUIT_THREE"].Count);

			for (int i = 0; i < expectedValues.Length; i++)
			{
				var org = expectedValues[i];
				var (orgCode, orgName, accOrgTaxConfigurationPK) = dictionary[org.expectedResultCUIT].FirstOrDefault(x => x.OrgCode == org.expectedOrgCode);

				Assert(dictionary.ContainsKey(org.expectedResultCUIT));
				AssertEquals(org.expectedOrgName, orgName);
				AssertEquals(expectedResultOrgTaxConfigPKList[i], accOrgTaxConfigurationPK);
			}

			OrgHeader SetOrgHeader(ZString code, ZString fullName, ZString expectedCode)
			{
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				orgHeader.OH_Code = code;
				orgHeader.OH_FullName = fullName;
				orgHeader.OH_IsActive = true;

				var customsCode = orgHeader.CustomsCodes.AddNew();
				customsCode.OK_CodeType = ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT;
				customsCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Argentina;
				customsCode.OK_CustomsRegNo = expectedCode;
				customsCode.OK_OH = orgHeader.PK;

				return orgHeader;
			}

			AccOrgTaxConfiguration SetTaxConfiguration(OrgCompanyData companyData, AccTaxConfiguration taxConfig)
			{
				var orgTaxConfiguration = Factory.New<AccOrgTaxConfiguration>();
				orgTaxConfiguration.Ledger = taxConfig.ETC_Ledger;
				orgTaxConfiguration.OTC_ETC = taxConfig.PK;
				orgTaxConfiguration.OTC_OB = companyData.PK;
				orgTaxConfiguration.OTC_IsActive = true;
				return orgTaxConfiguration;
			}
		}

		public void TestThrowFactory()
		{
			AssertExceptionThrown<ArgumentNullException>(() => OrganisationTaxRateImportDataProvider.RegNumbersToImport(null, ZGuid.Empty));
		}

		public void TestInvalidOrEmptyPK()
		{
			PrepareSimpleTestData();

			AssertInvalidOrEmptyPKs(TaxConfiguration.PK, 1);
			AssertInvalidOrEmptyPKs(ZGuid.Invalid);
			AssertInvalidOrEmptyPKs(ZGuid.Empty);

			void AssertInvalidOrEmptyPKs(ZGuid assertGuid, int loadCount = 0)
			{
				Factory.ResetDatabaseLoadCount();

				var dictionary = GetRegNumberToImport(assertGuid);
				AssertEquals("Precondition", loadCount, dictionary.Count);

				var tableHints = new Dictionary<string, int>();
				tableHints.Add(AccOrgTaxConfigurationSchema.Constants.TableName, 0);
				// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538 
				AssertDbHits(tableHints, Factory);
				AssertEquals("DatabaseLoadCount", loadCount, Factory.DatabaseLoadCount);
			}
		}

		ZGuid PrepareTestDataAndGetTaxConfigurationPK()
		{
			var company = TestObjectCreator.CreateNewCompany("CO1");
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Argentina;

			var taxAuthority1 = TaxFrameworkTestObjectCreator.CreateTaxAuthority("S01");
			var taxAuthority2 = TaxFrameworkTestObjectCreator.CreateTaxAuthority("S02");
			var taxSystem1 = TaxFrameworkTestObjectCreator.CreateTaxSystem("PER");
			var taxSystem2 = TaxFrameworkTestObjectCreator.CreateTaxSystem("RET");

			var taxConfig1 = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(company, taxAuthority1, taxSystem1, TaxConfigurationLedgers.AccountsReceivable.Code);
			var taxConfig2 = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(company, taxAuthority1, taxSystem2, TaxConfigurationLedgers.AccountsReceivable.Code);
			var taxConfig3 = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(company, taxAuthority2, taxSystem1, TaxConfigurationLedgers.AccountsReceivable.Code);
			var taxConfig4 = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(company, taxAuthority2, taxSystem2, TaxConfigurationLedgers.AccountsReceivable.Code);
			var taxConfig5 = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(company, taxAuthority1, taxSystem2, TaxConfigurationLedgers.AccountsPayable.Code);
			Factory.Save();

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "ARCOD";
			orgHeader.OH_FullName = "MY ARORG 1";
			orgHeader.OH_IsActive = true;

			var customsCode = orgHeader.CustomsCodes.AddNew();
			customsCode.OK_CodeType = ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT;
			customsCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Argentina;
			customsCode.OK_CustomsRegNo = "CUIT";
			customsCode.OK_OH = orgHeader.PK;

			var companyData = orgHeader.GetCompanyDataForGlbCompany(company);

			OrgTaxConfiguration = TaxFrameworkTestObjectCreator.CreateOrgTaxConfiguration(taxConfig1, companyData, true);
			TaxFrameworkTestObjectCreator.CreateOrgTaxConfiguration(taxConfig2, companyData, true);
			TaxFrameworkTestObjectCreator.CreateOrgTaxConfiguration(taxConfig3, companyData, true);
			TaxFrameworkTestObjectCreator.CreateOrgTaxConfiguration(taxConfig4, companyData, true);
			TaxFrameworkTestObjectCreator.CreateOrgTaxConfiguration(taxConfig5, companyData, true);
			Factory.Save();

			return taxConfig1.PK;
		}

		Dictionary<ZString, IReadOnlyList<(ZString OrgCode, ZString OrgName, ZGuid AccOrgTaxConfigurationPK)>> GetRegNumberToImport(ZGuid? taxConfigurationPK = null)
				=> OrganisationTaxRateImportDataProvider.RegNumbersToImport(Factory, taxConfigurationPK ?? TaxConfiguration.PK);

		void PrepareSimpleTestData()
		{
			Company = TestObjectCreator.CreateNewCompany("CO1");
			Company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Argentina;

			var taxAuthority1 = TaxFrameworkTestObjectCreator.CreateTaxAuthority("S01");
			var taxSystem1 = TaxFrameworkTestObjectCreator.CreateTaxSystem("PER");

			TaxConfiguration = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(Company, taxAuthority1, taxSystem1, TaxConfigurationLedgers.AccountsReceivable.Code);
			Factory.Save();

			OrgHeader = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader.OH_Code = "ARCOD";
			OrgHeader.OH_FullName = "MY ARORG 1";
			OrgHeader.OH_IsActive = true;

			var customsCode = OrgHeader.CustomsCodes.AddNew();
			customsCode.OK_CodeType = ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT;
			customsCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Argentina;
			customsCode.OK_CustomsRegNo = "CUIT";
			customsCode.OK_OH = OrgHeader.PK;

			CompanyData = OrgHeader.GetCompanyDataForGlbCompany(Company);

			OrgTaxConfiguration = TaxFrameworkTestObjectCreator.CreateOrgTaxConfiguration(TaxConfiguration, CompanyData, true);
			Factory.Save();
		}

		void PrepareMultipleTestData()
		{
			PrepareSimpleTestData();
			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader2.OH_Code = "ARCO2";
			orgHeader2.OH_FullName = "MY ARORG 2";
			orgHeader2.OH_IsActive = true;

			var customsCode = orgHeader2.CustomsCodes.AddNew();
			customsCode.OK_CodeType = ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT;
			customsCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Argentina;
			customsCode.OK_CustomsRegNo = "CUIT";
			customsCode.OK_OH = orgHeader2.PK;

			var companyData2 = orgHeader2.GetCompanyDataForGlbCompany(Company);

			OrgTaxConfiguration2 = TaxFrameworkTestObjectCreator.CreateOrgTaxConfiguration(TaxConfiguration, companyData2, true);
			Factory.Save();
		}

		GlbCompany Company;
		OrgHeader OrgHeader;
		OrgCompanyData CompanyData;

		AccOrgTaxConfiguration OrgTaxConfiguration;
		AccOrgTaxConfiguration OrgTaxConfiguration2;

		AccTaxConfiguration TaxConfiguration;

		IOrganisationTaxRateImportDataProvider OrganisationTaxRateImportDataProvider => organisationTaxRateImportDataProvider ?? (organisationTaxRateImportDataProvider = new OrganisationTaxRateImportDataProvider());
		IOrganisationTaxRateImportDataProvider organisationTaxRateImportDataProvider;

		TaxFrameworkTestObjectCreator TaxFrameworkTestObjectCreator => taxFrameworkTestObjectCreator ?? (taxFrameworkTestObjectCreator = new TaxFrameworkTestObjectCreator(Factory));
		TaxFrameworkTestObjectCreator taxFrameworkTestObjectCreator;

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
