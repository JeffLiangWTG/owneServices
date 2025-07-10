using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.Accounting.Business.Testing.ScriptTests.TaxFramework
{
	class Report_OrgTaxProfileTest : ScriptTest
	{
		[TestDate(2020, 09, 10)]
		public void TestReport_OrgTaxProfile_Filters()
		{
			var companyX = TestObjectCreator.CreateNewCompany("XXX");

			var companyY = TestObjectCreator.CreateNewCompany("YYY");
			var branchY = TestObjectCreator.CreateNewBranch(companyY, "Y01");
			var branchY2 = TestObjectCreator.CreateNewBranch(companyY, "Y02");

			Factory.Save();

			List<Tuple<ZGuid, string>> pkReplacment;
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), branchY.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var orgY1 = TestObjectCreator.CreateOrgHeader("YOrg1", true, true, "AUSYD");
				orgY1.CompanyData.OB_GB_ControllingBranch = branchY2.PK;
				orgY1.CompanyData.OB_GC = companyY.PK;
				var orgY2 = TestObjectCreator.CreateOrgHeader("YOrg2", true, true, "NZAKL");
				orgY2.CompanyData.OB_GC = companyY.PK;
				var orgY3 = TestObjectCreator.CreateOrgHeader("YOrg3", true, true, "AUMEL");
				orgY3.OH_IsActive = false;
				orgY3.CompanyData.OB_GC = companyY.PK;
				orgY1.MainAddress.OA_State = orgY2.MainAddress.OA_State = orgY3.MainAddress.OA_State = "NSW";

				var taxConfig1 = ObjectCreator.CreateTaxConfiguration(companyY, TaxConfigurationLedgers.AccountsPayable.Code, true);
				taxConfig1.ETC_Code = "YTC1";
				taxConfig1.ETC_Description = "Test configuration YTC1";

				var taxConfig2 = ObjectCreator.CreateTaxConfiguration(companyY, TaxConfigurationLedgers.AccountsPayable.Code, false);
				taxConfig2.ETC_Code = "YTC2";
				taxConfig2.ETC_Description = "Test configuration YTC2";

				var taxConfig3 = ObjectCreator.CreateTaxConfiguration(companyY, TaxConfigurationLedgers.AccountsReceivable.Code, true);
				taxConfig3.ETC_Code = "YTC3";
				taxConfig3.ETC_Description = "Test configuration YTC3";

				var taxConfig4 = ObjectCreator.CreateTaxConfiguration(companyY, TaxConfigurationLedgers.AccountsReceivable.Code, false);
				taxConfig4.ETC_Code = "YTC4";
				taxConfig4.ETC_Description = "Test configuration YTC4";

				var taxConfig5 = ObjectCreator.CreateTaxConfiguration(branchY, TaxConfigurationLedgers.AccountsPayable.Code, true);
				taxConfig5.ETC_Code = "YTC5";
				taxConfig5.ETC_Description = "Test configuration YTC5";

				var taxConfig6 = ObjectCreator.CreateTaxConfiguration(branchY, TaxConfigurationLedgers.AccountsReceivable.Code, true);
				taxConfig6.ETC_Code = "YTC6";
				taxConfig6.ETC_Description = "Test configuration YTC6";

				Factory.Save();

				var taxOrgConfig1 = ObjectCreator.CreateOrgTaxConfiguration(taxConfig1, orgY1.CompanyData, true); //AP - ORGY1 - Active Org - Active Config
				taxOrgConfig1.OTC_IsThresholdUsed = true;

				var taxOrgConfig2 = ObjectCreator.CreateOrgTaxConfiguration(taxConfig3, orgY1.CompanyData, true); //AR - ORGY1 - Active Org - Active Config
				taxOrgConfig2.OTC_IsThresholdUsed = true;

				var taxOrgConfig3 = ObjectCreator.CreateOrgTaxConfiguration(taxConfig1, orgY2.CompanyData, true); //AP - ORGY2 - Active Org - Active Config
				var taxOrgConfig4 = ObjectCreator.CreateOrgTaxConfiguration(taxConfig3, orgY2.CompanyData, true); //AR - ORGY2 - Active Org - Active Config

				var taxOrgConfig5 = ObjectCreator.CreateOrgTaxConfiguration(taxConfig2, orgY3.CompanyData, true); //AP - ORGY3 - InActive Org - Active Config
				var taxOrgConfig6 = ObjectCreator.CreateOrgTaxConfiguration(taxConfig4, orgY3.CompanyData, true); //AR - ORGY3 - InActive Org - Active Config

				var taxOrgConfig7 = ObjectCreator.CreateOrgTaxConfiguration(taxConfig2, orgY2.CompanyData, true); //AP - ORGY2 - InActive Org - Active Config
				var taxOrgConfig8 = ObjectCreator.CreateOrgTaxConfiguration(taxConfig4, orgY2.CompanyData, true); //AR - ORGY2 - InActive Org - Active Config

				var taxOrgConfig9 = ObjectCreator.CreateOrgTaxConfiguration(taxConfig5, orgY1.CompanyData, false); //AP - ORGY1 - Active Org - InActive Config - Branch Level
				var taxOrgConfig10 = ObjectCreator.CreateOrgTaxConfiguration(taxConfig6, orgY1.CompanyData, false); //AR - ORGY1 - Active Org - InActive Config - Branch Level

				var taxOrgConfig11 = ObjectCreator.CreateOrgTaxConfiguration(taxConfig5, orgY2.CompanyData, false); //AP - ORGY2 - Active Org - InActive Config - Branch Level
				var taxOrgConfig12 = ObjectCreator.CreateOrgTaxConfiguration(taxConfig6, orgY2.CompanyData, false); //AR - ORGY2 - Active Org - InActive Config - Branch Level

				Factory.Save();

				pkReplacment = new List<Tuple<ZGuid, string>>(new[]
				{
						new Tuple<ZGuid, string>(taxConfig1.PK, "taxConfig1"),
						new Tuple<ZGuid, string>(taxConfig2.PK, "taxConfig1"),
						new Tuple<ZGuid, string>(taxConfig3.PK, "taxConfig3"),
						new Tuple<ZGuid, string>(taxConfig4.PK, "taxConfig4"),
						new Tuple<ZGuid, string>(taxConfig5.PK, "taxConfig5"),
						new Tuple<ZGuid, string>(taxConfig6.PK, "taxConfig6"),

						new Tuple<ZGuid, string>(taxOrgConfig1.PK, "taxOrgConfig1"),
						new Tuple<ZGuid, string>(taxOrgConfig2.PK, "taxOrgConfig2"),
						new Tuple<ZGuid, string>(taxOrgConfig3.PK, "taxOrgConfig3"),
						new Tuple<ZGuid, string>(taxOrgConfig4.PK, "taxOrgConfig4"),
						new Tuple<ZGuid, string>(taxOrgConfig5.PK, "taxOrgConfig5"),
						new Tuple<ZGuid, string>(taxOrgConfig6.PK, "taxOrgConfig6"),
						new Tuple<ZGuid, string>(taxOrgConfig7.PK, "taxOrgConfig7"),
						new Tuple<ZGuid, string>(taxOrgConfig8.PK, "taxOrgConfig8"),
						new Tuple<ZGuid, string>(taxOrgConfig9.PK, "taxOrgConfig9"),
						new Tuple<ZGuid, string>(taxOrgConfig10.PK, "taxOrgConfig10"),
						new Tuple<ZGuid, string>(taxOrgConfig11.PK, "taxOrgConfig11"),
						new Tuple<ZGuid, string>(taxOrgConfig12.PK, "taxOrgConfig12"),
						new Tuple<ZGuid, string>(branchY.PK, "branchY"),
						new Tuple<ZGuid, string>(branchY2.PK, "branchY2"),
				});
			}

			var dataTable = RunScript(companyX.PK.ToGuid());
			AssertEquals(0, dataTable.Rows.Count);

			dataTable = RunScript(companyY.PK.ToGuid(), TaxConfigurationLedgers.AccountsReceivable.Code, "Active", "", Core.Constants.CountryCodes.Australia, "Active");
			var expectedResult = @"
OH_Code      OH_FullName                                                                                          OH_Category OH_RL_NKClosestPort CountryCode OA_State                  OA_City                                            OB_GB_ControllingBranch              BranchCode OH_IsTempAccount TaxConfigurationTemplate             OH_IsActive ETC_PK                               ETC_Code                       ETC_Description                                                                  TaxBranchPK                          TaxBranch OTC_IsActive OTC_RecoverTax IsThresholdUsed
------------ ---------------------------------------------------------------------------------------------------- ----------- ------------------- ----------- ------------------------- -------------------------------------------------- ------------------------------------ ---------- ---------------- ------------------------------------ ----------- ------------------------------------ ------------------------------ -------------------------------------------------------------------------------- ------------------------------------ --------- ------------ -------------- ---------------
TESNAMSYD    Test Company Name                                                                                    BUS         AUSYD               AU          NSW                       Alexandria                                         branchY2                             Y02        0                NULL                                 1           taxConfig3                           YTC3                           Test configuration YTC3                                                          NULL                                 NULL      1            0              Y
";
			AssertTableAsTextFromSQLServerManagenentStudio("AR - Active - Empty - AU - Active", dataTable, expectedResult, new List<string>() { }, pkReplacment);

			dataTable = RunScript(companyY.PK.ToGuid(), TaxConfigurationLedgers.AccountsReceivable.Code, "Active", "", Core.Constants.CountryCodes.NewZealand, "Active");
			expectedResult = @"
OH_Code      OH_FullName                                                                                          OH_Category OH_RL_NKClosestPort CountryCode OA_State                  OA_City                                            OB_GB_ControllingBranch              BranchCode OH_IsTempAccount TaxConfigurationTemplate             OH_IsActive ETC_PK                               ETC_Code                       ETC_Description                                                                  TaxBranchPK                          TaxBranch OTC_IsActive OTC_RecoverTax IsThresholdUsed
------------ ---------------------------------------------------------------------------------------------------- ----------- ------------------- ----------- ------------------------- -------------------------------------------------- ------------------------------------ ---------- ---------------- ------------------------------------ ----------- ------------------------------------ ------------------------------ -------------------------------------------------------------------------------- ------------------------------------ --------- ------------ -------------- ---------------
TESNAMAKL    Test Company Name                                                                                    BUS         NZAKL               NZ          NSW                       Alexandria                                         NULL                                 NULL       0                NULL                                 1           taxConfig3                           YTC3                           Test configuration YTC3                                                          NULL                                 NULL      1            0              N
TESNAMAKL    Test Company Name                                                                                    BUS         NZAKL               NZ          NSW                       Alexandria                                         NULL                                 NULL       0                NULL                                 1           taxConfig4                           YTC4                           Test configuration YTC4                                                          NULL                                 NULL      1            0              N
";
			AssertTableAsTextFromSQLServerManagenentStudio("AR - Active - Empty - NZ - Active", dataTable, expectedResult, new List<string>() { }, pkReplacment);

			dataTable = RunScript(companyY.PK.ToGuid(), TaxConfigurationLedgers.AccountsReceivable.Code, "Active", "", Core.Constants.CountryCodes.Australia, "inactive");
			expectedResult = @"
OH_Code      OH_FullName                                                                                          OH_Category OH_RL_NKClosestPort CountryCode OA_State                  OA_City                                            OB_GB_ControllingBranch              BranchCode OH_IsTempAccount TaxConfigurationTemplate             OH_IsActive ETC_PK                               ETC_Code                       ETC_Description                                                                  TaxBranchPK                          TaxBranch OTC_IsActive OTC_RecoverTax IsThresholdUsed
------------ ---------------------------------------------------------------------------------------------------- ----------- ------------------- ----------- ------------------------- -------------------------------------------------- ------------------------------------ ---------- ---------------- ------------------------------------ ----------- ------------------------------------ ------------------------------ -------------------------------------------------------------------------------- ------------------------------------ --------- ------------ -------------- ---------------
TESNAMSYD    Test Company Name                                                                                    BUS         AUSYD               AU          NSW                       Alexandria                                         branchY2                             Y02        0                NULL                                 1           taxConfig6                           YTC6                           Test configuration YTC6                                                          branchY                              Y01       0            0              N
";
			AssertTableAsTextFromSQLServerManagenentStudio("AR - Active - Empty - AU - Inactive", dataTable, expectedResult, new List<string>() { }, pkReplacment);

			dataTable = RunScript(companyY.PK.ToGuid(), TaxConfigurationLedgers.AccountsReceivable.Code, "INACTIVE", "", Core.Constants.CountryCodes.Australia, "inactive");
			expectedResult = @"
OH_Code      OH_FullName                                                                                          OH_Category OH_RL_NKClosestPort CountryCode OA_State                  OA_City                                            OB_GB_ControllingBranch              BranchCode OH_IsTempAccount TaxConfigurationTemplate             OH_IsActive ETC_PK                               ETC_Code                       ETC_Description                                                                  TaxBranchPK                          TaxBranch OTC_IsActive OTC_RecoverTax IsThresholdUsed
------------ ---------------------------------------------------------------------------------------------------- ----------- ------------------- ----------- ------------------------- -------------------------------------------------- ------------------------------------ ---------- ---------------- ------------------------------------ ----------- ------------------------------------ ------------------------------ -------------------------------------------------------------------------------- ------------------------------------ --------- ------------ -------------- ---------------
";
			AssertTableAsTextFromSQLServerManagenentStudio("AR - Inactive - Empty - AU - Inactive", dataTable, expectedResult, new List<string>() { }, pkReplacment);

			dataTable = RunScript(companyY.PK.ToGuid(), TaxConfigurationLedgers.AccountsPayable.Code, "Active", "", Core.Constants.CountryCodes.Australia, "Active");
			expectedResult = @"
OH_Code      OH_FullName                                                                                          OH_Category OH_RL_NKClosestPort CountryCode OA_State                  OA_City                                            OB_GB_ControllingBranch              BranchCode OH_IsTempAccount TaxConfigurationTemplate             OH_IsActive ETC_PK                               ETC_Code                       ETC_Description                                                                  TaxBranchPK                          TaxBranch OTC_IsActive OTC_RecoverTax IsThresholdUsed
------------ ---------------------------------------------------------------------------------------------------- ----------- ------------------- ----------- ------------------------- -------------------------------------------------- ------------------------------------ ---------- ---------------- ------------------------------------ ----------- ------------------------------------ ------------------------------ -------------------------------------------------------------------------------- ------------------------------------ --------- ------------ -------------- ---------------
TESNAMSYD    Test Company Name                                                                                    BUS         AUSYD               AU          NSW                       Alexandria                                         branchY2                             Y02        0                NULL                                 1           taxConfig1                           YTC1                           Test configuration YTC1                                                          NULL                                 NULL      1            0              Y
";
			AssertTableAsTextFromSQLServerManagenentStudio("AP - Active - Empty - AU - Active", dataTable, expectedResult, new List<string>() { }, pkReplacment);
		}

		public void TestReport_OrgTaxProfile_OrgHeaderCategoryFilter()
		{
			var company = TestObjectCreator.CreateNewCompany("XXX");
			var branch = TestObjectCreator.CreateNewBranch(company, "X01");

			Factory.Save();

			List<Tuple<ZGuid, string>> pkReplacment;
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var org1 = TestObjectCreator.CreateOrgHeader("Org1", true, true, "AUSYD");
				org1.CompanyData.OB_GC = company.PK;
				var org2 = TestObjectCreator.CreateOrgHeader("Org2", true, true, "AUBNE");
				org2.CompanyData.OB_GC = company.PK;
				var org3 = TestObjectCreator.CreateOrgHeader("Org3", true, true, "AUMEL");
				org3.CompanyData.OB_GC = company.PK;

				var address1 = org1.Addresses[0];
				address1.AddressCapability.SetIsNotMainAddress(OrgAddressType.Office.Code);
				address1.OA_City = "Org1 City1";
				address1.OA_State = "AAA";

				var address2 = org1.Addresses.AddNew();
				address2.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);
				address2.OA_City = "Org1 City2";
				address2.OA_State = "SSS";
				address2.OA_Address1 = "Org1 Main Office";

				org1.CompanyData.OB_GB_ControllingBranch = org2.CompanyData.OB_GB_ControllingBranch = org3.CompanyData.OB_GB_ControllingBranch = branch.PK;
				org2.MainAddress.OA_State = org3.MainAddress.OA_State = "NSW";
				org1.OH_Category = "BUS";
				org2.OH_Category = "NAT";
				org3.OH_Category = "NAT";

				var taxConfig1 = ObjectCreator.CreateTaxConfiguration(company, TaxConfigurationLedgers.AccountsPayable.Code);
				taxConfig1.ETC_Code = "TC1";
				taxConfig1.ETC_Description = "Test configuration TC1";

				var taxConfig2 = ObjectCreator.CreateTaxConfiguration(company, TaxConfigurationLedgers.AccountsReceivable.Code);
				taxConfig2.ETC_Code = "TC2";
				taxConfig2.ETC_Description = "Test configuration TC2";

				Factory.Save();

				var taxOrgConfig1 = ObjectCreator.CreateOrgTaxConfiguration(taxConfig1, org1.CompanyData);
				var taxOrgConfig2 = ObjectCreator.CreateOrgTaxConfiguration(taxConfig1, org2.CompanyData);

				var taxOrgConfig3 = ObjectCreator.CreateOrgTaxConfiguration(taxConfig2, org1.CompanyData);
				var taxOrgConfig4 = ObjectCreator.CreateOrgTaxConfiguration(taxConfig2, org2.CompanyData);
				var taxOrgConfig5 = ObjectCreator.CreateOrgTaxConfiguration(taxConfig2, org3.CompanyData);

				var orgAPTemplate = ObjectCreator.CreateAccOrgTaxConfigurationTemplate("APT", false, company.PK);
				var orgARTemplate = ObjectCreator.CreateAccOrgTaxConfigurationTemplate("ART", true, company.PK);

				ObjectCreator.LinkOrgHeaderWithTaxConfigurationTemplate(orgAPTemplate, org1);
				ObjectCreator.LinkOrgHeaderWithTaxConfigurationTemplate(orgARTemplate, org1);
				ObjectCreator.LinkOrgHeaderWithTaxConfigurationTemplate(orgAPTemplate, org2);
				ObjectCreator.LinkOrgHeaderWithTaxConfigurationTemplate(orgARTemplate, org2);

				Factory.Save();

				pkReplacment = new List<Tuple<ZGuid, string>>(new[]
				{
						new Tuple<ZGuid, string>(branch.PK, "branch"),
						new Tuple<ZGuid, string>(taxConfig1.PK, "taxConfig1"),
						new Tuple<ZGuid, string>(taxConfig2.PK, "taxConfig1"),
						new Tuple<ZGuid, string>(taxOrgConfig1.PK, "taxOrgConfig1"),
						new Tuple<ZGuid, string>(taxOrgConfig2.PK, "taxOrgConfig2"),
						new Tuple<ZGuid, string>(taxOrgConfig3.PK, "taxOrgConfig3"),
						new Tuple<ZGuid, string>(taxOrgConfig4.PK, "taxOrgConfig4"),
						new Tuple<ZGuid, string>(taxOrgConfig5.PK, "taxOrgConfig5"),
						new Tuple<ZGuid, string>(orgAPTemplate.PK, "orgAPTemplate"),
						new Tuple<ZGuid, string>(orgARTemplate.PK, "orgARTemplate"),
				});
			}

			var dataTable = RunScript(company.PK.ToGuid(), TaxConfigurationLedgers.AccountsPayable.Code, orgHeaderCategoryCode: "");
			AssertEquals(0, dataTable.Rows.Count);

			dataTable = RunScript(company.PK.ToGuid(), TaxConfigurationLedgers.AccountsReceivable.Code, orgHeaderCategoryCode: "");
			AssertEquals(0, dataTable.Rows.Count);

			dataTable = RunScript(company.PK.ToGuid(), TaxConfigurationLedgers.AccountsPayable.Code, orgHeaderCategoryCode: "ALL");
			var expectedResult = @"
