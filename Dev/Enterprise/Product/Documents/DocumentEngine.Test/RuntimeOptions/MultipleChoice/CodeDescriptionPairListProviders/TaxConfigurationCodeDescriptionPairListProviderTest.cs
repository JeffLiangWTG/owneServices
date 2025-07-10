using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting
{
	class TaxConfigurationCodeDescriptionPairListProviderTest : CodeDescriptionPairListProviderTest
	{
		public override void TestIsReturningCorrectCollection()
		{
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branchA.PK.ToGuid(), Guid.Empty))
			{
				var actualResult = CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList();
				var expectedResult = ExpectedTaxConfigurationsForCompanyA();

				AssertContainsExactElementsInAnyOrder(expectedResult, actualResult);
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branchB.PK.ToGuid(), Guid.Empty))
			{
				var actualResult = CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList();
				var expectedResult = ExpectedTaxConfigurationsForCompanyB();

				AssertContainsExactElementsInAnyOrder(expectedResult, actualResult);
			}
		}

		CodeDescriptionPairList ExpectedTaxConfigurationsForCompanyA()
		{
			var expectedResult = new CodeDescriptionPairList();

			expectedResult.AddPair(taxConfigA1_AR_Code, taxConfigA1_AR_Desc);
			expectedResult.AddPair(taxConfigA1_AP_Code, taxConfigA1_AP_Desc);
			expectedResult.AddPair(taxConfigA2_AR_Code, taxConfigA2_AR_Desc);
			expectedResult.AddPair(taxConfigA2_AP_Code, taxConfigA2_AP_Desc);

			return expectedResult;
		}

		CodeDescriptionPairList ExpectedTaxConfigurationsForCompanyB()
		{
			var expectedResult = new CodeDescriptionPairList();

			expectedResult.AddPair(taxConfigB1_AR_Code, taxConfigB1_AR_Desc);
			expectedResult.AddPair(taxConfigB1_AP_Code, taxConfigB1_AP_Desc);
			expectedResult.AddPair(taxConfigB2_AR_Code, taxConfigB2_AR_Desc);
			expectedResult.AddPair(taxConfigB2_AP_Code, taxConfigB2_AP_Desc);

			return expectedResult;
		}

		protected override void SetUp()
		{
			base.SetUp();

			factory = new BusinessObjectFactory();
			var companyA = factory.NewWithValidTestData<GlbCompany>();
			var companyB = factory.NewWithValidTestData<GlbCompany>();

			branchA = factory.NewWithValidTestData<GlbBranch>();
			branchA.GB_GC = companyA.PK;

			branchB = factory.NewWithValidTestData<GlbBranch>();
			branchB.GB_GC = companyB.PK;

			CreateTaxConfig(companyA, taxConfigA1_AR_Code, TaxConfigurationLedgers.AccountsReceivable.Code, taxConfigA1_AR_Desc);
			CreateTaxConfig(companyA, taxConfigA1_AP_Code, TaxConfigurationLedgers.AccountsPayable.Code, taxConfigA1_AP_Desc);
			CreateTaxConfig(branchA, taxConfigA2_AR_Code, TaxConfigurationLedgers.AccountsReceivable.Code, taxConfigA2_AR_Desc);
			CreateTaxConfig(branchA, taxConfigA2_AP_Code, TaxConfigurationLedgers.AccountsPayable.Code, taxConfigA2_AP_Desc);
			CreateTaxConfig(companyB, taxConfigB1_AR_Code, TaxConfigurationLedgers.AccountsReceivable.Code, taxConfigB1_AR_Desc);
			CreateTaxConfig(companyB, taxConfigB1_AP_Code, TaxConfigurationLedgers.AccountsPayable.Code, taxConfigB1_AP_Desc);
			CreateTaxConfig(branchB, taxConfigB2_AR_Code, TaxConfigurationLedgers.AccountsReceivable.Code, taxConfigB2_AR_Desc);
			CreateTaxConfig(branchB, taxConfigB2_AP_Code, TaxConfigurationLedgers.AccountsPayable.Code, taxConfigB2_AP_Desc);
			CreateTaxConfig(branchB, taxConfigB3_AR_Code, TaxConfigurationLedgers.AccountsReceivable.Code, taxConfigB3_AR_Desc, false);
			CreateTaxConfig(branchB, taxConfigB3_AP_Code, TaxConfigurationLedgers.AccountsPayable.Code, taxConfigB3_AP_Desc, false);

			factory.Save();
		}
		protected override ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider()
		{
			return new TaxConfigurationCodeDescriptionPairListProvider();
		}

		void CreateTaxConfig(BusinessObject parent, string code, string ledger, string desc, bool isActive = true)
		{
			var taxConfig = factory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfig.ETC_ParentTableCode = parent is GlbCompany ? GlbCompanySchema.Constants.Prefix : GlbBranchSchema.Constants.Prefix;
			taxConfig.ETC_ParentId = parent.PK;
			taxConfig.ETC_Ledger = ledger;
			taxConfig.ETC_IsActive = isActive;
			taxConfig.ETC_Code = code;
			taxConfig.ETC_Description = desc;
		}

		BusinessObjectFactory factory;
		internal GlbBranch branchA;
		internal GlbBranch branchB;

		internal string taxConfigA1_AR_Code = "A1-AR";
		internal string taxConfigA1_AR_Desc = "CompanyA AR config";

		internal string taxConfigA1_AP_Code = "A1-AP";
		internal string taxConfigA1_AP_Desc = "CompanyA AP config";

		internal string taxConfigA2_AR_Code = "A2-AR";
		internal string taxConfigA2_AR_Desc = "BranchA AR config";

		internal string taxConfigA2_AP_Code = "A2-AP";
		internal string taxConfigA2_AP_Desc = "BranchA AP config";

		internal string taxConfigB1_AR_Code = "B1-AR";
		internal string taxConfigB1_AR_Desc = "CompanyB AR config";

		internal string taxConfigB1_AP_Code = "B1-AP";
		internal string taxConfigB1_AP_Desc = "CompanyB AP config";

		internal string taxConfigB2_AR_Code = "B2-AR";
		internal string taxConfigB2_AR_Desc = "BranchB AR config";

		internal string taxConfigB2_AP_Code = "B2-AP";
		internal string taxConfigB2_AP_Desc = "BranchB AP config";

		const string taxConfigB3_AR_Code = "B3-AR";
		const string taxConfigB3_AR_Desc = "BranchB AR config 2";

		const string taxConfigB3_AP_Code = "B3-AP";
		const string taxConfigB3_AP_Desc = "BranchB AP config 2";
	}
}
