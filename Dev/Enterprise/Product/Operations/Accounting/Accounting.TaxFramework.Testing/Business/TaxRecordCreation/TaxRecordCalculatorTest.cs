using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using static Enterprise.Accounting.Utility.Testing.TaxFrameworkTestObjectCreator;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.Accounting.TaxFramework.Business.Testing
{
	class TaxRecordCalculatorTest : TestCaseWithFactory
	{
		readonly ITaxFrameworkConfigurationHelper taxFrameworkConfigurationHelper = new TaxFrameworkConfigurationHelper();

		public void TestIsUsagedAsDependency()
		{
			AssertType<TaxRecordCalculator>(new TaxRecordCreator().TaxRecordCalculator_ExposedForTestOnly);
		}

		public void TestArgumentNullExceptionWhenTaxFrameworkConfigurationHelperIsNUll()
		{
			AssertExceptionThrown<ArgumentNullException>("Value cannot be null.\r\nParameter name: taxFrameworkConfigurationHelper", () => new TaxRecordCalculator(null));

			AssertNoExceptionThrown(() => new TaxRecordCalculator(taxFrameworkConfigurationHelper));
		}

		#region GetInvoiceTaxOverrideGroupPKs

		public void TestGetInvoiceTaxOverrideGroupPKs_TaxOverrideGroupInfos()
		{
			var company = TestObjectCreator.CreateNewCompany("CO1");
			var branch = TestObjectCreator.CreateBranch("BR1", company);

			var taxAuthority = TaxFrameworkTestObjectCreator.CreateTaxAuthority("BA");
			var taxSystem = TaxFrameworkTestObjectCreator.CreateTaxSystem("PIB");

			var taxConfig1 = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(company, taxAuthority, taxSystem, TaxConfigurationLedgers.AccountsReceivable.Code, true);
			var taxConfig2 = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(branch, taxAuthority, taxSystem, TaxConfigurationLedgers.AccountsReceivable.Code, true);
			Factory.Save();

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var companyData = orgHeader.GetCompanyDataForGlbCompany(company);
			TaxFrameworkTestObjectCreator.CreateOrgTaxConfiguration(taxConfig1, companyData, true);
			TaxFrameworkTestObjectCreator.CreateOrgTaxConfiguration(taxConfig2, companyData, true);

			var taxId1 = Factory.NewWithValidTestData<AccTaxRate>();
			var taxId2 = Factory.NewWithValidTestData<AccTaxRate>();
			var defaultVATClass1 = Factory.NewWithValidTestData<AccInvMsg>();
			var defaultVATClass2 = Factory.NewWithValidTestData<AccInvMsg>();
			var taxOverrideGroup = TaxFrameworkTestObjectCreator.CreateTaxOverrideGroup(company, null);
			TaxFrameworkTestObjectCreator.CreateTaxOverrideGroupTaxConfigurationPivot(taxOverrideGroup, taxConfig1, 12, 3, "123", "Desc123");
			var taxOverrideGroup2 = TaxFrameworkTestObjectCreator.CreateTaxOverrideGroup(company, null);
			TaxFrameworkTestObjectCreator.CreateTaxOverrideGroupTaxConfigurationPivot(taxOverrideGroup2, taxConfig1, 1, 4, "321", "Desc321", taxId1.PK, defaultVATClass1.PK);
			TaxFrameworkTestObjectCreator.CreateTaxOverrideGroupTaxConfigurationPivot(taxOverrideGroup2, taxConfig2, 2, 7, "121", "Desc121", taxId2.PK, defaultVATClass2.PK);

			Factory.Save();

			var result = TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetInvoiceTaxOverrideGroupPKs(Factory, TaxConfigurationLedgers.AccountsReceivable.Code, orgHeader, new[] { branch });

			AssertEquals("result.TaxOverrideGroupInfos.Count", 2, result.TaxOverrideGroupInfos.Count);

			var taxGroupInfos = result.TaxOverrideGroupInfos[taxOverrideGroup.PK];
			AssertEquals("result.TaxOverrideGroupInfos.TaxOverrideGroupInfo.Count", 1, taxGroupInfos.Length);
			AssertTaxOverrideGroupInfos(taxGroupInfos[0], "123", "Desc123", 12, 3, taxConfig1.PK, ZGuid.Empty, ZGuid.Empty);

			taxGroupInfos = result.TaxOverrideGroupInfos[taxOverrideGroup2.PK].OrderByDescending(x => x.TaxAuthorityServiceCode).ToArray();
			AssertEquals("result.TaxOverrideGroupInfos.TaxOverrideGroupInfo.Count", 2, taxGroupInfos.Length);
			AssertTaxOverrideGroupInfos(taxGroupInfos[0], "321", "Desc321", 1, 4, taxConfig1.PK, taxId1.PK, defaultVATClass1.PK);
			AssertTaxOverrideGroupInfos(taxGroupInfos[1], "121", "Desc121", 2, 7, taxConfig2.PK, taxId2.PK, defaultVATClass2.PK);

			void AssertTaxOverrideGroupInfos(TaxRecordCalculator.TaxOverrideGroupInfo taxOverrideGroupInfo,
						ZString taxAuthorityServiceCode,
						ZString taxAuthorityServiceCodeDescription,
						ZInt rateNumerator,
						ZInt rateDenominator,
						ZGuid taxConfigurationPK,
						ZGuid taxIDPK,
						ZGuid defaultVATClassPK)
			{
				CombineAssertions(() =>
				{
					AssertEquals("result.TaxOverrideGroupInfos.TaxAuthorityServiceCode", taxAuthorityServiceCode, taxOverrideGroupInfo.TaxAuthorityServiceCode);
					AssertEquals("result.TaxOverrideGroupInfos.TaxAuthorityServiceCode", taxAuthorityServiceCodeDescription, taxOverrideGroupInfo.TaxAuthorityServiceCodeDescription);
					AssertEquals("result.TaxOverrideGroupInfos.TaxAuthorityServiceCode", rateNumerator, taxOverrideGroupInfo.RateNumerator);
					AssertEquals("result.TaxOverrideGroupInfos.TaxAuthorityServiceCode", rateDenominator, taxOverrideGroupInfo.RateDenominator);
					AssertEquals("result.TaxOverrideGroupInfos.TaxAuthorityServiceCode", taxConfigurationPK, taxOverrideGroupInfo.TaxConfigurationPK);
					AssertEquals("result.TaxOverrideGroupInfos.TaxAuthorityServiceCode", taxIDPK, taxOverrideGroupInfo.TaxIDPK);
					AssertEquals("result.TaxOverrideGroupInfos.TaxAuthorityServiceCode", defaultVATClassPK, taxOverrideGroupInfo.DefaultVATClassPK);
				});
			}
		}

		public void TestGetInvoiceTaxOverrideGroupPKs_CreationTrigger()
		{
			var company = TestObjectCreator.CreateNewCompany("CO1");

			var taxAuthority = TaxFrameworkTestObjectCreator.CreateTaxAuthority("BA");
			var taxSystem = TaxFrameworkTestObjectCreator.CreateTaxSystem("PIB");

			var taxConfig = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(company, taxAuthority, taxSystem, TaxConfigurationLedgers.AccountsReceivable.Code, true);
			taxConfig.ETC_TaxRecordCreationTrigger = TaxRecordCreationTrigger.MatchDate.Code;
			Factory.Save();

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var companyData = orgHeader.GetCompanyDataForGlbCompany(company);
			var orgTaxConfigAR = TaxFrameworkTestObjectCreator.CreateOrgTaxConfiguration(taxConfig, companyData, true);

			var taxOverrideGroup = TaxFrameworkTestObjectCreator.CreateTaxOverrideGroup(company, taxConfig);

			var branch = TestObjectCreator.CreateBranch("BR1", company);
			var arrayOfBranchesForCompany = new GlbBranch[] { branch };
			Factory.Save();

			var result = TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetInvoiceTaxOverrideGroupPKs(Factory, TaxConfigurationLedgers.AccountsReceivable.Code, orgHeader, arrayOfBranchesForCompany);
			AssertEquals("result.TaxOverrideGroupPKs.Count", 0, result.TaxOverrideGroupPKs.Count);

			taxConfig.ETC_TaxRecordCreationTrigger = TaxRecordCreationTrigger.PostDate.Code;
			Factory.Save();
			result = TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetInvoiceTaxOverrideGroupPKs(Factory, TaxConfigurationLedgers.AccountsReceivable.Code, orgHeader, arrayOfBranchesForCompany);
			AssertEquals("result.TaxOverrideGroupPKs.Count", 1, result.TaxOverrideGroupPKs.Count);
			AssertEquals("result.TaxOverrideGroupPKs.Value", taxOverrideGroup.PK, result.TaxOverrideGroupPKs[company.PK][0]);
			AssertEquals("result.TaxOverrideGroupInfos.Count", 1, result.TaxOverrideGroupInfos.Count);
			AssertEquals("result.TaxOverrideGroupInfos.TaxOverrideGroupInfo.Count", 1, result.TaxOverrideGroupInfos[taxOverrideGroup.PK].Length);
			AssertEquals("result.TaxOverrideGroupInfos.TaxAuthorityServiceCode", "5.2020(809)", result.TaxOverrideGroupInfos[taxOverrideGroup.PK][0].TaxAuthorityServiceCode);
		}

		public void TestGetInvoiceTaxOverrideGroupPKs_UsingDiferentLedger_AndIsActiveOptions()
		{
			var company = TestObjectCreator.CreateNewCompany("CO1");

			var taxAuthority = TaxFrameworkTestObjectCreator.CreateTaxAuthority("BA");
			var taxSystem = TaxFrameworkTestObjectCreator.CreateTaxSystem("PIB");

			var taxConfig = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(company, taxAuthority, taxSystem, TaxConfigurationLedgers.AccountsReceivable.Code, true);
			Factory.Save();

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var companyData = orgHeader.GetCompanyDataForGlbCompany(company);
			var orgTaxConfigAR = TaxFrameworkTestObjectCreator.CreateOrgTaxConfiguration(taxConfig, companyData, true);

			var taxOverrideGroup = TaxFrameworkTestObjectCreator.CreateTaxOverrideGroup(company, taxConfig);

			var branch = TestObjectCreator.CreateBranch("BR1", company);
			var arrayOfBranchesForCompany = new GlbBranch[] { branch };
			Factory.Save();

			orgTaxConfigAR.OTC_IsActive = true;
			Factory.Save();
			var result = TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetInvoiceTaxOverrideGroupPKs(Factory, TaxConfigurationLedgers.AccountsReceivable.Code, orgHeader, arrayOfBranchesForCompany);
			AssertEquals("result.TaxOverrideGroupPKs.Count", 1, result.TaxOverrideGroupPKs.Count);
			AssertEquals("result.TaxOverrideGroupPKs.Value", taxOverrideGroup.PK, result.TaxOverrideGroupPKs[company.PK][0]);
			AssertEquals("result.TaxOverrideGroupInfos.Count", 1, result.TaxOverrideGroupInfos.Count);
			AssertEquals("result.TaxOverrideGroupInfos.TaxOverrideGroupInfo.Count", 1, result.TaxOverrideGroupInfos[taxOverrideGroup.PK].Length);
			AssertEquals("result.TaxOverrideGroupInfos.TaxAuthorityServiceCode", "5.2020(809)", result.TaxOverrideGroupInfos[taxOverrideGroup.PK][0].TaxAuthorityServiceCode);

			orgTaxConfigAR.OTC_IsActive = false;
			Factory.Save();
			result = TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetInvoiceTaxOverrideGroupPKs(Factory, TaxConfigurationLedgers.AccountsReceivable.Code, orgHeader, arrayOfBranchesForCompany);
			AssertEquals("result.TaxOverrideGroupPKs.Count", 0, result.TaxOverrideGroupPKs.Count);

			orgTaxConfigAR.OTC_IsActive = true;
			taxConfig.ETC_IsActive = false;
			Factory.Save();
			result = TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetInvoiceTaxOverrideGroupPKs(Factory, TaxConfigurationLedgers.AccountsReceivable.Code, orgHeader, arrayOfBranchesForCompany);
			AssertEquals("result.TaxOverrideGroupPKs.Count", 0, result.TaxOverrideGroupPKs.Count);

			taxConfig.ETC_IsActive = true;
			Factory.Save();
			result = TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetInvoiceTaxOverrideGroupPKs(Factory, TaxConfigurationLedgers.AccountsReceivable.Code, orgHeader, arrayOfBranchesForCompany);
			AssertEquals("result.TaxOverrideGroupPKs.Count", 1, result.TaxOverrideGroupPKs.Count);
			AssertEquals("result.TaxOverrideGroupPKs.Value", taxOverrideGroup.PK, result.TaxOverrideGroupPKs[company.PK][0]);
			AssertEquals("result.TaxOverrideGroupInfos.Count", 1, result.TaxOverrideGroupInfos.Count);
			AssertEquals("result.TaxOverrideGroupInfos.TaxOverrideGroupInfo.Count", 1, result.TaxOverrideGroupInfos[taxOverrideGroup.PK].Length);
			AssertEquals("result.TaxOverrideGroupInfos.TaxAuthorityServiceCode", "5.2020(809)", result.TaxOverrideGroupInfos[taxOverrideGroup.PK][0].TaxAuthorityServiceCode);

			result = TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetInvoiceTaxOverrideGroupPKs(Factory, TaxConfigurationLedgers.AccountsPayable.Code, orgHeader, arrayOfBranchesForCompany);
			AssertEquals("result.TaxOverrideGroupPKs.Count", 0, result.TaxOverrideGroupPKs.Count);
		}

		public void TestGetInvoiceTaxOverrideGroupPKs_OrgHeaderHasOrgCompanyData_WithOut_CompanyPK()
		{
			var company1 = TestObjectCreator.CreateNewCompany("CO1");
			var company2 = TestObjectCreator.CreateNewCompany("CO2");

			var taxAuthority = TaxFrameworkTestObjectCreator.CreateTaxAuthority("BA");
			var taxSystem = TaxFrameworkTestObjectCreator.CreateTaxSystem("PIB");

			var taxConfig = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(company1, taxAuthority, taxSystem, TaxConfigurationLedgers.AccountsReceivable.Code, true);
			Factory.Save();

			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			var companyData1 = orgHeader1.GetCompanyDataForGlbCompany(company1);
			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			var companyData2 = orgHeader2.GetCompanyDataForGlbCompany(company2);

			var orgTaxConfigAR1 = TaxFrameworkTestObjectCreator.CreateOrgTaxConfiguration(taxConfig, companyData1, true);

			var taxOverrideGroup = TaxFrameworkTestObjectCreator.CreateTaxOverrideGroup(company1, taxConfig);

			var branch = TestObjectCreator.CreateBranch("BR1", company1);
			var arrayOfBranchesForCompany = new GlbBranch[] { branch };
			Factory.Save();

			var result = TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetInvoiceTaxOverrideGroupPKs(Factory, TaxConfigurationLedgers.AccountsReceivable.Code, orgHeader2, arrayOfBranchesForCompany);
			AssertEquals("result.TaxOverrideGroupPKs.Count", 0, result.TaxOverrideGroupPKs.Count);

			result = TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetInvoiceTaxOverrideGroupPKs(Factory, TaxConfigurationLedgers.AccountsReceivable.Code, orgHeader1, arrayOfBranchesForCompany);
			AssertEquals("result.TaxOverrideGroupPKs.Count", 1, result.TaxOverrideGroupPKs.Count);
			AssertEquals("result.TaxOverrideGroupPKs.Value", taxOverrideGroup.PK, result.TaxOverrideGroupPKs[company1.PK][0]);
			AssertEquals("result.TaxOverrideGroupInfos.Count", 1, result.TaxOverrideGroupInfos.Count);
			AssertEquals("result.TaxOverrideGroupInfos.TaxOverrideGroupInfo.Count", 1, result.TaxOverrideGroupInfos[taxOverrideGroup.PK].Length);
			AssertEquals("result.TaxOverrideGroupInfos.TaxAuthorityServiceCode", "5.2020(809)", result.TaxOverrideGroupInfos[taxOverrideGroup.PK][0].TaxAuthorityServiceCode);
		}

		public void TestGetInvoiceTaxOverrideGroupPKs_AccOrgTaxConfigurations_HaveAccTaxConfiguration_ButNot_TaxOverrideGroupAssigned()
		{
			var company = TestObjectCreator.CreateNewCompany("CO1");

			var taxAuthority = TaxFrameworkTestObjectCreator.CreateTaxAuthority("BA");
			var taxSystem = TaxFrameworkTestObjectCreator.CreateTaxSystem("PIB");

			var taxConfig1 = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(company, taxAuthority, taxSystem, TaxConfigurationLedgers.AccountsPayable.Code, true);
			var taxConfig2 = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(company, taxAuthority, taxSystem, TaxConfigurationLedgers.AccountsReceivable.Code, true);
			Factory.Save();

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var companyData = orgHeader.GetCompanyDataForGlbCompany(company);
			var orgTaxConfigAP1 = TaxFrameworkTestObjectCreator.CreateOrgTaxConfiguration(taxConfig1, companyData, true);

			var taxOverrideGroup = TaxFrameworkTestObjectCreator.CreateTaxOverrideGroup(company, taxConfig1);

			var branch = TestObjectCreator.CreateBranch("BR1", company);
			var arrayOfBranchesForCompany = new GlbBranch[] { branch };
			Factory.Save();

			var result = TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetInvoiceTaxOverrideGroupPKs(Factory, TaxConfigurationLedgers.AccountsPayable.Code, orgHeader, arrayOfBranchesForCompany);
			AssertEquals("result.TaxOverrideGroupPKs.Count", 1, result.TaxOverrideGroupPKs.Count);
			AssertEquals("result.TaxOverrideGroupPKs.Value", taxOverrideGroup.PK, result.TaxOverrideGroupPKs[company.PK][0]);
			AssertEquals("result.TaxOverrideGroupInfos.Count", 1, result.TaxOverrideGroupInfos.Count);
			AssertEquals("result.TaxOverrideGroupInfos.TaxOverrideGroupInfo.Count", 1, result.TaxOverrideGroupInfos[taxOverrideGroup.PK].Length);
			AssertEquals("result.TaxOverrideGroupInfos.TaxAuthorityServiceCode", "5.2020(809)", result.TaxOverrideGroupInfos[taxOverrideGroup.PK][0].TaxAuthorityServiceCode);

			taxOverrideGroup.TaxOverrideGroupTaxConfigurationPivots[0].AXP_ETC_TaxConfiguration = taxConfig2.PK;
			Factory.Save();

			result = TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetInvoiceTaxOverrideGroupPKs(Factory, TaxConfigurationLedgers.AccountsPayable.Code, orgHeader, arrayOfBranchesForCompany);
			AssertEquals("result.TaxOverrideGroupPKs.Count", 0, result.TaxOverrideGroupPKs.Count);
		}

		public void TestGetInvoiceTaxOverrideGroupPKs_ListOfBranches_FromOtherCompany()
		{
			var company1 = TestObjectCreator.CreateNewCompany("CO1");
			var company2 = TestObjectCreator.CreateNewCompany("CO2");

			var taxAuthority = TaxFrameworkTestObjectCreator.CreateTaxAuthority("BA");
			var taxSystem = TaxFrameworkTestObjectCreator.CreateTaxSystem("PIB");

			var taxConfig = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(company1, taxAuthority, taxSystem, TaxConfigurationLedgers.AccountsReceivable.Code, true);
			Factory.Save();

			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			var companyData1 = orgHeader1.GetCompanyDataForGlbCompany(company1);
			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			var companyData2 = orgHeader2.GetCompanyDataForGlbCompany(company2);

			var orgTaxConfigAR = TaxFrameworkTestObjectCreator.CreateOrgTaxConfiguration(taxConfig, companyData1, true);

			var taxOverrideGroup = TaxFrameworkTestObjectCreator.CreateTaxOverrideGroup(company1, taxConfig);

			var branch2 = TestObjectCreator.CreateBranch("BR1", company1);
			var arrayOfBranchesForCompany = new GlbBranch[] { branch2 };
			Factory.Save();

			var result = TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetInvoiceTaxOverrideGroupPKs(Factory, TaxConfigurationLedgers.AccountsReceivable.Code, orgHeader1, arrayOfBranchesForCompany);
			AssertEquals("result.TaxOverrideGroupPKs.Count", 1, result.TaxOverrideGroupPKs.Count);
			AssertEquals("result.TaxOverrideGroupPKs.Value", taxOverrideGroup.PK, result.TaxOverrideGroupPKs[company1.PK][0]);
			AssertEquals("result.TaxOverrideGroupInfos.Count", 1, result.TaxOverrideGroupInfos.Count);
			AssertEquals("result.TaxOverrideGroupInfos.TaxOverrideGroupInfo.Count", 1, result.TaxOverrideGroupInfos[taxOverrideGroup.PK].Length);
			AssertEquals("result.TaxOverrideGroupInfos.TaxAuthorityServiceCode", "5.2020(809)", result.TaxOverrideGroupInfos[taxOverrideGroup.PK][0].TaxAuthorityServiceCode);

			branch2.GB_GC = company2.PK;
			Factory.Save();

			result = TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetInvoiceTaxOverrideGroupPKs(Factory, TaxConfigurationLedgers.AccountsReceivable.Code, orgHeader1, arrayOfBranchesForCompany);
			AssertEquals("result.TaxOverrideGroupPKs.Count", 0, result.TaxOverrideGroupPKs.Count);
		}

		public void TestGetInvoiceTaxOverrideGroupPKs_MultiplesTaxOverrideGroup_Same_TaxConfiguration()
		{
			var company = TestObjectCreator.CreateNewCompany("CO1");

			var taxAuthority = TaxFrameworkTestObjectCreator.CreateTaxAuthority("BA");
			var taxSystem = TaxFrameworkTestObjectCreator.CreateTaxSystem("PIB");

			var taxConfig1 = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(company, taxAuthority, taxSystem, TaxConfigurationLedgers.AccountsReceivable.Code, true);

			Factory.Save();

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var companyData = orgHeader.GetCompanyDataForGlbCompany(company);

			TaxFrameworkTestObjectCreator.CreateOrgTaxConfiguration(taxConfig1, companyData, true);
			var taxOverrideGroup1 = TaxFrameworkTestObjectCreator.CreateTaxOverrideGroup(company, taxConfig1);
			var taxOverrideGroup2 = TaxFrameworkTestObjectCreator.CreateTaxOverrideGroup(company, taxConfig1);

			var branch = TestObjectCreator.CreateBranch("BR1", company);
			var arrayOfBranchesForCompany = new GlbBranch[] { branch };
			Factory.Save();

			var result = TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetInvoiceTaxOverrideGroupPKs(Factory, TaxConfigurationLedgers.AccountsReceivable.Code, orgHeader, arrayOfBranchesForCompany);
			AssertEquals("result.TaxOverrideGroupPKs.Count", 1, result.TaxOverrideGroupPKs.Count);
			var values = result.TaxOverrideGroupPKs[company.PK];
			AssertEquals("result.Value Count", 2, values.Length);
			AssertContainsExactElementsInAnyOrder("Values", new[] { taxOverrideGroup1.PK, taxOverrideGroup2.PK }, values);
			AssertEquals("result.TaxOverrideGroupInfos.Count", 2, result.TaxOverrideGroupInfos.Count);
			AssertEquals("result.TaxOverrideGroupInfos.TaxOverrideGroupInfo.Count", 1, result.TaxOverrideGroupInfos[taxOverrideGroup1.PK].Length);
			AssertEquals("result.TaxOverrideGroupInfos.TaxAuthorityServiceCode", "5.2020(809)", result.TaxOverrideGroupInfos[taxOverrideGroup1.PK][0].TaxAuthorityServiceCode);
			AssertEquals("result.TaxOverrideGroupInfos.TaxOverrideGroupInfo.Count", 1, result.TaxOverrideGroupInfos[taxOverrideGroup2.PK].Length);
			AssertEquals("result.TaxOverrideGroupInfos.TaxAuthorityServiceCode", "5.2020(809)", result.TaxOverrideGroupInfos[taxOverrideGroup2.PK][0].TaxAuthorityServiceCode);
		}

		public void TestGetInvoiceTaxOverrideGroupPKs_Multiples_TaxOverrideGroup_And_TaxConfiguration()
		{
			var company = TestObjectCreator.CreateNewCompany("CO1");

			var taxAuthority = TaxFrameworkTestObjectCreator.CreateTaxAuthority("BA");
			var taxSystem1 = TaxFrameworkTestObjectCreator.CreateTaxSystem("PIB");
			var taxSystem2 = TaxFrameworkTestObjectCreator.CreateTaxSystem("RIB");

			var taxConfig1 = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(company, taxAuthority, taxSystem1, TaxConfigurationLedgers.AccountsReceivable.Code, true);
			var taxConfig2 = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(company, taxAuthority, taxSystem2, TaxConfigurationLedgers.AccountsReceivable.Code, true);
			Factory.Save();

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var companyData = orgHeader.GetCompanyDataForGlbCompany(company);

			TaxFrameworkTestObjectCreator.CreateOrgTaxConfiguration(taxConfig1, companyData, true);
			TaxFrameworkTestObjectCreator.CreateOrgTaxConfiguration(taxConfig2, companyData, true);
			var taxOverrideGroup1 = TaxFrameworkTestObjectCreator.CreateTaxOverrideGroup(company, taxConfig1);
			var taxOverrideGroup2 = TaxFrameworkTestObjectCreator.CreateTaxOverrideGroup(company, taxConfig2);

			var branch = TestObjectCreator.CreateBranch("BR1", company);
			var arrayOfBranchesForCompany = new GlbBranch[] { branch };
			Factory.Save();

			var result = TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetInvoiceTaxOverrideGroupPKs(Factory, TaxConfigurationLedgers.AccountsReceivable.Code, orgHeader, arrayOfBranchesForCompany);
			AssertEquals("result.TaxOverrideGroupPKs.Count", 1, result.TaxOverrideGroupPKs.Count);
			var values = result.TaxOverrideGroupPKs[company.PK];
			AssertEquals("result.Value Count", 2, values.Length);
			AssertContainsExactElementsInAnyOrder("Values", new[] { taxOverrideGroup1.PK, taxOverrideGroup2.PK }, values);
			AssertEquals("result.TaxOverrideGroupInfos.Count", 2, result.TaxOverrideGroupInfos.Count);
			AssertEquals("result.TaxOverrideGroupInfos.TaxOverrideGroupInfo.Count", 1, result.TaxOverrideGroupInfos[taxOverrideGroup1.PK].Length);
			AssertEquals("result.TaxOverrideGroupInfos.TaxAuthorityServiceCode", "5.2020(809)", result.TaxOverrideGroupInfos[taxOverrideGroup1.PK][0].TaxAuthorityServiceCode);
			AssertEquals("result.TaxOverrideGroupInfos.TaxOverrideGroupInfo.Count", 1, result.TaxOverrideGroupInfos[taxOverrideGroup2.PK].Length);
			AssertEquals("result.TaxOverrideGroupInfos.TaxAuthorityServiceCode", "5.2020(809)", result.TaxOverrideGroupInfos[taxOverrideGroup2.PK][0].TaxAuthorityServiceCode);
		}

		public void TestGetInvoiceTaxOverrideGroupPKs_MultiplesAndDistinctsCompanyBranches()
		{
			var company1 = TestObjectCreator.CreateNewCompany("CO1");
			var company2 = TestObjectCreator.CreateNewCompany("CO2");

			var branch1 = TestObjectCreator.CreateBranch("BR1", company1);
			var branch2 = TestObjectCreator.CreateBranch("BR2", company1);
			var arrayOfBranchesForCompany1 = new GlbBranch[] { branch1, branch2 };

			var branch3 = TestObjectCreator.CreateBranch("BR3", company2);
			var branch4 = TestObjectCreator.CreateBranch("BR4", company2);
			var arrayOfBranchesForCompany2 = new GlbBranch[] { branch3, branch4 };

			var taxAuthority = TaxFrameworkTestObjectCreator.CreateTaxAuthority("BA");
			var taxSystem = TaxFrameworkTestObjectCreator.CreateTaxSystem("PIB");
			var taxSystem2 = TaxFrameworkTestObjectCreator.CreateTaxSystem("RIB");

			var taxConfig = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(company1, taxAuthority, taxSystem, TaxConfigurationLedgers.AccountsReceivable.Code, true);
			var taxConfig2 = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(branch2, taxAuthority, taxSystem2, TaxConfigurationLedgers.AccountsReceivable.Code, true);
			Factory.Save();

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var companyData = orgHeader.GetCompanyDataForGlbCompany(company1);

			TaxFrameworkTestObjectCreator.CreateOrgTaxConfiguration(taxConfig, companyData, true);
			TaxFrameworkTestObjectCreator.CreateOrgTaxConfiguration(taxConfig2, companyData, true);

			var taxOverrideGroup = TaxFrameworkTestObjectCreator.CreateTaxOverrideGroup(company1, taxConfig);
			var taxOverrideGroupBranch = TaxFrameworkTestObjectCreator.CreateTaxOverrideGroup(company1, taxConfig2);

			Factory.Save();

			var result = TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetInvoiceTaxOverrideGroupPKs(Factory, TaxConfigurationLedgers.AccountsReceivable.Code, orgHeader, arrayOfBranchesForCompany1);
			AssertEquals("result.TaxOverrideGroupPKs.Count", 2, result.TaxOverrideGroupPKs.Count);
			AssertEquals("result.TaxOverrideGroupPKs.Value", taxOverrideGroup.PK, result.TaxOverrideGroupPKs[company1.PK][0]);
			AssertEquals("result.TaxOverrideGroupPKs.Value", taxOverrideGroupBranch.PK, result.TaxOverrideGroupPKs[branch2.PK][0]);
			AssertEquals("result.TaxOverrideGroupInfos.Count", 2, result.TaxOverrideGroupInfos.Count);
			AssertEquals("result.TaxOverrideGroupInfos.TaxOverrideGroupInfo.Count", 1, result.TaxOverrideGroupInfos[taxOverrideGroup.PK].Length);
			AssertEquals("result.TaxOverrideGroupInfos.TaxAuthorityServiceCode", "5.2020(809)", result.TaxOverrideGroupInfos[taxOverrideGroup.PK][0].TaxAuthorityServiceCode);
			AssertEquals("result.TaxOverrideGroupInfos.TaxOverrideGroupInfo.Count", 1, result.TaxOverrideGroupInfos[taxOverrideGroupBranch.PK].Length);
			AssertEquals("result.TaxOverrideGroupInfos.TaxAuthorityServiceCode", "5.2020(809)", result.TaxOverrideGroupInfos[taxOverrideGroupBranch.PK][0].TaxAuthorityServiceCode);

			result = TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetInvoiceTaxOverrideGroupPKs(Factory, TaxConfigurationLedgers.AccountsReceivable.Code, orgHeader, arrayOfBranchesForCompany2);
			AssertEquals("result.TaxOverrideGroupPKs.Count", 0, result.TaxOverrideGroupPKs.Count);
		}

		public void TestGetInvoiceTaxOverrideGroupPKs_OnlyBranches()
		{
			var company = TestObjectCreator.CreateNewCompany("CO1");
			var branch1 = TestObjectCreator.CreateNewBranch(company, "BR1");
			var branch2 = TestObjectCreator.CreateNewBranch(company, "BR2");

			var taxAuthority = TaxFrameworkTestObjectCreator.CreateTaxAuthority("BA");
			var taxSystem1 = TaxFrameworkTestObjectCreator.CreateTaxSystem("PIB", TaxSystemRegistrationLevels.Branch.Code);
			var taxSystem2 = TaxFrameworkTestObjectCreator.CreateTaxSystem("RIB", TaxSystemRegistrationLevels.Branch.Code);
			var taxSystem3 = TaxFrameworkTestObjectCreator.CreateTaxSystem("OTH", TaxSystemRegistrationLevels.Branch.Code);

			var taxConfig1 = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(branch1, taxAuthority, taxSystem1, TaxConfigurationLedgers.AccountsReceivable.Code, true);
			var taxConfig2 = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(branch2, taxAuthority, taxSystem2, TaxConfigurationLedgers.AccountsReceivable.Code, true);
			var taxConfig3 = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(branch2, taxAuthority, taxSystem3, TaxConfigurationLedgers.AccountsReceivable.Code, true);
			Factory.Save();

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var companyData = orgHeader.GetCompanyDataForGlbCompany(company);

			TaxFrameworkTestObjectCreator.CreateOrgTaxConfiguration(taxConfig1, companyData, true);
			TaxFrameworkTestObjectCreator.CreateOrgTaxConfiguration(taxConfig2, companyData, true);
			TaxFrameworkTestObjectCreator.CreateOrgTaxConfiguration(taxConfig3, companyData, true);

			var taxOverrideGroup1 = TaxFrameworkTestObjectCreator.CreateTaxOverrideGroup(company, taxConfig1);
			var taxOverrideGroup2 = TaxFrameworkTestObjectCreator.CreateTaxOverrideGroup(company, taxConfig2);
			var taxOverrideGroup3 = TaxFrameworkTestObjectCreator.CreateTaxOverrideGroup(company, taxConfig3);
			var arrayOfBranchesForCompany = new GlbBranch[] { branch1, branch2 };
			Factory.Save();

			var result = TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetInvoiceTaxOverrideGroupPKs(Factory, TaxConfigurationLedgers.AccountsReceivable.Code, orgHeader, arrayOfBranchesForCompany);
			AssertEquals("result.TaxOverrideGroupPKs.Count", 2, result.TaxOverrideGroupPKs.Count);
			AssertEquals("result.TaxOverrideGroupPKs.TaxOverrideGroupPK.Count", 1, result.TaxOverrideGroupPKs[branch1.PK].Length);
			AssertEquals("result.TaxOverrideGroupPKs.Value", taxOverrideGroup1.PK, result.TaxOverrideGroupPKs[branch1.PK][0]);
			var branch2Values = result.TaxOverrideGroupPKs[branch2.PK];
			AssertEquals("result.TaxOverrideGroupPKs.TaxOverrideGroupPK.Count", 2, branch2Values.Length);
			AssertContainsExactElementsInAnyOrder("Branch2Values", new[] { taxOverrideGroup2.PK, taxOverrideGroup3.PK }, branch2Values);
			AssertEquals("result.TaxOverrideGroupInfos.Count", 3, result.TaxOverrideGroupInfos.Count);
			AssertEquals("result.TaxOverrideGroupInfos.TaxOverrideGroupInfo.Count", 1, result.TaxOverrideGroupInfos[taxOverrideGroup1.PK].Length);
			AssertEquals("result.TaxOverrideGroupInfos.TaxAuthorityServiceCode", "5.2020(809)", result.TaxOverrideGroupInfos[taxOverrideGroup1.PK][0].TaxAuthorityServiceCode);
			AssertEquals("result.TaxOverrideGroupInfos.TaxOverrideGroupInfo.Count", 1, result.TaxOverrideGroupInfos[taxOverrideGroup2.PK].Length);
			AssertEquals("result.TaxOverrideGroupInfos.TaxAuthorityServiceCode", "5.2020(809)", result.TaxOverrideGroupInfos[taxOverrideGroup2.PK][0].TaxAuthorityServiceCode);
			AssertEquals("result.TaxOverrideGroupInfos.TaxOverrideGroupInfo.Count", 1, result.TaxOverrideGroupInfos[taxOverrideGroup3.PK].Length);
			AssertEquals("result.TaxOverrideGroupInfos.TaxAuthorityServiceCode", "5.2020(809)", result.TaxOverrideGroupInfos[taxOverrideGroup3.PK][0].TaxAuthorityServiceCode);
		}

		#endregion

		#region GetLineTaxOverrideGroupPKs

		public void TestGetLineTaxOverrideGroupPKs_ForBranch()
		{
			var company = TestObjectCreator.CreateNewCompany("CO1");
			var branch1 = TestObjectCreator.CreateBranch("BR1", company);
			var branch2 = TestObjectCreator.CreateBranch("BR2", company);

			var chargeCode1 = TestObjectCreator.CreateChargeCode("CC1");
			var chargeCode2 = TestObjectCreator.CreateChargeCode("CC2");

			var taxAuthority = TaxFrameworkTestObjectCreator.CreateTaxAuthority("TA");
			var taxSystem = TaxFrameworkTestObjectCreator.CreateTaxSystem("TS");

			var taxConfiguration1 = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(branch1, taxAuthority, taxSystem, TaxConfigurationLedgers.AccountsReceivable.Code, true);
			var taxConfiguration2 = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(branch2, taxAuthority, taxSystem, TaxConfigurationLedgers.AccountsReceivable.Code, true);

			var taxFrameTaxOverrideGroup1 = CreateTaxOverrideAndPivot(company, taxConfiguration1, chargeCode1);
			var taxFrameTaxOverrideGroup2 = CreateTaxOverrideAndPivot(company, taxConfiguration1, chargeCode2);
			var taxFrameTaxOverrideGroup3 = CreateTaxOverrideAndPivot(company, taxConfiguration2, chargeCode1);

			var invoiceTaxOverrideGroupPKs = new Dictionary<ZGuid, ZGuid[]>();
			invoiceTaxOverrideGroupPKs.Add(branch1.PK, new ZGuid[] { taxFrameTaxOverrideGroup1.PK, taxFrameTaxOverrideGroup2.PK });
			invoiceTaxOverrideGroupPKs.Add(branch2.PK, new ZGuid[] { taxFrameTaxOverrideGroup3.PK });

			Factory.Save();

			CombineAssertions(() =>
			{
				var lineTaxOverrideGroupPKs = TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetLineTaxOverrideGroupPKs(Factory, chargeCode1.PK, branch1, invoiceTaxOverrideGroupPKs);
				AssertEquals(true, lineTaxOverrideGroupPKs.Contains(taxFrameTaxOverrideGroup1.PK));
				AssertEquals("The collection should have one tax override group", 1, lineTaxOverrideGroupPKs.Length);

				lineTaxOverrideGroupPKs = TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetLineTaxOverrideGroupPKs(Factory, chargeCode1.PK, branch2, invoiceTaxOverrideGroupPKs);
				AssertEquals(true, lineTaxOverrideGroupPKs.Contains(taxFrameTaxOverrideGroup3.PK));
				AssertEquals("The collection should have one tax override group", 1, lineTaxOverrideGroupPKs.Length);

				lineTaxOverrideGroupPKs = TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetLineTaxOverrideGroupPKs(Factory, chargeCode2.PK, branch2, invoiceTaxOverrideGroupPKs);
				AssertEquals("The collection result should not have tax override group", false, lineTaxOverrideGroupPKs.Any());

				lineTaxOverrideGroupPKs = TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetLineTaxOverrideGroupPKs(Factory, Factory.New<AccChargeCode>().PK, branch1, invoiceTaxOverrideGroupPKs);
				AssertEquals("The collection result should not have tax override group", false, lineTaxOverrideGroupPKs.Any());
			});
		}

		public void TestGetLineTaxOverrideGroupPKs_ForCompany()
		{
			var company = TestObjectCreator.CreateNewCompany("CO1");
			var company2 = TestObjectCreator.CreateNewCompany("CO2");
			var branch1 = TestObjectCreator.CreateBranch("BR1", company);
			var branch2 = TestObjectCreator.CreateBranch("BR2", company);
			var branch3 = TestObjectCreator.CreateBranch("BR3", company2);

			var chargeCode1 = TestObjectCreator.CreateChargeCode("CC1");
			var chargeCode2 = TestObjectCreator.CreateChargeCode("CC2");

			var taxAuthority = TaxFrameworkTestObjectCreator.CreateTaxAuthority("TA");
			var taxSystem = TaxFrameworkTestObjectCreator.CreateTaxSystem("TS");

			var taxConfigurationCompany = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(company, taxAuthority, taxSystem, TaxConfigurationLedgers.AccountsReceivable.Code, true);

			var taxFrameTaxOverrideGroup1 = CreateTaxOverrideAndPivot(company, taxConfigurationCompany, chargeCode1);
			var taxFrameTaxOverrideGroup2 = CreateTaxOverrideAndPivot(company, taxConfigurationCompany, chargeCode2);

			var invoiceTaxOverrideGroupPKs = new Dictionary<ZGuid, ZGuid[]>();
			invoiceTaxOverrideGroupPKs.Add(company.PK, new ZGuid[] { taxFrameTaxOverrideGroup1.PK, taxFrameTaxOverrideGroup2.PK });

			Factory.Save();

			CombineAssertions(() =>
			{
				var lineTaxOverrideGroupPKs = TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetLineTaxOverrideGroupPKs(Factory, chargeCode1.PK, branch1, invoiceTaxOverrideGroupPKs);
				AssertEquals(true, lineTaxOverrideGroupPKs.Contains(taxFrameTaxOverrideGroup1.PK));
				AssertEquals("The collection result should have one tax override group", 1, lineTaxOverrideGroupPKs.Length);

				lineTaxOverrideGroupPKs = TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetLineTaxOverrideGroupPKs(Factory, chargeCode2.PK, branch1, invoiceTaxOverrideGroupPKs);
				AssertEquals(true, lineTaxOverrideGroupPKs.Contains(taxFrameTaxOverrideGroup2.PK));
				AssertEquals("The collection result should have one tax override group", 1, lineTaxOverrideGroupPKs.Length);

				lineTaxOverrideGroupPKs = TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetLineTaxOverrideGroupPKs(Factory, chargeCode2.PK, branch2, invoiceTaxOverrideGroupPKs);
				AssertEquals(true, lineTaxOverrideGroupPKs.Contains(taxFrameTaxOverrideGroup2.PK));
				AssertEquals("The collection result should have one tax override group", 1, lineTaxOverrideGroupPKs.Length);

				lineTaxOverrideGroupPKs = TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetLineTaxOverrideGroupPKs(Factory, chargeCode2.PK, branch3, invoiceTaxOverrideGroupPKs);
				AssertEquals("The collection result should not have tax override group", false, lineTaxOverrideGroupPKs.Any());

				lineTaxOverrideGroupPKs = TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetLineTaxOverrideGroupPKs(Factory, Factory.New<AccChargeCode>().PK, branch1, invoiceTaxOverrideGroupPKs);
				AssertEquals("The collection result should not have tax override group", false, lineTaxOverrideGroupPKs.Any());
			});
		}

		public void TestGetLineTaxOverrideGroupPKs_ForMultipleRecords()
		{
			var company = TestObjectCreator.CreateNewCompany("CO1");
			var branch = TestObjectCreator.CreateBranch("BR1", company);
			var chargeCode1 = TestObjectCreator.CreateChargeCode("CC1");

			var taxAuthority = TaxFrameworkTestObjectCreator.CreateTaxAuthority("TA");
			var taxSystem = TaxFrameworkTestObjectCreator.CreateTaxSystem("TS");

			var taxConfigurationCompany = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(company, taxAuthority, taxSystem, TaxConfigurationLedgers.AccountsReceivable.Code, true);
			var taxConfigurationBranch = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(branch, taxAuthority, taxSystem, TaxConfigurationLedgers.AccountsReceivable.Code, true);

			var taxFrameTaxOverrideGroup1 = CreateTaxOverrideAndPivot(company, taxConfigurationCompany, chargeCode1);
			var taxFrameTaxOverrideGroup2 = CreateTaxOverrideAndPivot(company, taxConfigurationBranch, chargeCode1);

			var invoiceTaxOverrideGroupPKs = new Dictionary<ZGuid, ZGuid[]>();
			invoiceTaxOverrideGroupPKs.Add(company.PK, new ZGuid[] { taxFrameTaxOverrideGroup1.PK, taxFrameTaxOverrideGroup2.PK });

			Factory.Save();

			var lineTaxOverrideGroupPKs = TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetLineTaxOverrideGroupPKs(Factory, chargeCode1.PK, branch, invoiceTaxOverrideGroupPKs);
			AssertContainsExactElementsInAnyOrder(new[] { taxFrameTaxOverrideGroup1.PK, taxFrameTaxOverrideGroup2.PK }, lineTaxOverrideGroupPKs);
		}

		#endregion

		public void TestGetLineTaxRule()
		{
			var taxGroup1 = Factory.NewWithValidTestData<AccTaxOverrideGroup>();
			var taxRule11 = TaxFrameworkTestObjectCreator.CreateTaxOverrideRule(taxGroup1, TestObjectCreator.GST1.PK, direction: OrgConstants.ServiceDirection.Code.Domestic);
			var taxRule12 = TaxFrameworkTestObjectCreator.CreateTaxOverrideRule(taxGroup1, TestObjectCreator.GSTFREE1.PK);

			var taxGroup2 = Factory.NewWithValidTestData<AccTaxOverrideGroup>();
			var taxRule21 = TaxFrameworkTestObjectCreator.CreateTaxOverrideRule(taxGroup2, TestObjectCreator.GST1.PK, direction: OrgConstants.ServiceDirection.Code.Domestic);
			var taxRule22 = TaxFrameworkTestObjectCreator.CreateTaxOverrideRule(taxGroup2, TestObjectCreator.GSTFREE1.PK);
			Factory.Save();

			var lineMock = new Mock<ITaxableTransactionLine>();
			lineMock.SetupGet(f => f.Factory).Returns(Factory);
			AssertNull(TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetLineTaxRule(ZGuid.NewZGuid(), lineMock.Object));

			var parameters = new AccChargeTaxOverrideMatcher.TaxCalculationParameters { JobType = JobInvoicingConsumerTypes.Shipment.Code, Direction = Directions.Domestic };
			lineMock.Setup(r => r.GetTaxCalculationParameters()).Returns(parameters);
			AssertEquals(taxRule11.PK, TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetLineTaxRule(taxGroup1.PK, lineMock.Object).PK);
			AssertEquals(taxRule21.PK, TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetLineTaxRule(taxGroup2.PK, lineMock.Object).PK);

			lineMock.Invocations.Clear();
			parameters.Direction = Directions.Import;
			lineMock.Setup(r => r.GetTaxCalculationParameters()).Returns(parameters);
			AssertEquals(taxRule12.PK, TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetLineTaxRule(taxGroup1.PK, lineMock.Object).PK);
			AssertEquals(taxRule22.PK, TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetLineTaxRule(taxGroup2.PK, lineMock.Object).PK);
		}

		public void TestGetTaxOverrideGroup()
		{
			var company = TestObjectCreator.CreateNewCompany("CO1");
			var taxAuthority = TaxFrameworkTestObjectCreator.CreateTaxAuthority("BA");
			var taxSystem = TaxFrameworkTestObjectCreator.CreateTaxSystem("PIB");
			var taxConfig = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(company, taxAuthority, taxSystem, TaxConfigurationLedgers.AccountsReceivable.Code, true);
			var taxOverrideGroup = TaxFrameworkTestObjectCreator.CreateTaxOverrideGroup(company, taxConfig);
			var taxRule = TaxFrameworkTestObjectCreator.CreateTaxOverrideRule(taxOverrideGroup, TestObjectCreator.GST1.PK, direction: OrgConstants.ServiceDirection.Code.Domestic);
			AssertEquals(taxOverrideGroup.PK, TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetTaxOverrideGroup(taxRule).PK);
			taxRule.AO_ParentID = ZGuid.NewZGuid();
			AssertNull(TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetTaxOverrideGroup(taxRule));
		}

		#region GetTaxRate
		public void TestOrganisationOnlyRateSource_SameConfigWithDifferentLedgerAndRate()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var taxConfigurationAP = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(TaxConfigurationLedgers.AccountsPayable.Code);
			var taxConfigurationAR = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(TaxConfigurationLedgers.AccountsReceivable.Code);
			Factory.Save();

			var accOrgTaxConfigurationAP = TaxFrameworkTestObjectCreator.CreateOrgTaxConfiguration(taxConfigurationAP, orgHeader.CompanyData, true);
			var accOrgTaxConfigurationAR = TaxFrameworkTestObjectCreator.CreateOrgTaxConfiguration(taxConfigurationAR, orgHeader.CompanyData, true);
			var taxRate1 = ((ZInt)1, (ZInt)1);
			TaxFrameworkTestObjectCreator.AddOrgTaxRateItem(accOrgTaxConfigurationAP, new ZDate(2019, 10, 5), new ZDate(2019, 10, 30), RateSourceMethods.Monthly.Code, taxRate1);
			TaxFrameworkTestObjectCreator.AddOrgTaxRateItem(accOrgTaxConfigurationAR, new ZDate(2019, 10, 5), new ZDate(2019, 10, 30), RateSourceMethods.Monthly.Code, taxRate1);

			var taxID = TaxFrameworkTestObjectCreator.CreateTaxRate_RateSource(TaxRateSources.OrganisationOnly.Code);

			var date = new ZDate(2019, 10, 11);

			AssertEquals($"a Tax Rate in Organisation must be found by Ledger {TaxConfigurationLedgers.AccountsPayable.Code} and {date}", taxRate1, TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetTaxRate(taxID, 0, 0, taxConfigurationAP, orgHeader, date));
			AssertEquals($"a Tax Rate in Organisation must be found by Ledger {TaxConfigurationLedgers.AccountsReceivable.Code} and {date}", taxRate1, TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetTaxRate(taxID, 0, 0, taxConfigurationAR, orgHeader, date));

			accOrgTaxConfigurationAP.TaxRates.DeleteAll();
			AssertNull($"a Tax Rate in Organisation must not be found by Ledger {TaxConfigurationLedgers.AccountsPayable.Code} and {date}", TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetTaxRate(taxID, 0, 0, taxConfigurationAP, orgHeader, date));
			AssertEquals($"a Tax Rate in Organisation must be found by Ledger {TaxConfigurationLedgers.AccountsReceivable.Code} and {date}", taxRate1, TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetTaxRate(taxID, 0, 0, taxConfigurationAR, orgHeader, date));

			accOrgTaxConfigurationAR.TaxRates.DeleteAll();
			AssertNull($"a Tax Rate in Organisation must not be found by Ledger {TaxConfigurationLedgers.AccountsPayable.Code} and {date}", TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetTaxRate(taxID, 0, 0, taxConfigurationAP, orgHeader, date));
			AssertNull($"a Tax Rate in Organisation must not be found by Ledger {TaxConfigurationLedgers.AccountsReceivable.Code} and {date}", TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetTaxRate(taxID, 0, 0, taxConfigurationAR, orgHeader, date));
		}

		public void TestOrganisationOnlyRateWhenRateInDateRangeDifferentLedger()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var taxConfigurationAP = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(TaxConfigurationLedgers.AccountsPayable.Code);
			var taxConfigurationAR = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(TaxConfigurationLedgers.AccountsReceivable.Code);
			Factory.Save();

			var accOrgTaxConfigurationAP = TaxFrameworkTestObjectCreator.CreateOrgTaxConfiguration(taxConfigurationAP, orgHeader.CompanyData, true);
			var accOrgTaxConfigurationAR = TaxFrameworkTestObjectCreator.CreateOrgTaxConfiguration(taxConfigurationAR, orgHeader.CompanyData, true);

			var taxRate1 = ((ZInt)1, (ZInt)1);
			TaxFrameworkTestObjectCreator.AddOrgTaxRateItem(accOrgTaxConfigurationAP, new ZDate(2019, 10, 5), new ZDate(2019, 10, 10), RateSourceMethods.ManualOverride.Code, taxRate1);
			TaxFrameworkTestObjectCreator.AddOrgTaxRateItem(accOrgTaxConfigurationAR, new ZDate(2019, 11, 15), new ZDate(2019, 11, 20), RateSourceMethods.ManualOverride.Code, taxRate1);

			var date1 = new ZDate(2019, 10, 10);
			var date2 = new ZDate(2019, 11, 20);

			var taxID = TaxFrameworkTestObjectCreator.CreateTaxRate_RateSource(TaxRateSources.OrganisationOnly.Code);
			AssertEquals($"a Tax Rate in Organisation must be found by Ledger {TaxConfigurationLedgers.AccountsPayable.Code} and {date1}", taxRate1, TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetTaxRate(taxID, 0, 0, taxConfigurationAP, orgHeader, date1));
			AssertNull($"a Tax Rate in Organisation must not be found by Ledger {TaxConfigurationLedgers.AccountsReceivable.Code} and {date1}", TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetTaxRate(taxID, 0, 0, taxConfigurationAR, orgHeader, date1));

			AssertNull($"a Tax Rate in Organisation must not be found by Ledger {TaxConfigurationLedgers.AccountsPayable.Code} and {date2}", TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetTaxRate(taxID, 0, 0, taxConfigurationAP, orgHeader, date2));
			AssertEquals($"a Tax Rate in Organisation must be found by Ledger {TaxConfigurationLedgers.AccountsReceivable.Code} and {date2}", taxRate1, TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetTaxRate(taxID, 0, 0, taxConfigurationAR, orgHeader, date2));
		}

		public void TestOrganisationOnlyRateWhenRateInDateRangeAP()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var taxConfigurationAP = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(TaxConfigurationLedgers.AccountsPayable.Code);
			Factory.Save();

			var accOrgTaxConfigurationAP = TaxFrameworkTestObjectCreator.CreateOrgTaxConfiguration(taxConfigurationAP, orgHeader.CompanyData, true);
			var taxRate1 = ((ZInt)1, (ZInt)4);
			var taxRate2 = ((ZInt)2, (ZInt)4);
			TaxFrameworkTestObjectCreator.AddOrgTaxRateItem(accOrgTaxConfigurationAP, new ZDate(2019, 10, 5), new ZDate(2019, 10, 10), RateSourceMethods.ManualOverride.Code, taxRate1);
			TaxFrameworkTestObjectCreator.AddOrgTaxRateItem(accOrgTaxConfigurationAP, new ZDate(2019, 10, 20), new ZDate(2019, 10, 30), RateSourceMethods.Quarterly.Code, taxRate2);

			var date1 = new ZDate(2019, 10, 5);
			var date2 = new ZDate(2019, 10, 30);
			var date3 = new ZDate(2019, 10, 15);

			var taxID = TaxFrameworkTestObjectCreator.CreateTaxRate_RateSource(TaxRateSources.OrganisationOnly.Code);
			AssertEquals($"a Tax Rate in Organisation must be found by Ledger {taxConfigurationAP.ETC_Ledger} and {date1}", taxRate1, TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetTaxRate(taxID, 0, 0, taxConfigurationAP, orgHeader, date1));
			AssertEquals($"a Tax Rate in Organisation must be found by Ledger {taxConfigurationAP.ETC_Ledger} and {date2}", taxRate2, TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetTaxRate(taxID, 0, 0, taxConfigurationAP, orgHeader, date2));
			AssertNull($"a Tax Rate in Organisation must not be found by Ledger {taxConfigurationAP.ETC_Ledger} and {date3}", TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetTaxRate(taxID, 0, 0, taxConfigurationAP, orgHeader, date3));
		}

		public void TestOrganisationOnlyRateWhenRateInDiferentTaxConfigurationAP()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var taxConfiguration1 = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(TaxConfigurationLedgers.AccountsPayable.Code);
			var taxConfiguration2 = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(TaxConfigurationLedgers.AccountsPayable.Code);
			Factory.Save();

			var accOrgTaxConfiguratio1 = TaxFrameworkTestObjectCreator.CreateOrgTaxConfiguration(taxConfiguration1, orgHeader.CompanyData, true);
			var taxRate1 = ((ZInt)1, (ZInt)4);
			TaxFrameworkTestObjectCreator.AddOrgTaxRateItem(accOrgTaxConfiguratio1, new ZDate(2019, 10, 5), new ZDate(2019, 10, 10), RateSourceMethods.ManualOverride.Code, taxRate1);

			var date = new ZDate(2019, 10, 5);
			var taxID = TaxFrameworkTestObjectCreator.CreateTaxRate_RateSource(TaxRateSources.OrganisationOnly.Code);
			AssertEquals($"a Tax Rate in Organisation must be found by Ledger {taxConfiguration1.ETC_Ledger} and {date}", taxRate1, TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetTaxRate(taxID, 0, 0, taxConfiguration1, orgHeader, date));

			var accOrgTaxConfiguration2 = TaxFrameworkTestObjectCreator.CreateOrgTaxConfiguration(taxConfiguration2, orgHeader.CompanyData, true);
			var taxRate2 = ((ZInt)2, (ZInt)4);
			TaxFrameworkTestObjectCreator.AddOrgTaxRateItem(accOrgTaxConfiguration2, new ZDate(2019, 10, 5), new ZDate(2019, 10, 10), RateSourceMethods.ManualOverride.Code, taxRate2);

			AssertEquals($"a Tax Rate in Organisation must be found by Ledger {taxConfiguration2.ETC_Ledger} and {date}", taxRate2, TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetTaxRate(taxID, 0, 0, taxConfiguration2, orgHeader, date));
		}

		public void TestOrganisationOnlyRateWhenRateInDiferentTaxConfigurationAR()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var taxConfiguration1 = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(TaxConfigurationLedgers.AccountsReceivable.Code);
			var taxConfiguration2 = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(TaxConfigurationLedgers.AccountsReceivable.Code);
			Factory.Save();

			var accOrgTaxConfiguratio1 = TaxFrameworkTestObjectCreator.CreateOrgTaxConfiguration(taxConfiguration1, orgHeader.CompanyData, true);
			var taxRate1 = ((ZInt)1, (ZInt)4);
			TaxFrameworkTestObjectCreator.AddOrgTaxRateItem(accOrgTaxConfiguratio1, new ZDate(2019, 10, 5), new ZDate(2019, 10, 10), RateSourceMethods.ManualOverride.Code, taxRate1);

			var date = new ZDate(2019, 10, 5);
			var taxID = TaxFrameworkTestObjectCreator.CreateTaxRate_RateSource(TaxRateSources.OrganisationOnly.Code);
			AssertEquals($"a Tax Rate in Organisation must be found by Ledger {taxConfiguration1.ETC_Ledger} and {date}", taxRate1, TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetTaxRate(taxID, 0, 0, taxConfiguration1, orgHeader, date));

			var accOrgTaxConfiguration2 = TaxFrameworkTestObjectCreator.CreateOrgTaxConfiguration(taxConfiguration2, orgHeader.CompanyData, true);
			var taxRate2 = ((ZInt)2, (ZInt)4);
			TaxFrameworkTestObjectCreator.AddOrgTaxRateItem(accOrgTaxConfiguration2, new ZDate(2019, 10, 5), new ZDate(2019, 10, 10), RateSourceMethods.ManualOverride.Code, taxRate2);

			AssertEquals($"a Tax Rate in Organisation must be found by Ledger {taxConfiguration2.ETC_Ledger} and {date}", taxRate2, TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetTaxRate(taxID, 0, 0, taxConfiguration2, orgHeader, date));
		}

		public void TestOrganisationOnlyRateSourceFallback()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var taxConfigurationAP = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(TaxConfigurationLedgers.AccountsPayable.Code);
			var taxConfigurationAR = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(TaxConfigurationLedgers.AccountsReceivable.Code);
			Factory.Save();

			var date = new ZDate(2019, 10, 10);
			var taxID = TaxFrameworkTestObjectCreator.CreateTaxRate_RateSource(TaxRateSources.OrganisationOnly.Code);
			var taxRateManualOverride = ((ZInt)1, (ZInt)4);
			var taxRateQuarterly = ((ZInt)2, (ZInt)4);
			var taxRateMonthly = ((ZInt)3, (ZInt)4);

			var accOrgTaxConfigurationAP = TaxFrameworkTestObjectCreator.CreateOrgTaxConfiguration(taxConfigurationAP, orgHeader.CompanyData, true);
			var taxRateItemManualAP = TaxFrameworkTestObjectCreator.AddOrgTaxRateItem(accOrgTaxConfigurationAP, new ZDate(2019, 10, 5), new ZDate(2019, 10, 10), RateSourceMethods.ManualOverride.Code, taxRateManualOverride);
			var taxRateItemQuarterlyAP = TaxFrameworkTestObjectCreator.AddOrgTaxRateItem(accOrgTaxConfigurationAP, new ZDate(2019, 10, 5), new ZDate(2019, 10, 10), RateSourceMethods.Quarterly.Code, taxRateQuarterly);
			var taxRateItemMMonthlyAP = TaxFrameworkTestObjectCreator.AddOrgTaxRateItem(accOrgTaxConfigurationAP, new ZDate(2019, 10, 5), new ZDate(2019, 10, 10), RateSourceMethods.Monthly.Code, taxRateMonthly);

			AssertEquals($"a Tax Rate in Organisation must be found by Ledger {TaxConfigurationLedgers.AccountsPayable.Code} and {date}", taxRateManualOverride, TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetTaxRate(taxID, 0, 0, taxConfigurationAP, orgHeader, date));
			accOrgTaxConfigurationAP.TaxRates.Delete(taxRateItemManualAP);
			AssertEquals($"a Tax Rate in Organisation must be found by Ledger {TaxConfigurationLedgers.AccountsPayable.Code} and {date}", taxRateQuarterly, TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetTaxRate(taxID, 0, 0, taxConfigurationAP, orgHeader, date));
			accOrgTaxConfigurationAP.TaxRates.Delete(taxRateItemQuarterlyAP);
			AssertEquals($"a Tax Rate in Organisation must be found by Ledger {TaxConfigurationLedgers.AccountsPayable.Code} and {date}", taxRateMonthly, TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetTaxRate(taxID, 0, 0, taxConfigurationAP, orgHeader, date));
			accOrgTaxConfigurationAP.TaxRates.Delete(taxRateItemMMonthlyAP);
			AssertNull($"a Tax Rate in Organisation must not be found by Ledger {TaxConfigurationLedgers.AccountsPayable.Code} and {date}", TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetTaxRate(taxID, 0, 0, taxConfigurationAP, orgHeader, date));

			var accOrgTaxConfigurationAR = TaxFrameworkTestObjectCreator.CreateOrgTaxConfiguration(taxConfigurationAR, orgHeader.CompanyData, true);
			var taxRateItemManualAR = TaxFrameworkTestObjectCreator.AddOrgTaxRateItem(accOrgTaxConfigurationAR, new ZDate(2019, 10, 5), new ZDate(2019, 10, 10), RateSourceMethods.ManualOverride.Code, taxRateManualOverride);
			var taxRateItemQuarterlyAR = TaxFrameworkTestObjectCreator.AddOrgTaxRateItem(accOrgTaxConfigurationAR, new ZDate(2019, 10, 5), new ZDate(2019, 10, 10), RateSourceMethods.Quarterly.Code, taxRateQuarterly);
			var taxRateItemMMonthlyAR = TaxFrameworkTestObjectCreator.AddOrgTaxRateItem(accOrgTaxConfigurationAR, new ZDate(2019, 10, 5), new ZDate(2019, 10, 10), RateSourceMethods.Monthly.Code, taxRateMonthly);

			AssertEquals($"a Tax Rate in Organisation must be found by Ledger {TaxConfigurationLedgers.AccountsReceivable.Code} and {date}", taxRateManualOverride, TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetTaxRate(taxID, 0, 0, taxConfigurationAR, orgHeader, date));
			accOrgTaxConfigurationAR.TaxRates.Delete(taxRateItemManualAR);
			AssertEquals($"a Tax Rate in Organisation must be found by Ledger {TaxConfigurationLedgers.AccountsReceivable.Code} and {date}", taxRateQuarterly, TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetTaxRate(taxID, 0, 0, taxConfigurationAR, orgHeader, date));
			accOrgTaxConfigurationAR.TaxRates.Delete(taxRateItemQuarterlyAR);
			AssertEquals($"a Tax Rate in Organisation must be found by Ledger {TaxConfigurationLedgers.AccountsReceivable.Code} and {date}", taxRateMonthly, TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetTaxRate(taxID, 0, 0, taxConfigurationAR, orgHeader, date));
			accOrgTaxConfigurationAR.TaxRates.Delete(taxRateItemMMonthlyAR);
			AssertNull($"a Tax Rate in Organisation must not be found by Ledger {TaxConfigurationLedgers.AccountsReceivable.Code} and {date}", TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetTaxRate(taxID, 0, 0, taxConfigurationAR, orgHeader, date));
		}

		public void TestTaxGroupOnlyRateSourceNoRecords()
		{
			var company = TestObjectCreator.CreateNewCompany("CO1");
			var taxAuthority = TaxFrameworkTestObjectCreator.CreateTaxAuthority("BA");
			var taxSystem = TaxFrameworkTestObjectCreator.CreateTaxSystem("PIB");
			var taxConfig = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(company, taxAuthority, taxSystem, TaxConfigurationLedgers.AccountsReceivable.Code, true);
			Factory.Save();

			var taxRate1 = ((ZInt)2, (ZInt)2);
			var taxRateDefault = ((ZInt)0, (ZInt)1);
			var taxID = TaxFrameworkTestObjectCreator.CreateTaxRate_RateSource(TaxRateSources.TaxGroupOnly.Code);
			var date = new ZDate(2019, 10, 10);

			AssertEquals("a Tax Rate in Tax Override Group1  must be found", taxRate1, TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetTaxRate(taxID, taxRate1.Item1, taxRate1.Item2, null, null, date));
			AssertEquals("a Tax Rate in Tax Override Group2 must not be found, it is default", taxRateDefault, TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetTaxRate(taxID, 0, 1, null, null, date));
		}

		public void TestTTaxIDOnlyRateSourceWithDifferentRate()
		{
			var taxID = TaxFrameworkTestObjectCreator.CreateTaxRate_RateSource(TaxRateSources.TaxIDOnly.Code);
			var taxRate1 = ((ZInt)1, (ZInt)1);
			var taxRate2 = ((ZInt)2, (ZInt)2);
			taxID.SetRate_ForTestOnly(taxRate1.Item1, taxRate1.Item2, new ZDate(2019, 10, 29), new ZDate(2019, 10, 31));
			taxID.SetRate_ForTestOnly(taxRate2.Item1, taxRate2.Item1, new ZDate(2020, 1, 1), new ZDate(2020, 1, 31));
			Factory.Save();
			var date1 = new ZDate(2019, 10, 30);
			var date2 = new ZDate(2020, 1, 5);
			var date3 = new ZDate(2019, 11, 30);
			AssertEquals($"a Tax Rate must be found by {date1}", taxRate1, TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetTaxRate(taxID, 0, 0, null, null, date1));
			AssertEquals($"a Tax Rate must be found by {date2}", taxRate2, TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetTaxRate(taxID, 0, 0, null, null, date2));
			AssertNull($"a Tax Rate must not be found by {date3}, it is rate null", TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetTaxRate(taxID, 0, 0, null, null, date3));
		}

		public void TestOrganisationFallbackToTaxGroup()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var taxConfigurationAP = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(TaxConfigurationLedgers.AccountsPayable.Code);
			Factory.Save();
			var taxRate1 = ((ZInt)1, (ZInt)1);
			var accOrgTaxConfigurationAP = TaxFrameworkTestObjectCreator.CreateOrgTaxConfiguration(taxConfigurationAP, orgHeader.CompanyData, true);
			TaxFrameworkTestObjectCreator.AddOrgTaxRateItem(accOrgTaxConfigurationAP, new ZDate(2019, 10, 5), new ZDate(2019, 10, 10), RateSourceMethods.ManualOverride.Code, taxRate1);
			var date1 = new ZDate(2019, 10, 5);

			var company = TestObjectCreator.CreateNewCompany("CO1");
			var taxAuthority = TaxFrameworkTestObjectCreator.CreateTaxAuthority("BA");
			var taxSystem = TaxFrameworkTestObjectCreator.CreateTaxSystem("PIB");
			TaxFrameworkTestObjectCreator.CreateTaxConfiguration(company, taxAuthority, taxSystem, TaxConfigurationLedgers.AccountsReceivable.Code, true);

			var taxRate2 = ((ZInt)2, (ZInt)2);
			var taxID = TaxFrameworkTestObjectCreator.CreateTaxRate_RateSource(TaxRateSources.OrganisationFallbackToTaxGroup.Code);
			AssertEquals($"a Tax Rate in Organisation must be found by Ledger {TaxConfigurationLedgers.AccountsPayable.Code} and {date1}", taxRate1, TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetTaxRate(taxID, taxRate2.Item1, taxRate2.Item2, taxConfigurationAP, orgHeader, date1));
			accOrgTaxConfigurationAP.TaxRates.DeleteAll();
			AssertEquals($"If a Tax Rate in Organisation is not found then it must be found in Tax Override Group", taxRate2, TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetTaxRate(taxID, taxRate2.Item1, taxRate2.Item2, taxConfigurationAP, orgHeader, date1));
		}

		public void TestOrganisationFallbackToTaxID()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var taxConfigurationAP = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(TaxConfigurationLedgers.AccountsPayable.Code);
			Factory.Save();
			var taxRate1 = ((ZInt)1, (ZInt)1);

			var accOrgTaxConfigurationAP = TaxFrameworkTestObjectCreator.CreateOrgTaxConfiguration(taxConfigurationAP, orgHeader.CompanyData, true);
			TaxFrameworkTestObjectCreator.AddOrgTaxRateItem(accOrgTaxConfigurationAP, new ZDate(2019, 10, 5), new ZDate(2019, 10, 10), RateSourceMethods.ManualOverride.Code, taxRate1);
			var date1 = new ZDate(2019, 10, 6);

			var taxRate2 = ((ZInt)2, (ZInt)2);
			var taxID = TaxFrameworkTestObjectCreator.CreateTaxRate_RateSource(TaxRateSources.OrganisationFallbackToTaxID.Code);
			taxID.SetRate_ForTestOnly(taxRate2.Item1, taxRate2.Item2, new ZDate(2019, 10, 1), new ZDate(2019, 10, 31));

			AssertEquals($"a Tax Rate in Organisation must be found by Ledger {TaxConfigurationLedgers.AccountsPayable.Code} and {date1}", taxRate1, TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetTaxRate(taxID, 0, 0, taxConfigurationAP, orgHeader, date1));
			accOrgTaxConfigurationAP.TaxRates.DeleteAll();
			AssertEquals($"If a Tax Rate in Organisation is not found then it must be found in TaxID", taxRate2, TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetTaxRate(taxID, 0, 0, taxConfigurationAP, orgHeader, date1));
		}

		public void TestGetTaxRateWhenRateSourceNotFound()
		{
			var taxRate = TaxFrameworkTestObjectCreator.CreateTaxRate_RateSource("RIV");
			AssertExceptionThrown(typeof(TaxFrameworkUnknownConfigurationValueException), $"{taxRate.AT_RateSource} rate source was not found.", () => TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetTaxRate(taxRate, 0, 0, null, null, new ZDate(2019, 10, 6)));
		}

		public void TestGetTaxRateWhenLedgerNotFound()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var taxConfiguration = TaxFrameworkTestObjectCreator.CreateTaxConfiguration("RI");
			Factory.SuspendValidation();
			var taxRate = TaxFrameworkTestObjectCreator.CreateTaxRate_RateSource(TaxRateSources.OrganisationFallbackToTaxID.Code);
			AssertExceptionThrown(typeof(TaxFrameworkUnknownConfigurationValueException), $"Tax Configuration for ledger RI was not found.", () => TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetTaxRate(taxRate, 0, 0, taxConfiguration, orgHeader, new ZDate(2019, 10, 6)));
		}

		public void TestGetTaxRateWhenSourceNotFound()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var taxConfiguration = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(TaxConfigurationLedgers.AccountsPayable.Code);
			Factory.Save();
			var accOrgTaxConfiguration = TaxFrameworkTestObjectCreator.CreateOrgTaxConfiguration(taxConfiguration, orgHeader.CompanyData, true);
			var taxRate1 = ((ZInt)1, (ZInt)1);
			TaxFrameworkTestObjectCreator.AddOrgTaxRateItem(accOrgTaxConfiguration, new ZDate(2019, 10, 5), new ZDate(2019, 10, 10), "RPC", taxRate1);
			var taxRate = TaxFrameworkTestObjectCreator.CreateTaxRate_RateSource(TaxRateSources.OrganisationFallbackToTaxID.Code);
			AssertExceptionThrown(typeof(TaxFrameworkUnknownConfigurationValueException), $"RPC organization rate source was not found.", () => TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetTaxRate(taxRate, 0, 0, taxConfiguration, orgHeader, new ZDate(2019, 10, 6)));
		}

		public void TestRateSource()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var taxConfiguration = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(TaxConfigurationLedgers.AccountsPayable.Code);
			Factory.Save();
			var accOrgTaxConfigurationAP = TaxFrameworkTestObjectCreator.CreateOrgTaxConfiguration(taxConfiguration, orgHeader.CompanyData, true);
			var company = TestObjectCreator.CreateNewCompany("CO1");
			var taxAuthority = TaxFrameworkTestObjectCreator.CreateTaxAuthority("BA");
			var taxSystem = TaxFrameworkTestObjectCreator.CreateTaxSystem("PIB");
			var taxConfig = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(company, taxAuthority, taxSystem, TaxConfigurationLedgers.AccountsReceivable.Code, true);
			var taxID = TaxFrameworkTestObjectCreator.CreateTaxRate_RateSource(TaxRateSources.OrganisationOnly.Code);
			var taxRateTaxIDOnly = ((ZInt)1, (ZInt)1);
			var taxRateOrganisationOnly = ((ZInt)2, (ZInt)1);
			var taxRateTaxGroupOnly = ((ZInt)3, (ZInt)1);
			var date = new ZDate(2019, 10, 11);

			taxID.SetRate_ForTestOnly(taxRateTaxIDOnly.Item1, taxRateTaxIDOnly.Item2, new ZDate(2019, 10, 1), new ZDate(2019, 10, 31));
			TaxFrameworkTestObjectCreator.AddOrgTaxRateItem(accOrgTaxConfigurationAP, new ZDate(2019, 10, 5), new ZDate(2019, 10, 30), RateSourceMethods.Monthly.Code, taxRateOrganisationOnly);
			AssertEquals($"a Tax Rate in Organisation must be found by Ledger {TaxConfigurationLedgers.AccountsPayable.Code} and {date}", taxRateOrganisationOnly, TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetTaxRate(taxID, 0, 0, taxConfiguration, orgHeader, date));

			taxID.AT_RateSource = TaxRateSources.TaxGroupOnly.Code;
			AssertEquals("a Tax Rate in Tax Override Group must be found", taxRateTaxGroupOnly, TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetTaxRate(taxID, taxRateTaxGroupOnly.Item1, taxRateTaxGroupOnly.Item2, null, null, date));

			taxID.AT_RateSource = TaxRateSources.TaxIDOnly.Code;
			AssertEquals($"a Tax Rate must be found by {date}", taxRateTaxIDOnly, TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetTaxRate(taxID, 0, 0, null, null, date));

			taxID.AT_RateSource = TaxRateSources.OrganisationFallbackToTaxGroup.Code;
			AssertEquals($"a Tax Rate in Organisation must be found by Ledger {TaxConfigurationLedgers.AccountsPayable.Code} and {date}", taxRateOrganisationOnly, TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetTaxRate(taxID, 0, 0, taxConfiguration, orgHeader, date));

			taxID.AT_RateSource = TaxRateSources.OrganisationFallbackToTaxID.Code;
			AssertEquals($"a Tax Rate in Organisation must be found by Ledger {TaxConfigurationLedgers.AccountsPayable.Code} and {date}", taxRateOrganisationOnly, TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetTaxRate(taxID, 0, 0, taxConfiguration, orgHeader, date));
		}

		#endregion

		#region CalculateTaxRecord

		public void TestTaxRecordsGLAccountsFields()
		{
			var taxRecords = new List<AccTaxTransaction>();

			var company = TestObjectCreator.CreateNewCompany("CO1");
			var taxSystem = TaxFrameworkTestObjectCreator.CreateTaxSystem("PIB");
			var taxSystem2 = TaxFrameworkTestObjectCreator.CreateTaxSystem("RIB");
			var taxSystemsConfigCollection = new TaxSystemsConfigurationCollection();
			taxSystemsConfigCollection.Add(taxSystem);
			taxSystemsConfigCollection.Add(taxSystem2);

			var taxConfiguration1 = Factory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfiguration1.ETC_TaxSystemCode = taxSystem.Code;
			taxConfiguration1.ETC_TaxRealisationMethod = TaxRealisationMethods.PostDate.Code;
			var expectedTaxControlAccountPK = ZGuid.NewZGuid();
			var expectedLedgerControlAccountPK = ZGuid.NewZGuid();
			var expectedTaxExpenseAccountPK = ZGuid.NewZGuid();
			var expectedTaxPendingControlAccountPK = ZGuid.NewZGuid();
			taxConfiguration1.ETC_AG_TaxControlAccount = expectedTaxControlAccountPK;
			taxConfiguration1.ETC_AG_LedgerControlAccount = expectedLedgerControlAccountPK;
			taxConfiguration1.ETC_AG_TaxExpenseAccount = expectedTaxExpenseAccountPK;
			taxConfiguration1.ETC_AG_TaxPendingControlAccount = expectedTaxPendingControlAccountPK;
			var taxConfiguration2 = Factory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfiguration2.ETC_TaxSystemCode = taxSystem2.Code;
			taxConfiguration2.ETC_TaxRealisationMethod = TaxRealisationMethods.MatchDate.Code;

			AccountingMasterFilesRegistry.Instance.TaxSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taxSystemsConfigCollection);
			taxSystemsConfigCollection.ClearCachedTaxSystemsByCode_ForTestOnly(Factory);

			var transactionHeader = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "001", TestObjectCreator.AUD, 1M, TestObjectCreator.ABIGAS);
			transactionHeader.AH_GC = company.PK;

			var taxId = Factory.NewWithValidTestData<AccTaxRate>();
			(ZInt, ZInt) taxRate = (21, 100);
			AccInvMsg taxMessage = Factory.NewWithValidTestData<AccInvMsg>();
			var lineTaxableTransactionMock = CreateITaxableTransactionLineMock(Factory, ZGuid.NewZGuid(), "USD", new ZDate(2020, 04, 17));
			var taxRecordParent = TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(transactionHeader);

			var calculatedTaxRecord = TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.CalculateTaxRecord(taxRecords, lineTaxableTransactionMock.Object, taxId, taxConfiguration1, taxRate, taxRecordParent, taxMessage.PK, "", "");
			var taxRecord = GetTaxRecord(taxId.PK, taxConfiguration1.PK, taxRate, taxMessage.PK, null);
			var pivot1 = GetPivot(lineTaxableTransactionMock.Object.PK, taxRecord.PK);
			AssertNotNull("Precondition: Pivot should be created", pivot1);
			Assert("ATP_IsTaxExpense when ATT_AG_TaxPendingControlAccount is non-empty", pivot1.ATP_IsTaxExpense);
			AssertEquals(nameof(calculatedTaxRecord), (taxRecord, pivot1), calculatedTaxRecord);

			AssertEquals("ATT_PostDate", (ZDate)transactionHeader.AH_PostDate, taxRecord.ATT_PostDate);

			AssertEquals("Precondition: ETC_AG_TaxControlAccount", expectedTaxControlAccountPK, taxConfiguration1.ETC_AG_TaxControlAccount);
			AssertEquals("ATT_AG_TaxControlAccount", expectedTaxControlAccountPK, taxRecord.ATT_AG_TaxControlAccount);

			AssertEquals("Precondition: ATT_AG_LedgerControlAccount", expectedLedgerControlAccountPK, taxConfiguration1.ETC_AG_LedgerControlAccount);
			AssertEquals("ATT_AG_LedgerControlAccount", expectedLedgerControlAccountPK, taxRecord.ATT_AG_LedgerControlAccount);

			AssertEquals("Precondition: ATT_AG_TaxExpenseAccount", expectedTaxExpenseAccountPK, taxConfiguration1.ETC_AG_TaxExpenseAccount);
			AssertEquals("ATT_AG_TaxExpenseAccount", taxConfiguration1.ETC_AG_TaxExpenseAccount, taxRecord.ATT_AG_TaxExpenseAccount);

			AssertEquals("Precondition: ATT_AG_TaxPendingControlAccount", expectedTaxPendingControlAccountPK, taxConfiguration1.ETC_AG_TaxPendingControlAccount);
			AssertEquals("ATT_AG_TaxPendingControlAccount", taxConfiguration1.ETC_AG_TaxPendingControlAccount, taxRecord.ATT_AG_TaxPendingControlAccount);

			AssertEquals("ATT_RealisationDate", (ZDate)transactionHeader.AH_PostDate, taxRecord.ATT_RealisationDate);

			calculatedTaxRecord = TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.CalculateTaxRecord(taxRecords, lineTaxableTransactionMock.Object, taxId, taxConfiguration2, taxRate, taxRecordParent, taxMessage.PK, "", "");
			var taxRecord2 = GetTaxRecord(taxId.PK, taxConfiguration2.PK, taxRate, taxMessage.PK, null);
			var pivot2 = GetPivot(lineTaxableTransactionMock.Object.PK, taxRecord2.PK);
			AssertNotNull("Precondition: Pivot should be created", pivot2);
			Assert("ATP_IsTaxExpense when ATT_AG_TaxPendingControlAccount is empty", !pivot2.ATP_IsTaxExpense);
			AssertEquals(nameof(calculatedTaxRecord), (taxRecord2, pivot2), calculatedTaxRecord);

			AssertEquals("ATT_RealisationDate", ZDate.Empty, taxRecord2.ATT_RealisationDate);
			AssertEquals("ATT_AG_TaxExpenseAccount", ZGuid.Empty, taxRecord2.ATT_AG_TaxExpenseAccount);
		}

		public void TestCalculateTaxRecord_NotCreateTaxRecord()
		{
			List<AccTaxTransaction> taxRecords = new List<AccTaxTransaction>();

			var transactionHeader = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "001", TestObjectCreator.AUD, 1M, TestObjectCreator.ABIGAS);
			GlbCompany company = Factory.NewWithValidTestData<GlbCompany>();
			AccInvMsg taxMessage = Factory.NewWithValidTestData<AccInvMsg>();

			var taxSystemsConfigCollection = new TaxSystemsConfigurationCollection();
			var taxSystem = TaxFrameworkTestObjectCreator.CreateTaxSystem("PIB");
			taxSystem.IncludeInInvoceTotal = true;
			taxSystemsConfigCollection.Add(taxSystem);
			AccountingMasterFilesRegistry.Instance.TaxSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taxSystemsConfigCollection);
			taxSystemsConfigCollection.ClearCachedTaxSystemsByCode_ForTestOnly(Factory);

			AccTaxRate taxId = Factory.NewWithValidTestData<AccTaxRate>();
			AccTaxConfiguration taxConfiguration = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(GlbCompany.CurrentCompany, null, taxSystem, TaxConfigurationLedgers.AccountsReceivable.Code);

			var lineTaxableTransactionMock = CreateITaxableTransactionLineMock(Factory, ZGuid.NewZGuid(), "USD", new ZDate(2019, 10, 30));
			var taxRecordParent = TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(transactionHeader);

			var calculatedTaxRecord = TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.CalculateTaxRecord(taxRecords, lineTaxableTransactionMock.Object, taxId, taxConfiguration, (0, 1), taxRecordParent, taxMessage.PK, "", "");
			(AccTaxTransaction, AccTaxRecordTransactionLinePivot) expectedValue = (null, null);
			AssertEquals("null tuple returned", expectedValue, calculatedTaxRecord);
			AssertEquals($"taxRecords should not be contain records", 0, taxRecords.Count);
			AssertEquals("Pivot should not be created", 0, Factory.Load<AccTaxRecordTransactionLinePivot>(new ZQuery { FetchOnlyFromLocalCache = true }).Length);
			AssertEquals("Tax Record should not be created", 0, Factory.Load<AccTaxTransaction>(new ZQuery { FetchOnlyFromLocalCache = true }).Length);

			var lineTaxableTransactionMock1 = CreateITaxableTransactionLineMock(Factory, ZGuid.NewZGuid(), "USD", new ZDate(2019, 10, 30));
			calculatedTaxRecord = TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.CalculateTaxRecord(taxRecords, lineTaxableTransactionMock1.Object, taxId, taxConfiguration, (1, 1), taxRecordParent, taxMessage.PK, "", "");
			var taxRecord = GetTaxRecord(taxId.PK, taxConfiguration.PK, (1, 1), taxMessage.PK, null);
			AssertNotNull("Tax Transaction should be created", taxRecord);
			var pivot = GetPivot(lineTaxableTransactionMock1.Object.PK, taxRecord.PK);
			AssertNotNull("Pivot should be created", pivot);
			AssertEquals(nameof(calculatedTaxRecord), (taxRecord, pivot), calculatedTaxRecord);
			AssertEquals($"Factory for AccTaxTransaction should contain 1 records", 1, Factory.Load<AccTaxTransaction>(new ZQuery { FetchOnlyFromLocalCache = true }).Length);
			AssertEquals($"taxRecords should contain 1 records", 1, taxRecords.Count);
			AssertCollectionContains(taxRecord, taxRecords);
			AssertEquals($"Factory for AccTaxRecordTransactionLinePivot should contain 1 record", 1, Factory.Load<AccTaxRecordTransactionLinePivot>(new ZQuery { FetchOnlyFromLocalCache = true }).Length);
		}

		public void TestCalculateTaxRecord_NewAccTransaction()
		{
			List<AccTaxTransaction> taxRecords = new List<AccTaxTransaction>();

			var department = TestObjectCreator.FEADepartment;
			var transactionHeader = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "001", TestObjectCreator.AUD, 1M, TestObjectCreator.ABIGAS);
			GlbCompany company = Factory.NewWithValidTestData<GlbCompany>();
			transactionHeader.AH_GC = company.PK;
			transactionHeader.AH_GE = department.PK;
			AccInvMsg taxMessage = Factory.NewWithValidTestData<AccInvMsg>();
			var taxAuthorityServiceCode1 = "9133";
			var taxAuthorityServiceCodeDescription1 = "Code 9133";
			var taxAuthorityServiceCode2 = "6653";
			var taxAuthorityServiceCodeDescription2 = "Code 6653";

			var taxSystemsConfigCollection = new TaxSystemsConfigurationCollection();
			var taxSystem1 = TaxFrameworkTestObjectCreator.CreateTaxSystem("PIB");
			taxSystem1.IncludeInInvoceTotal = true;
			taxSystemsConfigCollection.Add(taxSystem1);
			AccTaxConfiguration taxConfiguration1 = Factory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfiguration1.ETC_TaxSystemCode = taxSystem1.Code;
			taxConfiguration1.ETC_Ledger = TaxConfigurationLedgers.AccountsReceivable.Code;
			taxConfiguration1.ETC_TaxRecordCreationTrigger = TaxRecordCreationTrigger.PostDate.Code;
			taxConfiguration1.ETC_TaxRealisationMethod = TaxRealisationMethods.PostDate.Code;
			var taxSystem2 = TaxFrameworkTestObjectCreator.CreateTaxSystem("RIB");
			taxSystem2.IncludeInInvoceTotal = false;
			taxSystemsConfigCollection.Add(taxSystem2);
			AccTaxConfiguration taxConfiguration2 = Factory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfiguration2.ETC_TaxSystemCode = taxSystem2.Code;
			taxConfiguration2.ETC_Ledger = TaxConfigurationLedgers.AccountsPayable.Code;
			taxConfiguration2.ETC_TaxRecordCreationTrigger = TaxRecordCreationTrigger.PostDate.Code;
			taxConfiguration2.ETC_TaxRealisationMethod = TaxRealisationMethods.MatchDate.Code;
			AccountingMasterFilesRegistry.Instance.TaxSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taxSystemsConfigCollection);
			taxSystemsConfigCollection.ClearCachedTaxSystemsByCode_ForTestOnly(Factory);

			AccTaxRate taxId = Factory.NewWithValidTestData<AccTaxRate>();
			var dateTaxRate = new ZDate(2019, 10, 30);
			var lineTaxableTransactionMock = CreateITaxableTransactionLineMock(Factory, ZGuid.NewZGuid(), "USD", dateTaxRate);

			(ZInt, ZInt) taxRate = (21, 100);
			var taxRecordParent = TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(transactionHeader);
			var calculatedTaxRecord = TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.CalculateTaxRecord(taxRecords, lineTaxableTransactionMock.Object, taxId, taxConfiguration1, taxRate, taxRecordParent, taxMessage.PK, taxAuthorityServiceCode1, taxAuthorityServiceCodeDescription1);
			var taxRecord = GetTaxRecord(taxId.PK, taxConfiguration1.PK, taxRate, taxMessage.PK, taxAuthorityServiceCode1);
			var pivot1 = GetPivot(lineTaxableTransactionMock.Object.PK, taxRecord.PK);
			AssertNotNull("Pivot should be created", pivot1);
			AssertNotNull("Tax Transaction should be created", taxRecord);
			AssertEquals(nameof(calculatedTaxRecord), (taxRecord, pivot1), calculatedTaxRecord);
			AssertEquals("Number of AccTaxTransaction in Factory", 1, Factory.Load<AccTaxTransaction>(new ZQuery { FetchOnlyFromLocalCache = true }).Length);
			AssertEquals("taxRecords Count", 1, taxRecords.Count);
			AssertCollectionContains(taxRecord, taxRecords);
			AssertEquals("Number of AccTaxRecordTransactionLinePivot in Factory", 1, Factory.Load<AccTaxRecordTransactionLinePivot>(new ZQuery { FetchOnlyFromLocalCache = true }).Length);

			AssertEquals("ATT_AH", transactionHeader.PK, taxRecord.ATT_AH);
			AssertEquals("ATT_GC", company.PK, taxRecord.ATT_GC);
			AssertEquals("ATT_GE_Department", department.PK, taxRecord.ATT_GE_Department);
			AssertEquals("ATT_Ledger", TaxConfigurationLedgers.AccountsReceivable.Code, taxRecord.ATT_Ledger);
			AssertEquals("ATT_ETC", taxConfiguration1.PK, taxRecord.ATT_ETC);
			AssertEquals("ATT_TaxSystemCode", "PIB", taxRecord.ATT_TaxSystemCode);
			AssertEquals("ATT_PostDate", ZDateTime.Today.Date, taxRecord.ATT_PostDate);
			AssertEquals("ATT_RX_NKOSTaxCurrency", "AUD", taxRecord.ATT_RX_NKOSTaxCurrency);
			AssertEquals("ATT_AT_TaxID", taxId.PK, taxRecord.ATT_AT_TaxID);
			AssertEquals("ATT_TaxDate", dateTaxRate, taxRecord.ATT_TaxDate);
			AssertEquals("ATT_A9_TaxMessage", taxMessage.PK, taxRecord.ATT_A9_TaxMessage);
			AssertEquals("ATT_RateNumerator", taxRate.Item1, taxRecord.ATT_RateNumerator);
			AssertEquals("ATT_RateDenominator", taxRate.Item2, taxRecord.ATT_RateDenominator);
			AssertEquals("ATT_AffectsSourceTransactionTotal", true, taxRecord.ATT_AffectsSourceTransactionTotal);
			AssertEquals("ATT_Basis", TaxBasisList.Posting.Code, taxRecord.ATT_Basis);
			AssertEquals("ATT_TaxAuthorityServiceCode", taxAuthorityServiceCode1, taxRecord.ATT_TaxAuthorityServiceCode);
			AssertEquals("ATT_TaxAuthorityServiceCodeDescription", taxAuthorityServiceCodeDescription1, taxRecord.ATT_TaxAuthorityServiceCodeDescription);

			var dateTaxRateDefault = new ZDate(2019, 10, 31);
			var lineTaxableTransactionMockTaxRateDefault = CreateITaxableTransactionLineMock(Factory, ZGuid.NewZGuid(), "ARS", dateTaxRateDefault);

			(ZInt, ZInt) taxRateDefault = (0, 1);
			transactionHeader.AH_RX_NKTransactionCurrency = "ARS";
			calculatedTaxRecord = TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.CalculateTaxRecord(taxRecords, lineTaxableTransactionMockTaxRateDefault.Object, taxId, taxConfiguration2, null, taxRecordParent, taxMessage.PK, taxAuthorityServiceCode2, taxAuthorityServiceCodeDescription2);
			var taxRecordRateDefault = GetTaxRecord(taxId.PK, taxConfiguration2.PK, taxRateDefault, taxMessage.PK, taxAuthorityServiceCode2);
			AssertNotNull("Tax Transaction should be created", taxRecordRateDefault);
			var pivot2 = GetPivot(lineTaxableTransactionMockTaxRateDefault.Object.PK, taxRecordRateDefault.PK);
			AssertNotNull("Pivot should be created", pivot2);
			AssertEquals(nameof(calculatedTaxRecord), (taxRecordRateDefault, pivot2), calculatedTaxRecord);
			AssertEquals($"Number of AccTaxTransaction in Factory", 2, Factory.Load<AccTaxTransaction>(new ZQuery { FetchOnlyFromLocalCache = true }).Length);
			AssertEquals($"taxRecords Count", 2, taxRecords.Count);
			AssertCollectionContains(taxRecordRateDefault, taxRecords);
			AssertEquals($"Number of AccTaxRecordTransactionLinePivot in Factory", 2, Factory.Load<AccTaxRecordTransactionLinePivot>(new ZQuery { FetchOnlyFromLocalCache = true }).Length);

			AssertEquals("ATT_AH", transactionHeader.PK, taxRecordRateDefault.ATT_AH);
			AssertEquals("ATT_GC", company.PK, taxRecordRateDefault.ATT_GC);
			AssertEquals("ATT_Ledger", TaxConfigurationLedgers.AccountsPayable.Code, taxRecordRateDefault.ATT_Ledger);
			AssertEquals("ATT_ETC", taxConfiguration2.PK, taxRecordRateDefault.ATT_ETC);
			AssertEquals("ATT_TaxSystemCode", "RIB", taxRecordRateDefault.ATT_TaxSystemCode);
			AssertEquals("ATT_PostDate", ZDateTime.Today.Date, taxRecordRateDefault.ATT_PostDate);
			AssertEquals("ATT_RX_NKOSTaxCurrency", "ARS", taxRecordRateDefault.ATT_RX_NKOSTaxCurrency);
			AssertEquals("ATT_AT_TaxID", taxId.PK, taxRecordRateDefault.ATT_AT_TaxID);
			AssertEquals("ATT_TaxDate", dateTaxRateDefault, taxRecordRateDefault.ATT_TaxDate);
			AssertEquals("ATT_A9_TaxMessage", taxMessage.PK, taxRecordRateDefault.ATT_A9_TaxMessage);
			AssertEquals("ATT_RateNumerator", taxRateDefault.Item1, taxRecordRateDefault.ATT_RateNumerator);
			AssertEquals("ATT_RateDenominator", taxRateDefault.Item2, taxRecordRateDefault.ATT_RateDenominator);
			AssertEquals("ATT_AffectsSourceTransactionTotal", false, taxRecordRateDefault.ATT_AffectsSourceTransactionTotal);
			AssertEquals("ATT_Basis", TaxBasisList.Matching.Code, taxRecordRateDefault.ATT_Basis);
			AssertEquals("ATT_TaxAuthorityServiceCode", taxAuthorityServiceCode2, taxRecordRateDefault.ATT_TaxAuthorityServiceCode);
			AssertEquals("ATT_TaxAuthorityServiceCodeDescription", taxAuthorityServiceCodeDescription2, taxRecordRateDefault.ATT_TaxAuthorityServiceCodeDescription);
		}

		public void TestCalculateTaxRecord_TaxRecords_MatchingByTaxConfiguration()
		{
			var taxParent = CreateITaxRecordParentMock(Factory);
			var taxSystemsConfigCollection = new TaxSystemsConfigurationCollection();
			var taxSystem = TaxFrameworkTestObjectCreator.CreateTaxSystem("PIB");
			taxSystemsConfigCollection.Add(taxSystem);
			AccountingMasterFilesRegistry.Instance.TaxSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taxSystemsConfigCollection);
			taxSystemsConfigCollection.ClearCachedTaxSystemsByCode_ForTestOnly(Factory);
			AccTaxRate taxId = Factory.NewWithValidTestData<AccTaxRate>();
			AccTaxConfiguration taxConfiguration1 = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(GlbCompany.CurrentCompany, null, taxSystem);
			(ZInt, ZInt) taxRate = (11, 1);
			AccInvMsg taxMessage = Factory.NewWithValidTestData<AccInvMsg>();
			List<AccTaxTransaction> taxRecords = new List<AccTaxTransaction>();
			AccTransactionHeader transactionHeader = Factory.NewWithValidTestData<AccTransactionHeader>();
			AccTaxConfiguration taxConfiguration2 = Factory.NewWithValidTestData<AccTaxConfiguration>();
			var taxRecord1 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { TaxConfiguration = taxConfiguration2, TransactionHeader = transactionHeader, TaxId = taxId, TaxRate = taxRate });
			taxRecords.Add(taxRecord1);

			var lineTaxableTransactionNotMatchingMock = CreateITaxableTransactionLineMock(Factory, ZGuid.NewZGuid(), "ARS", new ZDate(2019, 10, 31));
			var caclualtedTaxRecord = TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.CalculateTaxRecord(taxRecords, lineTaxableTransactionNotMatchingMock.Object, taxId, taxConfiguration1, taxRate, taxParent.Object, taxMessage.PK, "", "");
			var taxRecord2 = GetTaxRecord(taxId.PK, taxConfiguration1.PK, taxRate, taxMessage.PK, null);
			AssertNotNull("Tax Transaction should be created", taxRecord2);
			var pivotNotMatching = GetPivot(lineTaxableTransactionNotMatchingMock.Object.PK, taxRecord2.PK);
			AssertNotNull("Pivot should be created", pivotNotMatching);
			AssertEquals(nameof(caclualtedTaxRecord), (taxRecord2, pivotNotMatching), caclualtedTaxRecord);
			AssertNotEquals(taxRecord1.PK, taxRecord2.PK);
			AssertEquals($"Factory for AccTaxTransaction should contain 2 records", 2, Factory.Load<AccTaxTransaction>(new ZQuery { FetchOnlyFromLocalCache = true }).Length);
			AssertEquals($"taxRecords should contain 2 records", 2, taxRecords.Count);
			AssertCollectionContains(taxRecord2, taxRecords);
			AssertEquals($"Factory for AccTaxRecordTransactionLinePivot should contain 1 record", 1, Factory.Load<AccTaxRecordTransactionLinePivot>(new ZQuery { FetchOnlyFromLocalCache = true }).Length);

			var lineTaxableTransactionMatchingMock = CreateITaxableTransactionLineMock(Factory, ZGuid.NewZGuid(), "ARS", new ZDate(2019, 10, 31));
			caclualtedTaxRecord = TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.CalculateTaxRecord(taxRecords, lineTaxableTransactionMatchingMock.Object, taxId, taxConfiguration1, taxRate, taxParent.Object, taxMessage.PK, "", "");
			AssertEquals($"Factory for AccTaxTransaction should contain 2 records", 2, Factory.Load<AccTaxTransaction>(new ZQuery { FetchOnlyFromLocalCache = true }).Length);
			AssertEquals($"taxRecords should contain 2 records", 2, taxRecords.Count);
			AssertCollectionContains(taxRecord2, taxRecords);
			var pivotMatching = GetPivot(lineTaxableTransactionMatchingMock.Object.PK, taxRecord2.PK);
			AssertNotNull("Pivot should be created", pivotMatching);
			AssertEquals(nameof(caclualtedTaxRecord), (taxRecord2, pivotMatching), caclualtedTaxRecord);
			AssertEquals($"Factory for AccTaxRecordTransactionLinePivot should contain 2 records", 2, Factory.Load<AccTaxRecordTransactionLinePivot>(new ZQuery { FetchOnlyFromLocalCache = true }).Length);
		}

		public void TestCalculateTaxRecord_TaxRecords_MatchingByTaxId()
		{
			var taxParent = CreateITaxRecordParentMock(Factory);
			var taxSystemsConfigCollection = new TaxSystemsConfigurationCollection();
			var taxSystem = TaxFrameworkTestObjectCreator.CreateTaxSystem("PIB");
			taxSystemsConfigCollection.Add(taxSystem);
			AccountingMasterFilesRegistry.Instance.TaxSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taxSystemsConfigCollection);
			taxSystemsConfigCollection.ClearCachedTaxSystemsByCode_ForTestOnly(Factory);
			AccTaxRate taxId1 = Factory.NewWithValidTestData<AccTaxRate>();
			AccTaxConfiguration taxConfiguration = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(GlbCompany.CurrentCompany, null, taxSystem);
			taxConfiguration.ETC_TaxSystemCode = taxSystem.Code;
			(ZInt, ZInt) taxRate = (11, 1);
			AccInvMsg taxMessage = Factory.NewWithValidTestData<AccInvMsg>();
			List<AccTaxTransaction> taxRecords = new List<AccTaxTransaction>();
			AccTransactionHeader transactionHeader = Factory.NewWithValidTestData<AccTransactionHeader>();
			var taxId2 = Factory.NewWithValidTestData<AccTaxRate>();
			var taxRecord1 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { TaxConfiguration = taxConfiguration, TransactionHeader = transactionHeader, TaxId = taxId2, TaxRate = taxRate });
			taxRecords.Add(taxRecord1);

			var lineTaxableTransactionNotMatchingMock = CreateITaxableTransactionLineMock(Factory, ZGuid.NewZGuid(), "ARS", new ZDate(2019, 10, 31));
			var caclualtedTaxRecord = TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.CalculateTaxRecord(taxRecords, lineTaxableTransactionNotMatchingMock.Object, taxId1, taxConfiguration, taxRate, taxParent.Object, taxMessage.PK, "", "");
			var taxRecord2 = GetTaxRecord(taxId1.PK, taxConfiguration.PK, taxRate, taxMessage.PK, null);
			AssertNotNull("Tax Transaction should be created", taxRecord2);
			var pivotNotMatching = GetPivot(lineTaxableTransactionNotMatchingMock.Object.PK, taxRecord2.PK);
			AssertNotNull("Pivot should be created", pivotNotMatching);
			AssertEquals(nameof(caclualtedTaxRecord), (taxRecord2, pivotNotMatching), caclualtedTaxRecord);
			AssertNotEquals(taxRecord1.PK, taxRecord2.PK);
			AssertEquals($"Factory for AccTaxTransaction should contain 2 records", 2, Factory.Load<AccTaxTransaction>(new ZQuery { FetchOnlyFromLocalCache = true }).Length);
			AssertEquals($"taxRecords should contain 2 record", 2, taxRecords.Count);
			AssertCollectionContains(taxRecord2, taxRecords);
			AssertEquals($"Factory for AccTaxRecordTransactionLinePivot should contain 1 record", 1, Factory.Load<AccTaxRecordTransactionLinePivot>(new ZQuery { FetchOnlyFromLocalCache = true }).Length);

			var lineTaxableTransactionMatchingMock = CreateITaxableTransactionLineMock(Factory, ZGuid.NewZGuid(), "ARS", new ZDate(2019, 10, 31));
			caclualtedTaxRecord = TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.CalculateTaxRecord(taxRecords, lineTaxableTransactionMatchingMock.Object, taxId1, taxConfiguration, taxRate, taxParent.Object, taxMessage.PK, "", "");
			AssertEquals($"Factory for AccTaxTransaction should contain 2 records", 2, Factory.Load<AccTaxTransaction>(new ZQuery { FetchOnlyFromLocalCache = true }).Length);
			AssertEquals($"taxRecords should contain 2 records", 2, taxRecords.Count);
			AssertCollectionContains(taxRecord2, taxRecords);
			var pivotMatching = GetPivot(lineTaxableTransactionMatchingMock.Object.PK, taxRecord2.PK);
			AssertNotNull("Pivot should be created", pivotMatching);
			AssertEquals(nameof(caclualtedTaxRecord), (taxRecord2, pivotMatching), caclualtedTaxRecord);
			AssertEquals($"Factory for AccTaxRecordTransactionLinePivot should contain 2 records", 2, Factory.Load<AccTaxRecordTransactionLinePivot>(new ZQuery { FetchOnlyFromLocalCache = true }).Length);
		}

		public void TestCalculateTaxRecord_TaxRecords_MatchingByNumerator()
		{
			var taxParent = CreateITaxRecordParentMock(Factory);
			var taxSystemsConfigCollection = new TaxSystemsConfigurationCollection();
			var taxSystem = TaxFrameworkTestObjectCreator.CreateTaxSystem("PIB");
			taxSystemsConfigCollection.Add(taxSystem);
			AccountingMasterFilesRegistry.Instance.TaxSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taxSystemsConfigCollection);
			taxSystemsConfigCollection.ClearCachedTaxSystemsByCode_ForTestOnly(Factory);
			AccTaxRate taxId = Factory.NewWithValidTestData<AccTaxRate>();
			AccTaxConfiguration taxConfiguration = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(GlbCompany.CurrentCompany, null, taxSystem);
			AccTransactionHeader transactionHeader = Factory.NewWithValidTestData<AccTransactionHeader>();
			(ZInt, ZInt) taxRate = (1, 1);
			AccInvMsg taxMessage = Factory.NewWithValidTestData<AccInvMsg>();
			List<AccTaxTransaction> taxRecords = new List<AccTaxTransaction>();
			var taxRecord1 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { TaxConfiguration = taxConfiguration, TransactionHeader = transactionHeader, TaxId = taxId, TaxRate = (2, 1) });
			taxRecords.Add(taxRecord1);

			var lineTaxableTransactionNotMatchingMock = CreateITaxableTransactionLineMock(Factory, ZGuid.NewZGuid(), "ARS", new ZDate(2019, 10, 31));
			var caclualtedTaxRecord = TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.CalculateTaxRecord(taxRecords, lineTaxableTransactionNotMatchingMock.Object, taxId, taxConfiguration, taxRate, taxParent.Object, taxMessage.PK, "", "");
			var taxRecord2 = GetTaxRecord(taxId.PK, taxConfiguration.PK, taxRate, taxMessage.PK, null);
			AssertNotEquals(taxRecord1.PK, taxRecord2.PK);
			AssertNotNull("Tax Transaction should be created", taxRecord2);
			var pivotNotMatching = GetPivot(lineTaxableTransactionNotMatchingMock.Object.PK, taxRecord2.PK);
			AssertNotNull("Pivot should be created", pivotNotMatching);
			AssertEquals(nameof(caclualtedTaxRecord), (taxRecord2, pivotNotMatching), caclualtedTaxRecord);
			AssertEquals("Factory for AccTaxTransaction should contain 2 records", 2, Factory.Load<AccTaxTransaction>(new ZQuery { FetchOnlyFromLocalCache = true }).Length);
			AssertEquals("taxRecords should contain 2 records", 2, taxRecords.Count);
			AssertCollectionContains(taxRecord2, taxRecords);
			AssertEquals("Factory for AccTaxRecordTransactionLinePivot should contain 1 record", 1, Factory.Load<AccTaxRecordTransactionLinePivot>(new ZQuery { FetchOnlyFromLocalCache = true }).Length);

			var lineTaxableTransactionMatchingMock = CreateITaxableTransactionLineMock(Factory, ZGuid.NewZGuid(), "ARS", new ZDate(2019, 10, 31));
			caclualtedTaxRecord = TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.CalculateTaxRecord(taxRecords, lineTaxableTransactionMatchingMock.Object, taxId, taxConfiguration, taxRate, taxParent.Object, taxMessage.PK, "", "");
			AssertEquals("Factory for AccTaxTransaction should contain 2 records", 2, Factory.Load<AccTaxTransaction>(new ZQuery { FetchOnlyFromLocalCache = true }).Length);
			AssertEquals("taxRecords should contain 2 records", 2, taxRecords.Count);
			AssertCollectionContains(taxRecord2, taxRecords);
			var pivotMatching = GetPivot(lineTaxableTransactionMatchingMock.Object.PK, taxRecord2.PK);
			AssertNotNull("Pivot should be created", pivotMatching);
			AssertEquals(nameof(caclualtedTaxRecord), (taxRecord2, pivotMatching), caclualtedTaxRecord);
			AssertEquals("Factory for AccTaxRecordTransactionLinePivot should contain 2 records", 2, Factory.Load<AccTaxRecordTransactionLinePivot>(new ZQuery { FetchOnlyFromLocalCache = true }).Length);

			var lineTaxableTransactionNotMatchingDefaultMock = CreateITaxableTransactionLineMock(Factory, ZGuid.NewZGuid(), "ARS", new ZDate(2019, 10, 31));
			caclualtedTaxRecord = TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.CalculateTaxRecord(taxRecords, lineTaxableTransactionNotMatchingDefaultMock.Object, taxId, taxConfiguration, null, taxParent.Object, taxMessage.PK, "", "");
			var taxRecord3 = GetTaxRecord(taxId.PK, taxConfiguration.PK, (0, 1), taxMessage.PK, null);
			AssertNotNull("Tax Transaction should be created", taxRecord3);
			AssertNotEquals(taxRecord3.PK, taxRecord1.PK);
			AssertNotEquals(taxRecord3.PK, taxRecord2.PK);
			var pivotNotMatchingDefault = GetPivot(lineTaxableTransactionNotMatchingDefaultMock.Object.PK, taxRecord3.PK);
			AssertNotNull("Pivot should be created", pivotNotMatchingDefault);
			AssertEquals(nameof(caclualtedTaxRecord), (taxRecord3, pivotNotMatchingDefault), caclualtedTaxRecord);
			AssertEquals($"Factory for AccTaxTransaction should contain 3 records", 3, Factory.Load<AccTaxTransaction>(new ZQuery { FetchOnlyFromLocalCache = true }).Length);
			AssertEquals($"taxRecords should contain 3 record", 3, taxRecords.Count);
			AssertCollectionContains(taxRecord3, taxRecords);
			AssertEquals($"Factory for AccTaxRecordTransactionLinePivot should contain 3 record", 3, Factory.Load<AccTaxRecordTransactionLinePivot>(new ZQuery { FetchOnlyFromLocalCache = true }).Length);

			var lineTaxableTransactionMatchingDefaultMock = CreateITaxableTransactionLineMock(Factory, ZGuid.NewZGuid(), "ARS", new ZDate(2019, 10, 31));
			caclualtedTaxRecord = TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.CalculateTaxRecord(taxRecords, lineTaxableTransactionMatchingDefaultMock.Object, taxId, taxConfiguration, null, taxParent.Object, taxMessage.PK, "", "");
			AssertEquals($"Factory for AccTaxTransaction should contain 3 records", 3, Factory.Load<AccTaxTransaction>(new ZQuery { FetchOnlyFromLocalCache = true }).Length);
			AssertEquals($"taxRecords should contain 3 records", 3, taxRecords.Count);
			AssertCollectionContains(taxRecord3, taxRecords);
			var pivotMatchingDefault = GetPivot(lineTaxableTransactionMatchingDefaultMock.Object.PK, taxRecord3.PK);
			AssertNotNull("Pivot should be created", pivotMatchingDefault);
			AssertEquals(nameof(caclualtedTaxRecord), (taxRecord3, pivotMatchingDefault), caclualtedTaxRecord);
			AssertEquals($"Factory for AccTaxRecordTransactionLinePivot should contain 4 record", 4, Factory.Load<AccTaxRecordTransactionLinePivot>(new ZQuery { FetchOnlyFromLocalCache = true }).Length);
		}

		public void TestCalculateTaxRecord_TaxRecords_MatchingByDenominator()
		{
			var taxParent = CreateITaxRecordParentMock(Factory);
			var taxSystemsConfigCollection = new TaxSystemsConfigurationCollection();
			var taxSystem = TaxFrameworkTestObjectCreator.CreateTaxSystem("PIB");
			taxSystemsConfigCollection.Add(taxSystem);
			AccountingMasterFilesRegistry.Instance.TaxSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taxSystemsConfigCollection);
			taxSystemsConfigCollection.ClearCachedTaxSystemsByCode_ForTestOnly(Factory);
			AccTaxRate taxId = Factory.NewWithValidTestData<AccTaxRate>();
			AccTaxConfiguration taxConfiguration = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(GlbCompany.CurrentCompany, null, taxSystem);
			taxConfiguration.ETC_TaxSystemCode = taxSystem.Code;
			AccTransactionHeader transactionHeader = Factory.NewWithValidTestData<AccTransactionHeader>();
			(ZInt, ZInt) taxRate = (1, 1);
			AccInvMsg taxMessage = Factory.NewWithValidTestData<AccInvMsg>();
			List<AccTaxTransaction> taxRecords = new List<AccTaxTransaction>();
			var taxRecord1 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { TaxConfiguration = taxConfiguration, TransactionHeader = transactionHeader, TaxId = taxId, TaxRate = (1, 2) });
			taxRecords.Add(taxRecord1);

			var lineTaxableTransactionNotMatchingMock = CreateITaxableTransactionLineMock(Factory, ZGuid.NewZGuid(), "ARS", new ZDate(2019, 10, 31));
			var calculatedTaxRecord = TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.CalculateTaxRecord(taxRecords, lineTaxableTransactionNotMatchingMock.Object, taxId, taxConfiguration, taxRate, taxParent.Object, taxMessage.PK, "", "");
			var taxRecord2 = GetTaxRecord(taxId.PK, taxConfiguration.PK, taxRate, taxMessage.PK, null);
			AssertNotNull("Tax Transaction should be created", taxRecord2);
			var pivotNotMatching = GetPivot(lineTaxableTransactionNotMatchingMock.Object.PK, taxRecord2.PK);
			AssertNotNull("Pivot should be created", pivotNotMatching);
			AssertEquals(nameof(calculatedTaxRecord), (taxRecord2, pivotNotMatching), calculatedTaxRecord);
			AssertEquals("Factory for AccTaxTransaction should contain 2 records", 2, Factory.Load<AccTaxTransaction>(new ZQuery { FetchOnlyFromLocalCache = true }).Length);
			AssertEquals("taxRecords should contain 2 records", 2, taxRecords.Count);
			AssertCollectionContains(taxRecord2, taxRecords);
			AssertEquals("Factory for AccTaxRecordTransactionLinePivot should contain 1 record", 1, Factory.Load<AccTaxRecordTransactionLinePivot>(new ZQuery { FetchOnlyFromLocalCache = true }).Length);

			var lineTaxableTransactionMatchingMock = CreateITaxableTransactionLineMock(Factory, ZGuid.NewZGuid(), "ARS", new ZDate(2019, 10, 31));
			calculatedTaxRecord = TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.CalculateTaxRecord(taxRecords, lineTaxableTransactionMatchingMock.Object, taxId, taxConfiguration, taxRate, taxParent.Object, taxMessage.PK, "", "");
			AssertNotNull("Tax Transaction should be created", taxRecord2);
			AssertEquals("Factory for AccTaxTransaction should contain 2 records", 2, Factory.Load<AccTaxTransaction>(new ZQuery { FetchOnlyFromLocalCache = true }).Length);
			AssertEquals("taxRecords should contain 2 records", 2, taxRecords.Count);
			AssertCollectionContains(taxRecord2, taxRecords);
			var pivotMatching = GetPivot(lineTaxableTransactionMatchingMock.Object.PK, taxRecord2.PK);
			AssertNotNull("Pivot should be created", pivotMatching);
			AssertEquals(nameof(calculatedTaxRecord), (taxRecord2, pivotMatching), calculatedTaxRecord);
			AssertEquals("Factory for AccTaxRecordTransactionLinePivot should contain 2 records", 2, Factory.Load<AccTaxRecordTransactionLinePivot>(new ZQuery { FetchOnlyFromLocalCache = true }).Length);

			var lineTaxableTransactionNotMatchingDefaultMock = CreateITaxableTransactionLineMock(Factory, ZGuid.NewZGuid(), "ARS", new ZDate(2019, 10, 31));
			calculatedTaxRecord = TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.CalculateTaxRecord(taxRecords, lineTaxableTransactionNotMatchingDefaultMock.Object, taxId, taxConfiguration, null, taxParent.Object, taxMessage.PK, "", "");
			var taxRecord3 = GetTaxRecord(taxId.PK, taxConfiguration.PK, (0, 1), taxMessage.PK, null);
			AssertNotNull("Tax Transaction should be created", taxRecord3);
			var pivotNotMatchingDefault = GetPivot(lineTaxableTransactionNotMatchingDefaultMock.Object.PK, taxRecord3.PK);
			AssertNotNull("Pivot should be created", pivotNotMatchingDefault);
			AssertNotEquals(taxRecord3.PK, taxRecord1.PK);
			AssertNotEquals(taxRecord3.PK, taxRecord2.PK);
			AssertEquals("Factory for AccTaxTransaction should contain 3 records", 3, Factory.Load<AccTaxTransaction>(new ZQuery { FetchOnlyFromLocalCache = true }).Length);
			AssertEquals("taxRecords should contain 3 record", 3, taxRecords.Count);
			AssertCollectionContains(taxRecord3, taxRecords);
			AssertEquals(nameof(calculatedTaxRecord), (taxRecord3, pivotNotMatchingDefault), calculatedTaxRecord);
			AssertEquals("Factory for AccTaxRecordTransactionLinePivot should contain 3 record", 3, Factory.Load<AccTaxRecordTransactionLinePivot>(new ZQuery { FetchOnlyFromLocalCache = true }).Length);

			var lineTaxableTransactionMatchingDefaultMock = CreateITaxableTransactionLineMock(Factory, ZGuid.NewZGuid(), "ARS", new ZDate(2019, 10, 31));
			calculatedTaxRecord = TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.CalculateTaxRecord(taxRecords, lineTaxableTransactionMatchingDefaultMock.Object, taxId, taxConfiguration, null, taxParent.Object, taxMessage.PK, "", "");
			AssertEquals("Factory for AccTaxTransaction should contain 3 records", 3, Factory.Load<AccTaxTransaction>(new ZQuery { FetchOnlyFromLocalCache = true }).Length);
			AssertEquals("taxRecords should contain 3 records", 3, taxRecords.Count);
			AssertCollectionContains(taxRecord3, taxRecords);
			var pivotMatchingDefault = GetPivot(lineTaxableTransactionMatchingDefaultMock.Object.PK, taxRecord3.PK);
			AssertNotNull("Pivot should be created", pivotMatchingDefault);
			AssertEquals(nameof(calculatedTaxRecord), (taxRecord3, pivotMatchingDefault), calculatedTaxRecord);
			AssertEquals("Factory for AccTaxRecordTransactionLinePivot should contain 4 record", 4, Factory.Load<AccTaxRecordTransactionLinePivot>(new ZQuery { FetchOnlyFromLocalCache = true }).Length);
		}

		public void TestCalculateTaxRecord_TaxRecords_MatchingByTaxMessage()
		{
			var taxRecords = new List<AccTaxTransaction>();
			var taxParent = CreateITaxRecordParentMock(Factory);
			var taxMessage1 = Factory.NewWithValidTestData<AccInvMsg>();
			var taxMessage2 = Factory.NewWithValidTestData<AccInvMsg>();
			var taxID = Factory.NewWithValidTestData<AccTaxRate>();
			var taxSystemsConfigCollection = new TaxSystemsConfigurationCollection();
			var taxSystem = TaxFrameworkTestObjectCreator.CreateTaxSystem("PIB");
			taxSystemsConfigCollection.Add(taxSystem);
			AccountingMasterFilesRegistry.Instance.TaxSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taxSystemsConfigCollection);
			taxSystemsConfigCollection.ClearCachedTaxSystemsByCode_ForTestOnly(Factory);
			var taxConfiguration = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(GlbCompany.CurrentCompany, null, taxSystem);
			taxConfiguration.ETC_TaxSystemCode = taxSystem.Code;
			var taxRate = (21, 100);
			var expectedServiceCode = "9133";

			var lineMock = CreateITaxableTransactionLineMock(Factory, ZGuid.NewZGuid(), "ARS", new ZDate(2019, 10, 31));
			var calculatedTaxRecord = TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.CalculateTaxRecord(taxRecords, lineMock.Object, taxID, taxConfiguration, taxRate, taxParent.Object, taxMessage1.PK, expectedServiceCode, "");
			var taxRecord1 = GetTaxRecord(taxID.PK, taxConfiguration.PK, taxRate, taxMessage1.PK, expectedServiceCode);
			AssertNotNull("Tax Transaction should be created", taxRecord1);
			var pivotNotMatching1 = GetPivot(lineMock.Object.PK, taxRecord1.PK);
			AssertNotNull("Pivot should be created", pivotNotMatching1);
			AssertEquals(nameof(calculatedTaxRecord), (taxRecord1, pivotNotMatching1), calculatedTaxRecord);
			AssertEquals("Factory for AccTaxTransaction should contain 1 record", 1, Factory.Load<AccTaxTransaction>(new ZQuery { FetchOnlyFromLocalCache = true }).Length);
			AssertContainsExactElementsInAnyOrder(new[] { taxRecord1 }, taxRecords);
			AssertEquals("Factory for AccTaxRecordTransactionLinePivot should contain 1 record", 1, Factory.Load<AccTaxRecordTransactionLinePivot>(new ZQuery { FetchOnlyFromLocalCache = true }).Length);

			calculatedTaxRecord = TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.CalculateTaxRecord(taxRecords, lineMock.Object, taxID, taxConfiguration, taxRate, taxParent.Object, taxMessage2.PK, expectedServiceCode, "");
			var taxRecord2 = GetTaxRecord(taxID.PK, taxConfiguration.PK, taxRate, taxMessage2.PK, expectedServiceCode);
			AssertNotNull("Tax Transaction should be created", taxRecord2);
			AssertEquals("Factory for AccTaxTransaction should contain 2 records", 2, Factory.Load<AccTaxTransaction>(new ZQuery { FetchOnlyFromLocalCache = true }).Length);
			AssertContainsExactElementsInAnyOrder(new[] { taxRecord1, taxRecord2 }, taxRecords);
			var pivotNotMatching2 = GetPivot(lineMock.Object.PK, taxRecord2.PK);
			AssertNotNull("Pivot should be created", pivotNotMatching2);
			AssertEquals(nameof(calculatedTaxRecord), (taxRecord2, pivotNotMatching2), calculatedTaxRecord);
			AssertEquals("Factory for AccTaxRecordTransactionLinePivot should contain 2 records", 2, Factory.Load<AccTaxRecordTransactionLinePivot>(new ZQuery { FetchOnlyFromLocalCache = true }).Length);

			calculatedTaxRecord = TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.CalculateTaxRecord(taxRecords, lineMock.Object, taxID, taxConfiguration, taxRate, taxParent.Object, taxMessage2.PK, expectedServiceCode, "");
			AssertEquals("Factory for AccTaxTransaction should contain 2 records", 2, Factory.Load<AccTaxTransaction>(new ZQuery { FetchOnlyFromLocalCache = true }).Length);
			AssertContainsExactElementsInAnyOrder("Tax Transaction should NOT be created", new[] { taxRecord1, taxRecord2 }, taxRecords);
			var pivotMatching = GetPivot(lineMock.Object.PK, taxRecord2.PK);
			AssertNotNull("Pivot should be created", pivotMatching);
			AssertEquals("Factory for AccTaxRecordTransactionLinePivot should contain 3 records", 3, Factory.Load<AccTaxRecordTransactionLinePivot>(new ZQuery { FetchOnlyFromLocalCache = true }).Length);
		}

		public void TestCalculateTaxRecord_TaxRecords_MatchingByTaxAuthorityServiceCode()
		{
			var taxRecords = new List<AccTaxTransaction>();
			var taxParent = CreateITaxRecordParentMock(Factory);
			var taxMessage = Factory.NewWithValidTestData<AccInvMsg>();
			var taxID = Factory.NewWithValidTestData<AccTaxRate>();
			var taxSystemsConfigCollection = new TaxSystemsConfigurationCollection();
			var taxSystem = TaxFrameworkTestObjectCreator.CreateTaxSystem("PIB");
			taxSystemsConfigCollection.Add(taxSystem);
			AccountingMasterFilesRegistry.Instance.TaxSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taxSystemsConfigCollection);
			taxSystemsConfigCollection.ClearCachedTaxSystemsByCode_ForTestOnly(Factory);
			var taxConfiguration = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(GlbCompany.CurrentCompany, null, taxSystem);
			taxConfiguration.ETC_TaxSystemCode = taxSystem.Code;
			var taxRate = (21, 100);
			var expectedServiceCode1 = "4456";
			var expectedServiceCode2 = "5544";

			var lineMock = CreateITaxableTransactionLineMock(Factory, ZGuid.NewZGuid(), "ARS", new ZDate(2019, 10, 31));
			var calculatedTaxRecord = TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.CalculateTaxRecord(taxRecords, lineMock.Object, taxID, taxConfiguration, taxRate, taxParent.Object, taxMessage.PK, expectedServiceCode1, "");
			var taxRecord1 = GetTaxRecord(taxID.PK, taxConfiguration.PK, taxRate, taxMessage.PK, expectedServiceCode1);
			AssertNotNull("Tax Transaction should be created", taxRecord1);
			var pivotNotMatching1 = GetPivot(lineMock.Object.PK, taxRecord1.PK);
			AssertNotNull("Pivot should be created", pivotNotMatching1);
			AssertEquals(nameof(calculatedTaxRecord), (taxRecord1, pivotNotMatching1), calculatedTaxRecord);
			AssertEquals("Factory for AccTaxTransaction should contain 1 record", 1, Factory.Load<AccTaxTransaction>(new ZQuery { FetchOnlyFromLocalCache = true }).Length);
			AssertContainsExactElementsInAnyOrder(new[] { taxRecord1 }, taxRecords);
			AssertEquals("Factory for AccTaxRecordTransactionLinePivot should contain 1 record", 1, Factory.Load<AccTaxRecordTransactionLinePivot>(new ZQuery { FetchOnlyFromLocalCache = true }).Length);

			calculatedTaxRecord = TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.CalculateTaxRecord(taxRecords, lineMock.Object, taxID, taxConfiguration, taxRate, taxParent.Object, taxMessage.PK, expectedServiceCode2, "");
			var taxRecord2 = GetTaxRecord(taxID.PK, taxConfiguration.PK, taxRate, taxMessage.PK, expectedServiceCode2);
			AssertNotNull("Tax Transaction should be created", taxRecord2);
			var pivotNotMatching2 = GetPivot(lineMock.Object.PK, taxRecord2.PK);
			AssertNotNull("Pivot should be created", pivotNotMatching2);
			AssertEquals(nameof(calculatedTaxRecord), (taxRecord2, pivotNotMatching2), calculatedTaxRecord);
			AssertEquals("Factory for AccTaxTransaction should contain 2 records", 2, Factory.Load<AccTaxTransaction>(new ZQuery { FetchOnlyFromLocalCache = true }).Length);
			AssertContainsExactElementsInAnyOrder(new[] { taxRecord1, taxRecord2 }, taxRecords);
			AssertEquals("Factory for AccTaxRecordTransactionLinePivot should contain 2 records", 2, Factory.Load<AccTaxRecordTransactionLinePivot>(new ZQuery { FetchOnlyFromLocalCache = true }).Length);

			calculatedTaxRecord = TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.CalculateTaxRecord(taxRecords, lineMock.Object, taxID, taxConfiguration, taxRate, taxParent.Object, taxMessage.PK, expectedServiceCode2, "");
			AssertEquals("Factory for AccTaxTransaction should contain 2 records", 2, Factory.Load<AccTaxTransaction>(new ZQuery { FetchOnlyFromLocalCache = true }).Length);
			AssertContainsExactElementsInAnyOrder("Tax Transaction should NOT be created", new[] { taxRecord1, taxRecord2 }, taxRecords);
			var pivotMatching = GetPivot(lineMock.Object.PK, taxRecord2.PK);
			AssertNotNull("Pivot should be created", pivotMatching);
			AssertEquals("Factory for AccTaxRecordTransactionLinePivot should contain 3 records", 3, Factory.Load<AccTaxRecordTransactionLinePivot>(new ZQuery { FetchOnlyFromLocalCache = true }).Length);

			calculatedTaxRecord = TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.CalculateTaxRecord(taxRecords, lineMock.Object, taxID, taxConfiguration, taxRate, taxParent.Object, taxMessage.PK, "", "");
			var taxRecord3 = GetTaxRecord(taxID.PK, taxConfiguration.PK, taxRate, taxMessage.PK, null);
			AssertNotNull("Tax Transaction should be created", taxRecord3);
			var pivotNotMatching3 = GetPivot(lineMock.Object.PK, taxRecord3.PK);
			AssertNotNull("Pivot should be created", pivotNotMatching3);
			AssertEquals(nameof(calculatedTaxRecord), (taxRecord3, pivotNotMatching3), calculatedTaxRecord);
			AssertEquals("Factory for AccTaxTransaction should contain 3 records", 3, Factory.Load<AccTaxTransaction>(new ZQuery { FetchOnlyFromLocalCache = true }).Length);
			AssertContainsExactElementsInAnyOrder(new[] { taxRecord1, taxRecord2, taxRecord3 }, taxRecords);
			AssertEquals("Factory for AccTaxRecordTransactionLinePivot should contain 4 records", 4, Factory.Load<AccTaxRecordTransactionLinePivot>(new ZQuery { FetchOnlyFromLocalCache = true }).Length);
		}

		public void TestCalculateTaxRecord_UnSupportedTaxSystem()
		{
			using (AccountingMasterFilesRegistry.Instance.TaxSystems.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new TaxSystemsConfigurationCollection())) // To achieve "No tax system setup."
			{
				List<AccTaxTransaction> taxRecords = new List<AccTaxTransaction>();
				AccTaxRate taxId = Factory.NewWithValidTestData<AccTaxRate>();
				AccTaxConfiguration taxConfiguration = Factory.NewWithValidTestData<AccTaxConfiguration>();
				taxConfiguration.ETC_TaxSystemCode = "ISS";

				var lineTaxableTransactionMock = new Mock<ITaxableTransactionLine>();
				lineTaxableTransactionMock.SetupGet(f => f.Factory).Returns(Factory);
				lineTaxableTransactionMock.SetupGet(f => f.PK).Returns(ZGuid.NewZGuid());

				AssertExceptionThrown("Tax System should not match because it was not set up", typeof(TaxFrameworkUnknownConfigurationValueException),
					"Tax System ISS can't be found.", () => TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.CalculateTaxRecord(taxRecords, lineTaxableTransactionMock.Object, taxId, taxConfiguration, null, null, ZGuid.Empty, "", ""));
				AssertEquals($"Factory for AccTaxTransaction should not contain records", 0, Factory.Load<AccTaxTransaction>(new ZQuery { FetchOnlyFromLocalCache = true }).Length);
				AssertEquals($"Factory for AccTaxRecordTransactionLinePivot should not contain records", 0, Factory.Load<AccTaxRecordTransactionLinePivot>(new ZQuery { FetchOnlyFromLocalCache = true }).Length);
			}
		}

		public void TestCalculateTaxRecord_BasisWhenCreationAndRealisationMTD()
		{
			var taxParent = CreateITaxRecordParentMock(Factory);
			var taxSystemsConfigCollection = new TaxSystemsConfigurationCollection();
			var taxSystem = TaxFrameworkTestObjectCreator.CreateTaxSystem("PIB");
			taxSystemsConfigCollection.Add(taxSystem);
			AccountingMasterFilesRegistry.Instance.TaxSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taxSystemsConfigCollection);
			taxSystemsConfigCollection.ClearCachedTaxSystemsByCode_ForTestOnly(Factory);
			AccTaxConfiguration taxConfiguration = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(GlbCompany.CurrentCompany, TaxFrameworkTestObjectCreator.CreateTaxAuthority("ATO"), taxSystem, TaxConfigurationLedgers.AccountsPayable.Code);
			taxConfiguration.ETC_TaxRecordCreationTrigger = TaxRecordCreationTrigger.MatchDate.Code;
			taxConfiguration.ETC_TaxRealisationMethod = TaxRealisationMethods.MatchDate.Code;

			var taxId = AccTaxRate.CreateTaxRate_ForTestOnly(Factory);
			var lineTaxableTransactionMock = CreateITaxableTransactionLineMock(Factory, ZGuid.NewZGuid(), "AUD", ZDate.Today);

			var taxRecords = new List<AccTaxTransaction>();
			var calculatedTaxRecord = TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.CalculateTaxRecord(taxRecords, lineTaxableTransactionMock.Object, taxId, taxConfiguration, (1, 1), taxParent.Object, ZGuid.Empty, "", "");
			AssertEquals("taxRecords Count", 1, taxRecords.Count);
			var taxRecord = taxRecords[0];
			var pivots = GetPivots(taxRecord.PK);
			AssertEquals(nameof(calculatedTaxRecord), (taxRecord, pivots[0]), calculatedTaxRecord);
			AssertEquals("ATT_Basis", TaxBasisList.Matching.Code, taxRecord.ATT_Basis);
		}

		public void TestCalculateTaxRecord_BasisWhenRealisationPTM()
		{
			var taxParent = CreateITaxRecordParentMock(Factory);
			var taxSystemsConfigCollection = new TaxSystemsConfigurationCollection();
			var taxSystem = TaxFrameworkTestObjectCreator.CreateTaxSystem("PIB");
			taxSystemsConfigCollection.Add(taxSystem);
			AccountingMasterFilesRegistry.Instance.TaxSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taxSystemsConfigCollection);
			taxSystemsConfigCollection.ClearCachedTaxSystemsByCode_ForTestOnly(Factory);
			var taxConfiguration = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(GlbCompany.CurrentCompany, TaxFrameworkTestObjectCreator.CreateTaxAuthority("ATO"), taxSystem, TaxConfigurationLedgers.AccountsPayable.Code);
			taxConfiguration.ETC_TaxRealisationMethod = TaxRealisationMethods.PostDateOfMatchTransaction.Code;

			var taxId = AccTaxRate.CreateTaxRate_ForTestOnly(Factory);
			var lineTaxableTransactionMock = CreateITaxableTransactionLineMock(Factory, ZGuid.NewZGuid(), "AUD", ZDate.Today);

			var taxRecords = new List<AccTaxTransaction>();
			var calculatedTaxRecord = TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.CalculateTaxRecord(taxRecords, lineTaxableTransactionMock.Object, taxId, taxConfiguration, (1, 1), taxParent.Object, ZGuid.Empty, "", "");
			AssertEquals("taxRecords Count", 1, taxRecords.Count);
			var taxRecord = taxRecords[0];
			var pivots = GetPivots(taxRecord.PK);
			AssertEquals(nameof(calculatedTaxRecord), (taxRecord, pivots[0]), calculatedTaxRecord);
			AssertEquals("ATT_Basis", TaxBasisList.PostingOnMatching.Code, taxRecord.ATT_Basis);
			AssertEquals("ATT_RealisationDate", ZDate.Empty, taxRecord.ATT_RealisationDate);
		}

		public void TestCalculateTaxRecord_BasisWhenRealisationIsInvalid()
		{
			var taxParent = CreateITaxRecordParentMock(Factory);
			var taxSystemsConfigCollection = new TaxSystemsConfigurationCollection();
			var taxSystem = TaxFrameworkTestObjectCreator.CreateTaxSystem("PIB");
			taxSystemsConfigCollection.Add(taxSystem);
			AccountingMasterFilesRegistry.Instance.TaxSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taxSystemsConfigCollection);
			taxSystemsConfigCollection.ClearCachedTaxSystemsByCode_ForTestOnly(Factory);
			AccTaxConfiguration taxConfiguration = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(GlbCompany.CurrentCompany, TaxFrameworkTestObjectCreator.CreateTaxAuthority("ATO"), taxSystem, TaxConfigurationLedgers.AccountsPayable.Code);
			taxConfiguration.ETC_TaxRecordCreationTrigger = TaxRecordCreationTrigger.PostDate.Code;
			taxConfiguration.ETC_TaxRealisationMethod = "XXX";

			var taxId = AccTaxRate.CreateTaxRate_ForTestOnly(Factory);
			var lineTaxableTransactionMock = CreateITaxableTransactionLineMock(Factory, ZGuid.NewZGuid(), "AUD", ZDate.Today);

			var taxRecords = new List<AccTaxTransaction>();
			AssertExceptionThrown<TaxFrameworkUnknownConfigurationValueException>("", "Tax configuration 'AU-ATO-PIB-AP' has not supported realization method 'XXX'.",
				() => TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.CalculateTaxRecord(taxRecords, lineTaxableTransactionMock.Object, taxId, taxConfiguration, (1, 1), taxParent.Object, ZGuid.Empty, "", ""));

			taxRecords = new List<AccTaxTransaction>();
			taxConfiguration.ETC_TaxRealisationMethod = TaxRealisationMethods.MatchDate.Code;
			var calculatedTaxRecord = TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.CalculateTaxRecord(taxRecords, lineTaxableTransactionMock.Object, taxId, taxConfiguration, (1, 1), taxParent.Object, ZGuid.Empty, "", "");
			AssertEquals("taxRecords Count", 1, taxRecords.Count);
			var taxRecord = taxRecords[0];
			var pivots = GetPivots(taxRecord.PK);
			AssertEquals(nameof(calculatedTaxRecord), (taxRecord, pivots[0]), calculatedTaxRecord);
			AssertEquals("ATT_Basis", TaxBasisList.Matching.Code, taxRecord.ATT_Basis);
		}

		public void TestCalculateTaxRecord_TaxSuperType()
		{
			var taxParent = CreateITaxRecordParentMock(Factory);
			var taxSystemsConfigCollection = new TaxSystemsConfigurationCollection();
			var taxSystem = TaxFrameworkTestObjectCreator.CreateTaxSystem("PIB");
			var expectedSuperType = TaxSuperTypeList.StandardPaymentRetention.Code;
			taxSystem.TaxSuperType = expectedSuperType;
			taxSystemsConfigCollection.Add(taxSystem);
			AccountingMasterFilesRegistry.Instance.TaxSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taxSystemsConfigCollection);
			taxSystemsConfigCollection.ClearCachedTaxSystemsByCode_ForTestOnly(Factory);
			var taxConfiguration = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(GlbCompany.CurrentCompany, TaxFrameworkTestObjectCreator.CreateTaxAuthority("ATO"), taxSystem, TaxConfigurationLedgers.AccountsPayable.Code);

			var taxId = AccTaxRate.CreateTaxRate_ForTestOnly(Factory);
			var lineTaxableTransactionMock = CreateITaxableTransactionLineMock(Factory, ZGuid.NewZGuid(), "AUD", ZDate.Today);

			var taxRecords = new List<AccTaxTransaction>();
			var caclualtedTaxRecord = TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.CalculateTaxRecord(taxRecords, lineTaxableTransactionMock.Object, taxId, taxConfiguration, (1, 1), taxParent.Object, ZGuid.Empty, "", "");
			AssertEquals("taxRecords Count", 1, taxRecords.Count);
			var taxRecord = taxRecords[0];
			var pivots = GetPivots(taxRecord.PK);
			AssertEquals(nameof(caclualtedTaxRecord), (taxRecord, pivots[0]), caclualtedTaxRecord);
			AssertEquals("ATT_TaxSuperType", expectedSuperType, taxRecord.ATT_TaxSuperType);
		}

		#endregion

		#region CalculateTaxRecordAmounts

		public void TestCalculateTaxRecordAmounts_PositiveSign()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1M, TestObjectCreator.AALSHI);
			var taxSystemsConfigCollection = new TaxSystemsConfigurationCollection();
			taxSystemsConfigCollection.Add(TaxFrameworkTestObjectCreator.CreateTaxSystem("PIB"));
			AccountingMasterFilesRegistry.Instance.TaxSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taxSystemsConfigCollection);
			taxSystemsConfigCollection.ClearCachedTaxSystemsByCode_ForTestOnly(Factory);

			var taxRecord = Factory.NewWithValidTestData<AccTaxTransaction>();
			taxRecord.ATT_AH = invoice.PK;
			taxRecord.ATT_TaxSystemCode = "PIB";
			taxRecord.ATT_RateDenominator = 1;
			taxRecord.ATT_RateNumerator = 21;
			ChangeOSCurrencyButRetainAmounts(taxRecord, CurrencyCodes.UnitedStates);

			var lineTaxableTransactionMock1 = new Mock<ITaxableTransactionLine>();
			lineTaxableTransactionMock1.SetupGet(f => f.Factory).Returns(Factory);
			lineTaxableTransactionMock1.SetupGet(f => f.PK).Returns(ZGuid.NewZGuid());
			lineTaxableTransactionMock1.SetupGet(f => f.BaseOSAmount).Returns(100m);
			lineTaxableTransactionMock1.SetupGet(f => f.LocalAmount).Returns(200m);

			var lineTaxableTransactionMock2 = new Mock<ITaxableTransactionLine>();
			lineTaxableTransactionMock2.SetupGet(f => f.Factory).Returns(Factory);
			lineTaxableTransactionMock2.SetupGet(f => f.PK).Returns(ZGuid.NewZGuid());
			lineTaxableTransactionMock2.SetupGet(f => f.BaseOSAmount).Returns(300m);
			lineTaxableTransactionMock2.SetupGet(f => f.LocalAmount).Returns(400m);

			var accTaxRecordTransactionLinePivot1 = Factory.New<AccTaxRecordTransactionLinePivot>();
			accTaxRecordTransactionLinePivot1.ATP_ATT = taxRecord.PK;
			accTaxRecordTransactionLinePivot1.LinkLine(lineTaxableTransactionMock1.Object);

			var accTaxRecordTransactionLinePivot2 = Factory.New<AccTaxRecordTransactionLinePivot>();
			accTaxRecordTransactionLinePivot2.ATP_ATT = taxRecord.PK;
			accTaxRecordTransactionLinePivot2.LinkLine(lineTaxableTransactionMock2.Object);

			TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.CalculateTaxRecordAmounts(taxRecord, taxFrameworkConfigurationHelper);
			AssertEquals(100m, accTaxRecordTransactionLinePivot1.BaseOSAmount);
			AssertEquals(300m, accTaxRecordTransactionLinePivot2.BaseOSAmount);
			AssertEquals(200m, accTaxRecordTransactionLinePivot1.LocalTaxBaseAmount);
			AssertEquals(400m, accTaxRecordTransactionLinePivot2.LocalTaxBaseAmount);
			AssertEquals(400m, taxRecord.ATT_OSTaxBaseAmount);
			AssertEquals(600m, taxRecord.ATT_LocalTaxBaseAmount);
			AssertEquals(84m, taxRecord.ATT_OSTaxAmount);
			AssertEquals(126m, taxRecord.ATT_LocalTaxAmount);
			AssertEquals(0.21m, taxRecord.EffectiveRate);
		}

		public void TestCalculateTaxRecordAmounts_NegativeSign()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1M, TestObjectCreator.AALSHI);
			var taxSystemsConfigCollection = new TaxSystemsConfigurationCollection();
			taxSystemsConfigCollection.Add(TaxFrameworkTestObjectCreator.CreateTaxSystem("PIB"));
			AccountingMasterFilesRegistry.Instance.TaxSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taxSystemsConfigCollection);
			taxSystemsConfigCollection.ClearCachedTaxSystemsByCode_ForTestOnly(Factory);

			var taxRecord = Factory.NewWithValidTestData<AccTaxTransaction>();
			taxRecord.ATT_AH = invoice.PK;
			taxRecord.ATT_TaxSystemCode = "PIB";
			taxRecord.ATT_RateDenominator = 1;
			taxRecord.ATT_RateNumerator = 21;
			ChangeOSCurrencyButRetainAmounts(taxRecord, CurrencyCodes.UnitedStates);

			var lineTaxableTransactionMock1 = new Mock<ITaxableTransactionLine>();
			lineTaxableTransactionMock1.SetupGet(f => f.Factory).Returns(Factory);
			lineTaxableTransactionMock1.SetupGet(f => f.PK).Returns(ZGuid.NewZGuid());
			lineTaxableTransactionMock1.SetupGet(f => f.BaseOSAmount).Returns(100m);
			lineTaxableTransactionMock1.SetupGet(f => f.LocalAmount).Returns(200m);

			var lineTaxableTransactionMock2 = new Mock<ITaxableTransactionLine>();
			lineTaxableTransactionMock2.SetupGet(f => f.Factory).Returns(Factory);
			lineTaxableTransactionMock2.SetupGet(f => f.PK).Returns(ZGuid.NewZGuid());
			lineTaxableTransactionMock2.SetupGet(f => f.BaseOSAmount).Returns(300m);
			lineTaxableTransactionMock2.SetupGet(f => f.LocalAmount).Returns(400m);

			var accTaxRecordTransactionLinePivot1 = Factory.New<AccTaxRecordTransactionLinePivot>();
			accTaxRecordTransactionLinePivot1.ATP_ATT = taxRecord.PK;
			accTaxRecordTransactionLinePivot1.LinkLine(lineTaxableTransactionMock1.Object);

			var accTaxRecordTransactionLinePivot2 = Factory.New<AccTaxRecordTransactionLinePivot>();
			accTaxRecordTransactionLinePivot2.ATP_ATT = taxRecord.PK;
			accTaxRecordTransactionLinePivot2.LinkLine(lineTaxableTransactionMock2.Object);

			taxSystemsConfigCollection[0].AdjustmentSign = TaxCalculationAdjustmentSigns.Negative.Code;
			AccountingMasterFilesRegistry.Instance.TaxSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taxSystemsConfigCollection);
			taxSystemsConfigCollection.ClearCachedTaxSystemsByCode_ForTestOnly(Factory);
			TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.CalculateTaxRecordAmounts(taxRecord, taxFrameworkConfigurationHelper);
			AssertEquals(100m, accTaxRecordTransactionLinePivot1.BaseOSAmount);
			AssertEquals(300m, accTaxRecordTransactionLinePivot2.BaseOSAmount);
			AssertEquals(200m, accTaxRecordTransactionLinePivot1.LocalTaxBaseAmount);
			AssertEquals(400m, accTaxRecordTransactionLinePivot2.LocalTaxBaseAmount);
			AssertEquals(400m, taxRecord.ATT_OSTaxBaseAmount);
			AssertEquals(600m, taxRecord.ATT_LocalTaxBaseAmount);
			AssertEquals(-84m, taxRecord.ATT_OSTaxAmount);
			AssertEquals(-126m, taxRecord.ATT_LocalTaxAmount);
			AssertEquals(-0.21m, taxRecord.EffectiveRate);
		}

		public void TestCalculateTaxRecordAmounts_UnSupportedTaxSystem_TaxBaseCalculationMethod_TaxAmountCalculationMethod()
		{
			var taxSystemsConfigCollection = new TaxSystemsConfigurationCollection();
			var unSupportedBaseCalculationMethod = TaxFrameworkTestObjectCreator.CreateTaxSystem("FK1");
			unSupportedBaseCalculationMethod.TaxBaseCalculationMethod = "TBM";
			var unSupportedSystemAmountCalculationMethod = TaxFrameworkTestObjectCreator.CreateTaxSystem("FK2");
			unSupportedSystemAmountCalculationMethod.TaxAmountCalculationMethod = "TCM";
			taxSystemsConfigCollection.AddRange(unSupportedBaseCalculationMethod, unSupportedSystemAmountCalculationMethod);

			using (AccountingMasterFilesRegistry.Instance.TaxSystems.DataType.SuspendValidation())
			{
				AccountingMasterFilesRegistry.Instance.TaxSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taxSystemsConfigCollection);
			}
			taxSystemsConfigCollection.ClearCachedTaxSystemsByCode_ForTestOnly(Factory);

			var taxRecord = Factory.NewWithValidTestData<AccTaxTransaction>();
			taxRecord.ATT_TaxSystemCode = "FK1";

			var lineTaxableTransactionMock = new Mock<ITaxableTransactionLine>();
			lineTaxableTransactionMock.SetupGet(f => f.Factory).Returns(Factory);
			lineTaxableTransactionMock.SetupGet(f => f.PK).Returns(ZGuid.NewZGuid());

			var accTaxRecordTransactionLinePivot = Factory.NewWithValidTestData<AccTaxRecordTransactionLinePivot>();

			AssertExceptionThrown(typeof(TaxFrameworkUnknownConfigurationValueException), $"Tax Base Calculation Method TBM for the Tax System is not supported.", () => TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.CalculateTaxRecordAmounts(taxRecord, taxFrameworkConfigurationHelper));

			taxRecord.ATT_TaxSystemCode = "FK2";
			AssertExceptionThrown(typeof(TaxFrameworkUnknownConfigurationValueException), $"Tax Amount Calculation Method TCM for the Tax System is not supported.", () => TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.CalculateTaxRecordAmounts(taxRecord, taxFrameworkConfigurationHelper));
		}

		public void TestCalculateTaxRecordAmounts_NotExistTaxSystemConfiguration()
		{
			var taxSystemsConfigCollection = new TaxSystemsConfigurationCollection();
			var taxSystemConfiguration = TaxFrameworkTestObjectCreator.CreateTaxSystem("FK1");

			using (AccountingMasterFilesRegistry.Instance.TaxSystems.DataType.SuspendValidation())
			{
				AccountingMasterFilesRegistry.Instance.TaxSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taxSystemsConfigCollection);
			}
			taxSystemsConfigCollection.ClearCachedTaxSystemsByCode_ForTestOnly(Factory);

			var taxRecord = Factory.NewWithValidTestData<AccTaxTransaction>();
			taxRecord.ATT_TaxSystemCode = "FK2";

			var lineTaxableTransactionMock = new Mock<ITaxableTransactionLine>();
			lineTaxableTransactionMock.SetupGet(f => f.Factory).Returns(Factory);
			lineTaxableTransactionMock.SetupGet(f => f.PK).Returns(ZGuid.NewZGuid());

			AssertExceptionThrown(typeof(TaxFrameworkUnknownConfigurationValueException), "Tax System FK2 can't be found.", () => TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.CalculateTaxRecordAmounts(taxRecord, taxFrameworkConfigurationHelper));
		}

		public void TestCalculateTaxRecordAmounts_AssertExceptionThrown_TaxFrameworkUnknownConfigurationValueException_InExtensionMethod()
		{
			var company = TestObjectCreator.CreateNewCompany("CO1");

			var taxAuthority = TaxFrameworkTestObjectCreator.CreateTaxAuthority("BA");
			var taxSystem = TaxFrameworkTestObjectCreator.CreateTaxSystem("PIB");
			var taxConfig = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(company, taxAuthority, taxSystem, TaxConfigurationLedgers.AccountsReceivable.Code, true);

			var taxRecord = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { TaxConfiguration = taxConfig, TaxSystem = taxSystem });

			var lineTaxableTransactionMock = new Mock<ITaxableTransactionLine>();
			lineTaxableTransactionMock.SetupGet(f => f.Factory).Returns(Factory);
			lineTaxableTransactionMock.SetupGet(f => f.PK).Returns(ZGuid.NewZGuid());

			var dictionary = new Dictionary<ZGuid, ITaxableTransactionLine>();
			dictionary.Add(lineTaxableTransactionMock.Object.PK, lineTaxableTransactionMock.Object);

			AssertExceptionThrown(typeof(TaxFrameworkUnknownConfigurationValueException), "Tax System PIB can't be found.", () => TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.CalculateTaxRecordAmounts(taxRecord, taxFrameworkConfigurationHelper));
		}

		public void TestCalculateTaxRecordAmounts_AssertExceptionThrown_IncorrectAdjustmentSign()
		{
			var company = TestObjectCreator.CreateNewCompany("CO1");

			var taxAuthority = TaxFrameworkTestObjectCreator.CreateTaxAuthority("BA");
			var taxSystem = TaxFrameworkTestObjectCreator.CreateTaxSystem("PIB", "WRG", true);
			var taxConfig = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(company, taxAuthority, taxSystem, TaxConfigurationLedgers.AccountsReceivable.Code, true);

			Factory.Save();

			var taxSystemConfigurationCollection = new TaxSystemsConfigurationCollection();
			taxSystemConfigurationCollection.Add(taxSystem);

			using (AccountingMasterFilesRegistry.Instance.TaxSystems.DataType.SuspendValidation())
			{
				AccountingMasterFilesRegistry.Instance.TaxSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taxSystemConfigurationCollection);
			}
			taxSystemConfigurationCollection.ClearCachedTaxSystemsByCode_ForTestOnly(Factory);

			var taxRecord = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { TaxConfiguration = taxConfig, OsTaxBaseAmount = 1000, LocalTaxBaseAmount = 1000, OsTaxAmount = 100, LocalTaxAmount = 100, TaxSystem = taxSystem });

			var lineTaxableTransactionMock = new Mock<ITaxableTransactionLine>();
			lineTaxableTransactionMock.SetupGet(f => f.Factory).Returns(Factory);
			lineTaxableTransactionMock.SetupGet(f => f.PK).Returns(ZGuid.NewZGuid());

			AssertExceptionThrown(typeof(TaxFrameworkUnknownConfigurationValueException), "Tax System Adjustment Sign option WRG is not supported.", () => TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.CalculateTaxRecordAmounts(taxRecord, taxFrameworkConfigurationHelper));
		}

		public void TestCalculateTaxRecordAmounts_LocalCurrencyTaxRecord()
		{
			TaxFrameworkTestObjectCreator.SetupMinimumSettingsForTaxFramework(GlbCompany.CurrentCompany, TestObjectCreator.TestOrganisation, TestObjectCreator.NonAccrualChargeCode, TaxConfigurationLedgers.AccountsPayable.Code, isJobRelated: false);

			var taxRecord = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters());
			taxRecord.ATT_TaxSystemCode = "TS";
			taxRecord.ATT_RateDenominator = 4;
			taxRecord.ATT_RateNumerator = 20;

			var lineTaxableTransactionMock = new Mock<ITaxableTransactionLine>();
			lineTaxableTransactionMock.SetupGet(f => f.Factory).Returns(Factory);
			lineTaxableTransactionMock.SetupGet(f => f.PK).Returns(ZGuid.NewZGuid());
			lineTaxableTransactionMock.SetupGet(f => f.BaseOSAmount).Returns(100m);
			lineTaxableTransactionMock.SetupGet(f => f.LocalAmount).Returns(200m);

			var accTaxRecordTransactionLinePivot = Factory.New<AccTaxRecordTransactionLinePivot>();
			accTaxRecordTransactionLinePivot.ATP_ATT = taxRecord.PK;
			taxRecord.ATT_RX_NKOSTaxCurrency = CurrencyCodes.Australia;
			Assert(taxRecord.IsOSTaxCurrencyLocal);
			accTaxRecordTransactionLinePivot.LinkLine(lineTaxableTransactionMock.Object);

			TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.CalculateTaxRecordAmounts(taxRecord, taxFrameworkConfigurationHelper);
			AssertEquals(200m, accTaxRecordTransactionLinePivot.BaseOSAmount);
			AssertEquals(200m, accTaxRecordTransactionLinePivot.LocalTaxBaseAmount);
			AssertEquals(200m, taxRecord.ATT_OSTaxBaseAmount);
			AssertEquals(200m, taxRecord.ATT_LocalTaxBaseAmount);
			AssertEquals(10m, taxRecord.ATT_OSTaxAmount);
			AssertEquals(10m, taxRecord.ATT_LocalTaxAmount);
		}

		public void TestCalculateTaxRecordAmounts_ForeignCurrencyTaxRecord()
		{
			TaxFrameworkTestObjectCreator.SetupMinimumSettingsForTaxFramework(GlbCompany.CurrentCompany, TestObjectCreator.TestOrganisation, TestObjectCreator.NonAccrualChargeCode, TaxConfigurationLedgers.AccountsPayable.Code, isJobRelated: false);

			var taxRecord = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters());
			taxRecord.ATT_TaxSystemCode = "TS";
			taxRecord.ATT_RateDenominator = 4;
			taxRecord.ATT_RateNumerator = 20;

			var lineTaxableTransactionMock = new Mock<ITaxableTransactionLine>();
			lineTaxableTransactionMock.SetupGet(f => f.Factory).Returns(Factory);
			lineTaxableTransactionMock.SetupGet(f => f.PK).Returns(ZGuid.NewZGuid());
			lineTaxableTransactionMock.SetupGet(f => f.BaseOSAmount).Returns(100m);
			lineTaxableTransactionMock.SetupGet(f => f.LocalAmount).Returns(200m);

			var accTaxRecordTransactionLinePivot = Factory.New<AccTaxRecordTransactionLinePivot>();
			accTaxRecordTransactionLinePivot.ATP_ATT = taxRecord.PK;
			taxRecord.ATT_RX_NKOSTaxCurrency = CurrencyCodes.UnitedStates;
			Assert(!taxRecord.IsOSTaxCurrencyLocal);
			accTaxRecordTransactionLinePivot.LinkLine(lineTaxableTransactionMock.Object);

			TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.CalculateTaxRecordAmounts(taxRecord, taxFrameworkConfigurationHelper);
			AssertEquals(100m, accTaxRecordTransactionLinePivot.BaseOSAmount);
			AssertEquals(200m, accTaxRecordTransactionLinePivot.LocalTaxBaseAmount);
			AssertEquals(100m, taxRecord.ATT_OSTaxBaseAmount);
			AssertEquals(200m, taxRecord.ATT_LocalTaxBaseAmount);
			AssertEquals(5m, taxRecord.ATT_OSTaxAmount);
			AssertEquals(10m, taxRecord.ATT_LocalTaxAmount);
		}

		#endregion

		#region SetupTaxRecordsForLine

		public void TestSetupTaxRecordsForLine()
		{
			var taxSystemsConfigCollection = new TaxSystemsConfigurationCollection();
			var taxSystem1 = TaxFrameworkTestObjectCreator.CreateTaxSystem("PIB");
			var taxSystem2 = TaxFrameworkTestObjectCreator.CreateTaxSystem("RIB");
			var taxSystem3 = TaxFrameworkTestObjectCreator.CreateTaxSystem("CIB");
			taxSystemsConfigCollection.Add(taxSystem1);
			taxSystemsConfigCollection.Add(taxSystem2);
			taxSystemsConfigCollection.Add(taxSystem3);
			AccountingMasterFilesRegistry.Instance.TaxSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taxSystemsConfigCollection);
			taxSystemsConfigCollection.ClearCachedTaxSystemsByCode_ForTestOnly(Factory);
			var taxConfiguration1 = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(GlbCompany.CurrentCompany, null, taxSystem1);
			var taxConfiguration2 = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(GlbCompany.CurrentCompany, null, taxSystem2);
			var taxConfiguration3 = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(GlbCompany.CurrentCompany, null, taxSystem3);
			var taxMessage1 = Factory.NewWithValidTestData<AccInvMsg>();
			var taxMessage2 = Factory.NewWithValidTestData<AccInvMsg>();
			var taxMessage3 = Factory.NewWithValidTestData<AccInvMsg>();
			var taxID1 = TaxFrameworkTestObjectCreator.CreateTaxRate_RateSource(TaxRateSources.TaxIDOnly.Code);
			var taxID2 = TaxFrameworkTestObjectCreator.CreateTaxRate_RateSource(TaxRateSources.TaxIDOnly.Code);
			var taxID3 = TaxFrameworkTestObjectCreator.CreateTaxRate_RateSource(TaxRateSources.TaxIDOnly.Code);
			var taxRate1 = (2, 3);
			var taxRate2 = (5, 4);
			var taxRate3 = (7, 2);
			taxID1.SetRate_ForTestOnly(taxRate1.Item1, taxRate1.Item2);
			taxID2.SetRate_ForTestOnly(taxRate2.Item1, taxRate2.Item2);
			taxID3.SetRate_ForTestOnly(taxRate3.Item1, taxRate3.Item2);
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			var company2 = Factory.NewWithValidTestData<GlbCompany>();

			var taxRecordParentMock1 = CreateITaxRecordParentMock(Factory);
			taxRecordParentMock1.Setup(x => x.Company).Returns(company1);
			taxRecordParentMock1.Setup(x => x.Org).Returns(org1);
			var taxOverrideGroup1 = Factory.NewWithValidTestData<AccTaxOverrideGroup>();
			var chargeTaxOverride1 = Factory.NewWithValidTestData<AccChargeTaxOverride>();
			chargeTaxOverride1.AO_AT = taxID1.PK;
			chargeTaxOverride1.AO_ParentID = taxOverrideGroup1.PK;
			chargeTaxOverride1.AO_ParentTableCode = "AX";
			chargeTaxOverride1.AO_A9_DefaultVATClass = taxMessage1.PK;
			var taxOverrideGroup2 = Factory.NewWithValidTestData<AccTaxOverrideGroup>();
			var chargeTaxOverride2 = Factory.NewWithValidTestData<AccChargeTaxOverride>();
			chargeTaxOverride2.AO_AT = taxID2.PK;
			chargeTaxOverride2.AO_ParentID = taxOverrideGroup2.PK;
			chargeTaxOverride2.AO_ParentTableCode = "AX";
			chargeTaxOverride2.AO_A9_DefaultVATClass = taxMessage2.PK;
			List<AccTaxTransaction> taxRecords1 = new List<AccTaxTransaction>();
			var lineTaxableTransactionMock1 = CreateITaxableTransactionLineMock(Factory, ZGuid.NewZGuid(), "USD", new ZDate(2019, 10, 30));
			var taxGroupInfo1 = new TaxRecordCalculator.TaxOverrideGroupInfo("123", "", 0, 1, taxConfiguration1.PK, ZGuid.Empty, ZGuid.Empty);
			var taxGroupInfo2 = new TaxRecordCalculator.TaxOverrideGroupInfo("567", "", 0, 1, taxConfiguration2.PK, ZGuid.Empty, ZGuid.Empty);
			var dictionary = new Dictionary<ZGuid, TaxRecordCalculator.TaxOverrideGroupInfo[]>();
			dictionary.Add(taxOverrideGroup1.PK, new[] { taxGroupInfo1 });
			dictionary.Add(taxOverrideGroup2.PK, new[] { taxGroupInfo2 });
			var calculatedTaxRecords = TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.SetupTaxRecordsForLine(taxRecords1, lineTaxableTransactionMock1.Object, new[] { chargeTaxOverride1, chargeTaxOverride2 }, taxRecordParentMock1.Object, dictionary);
			AssertEquals("taxRecords1.Count", 2, taxRecords1.Count);
			var taxRecord1 = taxRecords1.FirstOrDefault(x => x.ATT_AT_TaxID == taxID1.PK);
			AssertTaxTransaction(taxRecord1, taxRecordParentMock1.Object, company1.PK, taxID1.PK, taxConfiguration1.PK, taxRate1, taxMessage1.PK, "123");
			var pivot1 = GetPivot(lineTaxableTransactionMock1.Object.PK, taxRecord1.PK);
			AssertEquals(lineTaxableTransactionMock1.Object.PK, pivot1.ATP_AL_TransactionLine);
			var taxRecord2 = taxRecords1.FirstOrDefault(x => x.ATT_AT_TaxID == taxID2.PK);
			AssertTaxTransaction(taxRecord2, taxRecordParentMock1.Object, company1.PK, taxID2.PK, taxConfiguration2.PK, taxRate2, taxMessage2.PK, "567");
			var pivot2 = GetPivot(lineTaxableTransactionMock1.Object.PK, taxRecord2.PK);
			AssertEquals(pivot2.ATP_AL_TransactionLine, lineTaxableTransactionMock1.Object.PK);
			var expectedValue = new List<(AccTaxTransaction, AccTaxRecordTransactionLinePivot)>();
			expectedValue.Add((taxRecord1, pivot1));
			expectedValue.Add((taxRecord2, pivot2));
			AssertContainsExactElementsInAnyOrder(nameof(calculatedTaxRecords), expectedValue, calculatedTaxRecords);

			List<AccTaxTransaction> taxRecords2 = new List<AccTaxTransaction>();
			var lineTaxableTransactionMock2 = CreateITaxableTransactionLineMock(Factory, ZGuid.NewZGuid(), "USD", new ZDate(2019, 10, 30));
			var taxOverrideGroup3 = Factory.NewWithValidTestData<AccTaxOverrideGroup>();
			var chargeTaxOverride3 = Factory.NewWithValidTestData<AccChargeTaxOverride>();
			chargeTaxOverride3.AO_AT = taxID3.PK;
			chargeTaxOverride3.AO_ParentID = taxOverrideGroup3.PK;
			chargeTaxOverride3.AO_ParentTableCode = "AX";
			chargeTaxOverride3.AO_A9_DefaultVATClass = taxMessage3.PK;
			var taxRecordParentMock2 = CreateITaxRecordParentMock(Factory);
			taxRecordParentMock2.Setup(x => x.Company).Returns(company2);
			taxRecordParentMock2.Setup(x => x.Org).Returns(org2);
			var taxGroupInfo3 = new TaxRecordCalculator.TaxOverrideGroupInfo("963", "", 0, 1, taxConfiguration3.PK, ZGuid.Empty, ZGuid.Empty);
			dictionary = new Dictionary<ZGuid, TaxRecordCalculator.TaxOverrideGroupInfo[]>();
			dictionary.Add(taxOverrideGroup3.PK, new[] { taxGroupInfo3 });
			calculatedTaxRecords = TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.SetupTaxRecordsForLine(taxRecords2, lineTaxableTransactionMock2.Object, new[] { chargeTaxOverride3 }, taxRecordParentMock2.Object, dictionary);
			AssertEquals("taxRecords2.Count", 1, taxRecords2.Count);
			var taxRecord3 = taxRecords2.FirstOrDefault(x => x.ATT_AT_TaxID == taxID3.PK);
			AssertTaxTransaction(taxRecord3, taxRecordParentMock2.Object, company2.PK, taxID3.PK, taxConfiguration3.PK, taxRate3, taxMessage3.PK, "963");
			var pivot3 = GetPivot(lineTaxableTransactionMock2.Object.PK, taxRecord3.PK);
			AssertEquals(pivot3.ATP_AL_TransactionLine, lineTaxableTransactionMock2.Object.PK);
			expectedValue = new List<(AccTaxTransaction, AccTaxRecordTransactionLinePivot)>();
			expectedValue.Add((taxRecord3, pivot3));
			AssertContainsExactElementsInAnyOrder(nameof(calculatedTaxRecords), expectedValue, calculatedTaxRecords);

			void AssertTaxTransaction(AccTaxTransaction accTaxTransaction, ITaxRecordParent taxRecordParent, ZGuid glbCompanyPK, ZGuid taxIDPK, ZGuid accTaxConfigurationPK, (ZInt, ZInt) taxRate, ZGuid taxMessagePK, ZString taxAuthorityServiceCode)
			{
				AssertEquals("ATT_AH", taxRecordParent.PK, accTaxTransaction.ATT_AH);
				AssertEquals("ATT_GC", glbCompanyPK, accTaxTransaction.ATT_GC);
				AssertEquals("ATT_AT_TaxID", taxIDPK, accTaxTransaction.ATT_AT_TaxID);
				AssertEquals("ATT_ETC", accTaxConfigurationPK, accTaxTransaction.ATT_ETC);
				AssertEquals("ATT_RateNumerator", taxRate.Item1, accTaxTransaction.ATT_RateNumerator);
				AssertEquals("ATT_RateDenominator", taxRate.Item2, accTaxTransaction.ATT_RateDenominator);
				AssertEquals("ATT_A9_TaxMessage", taxMessagePK, accTaxTransaction.ATT_A9_TaxMessage);
				AssertEquals("ATT_TaxAuthorityServiceCode", taxAuthorityServiceCode, accTaxTransaction.ATT_TaxAuthorityServiceCode);
			}
		}

		public void TestSetupTaxRecordsForLine_EmptyRecords()
		{
			var lineTaxableTransactionMock = CreateITaxableTransactionLineMock(Factory, ZGuid.NewZGuid(), "USD", new ZDate(2019, 10, 30));
			var taxRecordParentMock = CreateITaxRecordParentMock(Factory);
			var taxRecords = new List<AccTaxTransaction>();
			var emptyTaxOverride = new List<AccChargeTaxOverride>();
			var caclualtedTaxRecords = TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.SetupTaxRecordsForLine(taxRecords, lineTaxableTransactionMock.Object, emptyTaxOverride.ToArray(), taxRecordParentMock.Object, new Dictionary<ZGuid, TaxRecordCalculator.TaxOverrideGroupInfo[]>());
			AssertEquals("taxRecords.Count", 0, taxRecords.Count);
			AssertEquals("caclualtedTaxRecords.Count", 0, caclualtedTaxRecords.Count);
		}

		public void TestSetupTaxRecordsForLine_Exception_TaxGroup()
		{
			var taxRecordParentMock = CreateITaxRecordParentMock(Factory);

			var taxRecords = new List<AccTaxTransaction>();
			var lineTaxableTransactionMock = CreateITaxableTransactionLineMock(Factory, ZGuid.NewZGuid(), "USD", new ZDate(2019, 10, 30));
			var taxOverrideGroup = Factory.NewWithValidTestData<AccTaxOverrideGroup>();
			var chargeTaxOverride = Factory.NewWithValidTestData<AccChargeTaxOverride>();
			chargeTaxOverride.AO_AT = Factory.NewWithValidTestData<AccTaxRate>().PK;
			var taxSystemsConfigCollection = new TaxSystemsConfigurationCollection();
			var taxSystem = TaxFrameworkTestObjectCreator.CreateTaxSystem("PIB");
			taxSystemsConfigCollection.Add(taxSystem);
			AccountingMasterFilesRegistry.Instance.TaxSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taxSystemsConfigCollection);
			taxSystemsConfigCollection.ClearCachedTaxSystemsByCode_ForTestOnly(Factory);
			var taxConfiguration = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(GlbCompany.CurrentCompany, null, taxSystem);
			taxRecordParentMock.Setup(x => x.Company).Returns(Factory.NewWithValidTestData<GlbCompany>());

			var taxGroupInfo = new TaxRecordCalculator.TaxOverrideGroupInfo("", "", 0, 1, taxConfiguration.PK, ZGuid.Empty, ZGuid.Empty);
			var dictionary = new Dictionary<ZGuid, TaxRecordCalculator.TaxOverrideGroupInfo[]>();
			dictionary.Add(taxOverrideGroup.PK, new[] { taxGroupInfo });

			chargeTaxOverride.AO_ParentID = ZGuid.Empty;
			chargeTaxOverride.AO_ParentTableCode = ZString.Empty;
			AssertExceptionThrown(typeof(TaxFrameworkUnknownConfigurationValueException), "Tax Group is not found for a Tax Override.", () => TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.SetupTaxRecordsForLine(taxRecords, lineTaxableTransactionMock.Object, new[] { chargeTaxOverride }, taxRecordParentMock.Object, dictionary));

			chargeTaxOverride.AO_ParentID = taxOverrideGroup.PK;
			chargeTaxOverride.AO_ParentTableCode = "AX";
			AssertNoExceptionThrown(() => TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.SetupTaxRecordsForLine(taxRecords, lineTaxableTransactionMock.Object, new[] { chargeTaxOverride }, taxRecordParentMock.Object, dictionary));
		}

		public void TestSetupTaxRecordsForLine_Exception_TaxID()
		{
			var taxRecordParentMock = CreateITaxRecordParentMock(Factory);

			var taxRecords = new List<AccTaxTransaction>();
			var lineTaxableTransactionMock = CreateITaxableTransactionLineMock(Factory, ZGuid.NewZGuid(), "USD", new ZDate(2019, 10, 30));
			var taxOverrideGroup = Factory.NewWithValidTestData<AccTaxOverrideGroup>();
			taxOverrideGroup.AX_Code = "RRR";
			var chargeTaxOverride = Factory.NewWithValidTestData<AccChargeTaxOverride>();
			chargeTaxOverride.AO_ParentID = taxOverrideGroup.PK;
			chargeTaxOverride.AO_ParentTableCode = "AX";
			var taxSystemsConfigCollection = new TaxSystemsConfigurationCollection();
			var taxSystem = TaxFrameworkTestObjectCreator.CreateTaxSystem("PIB");
			taxSystemsConfigCollection.Add(taxSystem);
			AccountingMasterFilesRegistry.Instance.TaxSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taxSystemsConfigCollection);
			taxSystemsConfigCollection.ClearCachedTaxSystemsByCode_ForTestOnly(Factory);
			var taxConfiguration = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(GlbCompany.CurrentCompany, null, taxSystem);
			taxRecordParentMock.Setup(x => x.Company).Returns(Factory.NewWithValidTestData<GlbCompany>());

			var taxGroupInfo = new TaxRecordCalculator.TaxOverrideGroupInfo("", "", 0, 1, taxConfiguration.PK, ZGuid.Empty, ZGuid.Empty);
			var dictionary = new Dictionary<ZGuid, TaxRecordCalculator.TaxOverrideGroupInfo[]>();
			dictionary.Add(taxOverrideGroup.PK, new[] { taxGroupInfo });

			chargeTaxOverride.AO_AT = ZGuid.Empty;
			AssertExceptionThrown(typeof(TaxFrameworkUnknownConfigurationValueException), "Tax ID is not found for a Tax Override from Tax Group with code RRR.", () => TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.SetupTaxRecordsForLine(taxRecords, lineTaxableTransactionMock.Object, new[] { chargeTaxOverride }, taxRecordParentMock.Object, dictionary));

			chargeTaxOverride.AO_AT = Factory.NewWithValidTestData<AccTaxRate>().PK;
			AssertNoExceptionThrown(() => TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.SetupTaxRecordsForLine(taxRecords, lineTaxableTransactionMock.Object, new[] { chargeTaxOverride }, taxRecordParentMock.Object, dictionary));
		}

		public void TestSetupTaxRecordsForLine_Exception_TaxConfiguration()
		{
			var taxRecordParentMock = CreateITaxRecordParentMock(Factory);

			var taxRecords = new List<AccTaxTransaction>();
			var lineTaxableTransactionMock = CreateITaxableTransactionLineMock(Factory, ZGuid.NewZGuid(), "USD", new ZDate(2019, 10, 30));
			var taxOverrideGroup = Factory.NewWithValidTestData<AccTaxOverrideGroup>();
			taxOverrideGroup.AX_Code = "RRR";
			var chargeTaxOverride = Factory.NewWithValidTestData<AccChargeTaxOverride>();
			chargeTaxOverride.AO_ParentID = taxOverrideGroup.PK;
			chargeTaxOverride.AO_ParentTableCode = "AX";
			chargeTaxOverride.AO_AT = Factory.NewWithValidTestData<AccTaxRate>().PK;
			var taxSystemsConfigCollection = new TaxSystemsConfigurationCollection();
			var taxSystem = TaxFrameworkTestObjectCreator.CreateTaxSystem("PIB");
			taxSystemsConfigCollection.Add(taxSystem);
			AccountingMasterFilesRegistry.Instance.TaxSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taxSystemsConfigCollection);
			taxSystemsConfigCollection.ClearCachedTaxSystemsByCode_ForTestOnly(Factory);
			var taxConfiguration = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(GlbCompany.CurrentCompany, null, taxSystem);
			taxRecordParentMock.Setup(x => x.Company).Returns(Factory.NewWithValidTestData<GlbCompany>());

			var taxGroupInfo = new TaxRecordCalculator.TaxOverrideGroupInfo("", "", 0, 1, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty);
			var dictionary = new Dictionary<ZGuid, TaxRecordCalculator.TaxOverrideGroupInfo[]>();
			dictionary.Add(taxOverrideGroup.PK, new[] { taxGroupInfo });
			AssertExceptionThrown(typeof(TaxFrameworkUnknownConfigurationValueException), "Tax Configuration is not found for a Tax Group with code RRR.", () => TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.SetupTaxRecordsForLine(taxRecords, lineTaxableTransactionMock.Object, new[] { chargeTaxOverride }, taxRecordParentMock.Object, dictionary));

			taxGroupInfo = new TaxRecordCalculator.TaxOverrideGroupInfo("", "", 0, 1, taxConfiguration.PK, ZGuid.Empty, ZGuid.Empty);
			dictionary = new Dictionary<ZGuid, TaxRecordCalculator.TaxOverrideGroupInfo[]>();
			dictionary.Add(taxOverrideGroup.PK, new[] { taxGroupInfo });
			AssertNoExceptionThrown(() => TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.SetupTaxRecordsForLine(taxRecords, lineTaxableTransactionMock.Object, new[] { chargeTaxOverride }, taxRecordParentMock.Object, dictionary));
		}

		public void TestSetupTaxRecordsForLine_Exception_Company()
		{
			var taxRecordParentMock = CreateITaxRecordParentMock(Factory);

			var taxRecords = new List<AccTaxTransaction>();
			var lineTaxableTransactionMock = CreateITaxableTransactionLineMock(Factory, ZGuid.NewZGuid(), "USD", new ZDate(2019, 10, 30));
			var taxOverrideGroup = Factory.NewWithValidTestData<AccTaxOverrideGroup>();
			taxOverrideGroup.AX_Code = "RRR";
			var chargeTaxOverride = Factory.NewWithValidTestData<AccChargeTaxOverride>();
			chargeTaxOverride.AO_ParentID = taxOverrideGroup.PK;
			chargeTaxOverride.AO_ParentTableCode = "AX";
			chargeTaxOverride.AO_AT = Factory.NewWithValidTestData<AccTaxRate>().PK;
			var taxSystemsConfigCollection = new TaxSystemsConfigurationCollection();
			var taxSystem = TaxFrameworkTestObjectCreator.CreateTaxSystem("PIB");
			taxSystemsConfigCollection.Add(taxSystem);
			AccountingMasterFilesRegistry.Instance.TaxSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taxSystemsConfigCollection);
			taxSystemsConfigCollection.ClearCachedTaxSystemsByCode_ForTestOnly(Factory);
			var taxConfiguration = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(GlbCompany.CurrentCompany, null, taxSystem);
			taxRecordParentMock.Setup(x => x.Company).Returns(Factory.NewWithValidTestData<GlbCompany>());

			var taxGroupInfo = new TaxRecordCalculator.TaxOverrideGroupInfo("", "", 0, 1, taxConfiguration.PK, ZGuid.Empty, ZGuid.Empty);
			var dictionary = new Dictionary<ZGuid, TaxRecordCalculator.TaxOverrideGroupInfo[]>();
			dictionary.Add(taxOverrideGroup.PK, new[] { taxGroupInfo });

			taxRecordParentMock.Setup(x => x.Company).Returns((GlbCompany)null);
			AssertExceptionThrown(typeof(TaxFrameworkUnknownConfigurationValueException), "Transaction company is not found.", () => TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.SetupTaxRecordsForLine(taxRecords, lineTaxableTransactionMock.Object, new[] { chargeTaxOverride }, taxRecordParentMock.Object, dictionary));

			taxRecordParentMock.Setup(x => x.Company).Returns(Factory.NewWithValidTestData<GlbCompany>());
			AssertNoExceptionThrown(() => TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.SetupTaxRecordsForLine(taxRecords, lineTaxableTransactionMock.Object, new[] { chargeTaxOverride }, taxRecordParentMock.Object, dictionary));
		}

		public void TestSetupTaxRecordsForLine_Exception_TaxRecordsExistsForLine()
		{
			var taxRecordParentMock = CreateITaxRecordParentMock(Factory);

			var taxRecords = new List<AccTaxTransaction>();
			var lineTaxableTransactionMock = CreateITaxableTransactionLineMock(Factory, ZGuid.NewZGuid(), "USD", new ZDate(2019, 10, 30), chargeCode: TestObjectCreator.CC1);

			var taxSystemsConfigCollection = new TaxSystemsConfigurationCollection();
			var taxAuthority = TaxFrameworkTestObjectCreator.CreateTaxAuthority("TA");
			var taxSystem = TaxFrameworkTestObjectCreator.CreateTaxSystem("TS");
			taxSystemsConfigCollection.Add(taxSystem);
			AccountingMasterFilesRegistry.Instance.TaxSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taxSystemsConfigCollection);
			taxSystemsConfigCollection.ClearCachedTaxSystemsByCode_ForTestOnly(Factory);
			var taxConfiguration = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(GlbCompany.CurrentCompany, taxAuthority, taxSystem);
			taxConfiguration.ETC_Code = "TAXCFG1";
			var taxID = TaxFrameworkTestObjectCreator.CreateTaxRate_RateSource(TaxRateSources.TaxGroupOnly.Code);
			var taxRate1 = ((ZInt)3, (ZInt)1);
			var taxRate2 = ((ZInt)5, (ZInt)10);
			taxID.SetRate_ForTestOnly(taxRate1.Item1, taxRate1.Item2);

			var taxOverrideGroup1 = Factory.NewWithValidTestData<AccTaxOverrideGroup>();
			var taxOverrideGroup2 = Factory.NewWithValidTestData<AccTaxOverrideGroup>();

			taxOverrideGroup1.AX_Code = "TG1";
			taxOverrideGroup2.AX_Code = "TG2";

			var chargeTaxOverride1 = Factory.NewWithValidTestData<AccChargeTaxOverride>();
			var chargeTaxOverride2 = Factory.NewWithValidTestData<AccChargeTaxOverride>();

			chargeTaxOverride1.AO_ParentID = taxOverrideGroup1.PK;
			chargeTaxOverride2.AO_ParentID = taxOverrideGroup2.PK;
			chargeTaxOverride1.AO_ParentTableCode = "AX";
			chargeTaxOverride2.AO_ParentTableCode = "AX";
			chargeTaxOverride1.AO_A9_DefaultVATClass = chargeTaxOverride2.AO_A9_DefaultVATClass = Factory.NewWithValidTestData<AccInvMsg>().PK;
			chargeTaxOverride1.AO_AT = chargeTaxOverride2.AO_AT = taxID.PK;

			taxRecordParentMock.Setup(x => x.Company).Returns(Factory.NewWithValidTestData<GlbCompany>());
			var exceptionMessage = "Please review the Tax Override Group defaulting rules for Tax configuration 'TAXCFG1' and Charge Code 'ZZCC1'. Taxes cannot be calculated correctly because duplicate tax defaulting rules exist for Charge Code 'ZZCC1' across more than one Tax Override Group rule set.";

			var taxGroupInfo = new TaxRecordCalculator.TaxOverrideGroupInfo("1111", "Test Description", taxRate1.Item1, taxRate1.Item2, taxConfiguration.PK, ZGuid.Empty, ZGuid.Empty);
			var dictionary = new Dictionary<ZGuid, TaxRecordCalculator.TaxOverrideGroupInfo[]>();
			dictionary.Add(taxOverrideGroup1.PK, new[] { taxGroupInfo });
			dictionary.Add(taxOverrideGroup2.PK, new[] { taxGroupInfo });

			AssertExceptionThrown(typeof(TaxFrameworkConfigurationValueException), exceptionMessage, () => TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.SetupTaxRecordsForLine(taxRecords, lineTaxableTransactionMock.Object, new[] { chargeTaxOverride1, chargeTaxOverride2 }, taxRecordParentMock.Object, dictionary));

			chargeTaxOverride2.AO_AT = Factory.NewWithValidTestData<AccTaxRate>().PK;
			AssertNoExceptionThrown(() => TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.SetupTaxRecordsForLine(taxRecords, lineTaxableTransactionMock.Object, new[] { chargeTaxOverride1, chargeTaxOverride2 }, taxRecordParentMock.Object, dictionary));

			chargeTaxOverride2.AO_AT = chargeTaxOverride1.AO_AT;
			AssertExceptionThrown(typeof(TaxFrameworkConfigurationValueException), exceptionMessage, () => TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.SetupTaxRecordsForLine(taxRecords, lineTaxableTransactionMock.Object, new[] { chargeTaxOverride1, chargeTaxOverride2 }, taxRecordParentMock.Object, dictionary));

			chargeTaxOverride2.AO_A9_DefaultVATClass = Factory.NewWithValidTestData<AccInvMsg>().PK;
			AssertNoExceptionThrown(() => TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.SetupTaxRecordsForLine(taxRecords, lineTaxableTransactionMock.Object, new[] { chargeTaxOverride1, chargeTaxOverride2 }, taxRecordParentMock.Object, dictionary));

			chargeTaxOverride2.AO_A9_DefaultVATClass = chargeTaxOverride1.AO_A9_DefaultVATClass;
			AssertExceptionThrown(typeof(TaxFrameworkConfigurationValueException), exceptionMessage, () => TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.SetupTaxRecordsForLine(taxRecords, lineTaxableTransactionMock.Object, new[] { chargeTaxOverride1, chargeTaxOverride2 }, taxRecordParentMock.Object, dictionary));

			var taxGroupInfo1 = new TaxRecordCalculator.TaxOverrideGroupInfo("1111", "Test Description", taxRate2.Item1, taxRate1.Item2, taxConfiguration.PK, ZGuid.Empty, ZGuid.Empty);
			var taxGroupInfo2 = new TaxRecordCalculator.TaxOverrideGroupInfo("1111", "Test Description", taxRate1.Item1, taxRate1.Item2, taxConfiguration.PK, ZGuid.Empty, ZGuid.Empty);
			dictionary = new Dictionary<ZGuid, TaxRecordCalculator.TaxOverrideGroupInfo[]>();
			dictionary.Add(taxOverrideGroup1.PK, new[] { taxGroupInfo1 });
			dictionary.Add(taxOverrideGroup2.PK, new[] { taxGroupInfo2 });
			AssertNoExceptionThrown(() => TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.SetupTaxRecordsForLine(taxRecords, lineTaxableTransactionMock.Object, new[] { chargeTaxOverride1, chargeTaxOverride2 }, taxRecordParentMock.Object, dictionary));

			dictionary = new Dictionary<ZGuid, TaxRecordCalculator.TaxOverrideGroupInfo[]>();
			dictionary.Add(taxOverrideGroup1.PK, new[] { taxGroupInfo });
			dictionary.Add(taxOverrideGroup2.PK, new[] { taxGroupInfo });
			AssertExceptionThrown(typeof(TaxFrameworkConfigurationValueException), exceptionMessage, () => TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.SetupTaxRecordsForLine(taxRecords, lineTaxableTransactionMock.Object, new[] { chargeTaxOverride1, chargeTaxOverride2 }, taxRecordParentMock.Object, dictionary));

			taxGroupInfo1 = new TaxRecordCalculator.TaxOverrideGroupInfo("1111", "Test Description", taxRate1.Item1, taxRate2.Item2, taxConfiguration.PK, ZGuid.Empty, ZGuid.Empty);
			taxGroupInfo2 = new TaxRecordCalculator.TaxOverrideGroupInfo("1111", "Test Description", taxRate1.Item1, taxRate1.Item2, taxConfiguration.PK, ZGuid.Empty, ZGuid.Empty);
			dictionary = new Dictionary<ZGuid, TaxRecordCalculator.TaxOverrideGroupInfo[]>();
			dictionary.Add(taxOverrideGroup1.PK, new[] { taxGroupInfo1 });
			dictionary.Add(taxOverrideGroup2.PK, new[] { taxGroupInfo2 });
			AssertNoExceptionThrown(() => TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.SetupTaxRecordsForLine(taxRecords, lineTaxableTransactionMock.Object, new[] { chargeTaxOverride1, chargeTaxOverride2 }, taxRecordParentMock.Object, dictionary));

			dictionary = new Dictionary<ZGuid, TaxRecordCalculator.TaxOverrideGroupInfo[]>();
			dictionary.Add(taxOverrideGroup1.PK, new[] { taxGroupInfo });
			dictionary.Add(taxOverrideGroup2.PK, new[] { taxGroupInfo });
			AssertExceptionThrown(typeof(TaxFrameworkConfigurationValueException), exceptionMessage, () => TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.SetupTaxRecordsForLine(taxRecords, lineTaxableTransactionMock.Object, new[] { chargeTaxOverride1, chargeTaxOverride2 }, taxRecordParentMock.Object, dictionary));

			taxGroupInfo1 = new TaxRecordCalculator.TaxOverrideGroupInfo("1234", "Test Description", taxRate1.Item1, taxRate1.Item2, taxConfiguration.PK, ZGuid.Empty, ZGuid.Empty);
			taxGroupInfo2 = new TaxRecordCalculator.TaxOverrideGroupInfo("1111", "Test Description", taxRate1.Item1, taxRate1.Item2, taxConfiguration.PK, ZGuid.Empty, ZGuid.Empty);
			dictionary = new Dictionary<ZGuid, TaxRecordCalculator.TaxOverrideGroupInfo[]>();
			dictionary.Add(taxOverrideGroup1.PK, new[] { taxGroupInfo1 });
			dictionary.Add(taxOverrideGroup2.PK, new[] { taxGroupInfo2 });
			AssertNoExceptionThrown(() => TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.SetupTaxRecordsForLine(taxRecords, lineTaxableTransactionMock.Object, new[] { chargeTaxOverride1, chargeTaxOverride2 }, taxRecordParentMock.Object, dictionary));

			dictionary = new Dictionary<ZGuid, TaxRecordCalculator.TaxOverrideGroupInfo[]>();
			dictionary.Add(taxOverrideGroup1.PK, new[] { taxGroupInfo });
			dictionary.Add(taxOverrideGroup2.PK, new[] { taxGroupInfo });
			AssertExceptionThrown(typeof(TaxFrameworkConfigurationValueException), exceptionMessage, () => TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.SetupTaxRecordsForLine(taxRecords, lineTaxableTransactionMock.Object, new[] { chargeTaxOverride1, chargeTaxOverride2 }, taxRecordParentMock.Object, dictionary));

			taxGroupInfo1 = new TaxRecordCalculator.TaxOverrideGroupInfo("1111", "Test Description", taxRate1.Item1, taxRate1.Item2, taxConfiguration.PK, ZGuid.Empty, ZGuid.Empty);
			taxGroupInfo2 = new TaxRecordCalculator.TaxOverrideGroupInfo("1111", "Test Description", taxRate1.Item1, taxRate1.Item2, TaxFrameworkTestObjectCreator.CreateTaxConfiguration(GlbCompany.CurrentCompany, taxAuthority, taxSystem).PK, ZGuid.Empty, ZGuid.Empty);
			dictionary = new Dictionary<ZGuid, TaxRecordCalculator.TaxOverrideGroupInfo[]>();
			dictionary.Add(taxOverrideGroup1.PK, new[] { taxGroupInfo1 });
			dictionary.Add(taxOverrideGroup2.PK, new[] { taxGroupInfo2 });
			AssertNoExceptionThrown(() => TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.SetupTaxRecordsForLine(taxRecords, lineTaxableTransactionMock.Object, new[] { chargeTaxOverride1, chargeTaxOverride2 }, taxRecordParentMock.Object, dictionary));

			dictionary = new Dictionary<ZGuid, TaxRecordCalculator.TaxOverrideGroupInfo[]>();
			dictionary.Add(taxOverrideGroup1.PK, new[] { taxGroupInfo });
			dictionary.Add(taxOverrideGroup2.PK, new[] { taxGroupInfo });
			AssertExceptionThrown(typeof(TaxFrameworkConfigurationValueException), exceptionMessage, () => TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.SetupTaxRecordsForLine(taxRecords, lineTaxableTransactionMock.Object, new[] { chargeTaxOverride1, chargeTaxOverride2 }, taxRecordParentMock.Object, dictionary));
		}

		public void TestSetupTaxRecordsForLine_GetTaxIDAndVatClassFromTaxGroupInfo()
		{
			var taxSystemsConfigCollection = new TaxSystemsConfigurationCollection();
			var taxSystem1 = TaxFrameworkTestObjectCreator.CreateTaxSystem("PIB");
			var taxSystem2 = TaxFrameworkTestObjectCreator.CreateTaxSystem("RIB");
			var taxSystem3 = TaxFrameworkTestObjectCreator.CreateTaxSystem("CIB");
			taxSystemsConfigCollection.Add(taxSystem1);
			taxSystemsConfigCollection.Add(taxSystem2);
			taxSystemsConfigCollection.Add(taxSystem3);
			AccountingMasterFilesRegistry.Instance.TaxSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taxSystemsConfigCollection);
			taxSystemsConfigCollection.ClearCachedTaxSystemsByCode_ForTestOnly(Factory);
			var taxConfiguration1 = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(GlbCompany.CurrentCompany, null, taxSystem1);
			var taxConfiguration2 = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(GlbCompany.CurrentCompany, null, taxSystem2);
			var taxConfiguration3 = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(GlbCompany.CurrentCompany, null, taxSystem3);
			var taxMessage1 = Factory.NewWithValidTestData<AccInvMsg>();
			var taxMessage2 = Factory.NewWithValidTestData<AccInvMsg>();
			var taxID1 = TaxFrameworkTestObjectCreator.CreateTaxRate_RateSource(TaxRateSources.TaxIDOnly.Code);
			var taxID2 = TaxFrameworkTestObjectCreator.CreateTaxRate_RateSource(TaxRateSources.TaxIDOnly.Code);
			var taxID3 = TaxFrameworkTestObjectCreator.CreateTaxRate_RateSource(TaxRateSources.TaxIDOnly.Code);
			var taxRate1 = (2, 3);
			var taxRate2 = (5, 4);
			var taxRate3 = (7, 2);
			taxID1.SetRate_ForTestOnly(taxRate1.Item1, taxRate1.Item2);
			taxID2.SetRate_ForTestOnly(taxRate2.Item1, taxRate2.Item2);
			taxID3.SetRate_ForTestOnly(taxRate3.Item1, taxRate3.Item2);
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var company = Factory.NewWithValidTestData<GlbCompany>();

			var taxRecordParentMock1 = CreateITaxRecordParentMock(Factory);
			taxRecordParentMock1.Setup(x => x.Company).Returns(company);
			taxRecordParentMock1.Setup(x => x.Org).Returns(org);
			var taxOverrideGroup1 = Factory.NewWithValidTestData<AccTaxOverrideGroup>();
			var chargeTaxOverride1 = Factory.NewWithValidTestData<AccChargeTaxOverride>();
			chargeTaxOverride1.AO_AT = ZGuid.Empty;
			chargeTaxOverride1.AO_ParentID = taxOverrideGroup1.PK;
			chargeTaxOverride1.AO_ParentTableCode = "AX";
			chargeTaxOverride1.AO_A9_DefaultVATClass = ZGuid.Empty;
			var taxOverrideGroup2 = Factory.NewWithValidTestData<AccTaxOverrideGroup>();
			var chargeTaxOverride2 = Factory.NewWithValidTestData<AccChargeTaxOverride>();
			chargeTaxOverride2.AO_AT = ZGuid.Empty;
			chargeTaxOverride2.AO_ParentID = taxOverrideGroup2.PK;
			chargeTaxOverride2.AO_ParentTableCode = "AX";
			chargeTaxOverride2.AO_A9_DefaultVATClass = ZGuid.Empty;
			List<AccTaxTransaction> taxRecords1 = new List<AccTaxTransaction>();
			var lineTaxableTransactionMock1 = CreateITaxableTransactionLineMock(Factory, ZGuid.NewZGuid(), "USD", new ZDate(2019, 10, 30));
			var taxGroupInfo1 = new TaxRecordCalculator.TaxOverrideGroupInfo("123", "Desc1", 0, 1, taxConfiguration1.PK, taxID1.PK, taxMessage1.PK);
			var taxGroupInfo2 = new TaxRecordCalculator.TaxOverrideGroupInfo("567", "Desc2", 0, 1, taxConfiguration2.PK, taxID2.PK, taxMessage2.PK);
			var taxGroupInfo3 = new TaxRecordCalculator.TaxOverrideGroupInfo("489", "Desc3", 0, 1, taxConfiguration3.PK, taxID3.PK, ZGuid.Empty);
			var dictionary = new Dictionary<ZGuid, TaxRecordCalculator.TaxOverrideGroupInfo[]>();
			dictionary.Add(taxOverrideGroup1.PK, new[] { taxGroupInfo1 });
			dictionary.Add(taxOverrideGroup2.PK, new[] { taxGroupInfo2, taxGroupInfo3 });
			var calculatedTaxRecords = TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.SetupTaxRecordsForLine(taxRecords1, lineTaxableTransactionMock1.Object, new[] { chargeTaxOverride1, chargeTaxOverride2 }, taxRecordParentMock1.Object, dictionary);
			AssertEquals("taxRecords1.Count", 3, taxRecords1.Count);
			var taxRecord1 = taxRecords1.FirstOrDefault(x => x.ATT_AT_TaxID == taxID1.PK);
			AssertTaxTransaction(taxRecord1, taxRecordParentMock1.Object, company.PK, taxID1.PK, taxConfiguration1.PK, taxRate1, taxMessage1.PK, "123");
			var pivot1 = GetPivot(lineTaxableTransactionMock1.Object.PK, taxRecord1.PK);
			AssertEquals(lineTaxableTransactionMock1.Object.PK, pivot1.ATP_AL_TransactionLine);
			var taxRecord2 = taxRecords1.FirstOrDefault(x => x.ATT_AT_TaxID == taxID2.PK);
			AssertTaxTransaction(taxRecord2, taxRecordParentMock1.Object, company.PK, taxID2.PK, taxConfiguration2.PK, taxRate2, taxMessage2.PK, "567");
			var pivot2 = GetPivot(lineTaxableTransactionMock1.Object.PK, taxRecord2.PK);
			AssertEquals(lineTaxableTransactionMock1.Object.PK, pivot2.ATP_AL_TransactionLine);
			var taxRecord3 = taxRecords1.FirstOrDefault(x => x.ATT_AT_TaxID == taxID3.PK);
			AssertTaxTransaction(taxRecord3, taxRecordParentMock1.Object, company.PK, taxID3.PK, taxConfiguration3.PK, taxRate3, ZGuid.Empty, "489");
			var pivot3 = GetPivot(lineTaxableTransactionMock1.Object.PK, taxRecord3.PK);
			AssertEquals(lineTaxableTransactionMock1.Object.PK, pivot3.ATP_AL_TransactionLine);
			var expectedValue = new List<(AccTaxTransaction, AccTaxRecordTransactionLinePivot)>();
			expectedValue.Add((taxRecord1, pivot1));
			expectedValue.Add((taxRecord2, pivot2));
			expectedValue.Add((taxRecord3, pivot3));
			AssertContainsExactElementsInAnyOrder(nameof(calculatedTaxRecords), expectedValue, calculatedTaxRecords);

			void AssertTaxTransaction(AccTaxTransaction accTaxTransaction, ITaxRecordParent taxRecordParent, ZGuid glbCompanyPK, ZGuid taxIDPK, ZGuid accTaxConfigurationPK, (ZInt, ZInt) taxRate, ZGuid taxMessagePK, ZString taxAuthorityServiceCode)
			{
				AssertEquals("ATT_AH", taxRecordParent.PK, accTaxTransaction.ATT_AH);
				AssertEquals("ATT_GC", glbCompanyPK, accTaxTransaction.ATT_GC);
				AssertEquals("ATT_AT_TaxID", taxIDPK, accTaxTransaction.ATT_AT_TaxID);
				AssertEquals("ATT_ETC", accTaxConfigurationPK, accTaxTransaction.ATT_ETC);
				AssertEquals("ATT_RateNumerator", taxRate.Item1, accTaxTransaction.ATT_RateNumerator);
				AssertEquals("ATT_RateDenominator", taxRate.Item2, accTaxTransaction.ATT_RateDenominator);
				AssertEquals("ATT_A9_TaxMessage", taxMessagePK, accTaxTransaction.ATT_A9_TaxMessage);
				AssertEquals("ATT_TaxAuthorityServiceCode", taxAuthorityServiceCode, accTaxTransaction.ATT_TaxAuthorityServiceCode);
			}
		}

		#endregion

		public void TestCalculateTaxRecordsWithoutAmounts()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var company = TestObjectCreator.CreateNewCompany("CO1");
			var branch = TestObjectCreator.CreateBranch("BR", company);
			var companyData = orgHeader.GetCompanyDataForGlbCompany(company);
			var taxAuthority = TaxFrameworkTestObjectCreator.CreateTaxAuthority("TA");
			var taxSystem = TaxFrameworkTestObjectCreator.CreateTaxSystem("TS");

			var taxConfigCompany = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(company, taxAuthority, taxSystem, TaxConfigurationLedgers.AccountsReceivable.Code);
			var taxConfigurationBranch = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(branch, taxAuthority, taxSystem, TaxConfigurationLedgers.AccountsReceivable.Code, true);
			var taxOverrideGroup = TaxFrameworkTestObjectCreator.CreateTaxOverrideGroup(company, taxConfigCompany);

			var taxSystemsConfigCollection = new TaxSystemsConfigurationCollection();
			taxSystemsConfigCollection.Add(taxSystem);
			AccountingMasterFilesRegistry.Instance.TaxSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taxSystemsConfigCollection);
			taxSystemsConfigCollection.ClearCachedTaxSystemsByCode_ForTestOnly(Factory);
			Factory.Save();

			var orgTaxConfig = TaxFrameworkTestObjectCreator.CreateOrgTaxConfiguration(taxConfigCompany, companyData);

			var chargeCode1 = TestObjectCreator.CreateChargeCode("CC1");
			var chargeCode2 = TestObjectCreator.CreateChargeCode("CC2");
			var chargeCode3 = TestObjectCreator.CreateChargeCode("CC3");

			var taxFrameTaxOverrideGroup1 = CreateTaxOverrideAndPivot(company, taxConfigCompany, chargeCode1);
			var taxFrameTaxOverrideGroup2 = CreateTaxOverrideAndPivot(company, taxConfigCompany, chargeCode2);
			var taxFrameTaxOverrideGroup3 = CreateTaxOverrideAndPivot(company, taxConfigCompany, chargeCode3);

			var orgTaxConfigBranch = TaxFrameworkTestObjectCreator.CreateOrgTaxConfiguration(taxConfigurationBranch, companyData);
			var taxOverrideGroup1 = TaxFrameworkTestObjectCreator.CreateTaxOverrideGroup(company, taxConfigurationBranch);
			var taxFrameTaxOverrideGroup4 = CreateTaxOverrideAndPivot(company, taxConfigurationBranch, chargeCode3);

			taxFrameTaxOverrideGroup1.TaxOverrideGroupTaxConfigurationPivots[0].AXP_TaxAuthorityServiceCode = "111";
			taxFrameTaxOverrideGroup2.TaxOverrideGroupTaxConfigurationPivots[0].AXP_TaxAuthorityServiceCode = "222";
			taxFrameTaxOverrideGroup3.TaxOverrideGroupTaxConfigurationPivots[0].AXP_TaxAuthorityServiceCode = "333";
			taxFrameTaxOverrideGroup4.TaxOverrideGroupTaxConfigurationPivots[0].AXP_TaxAuthorityServiceCode = "444";

			var taxID1 = TaxFrameworkTestObjectCreator.CreateTaxRate_RateSource(TaxRateSources.TaxIDOnly.Code);
			var taxID2 = TaxFrameworkTestObjectCreator.CreateTaxRate_RateSource(TaxRateSources.TaxIDOnly.Code);
			var taxID3 = TaxFrameworkTestObjectCreator.CreateTaxRate_RateSource(TaxRateSources.TaxIDOnly.Code);

			var taxRule1 = TaxFrameworkTestObjectCreator.CreateTaxOverrideRule(taxFrameTaxOverrideGroup1, taxID1.PK, direction: OrgConstants.ServiceDirection.Code.Domestic);
			var taxRule2 = TaxFrameworkTestObjectCreator.CreateTaxOverrideRule(taxFrameTaxOverrideGroup2, taxID2.PK, direction: OrgConstants.ServiceDirection.Code.Domestic);
			var taxRule3 = TaxFrameworkTestObjectCreator.CreateTaxOverrideRule(taxFrameTaxOverrideGroup3, taxID3.PK, direction: OrgConstants.ServiceDirection.Code.Domestic);
			var taxRule4 = TaxFrameworkTestObjectCreator.CreateTaxOverrideRule(taxFrameTaxOverrideGroup4, taxID3.PK, direction: OrgConstants.ServiceDirection.Code.Domestic);
			Factory.Save();

			var taxRecordParentMock = CreateITaxRecordParentMock(Factory);
			taxRecordParentMock.SetupGet(f => f.Ledger).Returns(TaxConfigurationLedgers.AccountsReceivable.Code);
			taxRecordParentMock.SetupGet(f => f.Org).Returns(orgHeader);
			taxRecordParentMock.SetupGet(f => f.Company).Returns(company);
			taxRecordParentMock.SetupGet(f => f.Branch).Returns(GlbBranch.CurrentBranch);

			var parameters = new AccChargeTaxOverrideMatcher.TaxCalculationParameters { JobType = JobInvoicingConsumerTypes.Shipment.Code, Direction = Directions.Domestic };

			var lineTaxableTransactionMock1 = CreateITaxableTransactionLineMock(Factory, ZGuid.NewZGuid(), "USD", new ZDate(2019, 10, 30), branch, chargeCode1);
			lineTaxableTransactionMock1.Setup(r => r.GetTaxCalculationParameters()).Returns(parameters);

			var lineTaxableTransactionMock2 = CreateITaxableTransactionLineMock(Factory, ZGuid.NewZGuid(), "USD", new ZDate(2019, 10, 30), branch, chargeCode2);
			lineTaxableTransactionMock2.Setup(r => r.GetTaxCalculationParameters()).Returns(parameters);

			var lineTaxableTransactionMock3 = CreateITaxableTransactionLineMock(Factory, ZGuid.NewZGuid(), "USD", new ZDate(2019, 10, 30), branch, chargeCode3);
			lineTaxableTransactionMock3.Setup(r => r.GetTaxCalculationParameters()).Returns(parameters);

			taxRecordParentMock.Setup(r => r.GetLines()).Returns(new List<ITaxableTransactionLine> { lineTaxableTransactionMock1.Object, lineTaxableTransactionMock2.Object, lineTaxableTransactionMock3.Object });

			ITaxRecordCalculator taxCalculator = new TaxRecordCalculator(taxFrameworkConfigurationHelper);
			var taxRecords = new List<AccTaxTransaction>();
			taxCalculator.CalculateTaxRecords(taxRecordParentMock.Object, taxRecords);
			AssertEquals($"taxRecords.Count 4 records", 4, taxRecords.Count);
			AssertTaxTransaction(taxRecords[0], taxRecordParentMock.Object, company.PK, taxID1.PK, taxConfigCompany.PK, "111", taxRecordParentMock.Object.Branch.PK);
			AssertTaxTransaction(taxRecords[1], taxRecordParentMock.Object, company.PK, taxID2.PK, taxConfigCompany.PK, "222", taxRecordParentMock.Object.Branch.PK);
			AssertTaxTransaction(taxRecords[2], taxRecordParentMock.Object, company.PK, taxID3.PK, taxConfigCompany.PK, "333", taxRecordParentMock.Object.Branch.PK);
			AssertTaxTransaction(taxRecords[3], taxRecordParentMock.Object, company.PK, taxID3.PK, taxConfigurationBranch.PK, "444", branch.PK);

			branch.GB_GC = ZGuid.NewZGuid();
			lineTaxableTransactionMock1.SetupGet(f => f.Branch).Returns(branch);
			taxRecordParentMock.Setup(r => r.GetLines()).Returns(new List<ITaxableTransactionLine> { lineTaxableTransactionMock1.Object });
			taxRecords.Clear();
			taxCalculator.CalculateTaxRecords(taxRecordParentMock.Object, taxRecords);
			AssertEquals($"taxRecords.Count 0 records with not found InvoiceTaxOverrideGroupPKs", 0, taxRecords.Count);

			lineTaxableTransactionMock1.SetupGet(f => f.Branch).Returns((GlbBranch)null);
			taxRecordParentMock.Setup(r => r.GetLines()).Returns(new List<ITaxableTransactionLine> { lineTaxableTransactionMock1.Object });
			taxRecords.Clear();
			taxCalculator.CalculateTaxRecords(taxRecordParentMock.Object, taxRecords);
			AssertEquals($"taxRecords.Count 0 records with taxRecordParent with 1 lines and branch empty", 0, taxRecords.Count);

			taxRecordParentMock.Setup(r => r.GetLines()).Returns(new List<ITaxableTransactionLine>());
			taxRecords.Clear();
			taxCalculator.CalculateTaxRecords(taxRecordParentMock.Object, taxRecords);
			AssertEquals($"taxRecords.Count 0 records with taxRecordParent with 0 lines", 0, taxRecords.Count);

			void AssertTaxTransaction(AccTaxTransaction accTaxTransaction, ITaxRecordParent taxRecordParent, ZGuid glbCompanyPK, ZGuid taxIDPK, ZGuid accTaxConfigurationPK, ZString taxAuthorityServiceCode, ZGuid branchPK)
			{
				AssertEquals("ATT_AH", taxRecordParent.PK, accTaxTransaction.ATT_AH);
				AssertEquals("ATT_GC", glbCompanyPK, accTaxTransaction.ATT_GC);
				AssertEquals("ATT_AT_TaxID", taxIDPK, accTaxTransaction.ATT_AT_TaxID);
				AssertEquals("ATT_ETC", accTaxConfigurationPK, accTaxTransaction.ATT_ETC);
				AssertEquals("ATT_TaxAuthorityServiceCode", taxAuthorityServiceCode, accTaxTransaction.ATT_TaxAuthorityServiceCode);
				AssertEquals("ATT_GB", branchPK, accTaxTransaction.ATT_GB);
			}
		}

		public void TestGetTaxRecordsWithLinePivots()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var company = TestObjectCreator.CreateNewCompany("CO1");
			var branch = TestObjectCreator.CreateBranch("BR", company);
			var companyData = orgHeader.GetCompanyDataForGlbCompany(company);
			var taxAuthority = TaxFrameworkTestObjectCreator.CreateTaxAuthority("TA");
			var taxSystem1Code = "TS1";
			var taxSystem1 = TaxFrameworkTestObjectCreator.CreateTaxSystem(taxSystem1Code);
			var taxSystem2Code = "TS2";
			var taxSystem2 = TaxFrameworkTestObjectCreator.CreateTaxSystem(taxSystem2Code);

			var taxConfigCompany1 = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(company, taxAuthority, taxSystem1, TaxConfigurationLedgers.AccountsReceivable.Code);
			var taxConfigCompany2 = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(company, taxAuthority, taxSystem2, TaxConfigurationLedgers.AccountsReceivable.Code);
			var taxConfigurationBranch = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(branch, taxAuthority, taxSystem1, TaxConfigurationLedgers.AccountsReceivable.Code, true);

			var taxSystemsConfigCollection = new TaxSystemsConfigurationCollection();
			taxSystemsConfigCollection.Add(taxSystem1);
			taxSystemsConfigCollection.Add(taxSystem2);
			AccountingMasterFilesRegistry.Instance.TaxSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taxSystemsConfigCollection);
			taxSystemsConfigCollection.ClearCachedTaxSystemsByCode_ForTestOnly(Factory);
			Factory.Save();

			TaxFrameworkTestObjectCreator.CreateOrgTaxConfiguration(taxConfigCompany1, companyData);
			TaxFrameworkTestObjectCreator.CreateOrgTaxConfiguration(taxConfigCompany2, companyData);

			var chargeCode1 = TestObjectCreator.CreateChargeCode("CC1");
			var chargeCode2 = TestObjectCreator.CreateChargeCode("CC2");
			var chargeCode3 = TestObjectCreator.CreateChargeCode("CC3");

			var taxFrameTaxOverrideGroup1 = CreateTaxOverrideAndPivot(company, taxConfigCompany1, chargeCode1);
			var taxFrameTaxOverrideGroup2 = CreateTaxOverrideAndPivot(company, taxConfigCompany1, chargeCode2);
			var taxFrameTaxOverrideGroup3 = CreateTaxOverrideAndPivot(company, taxConfigCompany1, chargeCode3);

			var taxFrameTaxOverrideGroup5 = CreateTaxOverrideAndPivot(company, taxConfigCompany2, chargeCode1);

			var orgTaxConfigBranch = TaxFrameworkTestObjectCreator.CreateOrgTaxConfiguration(taxConfigurationBranch, companyData);
			var taxOverrideGroup1 = TaxFrameworkTestObjectCreator.CreateTaxOverrideGroup(company, taxConfigurationBranch);
			var taxFrameTaxOverrideGroup4 = CreateTaxOverrideAndPivot(company, taxConfigurationBranch, chargeCode3);

			taxFrameTaxOverrideGroup1.TaxOverrideGroupTaxConfigurationPivots[0].AXP_TaxAuthorityServiceCode = "111";
			taxFrameTaxOverrideGroup2.TaxOverrideGroupTaxConfigurationPivots[0].AXP_TaxAuthorityServiceCode = "222";
			taxFrameTaxOverrideGroup3.TaxOverrideGroupTaxConfigurationPivots[0].AXP_TaxAuthorityServiceCode = "333";
			taxFrameTaxOverrideGroup4.TaxOverrideGroupTaxConfigurationPivots[0].AXP_TaxAuthorityServiceCode = "444";
			taxFrameTaxOverrideGroup5.TaxOverrideGroupTaxConfigurationPivots[0].AXP_TaxAuthorityServiceCode = "555";

			var taxID1 = TaxFrameworkTestObjectCreator.CreateTaxRate_RateSource(TaxRateSources.TaxIDOnly.Code);
			var taxID2 = TaxFrameworkTestObjectCreator.CreateTaxRate_RateSource(TaxRateSources.TaxIDOnly.Code);
			var taxID3 = TaxFrameworkTestObjectCreator.CreateTaxRate_RateSource(TaxRateSources.TaxIDOnly.Code);

			var taxRule1 = TaxFrameworkTestObjectCreator.CreateTaxOverrideRule(taxFrameTaxOverrideGroup1, taxID1.PK, direction: OrgConstants.ServiceDirection.Code.Domestic);
			var taxRule2 = TaxFrameworkTestObjectCreator.CreateTaxOverrideRule(taxFrameTaxOverrideGroup2, taxID2.PK, direction: OrgConstants.ServiceDirection.Code.Domestic);
			var taxRule3 = TaxFrameworkTestObjectCreator.CreateTaxOverrideRule(taxFrameTaxOverrideGroup3, taxID3.PK, direction: OrgConstants.ServiceDirection.Code.Domestic);
			var taxRule4 = TaxFrameworkTestObjectCreator.CreateTaxOverrideRule(taxFrameTaxOverrideGroup4, taxID3.PK, direction: OrgConstants.ServiceDirection.Code.Domestic);
			var taxRule5 = TaxFrameworkTestObjectCreator.CreateTaxOverrideRule(taxFrameTaxOverrideGroup5, taxID1.PK, direction: OrgConstants.ServiceDirection.Code.Domestic);
			Factory.Save();

			var taxRecordParentMock = CreateITaxRecordParentBaseMock(Factory);
			taxRecordParentMock.SetupGet(f => f.Ledger).Returns(TaxConfigurationLedgers.AccountsReceivable.Code);
			taxRecordParentMock.SetupGet(f => f.Org).Returns(orgHeader);
			taxRecordParentMock.SetupGet(f => f.Company).Returns(company);
			taxRecordParentMock.SetupGet(f => f.Branch).Returns(GlbBranch.CurrentBranch);

			var parameters = new AccChargeTaxOverrideMatcher.TaxCalculationParameters { JobType = JobInvoicingConsumerTypes.Shipment.Code, Direction = Directions.Domestic };

			var lineTaxableTransactionMock1 = CreateITaxableTransactionLineMock(Factory, ZGuid.NewZGuid(), "USD", new ZDate(2019, 10, 30), branch, chargeCode1);
			lineTaxableTransactionMock1.Setup(r => r.GetTaxCalculationParameters()).Returns(parameters);

			var lineTaxableTransactionMock2 = CreateITaxableTransactionLineMock(Factory, ZGuid.NewZGuid(), "USD", new ZDate(2019, 10, 30), branch, chargeCode2);
			lineTaxableTransactionMock2.Setup(r => r.GetTaxCalculationParameters()).Returns(parameters);

			var lineTaxableTransactionMock3 = CreateITaxableTransactionLineMock(Factory, ZGuid.NewZGuid(), "USD", new ZDate(2019, 10, 30), branch, chargeCode3);
			lineTaxableTransactionMock3.Setup(r => r.GetTaxCalculationParameters()).Returns(parameters);

			taxRecordParentMock.Setup(r => r.GetLines()).Returns(new List<ITaxableTransactionLine> { lineTaxableTransactionMock1.Object, lineTaxableTransactionMock2.Object, lineTaxableTransactionMock3.Object });

			List<AccTaxTransaction> taxRecords;
			List<AccTaxRecordTransactionLinePivot> pivots;

			ITaxRecordCalculator taxCalculator = new TaxRecordCalculator(taxFrameworkConfigurationHelper);
			var results = taxCalculator.GetTaxRecordsWithLinePivots(taxRecordParentMock.Object);
			AssertAllFiveTaxRecordsReturned();

			results = taxCalculator.GetTaxRecordsWithLinePivots(taxRecordParentMock.Object, new List<ZString> { taxSystem1Code });
			taxRecords = results.Select(x => x.taxRecord).ToList();
			pivots = results.Select(x => x.linePivot).ToList();
			AssertEquals($"taxRecords.Count 4 records", 4, taxRecords.Count);
			AssertTaxTransaction(taxRecords[0], taxRecordParentMock.Object, company.PK, taxID1.PK, taxConfigCompany1.PK, "111", taxRecordParentMock.Object.Branch.PK);
			AssertTaxTransaction(taxRecords[1], taxRecordParentMock.Object, company.PK, taxID2.PK, taxConfigCompany1.PK, "222", taxRecordParentMock.Object.Branch.PK);
			AssertTaxTransaction(taxRecords[2], taxRecordParentMock.Object, company.PK, taxID3.PK, taxConfigCompany1.PK, "333", taxRecordParentMock.Object.Branch.PK);
			AssertTaxTransaction(taxRecords[3], taxRecordParentMock.Object, company.PK, taxID3.PK, taxConfigurationBranch.PK, "444", branch.PK);

			AssertEquals($"pivots.Count 4 records", 4, pivots.Count);
			AssertNotNull("Pivot for Line1", GetPivot(lineTaxableTransactionMock1.Object.PK, taxRecords[0].PK));
			AssertNotNull("Pivot for Line2", GetPivot(lineTaxableTransactionMock2.Object.PK, taxRecords[1].PK));
			AssertNotNull("Pivot for Line3", GetPivot(lineTaxableTransactionMock3.Object.PK, taxRecords[2].PK));
			AssertNotNull("Another Pivot for Line3", GetPivot(lineTaxableTransactionMock3.Object.PK, taxRecords[3].PK));

			results = taxCalculator.GetTaxRecordsWithLinePivots(taxRecordParentMock.Object, new List<ZString>() { });
			AssertAllFiveTaxRecordsReturned();

			results = taxCalculator.GetTaxRecordsWithLinePivots(taxRecordParentMock.Object, new List<ZString> { taxSystem2Code });
			taxRecords = results.Select(x => x.taxRecord).ToList();
			pivots = results.Select(x => x.linePivot).ToList();
			AssertEquals($"taxRecords.Count 1 records", 1, taxRecords.Count);
			AssertTaxTransaction(taxRecords[0], taxRecordParentMock.Object, company.PK, taxID1.PK, taxConfigCompany2.PK, "555", taxRecordParentMock.Object.Branch.PK);

			AssertEquals($"pivots.Count 1 records", 1, pivots.Count);
			AssertNotNull("Pivot for Line1", GetPivot(lineTaxableTransactionMock1.Object.PK, taxRecords[0].PK));

			branch.GB_GC = ZGuid.NewZGuid();
			lineTaxableTransactionMock1.SetupGet(f => f.Branch).Returns(branch);
			taxRecordParentMock.Setup(r => r.GetLines()).Returns(new List<ITaxableTransactionLine> { lineTaxableTransactionMock1.Object });
			taxRecords.Clear();
			taxCalculator.GetTaxRecordsWithLinePivots(taxRecordParentMock.Object);
			AssertEquals($"taxRecords.Count 0 records with not found InvoiceTaxOverrideGroupPKs", 0, taxRecords.Count);

			lineTaxableTransactionMock1.SetupGet(f => f.Branch).Returns((GlbBranch)null);
			taxRecordParentMock.Setup(r => r.GetLines()).Returns(new List<ITaxableTransactionLine> { lineTaxableTransactionMock1.Object });
			taxRecords.Clear();
			taxCalculator.GetTaxRecordsWithLinePivots(taxRecordParentMock.Object);
			AssertEquals($"taxRecords.Count 0 records with taxRecordParent with 1 lines and branch empty", 0, taxRecords.Count);

			taxRecordParentMock.Setup(r => r.GetLines()).Returns(new List<ITaxableTransactionLine>());
			taxRecords.Clear();
			taxCalculator.GetTaxRecordsWithLinePivots(taxRecordParentMock.Object);
			AssertEquals($"taxRecords.Count 0 records with taxRecordParent with 0 lines", 0, taxRecords.Count);

			void AssertTaxTransaction(AccTaxTransaction accTaxTransaction, ITaxRecordParentBase taxRecordParent, ZGuid glbCompanyPK, ZGuid taxIDPK, ZGuid accTaxConfigurationPK, ZString taxAuthorityServiceCode, ZGuid branchPK)
			{
				AssertEquals("ATT_AH", taxRecordParent.PK, accTaxTransaction.ATT_AH);
				AssertEquals("ATT_GC", glbCompanyPK, accTaxTransaction.ATT_GC);
				AssertEquals("ATT_AT_TaxID", taxIDPK, accTaxTransaction.ATT_AT_TaxID);
				AssertEquals("ATT_ETC", accTaxConfigurationPK, accTaxTransaction.ATT_ETC);
				AssertEquals("ATT_TaxAuthorityServiceCode", taxAuthorityServiceCode, accTaxTransaction.ATT_TaxAuthorityServiceCode);
				AssertEquals("ATT_GB", branchPK, accTaxTransaction.ATT_GB);
			}

			void AssertAllFiveTaxRecordsReturned()
			{
				taxRecords = results.Select(x => x.taxRecord).ToList();
				pivots = results.Select(x => x.linePivot).ToList();
				AssertEquals($"taxRecords.Count 5 records", 5, taxRecords.Count);
				AssertTaxTransaction(taxRecords[0], taxRecordParentMock.Object, company.PK, taxID1.PK, taxConfigCompany1.PK, "111", taxRecordParentMock.Object.Branch.PK);
				AssertTaxTransaction(taxRecords[1], taxRecordParentMock.Object, company.PK, taxID1.PK, taxConfigCompany2.PK, "555", taxRecordParentMock.Object.Branch.PK);
				AssertTaxTransaction(taxRecords[2], taxRecordParentMock.Object, company.PK, taxID2.PK, taxConfigCompany1.PK, "222", taxRecordParentMock.Object.Branch.PK);
				AssertTaxTransaction(taxRecords[3], taxRecordParentMock.Object, company.PK, taxID3.PK, taxConfigCompany1.PK, "333", taxRecordParentMock.Object.Branch.PK);
				AssertTaxTransaction(taxRecords[4], taxRecordParentMock.Object, company.PK, taxID3.PK, taxConfigurationBranch.PK, "444", branch.PK);

				AssertEquals($"pivots.Count 5 records", 5, pivots.Count);
				AssertNotNull("Pivot for Line1", GetPivot(lineTaxableTransactionMock1.Object.PK, taxRecords[0].PK));
				AssertNotNull("Pivot for Line1", GetPivot(lineTaxableTransactionMock1.Object.PK, taxRecords[1].PK));
				AssertNotNull("Pivot for Line2", GetPivot(lineTaxableTransactionMock2.Object.PK, taxRecords[2].PK));
				AssertNotNull("Pivot for Line3", GetPivot(lineTaxableTransactionMock3.Object.PK, taxRecords[3].PK));
				AssertNotNull("Another Pivot for Line3", GetPivot(lineTaxableTransactionMock3.Object.PK, taxRecords[4].PK));
			}
		}

		public void TestARInvoiceOrgSpecificRateCalculateTaxRecords()
		{
			SetupAndAssertOrgSpecificRate_CalculateTaxRecords(typeof(ARInvoice), TaxConfigurationLedgers.AccountsReceivable.Code);
		}

		public void TestAPInvoiceOrgSpecificRateCalculateTaxRecords()
		{
			SetupAndAssertOrgSpecificRate_CalculateTaxRecords(typeof(APInvoice), TaxConfigurationLedgers.AccountsPayable.Code);
		}

		public void SetupAndAssertOrgSpecificRate_CalculateTaxRecords(Type type, ZString ledgerCode)
		{
			var company = GlbCompany.CurrentCompany;
			var orgHeader = TestObjectCreator.CreateOrgHeader("TESTORG", true, true);
			var companyData = orgHeader.GetCompanyDataForGlbCompany(company);

			if (ledgerCode == TaxConfigurationLedgers.AccountsPayable.Code)
			{
				orgHeader.CompanyData.SetAPTaxApplicable(false);
			}
			else
			{
				orgHeader.CompanyData.SetARTaxApplicable(false);
			}

			var taxAuthority = TaxFrameworkTestObjectCreator.CreateTaxAuthority("TA");
			var taxSystem = TaxFrameworkTestObjectCreator.CreateTaxSystem("TS");
			taxSystem.TaxRateSource = TaxRateSources.OrganisationFallbackToTaxID.Code;
			var currency = TestObjectCreator.AUD;

			var taxConfigCompany = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(company, taxAuthority, taxSystem, ledgerCode);

			var taxSystemsConfigCollection = new TaxSystemsConfigurationCollection();
			taxSystemsConfigCollection.Add(taxSystem);
			AccountingMasterFilesRegistry.Instance.TaxSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taxSystemsConfigCollection);
			taxSystemsConfigCollection.ClearCachedTaxSystemsByCode_ForTestOnly(Factory);

			Factory.Save();

			var orgTaxConfig = TaxFrameworkTestObjectCreator.CreateOrgTaxConfiguration(taxConfigCompany, companyData);
			var chargeCode = TestObjectCreator.CreateChargeCode("CC1");
			var taxFrameTaxOverrideGroup1 = CreateTaxOverrideAndPivot(company, taxConfigCompany, chargeCode);

			var taxRate = TaxFrameworkTestObjectCreator.CreateTaxRate_RateSource(TaxRateSources.OrganisationFallbackToTaxID.Code);
			taxRate.AT_TaxSystemCode = "Other1";
			taxRate.SetRate_ForTestOnly(10, 1);

			var taxRatePair = ((ZInt)2478, (ZInt)1000);
			TaxFrameworkTestObjectCreator.AddOrgTaxRateItem(orgTaxConfig, ZDate.Today.AddMonths(-1), ZDate.Today.AddMonths(1), RateSourceMethods.Monthly.Code, taxRatePair);

			var taxRule = TaxFrameworkTestObjectCreator.CreateTaxOverrideRule(taxFrameTaxOverrideGroup1, taxRate.PK);

			Factory.Save();

			var apInvoice = TestObjectCreator.CreateInvoice(type, "001", currency, 1M, orgHeader);
			var line = TestObjectCreator.CreateInvoiceLine(apInvoice, 1000M, currency, 1M, setTaxes: false);
			line.GenericCharge = chargeCode.PK;

			ITaxRecordParent taxParent = TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(apInvoice);
			var lines = taxParent.GetLines();
			var taxDate = lines[0].TaxDate;

			var rateNumeratorDenominator = TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.GetTaxRate(taxRate, 0, 0, taxConfigCompany, orgHeader, taxDate);

			AssertEquals("ATT_RateNumerator", taxRatePair.Item1, rateNumeratorDenominator.Value.numerator);
			AssertEquals("ATT_RateDenominator", taxRatePair.Item2, rateNumeratorDenominator.Value.denominator);

			List<AccTaxTransaction> taxRecords = new List<AccTaxTransaction>();
			var records = TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.CalculateTaxRecord(taxRecords, lines[0], taxRate, taxConfigCompany, taxRatePair, taxParent, taxRule.AO_A9_DefaultVATClass, "33", "33Desc");
			AssertEquals("ATT_AT_TaxID", taxRate.PK, records.Item1.ATT_AT_TaxID);
			AssertEquals("ATT_TaxDate", ZDate.Today, records.Item1.ATT_TaxDate);
		}

		public void TestCompare_GetTaxRecordsWithLinePivots_CalculateTaxRecords()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var company = TestObjectCreator.CreateNewCompany("CO1");
			var branch = TestObjectCreator.CreateBranch("BR", company);
			var companyData = orgHeader.GetCompanyDataForGlbCompany(company);
			var taxAuthority = TaxFrameworkTestObjectCreator.CreateTaxAuthority("TA");
			var taxSystem = TaxFrameworkTestObjectCreator.CreateTaxSystem("TS");

			var taxConfigCompany = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(company, taxAuthority, taxSystem, TaxConfigurationLedgers.AccountsReceivable.Code);
			var taxConfigurationBranch = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(branch, taxAuthority, taxSystem, TaxConfigurationLedgers.AccountsReceivable.Code, true);
			var taxOverrideGroup = TaxFrameworkTestObjectCreator.CreateTaxOverrideGroup(company, taxConfigCompany);

			var taxSystemsConfigCollection = new TaxSystemsConfigurationCollection();
			taxSystemsConfigCollection.Add(taxSystem);
			AccountingMasterFilesRegistry.Instance.TaxSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taxSystemsConfigCollection);
			taxSystemsConfigCollection.ClearCachedTaxSystemsByCode_ForTestOnly(Factory);
			Factory.Save();

			var orgTaxConfig = TaxFrameworkTestObjectCreator.CreateOrgTaxConfiguration(taxConfigCompany, companyData);

			var chargeCode1 = TestObjectCreator.CreateChargeCode("CC1");
			var chargeCode2 = TestObjectCreator.CreateChargeCode("CC2");
			var chargeCode3 = TestObjectCreator.CreateChargeCode("CC3");

			var taxFrameTaxOverrideGroup1 = CreateTaxOverrideAndPivot(company, taxConfigCompany, chargeCode1);
			var taxFrameTaxOverrideGroup2 = CreateTaxOverrideAndPivot(company, taxConfigCompany, chargeCode2);
			var taxFrameTaxOverrideGroup3 = CreateTaxOverrideAndPivot(company, taxConfigCompany, chargeCode3);

			var orgTaxConfigBranch = TaxFrameworkTestObjectCreator.CreateOrgTaxConfiguration(taxConfigurationBranch, companyData);
			var taxOverrideGroup1 = TaxFrameworkTestObjectCreator.CreateTaxOverrideGroup(company, taxConfigurationBranch);

			var taxID1 = TaxFrameworkTestObjectCreator.CreateTaxRate_RateSource(TaxRateSources.TaxIDOnly.Code);
			taxID1.SetRate_ForTestOnly(10, 1);
			var taxID2 = TaxFrameworkTestObjectCreator.CreateTaxRate_RateSource(TaxRateSources.TaxIDOnly.Code);
			taxID2.SetRate_ForTestOnly(10, 1);
			var taxID3 = TaxFrameworkTestObjectCreator.CreateTaxRate_RateSource(TaxRateSources.TaxIDOnly.Code);
			taxID3.SetRate_ForTestOnly(10, 1);

			var taxRule1 = TaxFrameworkTestObjectCreator.CreateTaxOverrideRule(taxFrameTaxOverrideGroup1, taxID1.PK, direction: OrgConstants.ServiceDirection.Code.Domestic);
			var taxRule2 = TaxFrameworkTestObjectCreator.CreateTaxOverrideRule(taxFrameTaxOverrideGroup2, taxID2.PK, direction: OrgConstants.ServiceDirection.Code.Domestic);
			var taxRule3 = TaxFrameworkTestObjectCreator.CreateTaxOverrideRule(taxFrameTaxOverrideGroup3, taxID3.PK, direction: OrgConstants.ServiceDirection.Code.Domestic);
			Factory.Save();

			#region Mock ITaxRecordParent

			var taxRecordParentMock = CreateITaxRecordParentMock(Factory);
			taxRecordParentMock.SetupGet(f => f.Ledger).Returns(TaxConfigurationLedgers.AccountsReceivable.Code);
			taxRecordParentMock.SetupGet(f => f.Org).Returns(orgHeader);
			taxRecordParentMock.SetupGet(f => f.Company).Returns(company);
			taxRecordParentMock.SetupGet(f => f.Branch).Returns(GlbBranch.CurrentBranch);

			var parameters = new AccChargeTaxOverrideMatcher.TaxCalculationParameters { JobType = JobInvoicingConsumerTypes.Shipment.Code, Direction = Directions.Domestic };

			var currency = TestObjectCreator.USD.RX_Code;
			var taxDate = new ZDate(2019, 10, 30);
			var lineTaxableTransactionMock1 = CreateITaxableTransactionLineMock(Factory, ZGuid.NewZGuid(), currency, taxDate, branch, chargeCode1);
			lineTaxableTransactionMock1.Setup(r => r.GetTaxCalculationParameters()).Returns(parameters);
			lineTaxableTransactionMock1.SetupGet(r => r.BaseOSAmount).Returns(100M);
			lineTaxableTransactionMock1.SetupGet(r => r.LocalAmount).Returns(200M);

			var lineTaxableTransactionMock2 = CreateITaxableTransactionLineMock(Factory, ZGuid.NewZGuid(), currency, taxDate, branch, chargeCode2);
			lineTaxableTransactionMock2.Setup(r => r.GetTaxCalculationParameters()).Returns(parameters);
			lineTaxableTransactionMock2.SetupGet(r => r.BaseOSAmount).Returns(100M);
			lineTaxableTransactionMock2.SetupGet(r => r.LocalAmount).Returns(200M);

			var lineTaxableTransactionMock3 = CreateITaxableTransactionLineMock(Factory, ZGuid.NewZGuid(), currency, taxDate, branch, chargeCode3);
			lineTaxableTransactionMock3.Setup(r => r.GetTaxCalculationParameters()).Returns(parameters);
			lineTaxableTransactionMock3.SetupGet(r => r.BaseOSAmount).Returns(100M);
			lineTaxableTransactionMock3.SetupGet(r => r.LocalAmount).Returns(200M);

			taxRecordParentMock.Setup(r => r.GetLines()).Returns(new List<ITaxableTransactionLine> { lineTaxableTransactionMock1.Object, lineTaxableTransactionMock2.Object, lineTaxableTransactionMock3.Object });

			#endregion

			#region Mock ITaxRecordParentBase

			var taxRecordParentBaseMock = CreateITaxRecordParentBaseMock(Factory);
			taxRecordParentBaseMock.SetupGet(f => f.Ledger).Returns(taxRecordParentMock.Object.Ledger);
			taxRecordParentBaseMock.SetupGet(f => f.Org).Returns(taxRecordParentMock.Object.Org);
			taxRecordParentBaseMock.SetupGet(f => f.Company).Returns(taxRecordParentMock.Object.Company);
			taxRecordParentBaseMock.SetupGet(f => f.Branch).Returns(taxRecordParentMock.Object.Branch);

			var lineTaxableTransactionBaseMock1 = CreateITaxableTransactionLineBaseMock(Factory, ZGuid.NewZGuid(), currency, taxDate, branch, chargeCode1);
			lineTaxableTransactionBaseMock1.Setup(r => r.GetTaxCalculationParameters()).Returns(parameters);

			var lineTaxableTransactionBaseMock2 = CreateITaxableTransactionLineBaseMock(Factory, ZGuid.NewZGuid(), currency, taxDate, branch, chargeCode2);
			lineTaxableTransactionBaseMock2.Setup(r => r.GetTaxCalculationParameters()).Returns(parameters);

			var lineTaxableTransactionBaseMock3 = CreateITaxableTransactionLineBaseMock(Factory, ZGuid.NewZGuid(), currency, taxDate, branch, chargeCode3);
			lineTaxableTransactionBaseMock3.Setup(r => r.GetTaxCalculationParameters()).Returns(parameters);

			taxRecordParentBaseMock.Setup(r => r.GetLines()).Returns(new List<ITaxableTransactionLineBase> { lineTaxableTransactionBaseMock1.Object, lineTaxableTransactionBaseMock2.Object, lineTaxableTransactionBaseMock3.Object });

			#endregion

			ITaxRecordCalculator taxCalculator = new TaxRecordCalculator(taxFrameworkConfigurationHelper);
			var results = taxCalculator.GetTaxRecordsWithLinePivots(taxRecordParentBaseMock.Object);
			var taxRecords_GetTaxRecordsWithLinePivots = results.Select(x => x.taxRecord).ToList();
			var pivots = results.Select(x => x.linePivot).ToList();

			var taxRecords_CalculateTaxRecords = new List<AccTaxTransaction>();
			taxCalculator.CalculateTaxRecords(taxRecordParentMock.Object, taxRecords_CalculateTaxRecords);

			AssertEquals("3 tax records returned", 3, taxRecords_CalculateTaxRecords.Count);
			AssertEquals("Same number of tax records returned by both methods", taxRecords_GetTaxRecordsWithLinePivots.Count, taxRecords_CalculateTaxRecords.Count);

			CompareTaxRecords(taxRecords_GetTaxRecordsWithLinePivots[0], taxRecords_CalculateTaxRecords[0]);
			CompareTaxRecords(taxRecords_GetTaxRecordsWithLinePivots[1], taxRecords_CalculateTaxRecords[1]);
			CompareTaxRecords(taxRecords_GetTaxRecordsWithLinePivots[2], taxRecords_CalculateTaxRecords[2]);

			CheckLinePivotNotNull(GetPivot(lineTaxableTransactionBaseMock1.Object.PK, taxRecords_GetTaxRecordsWithLinePivots[0].PK), GetPivot(lineTaxableTransactionMock1.Object.PK, taxRecords_CalculateTaxRecords[0].PK));
			CheckLinePivotNotNull(GetPivot(lineTaxableTransactionBaseMock2.Object.PK, taxRecords_GetTaxRecordsWithLinePivots[1].PK), GetPivot(lineTaxableTransactionMock2.Object.PK, taxRecords_CalculateTaxRecords[1].PK));
			CheckLinePivotNotNull(GetPivot(lineTaxableTransactionBaseMock3.Object.PK, taxRecords_GetTaxRecordsWithLinePivots[2].PK), GetPivot(lineTaxableTransactionMock3.Object.PK, taxRecords_CalculateTaxRecords[2].PK));

			void CompareTaxRecords(AccTaxTransaction taxRecord1, AccTaxTransaction taxRecord2)
			{
				CombineAssertions(() =>
				{
					AssertNotEquals("ATT_AH will be different since two different parents are used", taxRecord1.ATT_AH, taxRecord2.ATT_AH);
					AssertEquals("ATT_GC", taxRecord1.ATT_GC, taxRecord2.ATT_GC);
					AssertEquals("ATT_AT_TaxID", taxRecord1.ATT_AT_TaxID, taxRecord2.ATT_AT_TaxID);
					AssertEquals("ATT_ETC", taxRecord1.ATT_ETC, taxRecord2.ATT_ETC);
					AssertEquals("ATT_TaxAuthorityServiceCode", taxRecord1.ATT_TaxAuthorityServiceCode, taxRecord2.ATT_TaxAuthorityServiceCode);
					AssertEquals("ATT_GB", taxRecord1.ATT_GB, taxRecord2.ATT_GB);
					AssertEquals("ATT_RateNumerator", taxRecord1.ATT_RateNumerator, taxRecord2.ATT_RateNumerator);
					AssertEquals("ATT_RateDenominator", taxRecord1.ATT_RateDenominator, taxRecord2.ATT_RateDenominator);

					AssertNotEquals("ATT_LocalTaxAmount", taxRecord1.ATT_LocalTaxAmount, taxRecord2.ATT_LocalTaxAmount);
					AssertEquals("taxRecord1.ATT_LocalTaxAmount", 0M, taxRecord1.ATT_LocalTaxAmount);
					AssertEquals("taxRecord2.ATT_LocalTaxAmount", 20M, taxRecord2.ATT_LocalTaxAmount);

					AssertNotEquals("ATT_LocalTaxBaseAmount", taxRecord1.ATT_LocalTaxBaseAmount, taxRecord2.ATT_LocalTaxBaseAmount);
					AssertEquals("taxRecord1.ATT_LocalTaxBaseAmount", 0M, taxRecord1.ATT_LocalTaxBaseAmount);
					AssertEquals("taxRecord2.ATT_LocalTaxBaseAmount", 200M, taxRecord2.ATT_LocalTaxBaseAmount);

					AssertNotEquals("ATT_OSTaxAmount", taxRecord1.ATT_OSTaxAmount, taxRecord2.ATT_OSTaxAmount);
					AssertEquals("taxRecord1.ATT_OSTaxAmount", 0M, taxRecord1.ATT_OSTaxAmount);
					AssertEquals("taxRecord2.ATT_OSTaxAmount", 10M, taxRecord2.ATT_OSTaxAmount);

					AssertNotEquals("ATT_OSTaxBaseAmount", taxRecord1.ATT_OSTaxBaseAmount, taxRecord2.ATT_OSTaxBaseAmount);
					AssertEquals("taxRecord1.ATT_OSTaxBaseAmount", 0M, taxRecord1.ATT_OSTaxBaseAmount);
					AssertEquals("taxRecord2.ATT_OSTaxBaseAmount", 100M, taxRecord2.ATT_OSTaxBaseAmount);
				});
			}

			void CheckLinePivotNotNull(AccTaxRecordTransactionLinePivot pivot1, AccTaxRecordTransactionLinePivot pivot2)
			{
				AssertNotNull(pivot1);
				AssertNotNull(pivot2);
			}
		}

		public void TestSetTaxAmounts()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1M, TestObjectCreator.AALSHI);
			var taxSystemsConfigCollection = new TaxSystemsConfigurationCollection();
			taxSystemsConfigCollection.Add(TaxFrameworkTestObjectCreator.CreateTaxSystem("PIB"));
			AccountingMasterFilesRegistry.Instance.TaxSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taxSystemsConfigCollection);
			taxSystemsConfigCollection.ClearCachedTaxSystemsByCode_ForTestOnly(Factory);

			var taxableTransactionLines = new List<ITaxableTransactionLine>();

			var taxRecord1 = Factory.NewWithValidTestData<AccTaxTransaction>();
			taxRecord1.ATT_AH = invoice.PK;
			ChangeOSCurrencyButRetainAmounts(taxRecord1, CurrencyCodes.UnitedStates);

			var taxRate1 = (21, 1);
			var taxRecord2 = Factory.NewWithValidTestData<AccTaxTransaction>();
			taxRecord2.ATT_AH = invoice.PK;
			ChangeOSCurrencyButRetainAmounts(taxRecord2, CurrencyCodes.UnitedStates);

			var taxRate2 = (10, 1);
			taxRecord2.ATT_AffectsSourceTransactionTotal = false;

			var taxRecord3 = Factory.NewWithValidTestData<AccTaxTransaction>();
			taxRecord3.ATT_AH = invoice.PK;
			ChangeOSCurrencyButRetainAmounts(taxRecord3, CurrencyCodes.UnitedStates);
			var taxRate3 = (5, 1);

			CreateAccTaxTransactionAddTaxableTransactionLines(taxRecord1, taxableTransactionLines, taxRate1);
			CreateAccTaxTransactionAddTaxableTransactionLines(taxRecord2, taxableTransactionLines, taxRate2);
			CreateAccTaxTransactionAddTaxableTransactionLines(taxRecord3, taxableTransactionLines, taxRate3);

			var taxRecords = new AccTaxTransaction[] { taxRecord1, taxRecord2, taxRecord3 };

			var taxRecordParentMock = CreateITaxRecordParentMock(Factory);
			taxRecordParentMock.SetupProperty(f => f.OSTaxAmount);
			taxRecordParentMock.SetupProperty(f => f.LocalTaxAmount);
			taxRecordParentMock.Setup(r => r.GetLines()).Returns(taxableTransactionLines);

			TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.SetTaxAmounts(taxRecords, taxFrameworkConfigurationHelper);

			AssertEquals("ATT_OSTaxBaseAmount", 400m, taxRecord1.ATT_OSTaxBaseAmount);
			AssertEquals("ATT_LocalTaxBaseAmount", 600m, taxRecord1.ATT_LocalTaxBaseAmount);
			AssertEquals("ATT_OSTaxAmount", 84m, taxRecord1.ATT_OSTaxAmount);
			AssertEquals("ATT_LocalTaxAmount", 126m, taxRecord1.ATT_LocalTaxAmount);

			AssertEquals("ATT_OSTaxBaseAmount", 400m, taxRecord2.ATT_OSTaxBaseAmount);
			AssertEquals("ATT_LocalTaxBaseAmount", 600m, taxRecord2.ATT_LocalTaxBaseAmount);
			AssertEquals("ATT_OSTaxAmount", 40m, taxRecord2.ATT_OSTaxAmount);
			AssertEquals("ATT_LocalTaxAmount", 60m, taxRecord2.ATT_LocalTaxAmount);

			AssertEquals("ATT_OSTaxBaseAmount", 400m, taxRecord3.ATT_OSTaxBaseAmount);
			AssertEquals("ATT_LocalTaxBaseAmount", 600m, taxRecord3.ATT_LocalTaxBaseAmount);
			AssertEquals("ATT_OSTaxAmount", 20m, taxRecord3.ATT_OSTaxAmount);
			AssertEquals("ATT_LocalTaxAmount", 30m, taxRecord3.ATT_LocalTaxAmount);
		}

		public void TestSetTaxAmounts_AssertExceptionThrown_TaxFrameworkUnknownConfigurationValueException_InExtensionMethod()
		{
			var company = TestObjectCreator.CreateNewCompany("CO1");

			var taxAuthority = TaxFrameworkTestObjectCreator.CreateTaxAuthority("BA");
			var taxSystem = TaxFrameworkTestObjectCreator.CreateTaxSystem("PIB");
			var taxConfig = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(company, taxAuthority, taxSystem, TaxConfigurationLedgers.AccountsReceivable.Code, true);

			var taxRecord = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { TaxConfiguration = taxConfig, TaxSystem = taxSystem });

			AssertExceptionThrown(typeof(TaxFrameworkUnknownConfigurationValueException), "Tax System PIB can't be found.", () => TaxRecordCalculator.TaxRecordCalculator_ForTestsOnly.SetTaxAmounts(new[] { taxRecord }, taxFrameworkConfigurationHelper));
		}

		public void TestCalculateTaxRecordsWithTaxDates()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var company = TestObjectCreator.CreateNewCompany("CO1");
			var branch = TestObjectCreator.CreateBranch("BR", company);
			var companyData = orgHeader.GetCompanyDataForGlbCompany(company);
			var taxAuthority = TaxFrameworkTestObjectCreator.CreateTaxAuthority("TA");
			var taxSystem = TaxFrameworkTestObjectCreator.CreateTaxSystem("TS");

			var taxConfigCompany = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(company, taxAuthority, taxSystem, TaxConfigurationLedgers.AccountsReceivable.Code);
			var taxConfigurationBranch = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(branch, taxAuthority, taxSystem, TaxConfigurationLedgers.AccountsReceivable.Code, true);
			var taxOverrideGroup = TaxFrameworkTestObjectCreator.CreateTaxOverrideGroup(company, taxConfigCompany);

			var taxSystemsConfigCollection = new TaxSystemsConfigurationCollection();
			taxSystemsConfigCollection.Add(taxSystem);
			AccountingMasterFilesRegistry.Instance.TaxSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taxSystemsConfigCollection);
			taxSystemsConfigCollection.ClearCachedTaxSystemsByCode_ForTestOnly(Factory);
			Factory.Save();

			var orgTaxConfig = TaxFrameworkTestObjectCreator.CreateOrgTaxConfiguration(taxConfigCompany, companyData);

			var chargeCode1 = TestObjectCreator.CreateChargeCode("CC1");
			var chargeCode2 = TestObjectCreator.CreateChargeCode("CC2");

			var taxFrameTaxOverrideGroup1 = CreateTaxOverrideAndPivot(company, taxConfigCompany, chargeCode1);
			var taxFrameTaxOverrideGroup2 = CreateTaxOverrideAndPivot(company, taxConfigCompany, chargeCode2);

			var orgTaxConfigBranch = TaxFrameworkTestObjectCreator.CreateOrgTaxConfiguration(taxConfigurationBranch, companyData);
			var taxOverrideGroup1 = TaxFrameworkTestObjectCreator.CreateTaxOverrideGroup(company, taxConfigurationBranch);

			var taxID1 = TaxFrameworkTestObjectCreator.CreateTaxRate_RateSource(TaxRateSources.TaxIDOnly.Code);
			var taxID2 = TaxFrameworkTestObjectCreator.CreateTaxRate_RateSource(TaxRateSources.TaxIDOnly.Code);

			var taxRule1 = TaxFrameworkTestObjectCreator.CreateTaxOverrideRule(taxFrameTaxOverrideGroup1, taxID1.PK, direction: OrgConstants.ServiceDirection.Code.Domestic);
			var taxRule2 = TaxFrameworkTestObjectCreator.CreateTaxOverrideRule(taxFrameTaxOverrideGroup2, taxID2.PK, direction: OrgConstants.ServiceDirection.Code.Domestic);
			Factory.Save();

			var taxRecordParentMock = CreateITaxRecordParentMock(Factory);
			taxRecordParentMock.SetupGet(f => f.Ledger).Returns(TaxConfigurationLedgers.AccountsReceivable.Code);
			taxRecordParentMock.SetupGet(f => f.Org).Returns(orgHeader);
			taxRecordParentMock.SetupGet(f => f.Company).Returns(company);
			taxRecordParentMock.SetupGet(f => f.Branch).Returns(GlbBranch.CurrentBranch);

			var parameters = new AccChargeTaxOverrideMatcher.TaxCalculationParameters { JobType = JobInvoicingConsumerTypes.Shipment.Code, Direction = Directions.Domestic };

			var taxDateEmpty = ZDate.Empty;
			var taxChargeCode1Date = new ZDate(2020, 5, 15);
			var taxChargeCode1DateOld1 = new ZDate(2020, 5, 1);

			var taxChargeCode2Date = new ZDate(2020, 4, 30);
			var taxChargeCode2DateOld2 = new ZDate(2020, 4, 25);
			var taxChargeCode2DateOld3 = new ZDate(2020, 4, 10);

			var lineTaxableTransactionMock1ChargeCode1 = CreateITaxableTransactionLineMock(Factory, ZGuid.NewZGuid(), "USD", taxChargeCode1Date, branch, chargeCode1, parameters);
			var lineTaxableTransactionMock2ChargeCode1DateOld1 = CreateITaxableTransactionLineMock(Factory, ZGuid.NewZGuid(), "USD", taxChargeCode1DateOld1, branch, chargeCode1, parameters);
			var lineTaxableTransactionMock3ChargeCode1DateEmpty = CreateITaxableTransactionLineMock(Factory, ZGuid.NewZGuid(), "USD", taxDateEmpty, branch, chargeCode1, parameters);

			var lineTaxableTransactionMock1ChargeCode2 = CreateITaxableTransactionLineMock(Factory, ZGuid.NewZGuid(), "USD", taxChargeCode2Date, branch, chargeCode2, parameters);
			var lineTaxableTransactionMock2ChargeCode2DateOld2 = CreateITaxableTransactionLineMock(Factory, ZGuid.NewZGuid(), "USD", taxChargeCode2DateOld2, branch, chargeCode2, parameters);
			var lineTaxableTransactionMock3ChargeCode2DateOld3 = CreateITaxableTransactionLineMock(Factory, ZGuid.NewZGuid(), "USD", taxChargeCode2DateOld3, branch, chargeCode2, parameters);
			var lineTaxableTransactionMock4ChargeCode2DateEmpty = CreateITaxableTransactionLineMock(Factory, ZGuid.NewZGuid(), "USD", taxDateEmpty, branch, chargeCode2, parameters);

			ITaxRecordCalculator taxCalculator = new TaxRecordCalculator(taxFrameworkConfigurationHelper);
			taxRecordParentMock.Setup(r => r.GetLines()).Returns(new List<ITaxableTransactionLine> { lineTaxableTransactionMock1ChargeCode1.Object, lineTaxableTransactionMock2ChargeCode1DateOld1.Object,
				lineTaxableTransactionMock3ChargeCode1DateEmpty.Object, lineTaxableTransactionMock1ChargeCode2.Object, lineTaxableTransactionMock2ChargeCode2DateOld2.Object, lineTaxableTransactionMock3ChargeCode2DateOld3.Object, lineTaxableTransactionMock4ChargeCode2DateEmpty.Object });
			var taxRecords = new List<AccTaxTransaction>();
			taxCalculator.CalculateTaxRecords(taxRecordParentMock.Object, taxRecords);
			AssertEquals($"taxRecords.Count 2 records", 2, taxRecords.Count);
			AssertTaxTransactionWithTaxDate(taxRecords[0], taxID1.PK, taxChargeCode1DateOld1);
			AssertTaxTransactionWithTaxDate(taxRecords[1], taxID2.PK, taxChargeCode2DateOld3);

			taxRecordParentMock.Setup(r => r.GetLines()).Returns(new List<ITaxableTransactionLine> { lineTaxableTransactionMock1ChargeCode1.Object, lineTaxableTransactionMock3ChargeCode1DateEmpty.Object,
				lineTaxableTransactionMock1ChargeCode2.Object, lineTaxableTransactionMock2ChargeCode2DateOld2.Object, lineTaxableTransactionMock4ChargeCode2DateEmpty.Object });
			taxRecords.Clear();
			taxCalculator.CalculateTaxRecords(taxRecordParentMock.Object, taxRecords);
			AssertEquals($"taxRecords.Count 2 records", 2, taxRecords.Count);
			AssertTaxTransactionWithTaxDate(taxRecords[0], taxID1.PK, taxChargeCode1Date);
			AssertTaxTransactionWithTaxDate(taxRecords[1], taxID2.PK, taxChargeCode2DateOld2);

			taxRecordParentMock.Setup(r => r.GetLines()).Returns(new List<ITaxableTransactionLine> { lineTaxableTransactionMock3ChargeCode1DateEmpty.Object,
				lineTaxableTransactionMock1ChargeCode2.Object, lineTaxableTransactionMock4ChargeCode2DateEmpty.Object });
			taxRecords.Clear();
			taxCalculator.CalculateTaxRecords(taxRecordParentMock.Object, taxRecords);
			AssertEquals($"taxRecords.Count 2 records", 2, taxRecords.Count);
			AssertTaxTransactionWithTaxDate(taxRecords[0], taxID1.PK, ZDate.Empty);
			AssertTaxTransactionWithTaxDate(taxRecords[1], taxID2.PK, taxChargeCode2Date);

			taxRecordParentMock.Setup(r => r.GetLines()).Returns(new List<ITaxableTransactionLine> { lineTaxableTransactionMock4ChargeCode2DateEmpty.Object });
			taxRecords.Clear();
			taxCalculator.CalculateTaxRecords(taxRecordParentMock.Object, taxRecords);
			AssertEquals($"taxRecords.Count 1 records", 1, taxRecords.Count);
			AssertTaxTransactionWithTaxDate(taxRecords[0], taxID2.PK, ZDate.Empty);

			void AssertTaxTransactionWithTaxDate(AccTaxTransaction accTaxTransaction, ZGuid taxIDPK, ZDate taxDate)
			{
				AssertEquals("ATT_AT_TaxID", taxIDPK, accTaxTransaction.ATT_AT_TaxID);
				AssertEquals("ATT_TaxDate", taxDate, accTaxTransaction.ATT_TaxDate);
			}
		}

		#region Helpers

		AccTaxOverrideGroup CreateTaxOverrideAndPivot(GlbCompany company, AccTaxConfiguration taxConfig, AccChargeCode chargeCode)
		{
			var taxFrameTaxOverrideGroup = TaxFrameworkTestObjectCreator.CreateTaxOverrideGroup(company, taxConfig);
			TaxFrameworkTestObjectCreator.CreateTaxOverrideGroupChargeCodePivot(taxFrameTaxOverrideGroup, chargeCode);

			return taxFrameTaxOverrideGroup;
		}

		void CreateAccTaxTransactionAddTaxableTransactionLines(AccTaxTransaction taxRecord, List<ITaxableTransactionLine> refTaxableTransactionLines, (ZInt numerator, ZInt denominator) taxRate)
		{
			taxRecord.ATT_TaxSystemCode = "PIB";
			taxRecord.ATT_RateNumerator = taxRate.numerator;
			taxRecord.ATT_RateDenominator = taxRate.denominator;

			var lineTaxableTransactionMock1 = new Mock<ITaxableTransactionLine>();
			lineTaxableTransactionMock1.SetupGet(f => f.Factory).Returns(Factory);
			lineTaxableTransactionMock1.SetupGet(f => f.PK).Returns(ZGuid.NewZGuid());
			lineTaxableTransactionMock1.SetupGet(f => f.BaseOSAmount).Returns(100m);
			lineTaxableTransactionMock1.SetupGet(f => f.LocalAmount).Returns(200m);

			var lineTaxableTransactionMock2 = new Mock<ITaxableTransactionLine>();
			lineTaxableTransactionMock2.SetupGet(f => f.Factory).Returns(Factory);
			lineTaxableTransactionMock2.SetupGet(f => f.PK).Returns(ZGuid.NewZGuid());
			lineTaxableTransactionMock2.SetupGet(f => f.BaseOSAmount).Returns(300m);
			lineTaxableTransactionMock2.SetupGet(f => f.LocalAmount).Returns(400m);

			var accTaxRecordTransactionLinePivot1 = Factory.New<AccTaxRecordTransactionLinePivot>();
			accTaxRecordTransactionLinePivot1.ATP_ATT = taxRecord.PK;
			accTaxRecordTransactionLinePivot1.LinkLine(lineTaxableTransactionMock1.Object);

			var accTaxRecordTransactionLinePivot2 = Factory.New<AccTaxRecordTransactionLinePivot>();
			accTaxRecordTransactionLinePivot2.ATP_ATT = taxRecord.PK;
			accTaxRecordTransactionLinePivot2.LinkLine(lineTaxableTransactionMock2.Object);

			refTaxableTransactionLines.Add(lineTaxableTransactionMock1.Object);
			refTaxableTransactionLines.Add(lineTaxableTransactionMock2.Object);
		}

		AccTaxRecordTransactionLinePivot GetPivot(ZGuid transactionLinePK, ZGuid taxTransactionPK)
		{
			var query = new ZQuery(AccTaxRecordTransactionLinePivotSchema.ATP_AL_TransactionLine, transactionLinePK);
			query.AddToFilter(AccTaxRecordTransactionLinePivotSchema.ATP_ATT, taxTransactionPK);
			return Factory.LoadTop1<AccTaxRecordTransactionLinePivot>(query);
		}

		List<AccTaxRecordTransactionLinePivot> GetPivots(ZGuid taxTransactionPK)
		{
			var query = new ZQuery(AccTaxRecordTransactionLinePivotSchema.ATP_ATT, taxTransactionPK);
			var pivots = Factory.Load<AccTaxRecordTransactionLinePivot>(query);
			if (pivots.Any())
			{
				return pivots.ToList();
			}
			else
			{
				return new List<AccTaxRecordTransactionLinePivot>();
			}
		}

		AccTaxTransaction GetTaxRecord(ZGuid taxIdPK, ZGuid taxConfigurationPK, (ZInt numerator, ZInt denominator) taxRate, ZGuid taxMessagePK, ZString taxAuthorityServiceCode)
		{
			var query = new ZQuery(AccTaxTransactionSchema.ATT_AT_TaxID, taxIdPK);
			query.AddToFilter(AccTaxTransactionSchema.ATT_ETC, taxConfigurationPK);
			query.AddToFilter(AccTaxTransactionSchema.ATT_RateNumerator, taxRate.numerator);
			query.AddToFilter(AccTaxTransactionSchema.ATT_RateDenominator, taxRate.denominator);
			query.AddToFilter(AccTaxTransactionSchema.ATT_A9_TaxMessage, taxMessagePK);
			query.AddToFilter(AccTaxTransactionSchema.ATT_TaxAuthorityServiceCode, taxAuthorityServiceCode);
			return Factory.LoadTop1<AccTaxTransaction>(query);
		}

		void ChangeOSCurrencyButRetainAmounts(AccTaxTransaction taxRecord, ZString newCurrency)
		{
			var osTaxBasAmount = taxRecord.ATT_OSTaxBaseAmount;
			var osTaxAmount = taxRecord.ATT_OSTaxAmount;
			taxRecord.ATT_OSTaxBaseAmount = 0;
			taxRecord.ATT_OSTaxAmount = 0;

			AssertNoExceptionThrown(() => taxRecord.ATT_RX_NKOSTaxCurrency = newCurrency);

			taxRecord.ATT_OSTaxBaseAmount = osTaxBasAmount;
			taxRecord.ATT_OSTaxAmount = osTaxAmount;
		}

		static Mock<ITaxRecordParentBase> CreateITaxRecordParentBaseMock(BusinessObjectFactory factory)
		{
			var taxParent = new Mock<ITaxRecordParentBase>();
			taxParent.SetupGet(f => f.Factory).Returns(factory);
			taxParent.SetupGet(f => f.PK).Returns(ZGuid.NewZGuid());
			taxParent.SetupGet(f => f.Company).Returns(GlbCompany.CurrentCompany);
			taxParent.SetupGet(f => f.Branch).Returns(GlbBranch.CurrentBranch);
			taxParent.SetupGet(f => f.Department).Returns(GlbDepartment.CurrentDepartment);
			taxParent.SetupGet(f => f.IsPosted).Returns(false);

			return taxParent;
		}

		static Mock<ITaxRecordParent> CreateITaxRecordParentMock(BusinessObjectFactory factory)
		{
			var taxParent = new Mock<ITaxRecordParent>();
			taxParent.SetupGet(f => f.Factory).Returns(factory);
			taxParent.SetupGet(f => f.PK).Returns(ZGuid.NewZGuid());
			taxParent.SetupGet(f => f.Company).Returns(GlbCompany.CurrentCompany);
			taxParent.SetupGet(f => f.Branch).Returns(GlbBranch.CurrentBranch);
			taxParent.SetupGet(f => f.Department).Returns(GlbDepartment.CurrentDepartment);
			taxParent.SetupGet(f => f.IsPosted).Returns(false);

			return taxParent;
		}

		static Mock<ITaxableTransactionLineBase> CreateITaxableTransactionLineBaseMock(BusinessObjectFactory factory, ZGuid pk, ZString currency, ZDate taxDate, GlbBranch branch = null, AccChargeCode chargeCode = null, AccChargeTaxOverrideMatcher.TaxCalculationParameters parameters = null)
		{
			var lineTaxableTransactionMock = new Mock<ITaxableTransactionLineBase>();
			lineTaxableTransactionMock.SetupGet(f => f.Factory).Returns(factory);
			lineTaxableTransactionMock.SetupGet(f => f.PK).Returns(pk);
			lineTaxableTransactionMock.SetupGet(f => f.Currency).Returns(currency);
			lineTaxableTransactionMock.SetupGet(f => f.TaxDate).Returns(taxDate);
			if (branch != null)
			{
				lineTaxableTransactionMock.SetupGet(f => f.Branch).Returns(branch);
			}
			if (chargeCode != null)
			{
				lineTaxableTransactionMock.SetupGet(f => f.ChargeCode).Returns(chargeCode);
			}

			if (parameters != null)
			{
				lineTaxableTransactionMock.Setup(r => r.GetTaxCalculationParameters()).Returns(parameters);
			}

			return lineTaxableTransactionMock;
		}

		static Mock<ITaxableTransactionLine> CreateITaxableTransactionLineMock(BusinessObjectFactory factory, ZGuid pk, ZString currency, ZDate taxDate, GlbBranch branch = null, AccChargeCode chargeCode = null, AccChargeTaxOverrideMatcher.TaxCalculationParameters parameters = null)
		{
			var lineTaxableTransactionMock = new Mock<ITaxableTransactionLine>();
			lineTaxableTransactionMock.SetupGet(f => f.Factory).Returns(factory);
			lineTaxableTransactionMock.SetupGet(f => f.PK).Returns(pk);
			lineTaxableTransactionMock.SetupGet(f => f.Currency).Returns(currency);
			lineTaxableTransactionMock.SetupGet(f => f.TaxDate).Returns(taxDate);
			if (branch != null)
			{
				lineTaxableTransactionMock.SetupGet(f => f.Branch).Returns(branch);
			}
			if (chargeCode != null)
			{
				lineTaxableTransactionMock.SetupGet(f => f.ChargeCode).Returns(chargeCode);
			}

			if (parameters != null)
			{
				lineTaxableTransactionMock.Setup(r => r.GetTaxCalculationParameters()).Returns(parameters);
			}

			return lineTaxableTransactionMock;
		}

		TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;

		TaxFrameworkTestObjectCreator TaxFrameworkTestObjectCreator
		{
			get { return taxFrameworkTestObjectCreator ?? (taxFrameworkTestObjectCreator = new TaxFrameworkTestObjectCreator(Factory)); }
		}
		TaxFrameworkTestObjectCreator taxFrameworkTestObjectCreator;

		#endregion
	}
}