OH_Code      OH_FullName                                                                                          OH_Category OH_RL_NKClosestPort CountryCode OA_State                  OA_City                                            OB_GB_ControllingBranch              BranchCode OH_IsTempAccount TaxConfigurationTemplate             OH_IsActive ETC_PK                               ETC_Code                       ETC_Description                                                                  TaxBranchPK                          TaxBranch OTC_IsActive OTC_RecoverTax IsThresholdUsed
------------ ---------------------------------------------------------------------------------------------------- ----------- ------------------- ----------- ------------------------- -------------------------------------------------- ------------------------------------ ---------- ---------------- ------------------------------------ ----------- ------------------------------------ ------------------------------ -------------------------------------------------------------------------------- ------------------------------------ --------- ------------ -------------- ---------------
TESNAMSYD    Test Company Name                                                                                    BUS         AUSYD               AU          SSS                       Org1 City2                                         branch                               X01        0                APT                                  1           taxConfig1                           TC1                            Test configuration TC1                                                           NULL                                 NULL      1            0              N
TESNAMBNE    Test Company Name                                                                                    NAT         AUBNE               AU          NSW                       Alexandria                                         branch                               X01        0                APT                                  1           taxConfig1                           TC1                            Test configuration TC1                                                           NULL                                 NULL      1            0              N
";
			AssertTableAsTextFromSQLServerManagenentStudio("AP - OrgHeader Category: ALL", dataTable, expectedResult, new List<string>() { }, pkReplacment);

			dataTable = RunScript(company.PK.ToGuid(), TaxConfigurationLedgers.AccountsReceivable.Code, orgHeaderCategoryCode: "ALL");
			expectedResult = @"
OH_Code      OH_FullName                                                                                          OH_Category OH_RL_NKClosestPort CountryCode OA_State                  OA_City                                            OB_GB_ControllingBranch              BranchCode OH_IsTempAccount TaxConfigurationTemplate             OH_IsActive ETC_PK                               ETC_Code                       ETC_Description                                                                  TaxBranchPK                          TaxBranch OTC_IsActive OTC_RecoverTax IsThresholdUsed
------------ ---------------------------------------------------------------------------------------------------- ----------- ------------------- ----------- ------------------------- -------------------------------------------------- ------------------------------------ ---------- ---------------- ------------------------------------ ----------- ------------------------------------ ------------------------------ -------------------------------------------------------------------------------- ------------------------------------ --------- ------------ -------------- ---------------
TESNAMSYD    Test Company Name                                                                                    BUS         AUSYD               AU          SSS                       Org1 City2                                         branch                               X01        0                ART                                  1           taxConfig1                           TC2                            Test configuration TC2                                                           NULL                                 NULL      1            0              N
TESNAMBNE    Test Company Name                                                                                    NAT         AUBNE               AU          NSW                       Alexandria                                         branch                               X01        0                ART                                  1           taxConfig1                           TC2                            Test configuration TC2                                                           NULL                                 NULL      1            0              N
TESNAMMEL    Test Company Name                                                                                    NAT         AUMEL               AU          NSW                       Alexandria                                         branch                               X01        0                NULL                                 1           taxConfig1                           TC2                            Test configuration TC2                                                           NULL                                 NULL      1            0              N
";
			AssertTableAsTextFromSQLServerManagenentStudio("AR - OrgHeader Category: ALL", dataTable, expectedResult, new List<string>() { }, pkReplacment);

			dataTable = RunScript(company.PK.ToGuid(), TaxConfigurationLedgers.AccountsPayable.Code, orgHeaderCategoryCode: "NAT");
			expectedResult = @"
OH_Code      OH_FullName                                                                                          OH_Category OH_RL_NKClosestPort CountryCode OA_State                  OA_City                                            OB_GB_ControllingBranch              BranchCode OH_IsTempAccount TaxConfigurationTemplate             OH_IsActive ETC_PK                               ETC_Code                       ETC_Description                                                                  TaxBranchPK                          TaxBranch OTC_IsActive OTC_RecoverTax IsThresholdUsed
------------ ---------------------------------------------------------------------------------------------------- ----------- ------------------- ----------- ------------------------- -------------------------------------------------- ------------------------------------ ---------- ---------------- ------------------------------------ ----------- ------------------------------------ ------------------------------ -------------------------------------------------------------------------------- ------------------------------------ --------- ------------ -------------- ---------------
TESNAMBNE    Test Company Name                                                                                    NAT         AUBNE               AU          NSW                       Alexandria                                         branch                               X01        0                APT                                  1           taxConfig1                           TC1                            Test configuration TC1                                                           NULL                                 NULL      1            0              N
";
			AssertTableAsTextFromSQLServerManagenentStudio("AP - OrgHeader Category: NAT", dataTable, expectedResult, new List<string>() { }, pkReplacment);

			dataTable = RunScript(company.PK.ToGuid(), TaxConfigurationLedgers.AccountsReceivable.Code, orgHeaderCategoryCode: "NAT");
			expectedResult = @"
OH_Code      OH_FullName                                                                                          OH_Category OH_RL_NKClosestPort CountryCode OA_State                  OA_City                                            OB_GB_ControllingBranch              BranchCode OH_IsTempAccount TaxConfigurationTemplate             OH_IsActive ETC_PK                               ETC_Code                       ETC_Description                                                                  TaxBranchPK                          TaxBranch OTC_IsActive OTC_RecoverTax IsThresholdUsed
------------ ---------------------------------------------------------------------------------------------------- ----------- ------------------- ----------- ------------------------- -------------------------------------------------- ------------------------------------ ---------- ---------------- ------------------------------------ ----------- ------------------------------------ ------------------------------ -------------------------------------------------------------------------------- ------------------------------------ --------- ------------ -------------- ---------------
TESNAMBNE    Test Company Name                                                                                    NAT         AUBNE               AU          NSW                       Alexandria                                         branch                               X01        0                ART                                  1           taxConfig1                           TC2                            Test configuration TC2                                                           NULL                                 NULL      1            0              N
TESNAMMEL    Test Company Name                                                                                    NAT         AUMEL               AU          NSW                       Alexandria                                         branch                               X01        0                NULL                                 1           taxConfig1                           TC2                            Test configuration TC2                                                           NULL                                 NULL      1            0              N
";
			AssertTableAsTextFromSQLServerManagenentStudio("AR - OrgHeader Category: NAT", dataTable, expectedResult, new List<string>() { }, pkReplacment);
		}

		DataTable RunScript(Guid companyPK, string ledger = "AR", string accountTypeActive = "Active", string organisationPKs = "", string countryCode = "AU", string taxCodeActive = "", string orgHeaderCategoryCode = "ALL")
		{
			return DataUtils.GetDataTableFromQuery(Db.Connection, $@"
SELECT * 
FROM Report_OrgTaxProfile(
	'{companyPK}',
	'{ledger}',
	'{accountTypeActive}',
	'{organisationPKs}',
	'{countryCode}',
	'{taxCodeActive}',
	'{orgHeaderCategoryCode}'
) ORDER BY ETC_Code"
			);
		}

		AccountingTestObjectCreator ObjectCreator => objectCreator ?? (objectCreator = new AccountingTestObjectCreator(Factory));
		AccountingTestObjectCreator objectCreator;
	}
}
