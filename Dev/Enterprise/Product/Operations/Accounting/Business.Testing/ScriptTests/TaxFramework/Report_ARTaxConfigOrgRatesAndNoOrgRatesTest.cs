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
	public class Report_ARTaxConfigOrgRatesAndNoOrgRatesTest : ScriptTest
	{
		[TestDate(2023, 04, 20)]
		public void TestARTaxConfigOrgRates_FilterByOrgCategoryAndIncludeInactiveTaxConfigAndOrg()
		{
			var (companyX, pkReplacment) = SetupData();
			var dateOnlyColumns = new[] { "OTR_StartDate", "OTR_EndDate" };
			var startDate = ZDate.Today;
			var endDate = ZDate.Today.AddDays(100);

			var dataTable = RunScriptForOrgRates(companyX.PK.ToGuid(), startDate, endDate, 'Y', 'Y', "");
			AssertEquals(0, dataTable.Rows.Count);

			dataTable = RunScriptForOrgRates(companyX.PK.ToGuid(), startDate, endDate, 'Y', 'Y', "BUS");
			AssertEquals(2, dataTable.Rows.Count);

			var expectedResult = @"
OH_Code OH_FullName OH_Category OH_RL_NKClosestPort CountryCode OA_State OA_City OH_IsActive ETC_Code ETC_Description OTC_IsActive OTR_StartDate OTR_EndDate OTR_Source OTR_RateNumerator OTR_RateDenominator TaxRate 
------------ ---------------------------------------------------------------------------------------------------- ----------- ------------------- ----------- ------------------------- -------------------------------------------------- ------------------------------------ ---------- ---------------- ------------------------------------ ----------- ------------------------------------ ------------------------------ -------------------------------------------------------------------------------- ------------------------------------ 
TESNAMSYD Test Company Name BUS AUSYD AU NSW Alexandria 1 XTC1 Test configuration XTC1 1 2023-05-10 2023-05-15 MOV 5 1 5.00 
TESNAMSYD Test Company Name BUS AUSYD AU NSW Alexandria 1 XTC1 Test configuration XTC1 1 2023-05-30 2023-06-04 MOV 10 1 10.00 ";

			AssertTableAsTextFromSQLServerManagenentStudio("AR - IncludeInActiveConfig - IncludeInActiveOrganization - BUS", dataTable, expectedResult, new List<string>() { }, dateOnlyColumns, pkReplacment);

			dataTable = RunScriptForOrgRates(companyX.PK.ToGuid(), startDate, endDate, 'Y', 'Y', "ALL");
			AssertEquals(8, dataTable.Rows.Count);

			dataTable = RunScriptForOrgRates(companyX.PK.ToGuid(), startDate, endDate, 'Y', 'Y');
			AssertEquals(8, dataTable.Rows.Count);

			expectedResult = @"
OH_Code OH_FullName OH_Category OH_RL_NKClosestPort CountryCode OA_State OA_City OH_IsActive ETC_Code ETC_Description OTC_IsActive OTR_StartDate OTR_EndDate OTR_Source OTR_RateNumerator OTR_RateDenominator TaxRate 
------------ ---------------------------------------------------------------------------------------------------- ----------- ------------------- ----------- ------------------------- -------------------------------------------------- ------------------------------------ ---------- ---------------- ------------------------------------ ----------- ------------------------------------ ------------------------------ -------------------------------------------------------------------------------- ------------------------------------ 
TESNAMBNE Test Company Name NAT AUBNE AU NSW Alexandria 1 XTC1 Test configuration XTC1 0 2023-05-30 2023-06-04 MOV 15 1 15.00 
TESNAMBNE Test Company Name NAT AUBNE AU NSW Alexandria 1 XTC1 Test configuration XTC1 0 2023-06-19 2023-06-24 MOV 20 1 20.00 
TESNAMMEL Test Company Name GOV AUMEL AU NSW Alexandria 0 XTC1 Test configuration XTC1 1 2023-06-19 2023-06-24 MOV 25 1 25.00 
TESNAMMEL Test Company Name GOV AUMEL AU NSW Alexandria 0 XTC1 Test configuration XTC1 1 2023-07-09 2023-07-14 MOV 30 1 30.00 
TESNAMPER Test Company Name NGO AUPER AU NSW Alexandria 0 XTC1 Test configuration XTC1 0 2023-06-29 2023-07-04 MOV 35 1 35.00 
TESNAMPER Test Company Name NGO AUPER AU NSW Alexandria 0 XTC1 Test configuration XTC1 0 2023-07-19 2023-07-24 MOV 40 1 40.00 
TESNAMSYD Test Company Name BUS AUSYD AU NSW Alexandria 1 XTC1 Test configuration XTC1 1 2023-05-10 2023-05-15 MOV 5 1 5.00 
TESNAMSYD Test Company Name BUS AUSYD AU NSW Alexandria 1 XTC1 Test configuration XTC1 1 2023-05-30 2023-06-04 MOV 10 1 10.00 ";

			AssertTableAsTextFromSQLServerManagenentStudio("AR - IncludeInActiveConfig - IncludeInActiveOrganization - ALL", dataTable, expectedResult, new List<string>() { }, dateOnlyColumns, pkReplacment);

			dataTable = RunScriptForOrgRates(companyX.PK.ToGuid(), startDate, endDate, 'N', 'Y');
			AssertEquals(4, dataTable.Rows.Count);

			expectedResult = @"
OH_Code OH_FullName OH_Category OH_RL_NKClosestPort CountryCode OA_State OA_City OH_IsActive ETC_Code ETC_Description OTC_IsActive OTR_StartDate OTR_EndDate OTR_Source OTR_RateNumerator OTR_RateDenominator TaxRate 
------------ ---------------------------------------------------------------------------------------------------- ----------- ------------------- ----------- ------------------------- -------------------------------------------------- ------------------------------------ ---------- ---------------- ------------------------------------ ----------- ------------------------------------ ------------------------------ -------------------------------------------------------------------------------- ------------------------------------ 
TESNAMMEL Test Company Name GOV AUMEL AU NSW Alexandria 0 XTC1 Test configuration XTC1 1 2023-06-19 2023-06-24 MOV 25 1 25.00 
TESNAMMEL Test Company Name GOV AUMEL AU NSW Alexandria 0 XTC1 Test configuration XTC1 1 2023-07-09 2023-07-14 MOV 30 1 30.00 
TESNAMSYD Test Company Name BUS AUSYD AU NSW Alexandria 1 XTC1 Test configuration XTC1 1 2023-05-10 2023-05-15 MOV 5 1 5.00 
TESNAMSYD Test Company Name BUS AUSYD AU NSW Alexandria 1 XTC1 Test configuration XTC1 1 2023-05-30 2023-06-04 MOV 10 1 10.00 ";

			AssertTableAsTextFromSQLServerManagenentStudio("AR - OnlyActiveConfig - IncludeInActiveOrganization - ALL", dataTable, expectedResult, new List<string>() { }, dateOnlyColumns, pkReplacment);

			dataTable = RunScriptForOrgRates(companyX.PK.ToGuid(), startDate, endDate, 'Y', 'N');
			AssertEquals(4, dataTable.Rows.Count);

			expectedResult = @"
OH_Code OH_FullName OH_Category OH_RL_NKClosestPort CountryCode OA_State OA_City OH_IsActive ETC_Code ETC_Description OTC_IsActive OTR_StartDate OTR_EndDate OTR_Source OTR_RateNumerator OTR_RateDenominator TaxRate 
------------ ---------------------------------------------------------------------------------------------------- ----------- ------------------- ----------- ------------------------- -------------------------------------------------- ------------------------------------ ---------- ---------------- ------------------------------------ ----------- ------------------------------------ ------------------------------ -------------------------------------------------------------------------------- ------------------------------------ 
TESNAMBNE Test Company Name NAT AUBNE AU NSW Alexandria 1 XTC1 Test configuration XTC1 0 2023-05-30 2023-06-04 MOV 15 1 15.00 
TESNAMBNE Test Company Name NAT AUBNE AU NSW Alexandria 1 XTC1 Test configuration XTC1 0 2023-06-19 2023-06-24 MOV 20 1 20.00 
TESNAMSYD Test Company Name BUS AUSYD AU NSW Alexandria 1 XTC1 Test configuration XTC1 1 2023-05-10 2023-05-15 MOV 5 1 5.00 
TESNAMSYD Test Company Name BUS AUSYD AU NSW Alexandria 1 XTC1 Test configuration XTC1 1 2023-05-30 2023-06-04 MOV 10 1 10.00 ";

			AssertTableAsTextFromSQLServerManagenentStudio("AR - IncludeInActiveConfig - OnlyActiveOrganization - ALL", dataTable, expectedResult, new List<string>() { }, dateOnlyColumns, pkReplacment);

			dataTable = RunScriptForOrgRates(companyX.PK.ToGuid(), startDate, endDate, 'N', 'N');
			AssertEquals(2, dataTable.Rows.Count);

			expectedResult = @"
OH_Code OH_FullName OH_Category OH_RL_NKClosestPort CountryCode OA_State OA_City OH_IsActive ETC_Code ETC_Description OTC_IsActive OTR_StartDate OTR_EndDate OTR_Source OTR_RateNumerator OTR_RateDenominator TaxRate 
------------ ---------------------------------------------------------------------------------------------------- ----------- ------------------- ----------- ------------------------- -------------------------------------------------- ------------------------------------ ---------- ---------------- ------------------------------------ ----------- ------------------------------------ ------------------------------ -------------------------------------------------------------------------------- ------------------------------------ 
TESNAMSYD Test Company Name BUS AUSYD AU NSW Alexandria 1 XTC1 Test configuration XTC1 1 2023-05-10 2023-05-15 MOV 5 1 5.00 
TESNAMSYD Test Company Name BUS AUSYD AU NSW Alexandria 1 XTC1 Test configuration XTC1 1 2023-05-30 2023-06-04 MOV 10 1 10.00 ";

			AssertTableAsTextFromSQLServerManagenentStudio("AR - OnlyActiveConfig - OnlyActiveOrganization - ALL", dataTable, expectedResult, new List<string>() { }, dateOnlyColumns, pkReplacment);

			endDate = ZDate.Today.AddDays(80);
			dataTable = RunScriptForOrgRates(companyX.PK.ToGuid(), startDate, endDate, 'Y', 'Y');
			AssertEquals(7, dataTable.Rows.Count);

			expectedResult = @"
OH_Code OH_FullName OH_Category OH_RL_NKClosestPort CountryCode OA_State OA_City OH_IsActive ETC_Code ETC_Description OTC_IsActive OTR_StartDate OTR_EndDate OTR_Source OTR_RateNumerator OTR_RateDenominator TaxRate 
------------ ---------------------------------------------------------------------------------------------------- ----------- ------------------- ----------- ------------------------- -------------------------------------------------- ------------------------------------ ---------- ---------------- ------------------------------------ ----------- ------------------------------------ ------------------------------ -------------------------------------------------------------------------------- ------------------------------------ 
TESNAMBNE Test Company Name NAT AUBNE AU NSW Alexandria 1 XTC1 Test configuration XTC1 0 2023-05-30 2023-06-04 MOV 15 1 15.00 
TESNAMBNE Test Company Name NAT AUBNE AU NSW Alexandria 1 XTC1 Test configuration XTC1 0 2023-06-19 2023-06-24 MOV 20 1 20.00 
TESNAMMEL Test Company Name GOV AUMEL AU NSW Alexandria 0 XTC1 Test configuration XTC1 1 2023-06-19 2023-06-24 MOV 25 1 25.00 
TESNAMMEL Test Company Name GOV AUMEL AU NSW Alexandria 0 XTC1 Test configuration XTC1 1 2023-07-09 2023-07-14 MOV 30 1 30.00 
TESNAMPER Test Company Name NGO AUPER AU NSW Alexandria 0 XTC1 Test configuration XTC1 0 2023-06-29 2023-07-04 MOV 35 1 35.00 
TESNAMSYD Test Company Name BUS AUSYD AU NSW Alexandria 1 XTC1 Test configuration XTC1 1 2023-05-10 2023-05-15 MOV 5 1 5.00 
TESNAMSYD Test Company Name BUS AUSYD AU NSW Alexandria 1 XTC1 Test configuration XTC1 1 2023-05-30 2023-06-04 MOV 10 1 10.00 ";

			AssertTableAsTextFromSQLServerManagenentStudio("AR - IncludeInActiveConfig - IncludeInActiveOrganization - Organization has different rates within daterange and outside the daterange - ALL", dataTable, expectedResult, new List<string>() { }, dateOnlyColumns, pkReplacment);
		}

		[TestDate(2023, 04, 20)]
		public void TestARTaxConfigNoOrgRates_FilterByOrgCategoryAndIncludeInactiveTaxConfigAndOrg()
		{
			var (companyX, pkReplacment) = SetupData();
			var dateOnlyColumns = new[] { "OTR_StartDate", "OTR_EndDate" };
			var startDate = ZDate.Today;
			var endDate = ZDate.Today.AddDays(10);

			var dataTable = RunScriptForOrgRates(companyX.PK.ToGuid(), startDate, endDate, 'Y', 'Y', "");
			AssertEquals(0, dataTable.Rows.Count);

			dataTable = RunScriptForNoOrgRates(companyX.PK.ToGuid(), startDate, endDate, 'Y', 'Y', "BUS");
			AssertEquals(2, dataTable.Rows.Count);

			var expectedResult = @"
OH_Code OH_FullName OH_Category OH_RL_NKClosestPort CountryCode OA_State OA_City OH_IsActive ETC_Code ETC_Description OTC_IsActive OTR_StartDate OTR_EndDate OTR_Source OTR_RateNumerator OTR_RateDenominator TaxRate 
------------ ---------------------------------------------------------------------------------------------------- ----------- ------------------- ----------- ------------------------- -------------------------------------------------- ------------------------------------ ---------- ---------------- ------------------------------------ ----------- ------------------------------------ ------------------------------ -------------------------------------------------------------------------------- ------------------------------------ 
TESNAMAKL Test Company Name BUS NZAKL NZ NSW Alexandria 1 XTC1 Test configuration XTC1 1 NULL NULL NULL NULL NULL NULL 
TESNAMSYD Test Company Name BUS AUSYD AU NSW Alexandria 1 XTC1 Test configuration XTC1 1 NULL NULL NULL NULL NULL NULL ";

			AssertTableAsTextFromSQLServerManagenentStudio("AR - IncludeInActiveConfig - IncludeInActiveOrganization - BUS", dataTable, expectedResult, new List<string>() { }, dateOnlyColumns, pkReplacment);

			dataTable = RunScriptForNoOrgRates(companyX.PK.ToGuid(), startDate, endDate, 'Y', 'Y', "ALL");
			AssertEquals(5, dataTable.Rows.Count);

			dataTable = RunScriptForNoOrgRates(companyX.PK.ToGuid(), startDate, endDate, 'Y', 'Y');
			AssertEquals(5, dataTable.Rows.Count);

			expectedResult = @"
OH_Code OH_FullName OH_Category OH_RL_NKClosestPort CountryCode OA_State OA_City OH_IsActive ETC_Code ETC_Description OTC_IsActive OTR_StartDate OTR_EndDate OTR_Source OTR_RateNumerator OTR_RateDenominator TaxRate 
------------ ---------------------------------------------------------------------------------------------------- ----------- ------------------- ----------- ------------------------- -------------------------------------------------- ------------------------------------ ---------- ---------------- ------------------------------------ ----------- ------------------------------------ ------------------------------ -------------------------------------------------------------------------------- ------------------------------------ 
TESNAMAKL Test Company Name BUS NZAKL NZ NSW Alexandria 1 XTC1 Test configuration XTC1 1 NULL NULL NULL NULL NULL NULL 
TESNAMBNE Test Company Name NAT AUBNE AU NSW Alexandria 1 XTC1 Test configuration XTC1 0 NULL NULL NULL NULL NULL NULL 
TESNAMMEL Test Company Name GOV AUMEL AU NSW Alexandria 0 XTC1 Test configuration XTC1 1 NULL NULL NULL NULL NULL NULL 
TESNAMPER Test Company Name NGO AUPER AU NSW Alexandria 0 XTC1 Test configuration XTC1 0 NULL NULL NULL NULL NULL NULL 
TESNAMSYD Test Company Name BUS AUSYD AU NSW Alexandria 1 XTC1 Test configuration XTC1 1 NULL NULL NULL NULL NULL NULL ";

			AssertTableAsTextFromSQLServerManagenentStudio("AR - IncludeInActiveConfig - IncludeInActiveOrganization - ALL", dataTable, expectedResult, new List<string>() { }, dateOnlyColumns, pkReplacment);

			dataTable = RunScriptForNoOrgRates(companyX.PK.ToGuid(), startDate, endDate, 'N', 'Y');
			AssertEquals(3, dataTable.Rows.Count);

			expectedResult = @"
OH_Code OH_FullName OH_Category OH_RL_NKClosestPort CountryCode OA_State OA_City OH_IsActive ETC_Code ETC_Description OTC_IsActive OTR_StartDate OTR_EndDate OTR_Source OTR_RateNumerator OTR_RateDenominator TaxRate 
------------ ---------------------------------------------------------------------------------------------------- ----------- ------------------- ----------- ------------------------- -------------------------------------------------- ------------------------------------ ---------- ---------------- ------------------------------------ ----------- ------------------------------------ ------------------------------ -------------------------------------------------------------------------------- ------------------------------------ 
TESNAMAKL Test Company Name BUS NZAKL NZ NSW Alexandria 1 XTC1 Test configuration XTC1 1 NULL NULL NULL NULL NULL NULL 
TESNAMMEL Test Company Name GOV AUMEL AU NSW Alexandria 0 XTC1 Test configuration XTC1 1 NULL NULL NULL NULL NULL NULL 
TESNAMSYD Test Company Name BUS AUSYD AU NSW Alexandria 1 XTC1 Test configuration XTC1 1 NULL NULL NULL NULL NULL NULL ";

			AssertTableAsTextFromSQLServerManagenentStudio("AR - OnlyActiveConfig - IncludeInActiveOrganization - ALL", dataTable, expectedResult, new List<string>() { }, dateOnlyColumns, pkReplacment);

			dataTable = RunScriptForNoOrgRates(companyX.PK.ToGuid(), startDate, endDate, 'Y', 'N');
			AssertEquals(3, dataTable.Rows.Count);

			expectedResult = @"
OH_Code OH_FullName OH_Category OH_RL_NKClosestPort CountryCode OA_State OA_City OH_IsActive ETC_Code ETC_Description OTC_IsActive OTR_StartDate OTR_EndDate OTR_Source OTR_RateNumerator OTR_RateDenominator TaxRate 
------------ ---------------------------------------------------------------------------------------------------- ----------- ------------------- ----------- ------------------------- -------------------------------------------------- ------------------------------------ ---------- ---------------- ------------------------------------ ----------- ------------------------------------ ------------------------------ -------------------------------------------------------------------------------- ------------------------------------ 
TESNAMAKL Test Company Name BUS NZAKL NZ NSW Alexandria 1 XTC1 Test configuration XTC1 1 NULL NULL NULL NULL NULL NULL 
TESNAMBNE Test Company Name NAT AUBNE AU NSW Alexandria 1 XTC1 Test configuration XTC1 0 NULL NULL NULL NULL NULL NULL 
TESNAMSYD Test Company Name BUS AUSYD AU NSW Alexandria 1 XTC1 Test configuration XTC1 1 NULL NULL NULL NULL NULL NULL ";

			AssertTableAsTextFromSQLServerManagenentStudio("AR - IncludeInActiveConfig - OnlyActiveOrganization - ALL", dataTable, expectedResult, new List<string>() { }, dateOnlyColumns, pkReplacment);

			dataTable = RunScriptForNoOrgRates(companyX.PK.ToGuid(), startDate, endDate, 'N', 'N');
			AssertEquals(2, dataTable.Rows.Count);

			expectedResult = @"
OH_Code OH_FullName OH_Category OH_RL_NKClosestPort CountryCode OA_State OA_City OH_IsActive ETC_Code ETC_Description OTC_IsActive OTR_StartDate OTR_EndDate OTR_Source OTR_RateNumerator OTR_RateDenominator TaxRate 
------------ ---------------------------------------------------------------------------------------------------- ----------- ------------------- ----------- ------------------------- -------------------------------------------------- ------------------------------------ ---------- ---------------- ------------------------------------ ----------- ------------------------------------ ------------------------------ -------------------------------------------------------------------------------- ------------------------------------ 
TESNAMAKL Test Company Name BUS NZAKL NZ NSW Alexandria 1 XTC1 Test configuration XTC1 1 NULL NULL NULL NULL NULL NULL 
TESNAMSYD Test Company Name BUS AUSYD AU NSW Alexandria 1 XTC1 Test configuration XTC1 1 NULL NULL NULL NULL NULL NULL ";

			AssertTableAsTextFromSQLServerManagenentStudio("AR - OnlyActiveConfig - OnlyActiveOrganization - ALL", dataTable, expectedResult, new List<string>() { }, dateOnlyColumns, pkReplacment);

			endDate = ZDate.Today.AddDays(50);
			dataTable = RunScriptForNoOrgRates(companyX.PK.ToGuid(), startDate, endDate, 'Y', 'Y');
			AssertEquals(3, dataTable.Rows.Count);

			expectedResult = @"
OH_Code OH_FullName OH_Category OH_RL_NKClosestPort CountryCode OA_State OA_City OH_IsActive ETC_Code ETC_Description OTC_IsActive OTR_StartDate OTR_EndDate OTR_Source OTR_RateNumerator OTR_RateDenominator TaxRate 
------------ ---------------------------------------------------------------------------------------------------- ----------- ------------------- ----------- ------------------------- -------------------------------------------------- ------------------------------------ ---------- ---------------- ------------------------------------ ----------- ------------------------------------ ------------------------------ -------------------------------------------------------------------------------- ------------------------------------ 
TESNAMAKL Test Company Name BUS NZAKL NZ NSW Alexandria 1 XTC1 Test configuration XTC1 1 NULL NULL NULL NULL NULL NULL 
TESNAMMEL Test Company Name GOV AUMEL AU NSW Alexandria 0 XTC1 Test configuration XTC1 1 NULL NULL NULL NULL NULL NULL 
TESNAMPER Test Company Name NGO AUPER AU NSW Alexandria 0 XTC1 Test configuration XTC1 0 NULL NULL NULL NULL NULL NULL ";

			AssertTableAsTextFromSQLServerManagenentStudio("AR - IncludeInActiveConfig - IncludeInActiveOrganization - Organization has different rates within daterange and outside the daterange - ALL", dataTable, expectedResult, new List<string>() { }, dateOnlyColumns, pkReplacment);
		}

		[TestDate(2023, 04, 20)]
		public void TestARTaxConfigurationsOnActiveAndInactiveBranch()
		{
			var companyX = TestObjectCreator.CreateNewCompany("XXX");
			var branchX = TestObjectCreator.CreateNewBranch(companyX, "X01");
			Factory.Save();

			var startDate = ZDate.Today;
			var endDate = ZDate.Today.AddDays(25);
			var dateOnlyColumns = new[] { "OTR_StartDate", "OTR_EndDate" };

			List<Tuple<ZGuid, string>> pkReplacment;
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), branchX.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var org = TestObjectCreator.CreateOrgHeader("Org", true, true, "AUSYD");
				org.CompanyData.OB_GC = companyX.PK;
				org.MainAddress.State = "Org State";
				org.MainAddress.City = "Org City";

				var taxConfig = ObjectCreator.CreateTaxConfiguration(branchX, TaxConfigurationLedgers.AccountsReceivable.Code);
				taxConfig.ETC_Code = "XTC";
				taxConfig.ETC_Description = "Test configuration XTC";
				Factory.Save();

				var taxOrgConfig = ObjectCreator.CreateOrgTaxConfiguration(taxConfig, org.CompanyData, true);
				Factory.Save();

				pkReplacment = new List<Tuple<ZGuid, string>>(new[]
				{
					new Tuple<ZGuid, string>(taxConfig.PK, "taxConfig"),
					new Tuple<ZGuid, string>(taxOrgConfig.PK, "taxOrgConfig"),
					new Tuple<ZGuid, string>(branchX.PK, "branchX")
				});
			}

			var dataTable = RunScript(companyX.PK.ToGuid(), startDate, endDate, 'Y', 'Y');
			AssertEquals("Active Branch", 1, dataTable.Rows.Count);

			var expectedResult = @"
OH_Code OH_FullName OH_Category OH_RL_NKClosestPort CountryCode OA_State OA_City OH_IsActive ETC_Code ETC_Description OTC_IsActive OTR_StartDate OTR_EndDate OTR_Source OTR_RateNumerator OTR_RateDenominator TaxRate 
------------ ---------------------------------------------------------------------------------------------------- ----------- ------------------- ----------- ------------------------- -------------------------------------------------- ------------------------------------ ---------- ---------------- ------------------------------------ ----------- ------------------------------------ ------------------------------ -------------------------------------------------------------------------------- ------------------------------------ 
TESNAMSYD Test Company Name BUS AUSYD AU Org State Org City 1 XTC Test configuration XTC 1 NULL NULL NULL NULL NULL NULL
";

			AssertTableAsTextFromSQLServerManagenentStudio("Active Branch", dataTable, expectedResult, new List<string>() { }, dateOnlyColumns, pkReplacment);

			branchX.GB_IsActive = false;
			Factory.Save();

			dataTable = RunScript(companyX.PK.ToGuid(), startDate, endDate, 'Y', 'Y');
			AssertEquals("Inactive Branch", 0, dataTable.Rows.Count);
		}

		[TestDate(2023, 04, 20)]
		public void TestARTaxConfigurations_OrgWithMainAddress()
		{
			var companyX = TestObjectCreator.CreateNewCompany("XXX");
			var branchX = TestObjectCreator.CreateNewBranch(companyX, "X01");
			var org = TestObjectCreator.CreateOrgHeader("Org", true, true, "AUSYD");
			org.CompanyData.OB_GC = companyX.PK;
			org.MainAddress.OA_Address1 = "Org Address1";
			org.MainAddress.State = "Org State1";
			org.MainAddress.City = "Org City1";
			Factory.Save();

			var startDate = ZDate.Today;
			var endDate = ZDate.Today.AddDays(25);
			var dateOnlyColumns = new[] { "OTR_StartDate", "OTR_EndDate" };

			List<Tuple<ZGuid, string>> pkReplacment;
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), branchX.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var taxConfig = ObjectCreator.CreateTaxConfiguration(branchX, TaxConfigurationLedgers.AccountsReceivable.Code);
				taxConfig.ETC_Code = "XTC";
				taxConfig.ETC_Description = "Test configuration XTC";
				Factory.Save();

				var taxOrgConfig = ObjectCreator.CreateOrgTaxConfiguration(taxConfig, org.CompanyData, true);
				ObjectCreator.AddOrgTaxRateItem(taxOrgConfig, ZDate.Today.AddDays(20), ZDate.Today.AddDays(25), RateSourceMethods.ManualOverride.Code, ((ZInt)5, (ZInt)1));
				Factory.Save();

				pkReplacment = new List<Tuple<ZGuid, string>>(new[]
				{
					new Tuple<ZGuid, string>(taxConfig.PK, "taxConfig"),
					new Tuple<ZGuid, string>(taxOrgConfig.PK, "taxOrgConfig"),
					new Tuple<ZGuid, string>(branchX.PK, "branchX")
				});
			}

			var expectedResult = @"
OH_Code OH_FullName OH_Category OH_RL_NKClosestPort CountryCode OA_State OA_City OH_IsActive ETC_Code ETC_Description OTC_IsActive OTR_StartDate OTR_EndDate OTR_Source OTR_RateNumerator OTR_RateDenominator TaxRate 
------------ ---------------------------------------------------------------------------------------------------- ----------- ------------------- ----------- ------------------------- -------------------------------------------------- ------------------------------------ ---------- ---------------- ------------------------------------ ----------- ------------------------------------ ------------------------------ -------------------------------------------------------------------------------- ------------------------------------ 
TESNAMSYD Test Company Name BUS AUSYD AU Org State1 Org City1 1 XTC Test configuration XTC 1 2023-05-10 2023-05-15 MOV 5 1 5.00
";
			var dataTable = RunScript(companyX.PK.ToGuid(), startDate, endDate, 'Y', 'Y');
			AssertTableAsTextFromSQLServerManagenentStudio("Main office Address", dataTable, expectedResult, new List<string>() { }, dateOnlyColumns, pkReplacment);

			var address = org.Addresses.AddNew();
			address.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);
			address.OA_Address1 = "Org Address2";
			address.OA_City = "Org City2";
			address.OA_State = "Org State2";
			Factory.Save();

			expectedResult = @"
OH_Code OH_FullName OH_Category OH_RL_NKClosestPort CountryCode OA_State OA_City OH_IsActive ETC_Code ETC_Description OTC_IsActive OTR_StartDate OTR_EndDate OTR_Source OTR_RateNumerator OTR_RateDenominator TaxRate 
------------ ---------------------------------------------------------------------------------------------------- ----------- ------------------- ----------- ------------------------- -------------------------------------------------- ------------------------------------ ---------- ---------------- ------------------------------------ ----------- ------------------------------------ ------------------------------ -------------------------------------------------------------------------------- ------------------------------------ 
TESNAMSYD Test Company Name BUS AUSYD AU Org State2 Org City2 1 XTC Test configuration XTC 1 2023-05-10 2023-05-15 MOV 5 1 5.00
";

			dataTable = RunScript(companyX.PK.ToGuid(), startDate, endDate, 'Y', 'Y');
			AssertTableAsTextFromSQLServerManagenentStudio("New Main office Address", dataTable, expectedResult, new List<string>() { }, dateOnlyColumns, pkReplacment);

			org.MainAddress.DeleteAddressType(OrgAddressType.Office);

			address = org.Addresses.AddNew();
			address.AddressCapability.SetIsMainAddress(OrgAddressType.Postal.Code);
			address.OA_Address1 = "Org Address3";
			address.OA_City = "Org City3";
			address.OA_State = "Org State3";
			Factory.Save();

			expectedResult = @"
OH_Code OH_FullName OH_Category OH_RL_NKClosestPort CountryCode OA_State OA_City OH_IsActive ETC_Code ETC_Description OTC_IsActive OTR_StartDate OTR_EndDate OTR_Source OTR_RateNumerator OTR_RateDenominator TaxRate 
------------ ---------------------------------------------------------------------------------------------------- ----------- ------------------- ----------- ------------------------- -------------------------------------------------- ------------------------------------ ---------- ---------------- ------------------------------------ ----------- ------------------------------------ ------------------------------ -------------------------------------------------------------------------------- ------------------------------------ 
TESNAMSYD Test Company Name BUS AUSYD AU 1 XTC Test configuration XTC 1 2023-05-10 2023-05-15 MOV 5 1 5.00
";

			dataTable = RunScript(companyX.PK.ToGuid(), startDate, endDate, 'Y', 'Y');
			AssertTableAsTextFromSQLServerManagenentStudio("No Main office Address", dataTable, expectedResult, new List<string>() { }, dateOnlyColumns, pkReplacment);

			address = org.Addresses.AddNew();
			address.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);
			address.OA_Address1 = "Org Address4";
			address.OA_City = "Org City4";
			address.OA_State = "Org State4";
			Factory.Save();

			expectedResult = @"
OH_Code OH_FullName OH_Category OH_RL_NKClosestPort CountryCode OA_State OA_City OH_IsActive ETC_Code ETC_Description OTC_IsActive OTR_StartDate OTR_EndDate OTR_Source OTR_RateNumerator OTR_RateDenominator TaxRate 
------------ ---------------------------------------------------------------------------------------------------- ----------- ------------------- ----------- ------------------------- -------------------------------------------------- ------------------------------------ ---------- ---------------- ------------------------------------ ----------- ------------------------------------ ------------------------------ -------------------------------------------------------------------------------- ------------------------------------ 
TESNAMSYD Test Company Name BUS AUSYD AU Org State4 Org City4 1 XTC Test configuration XTC 1 2023-05-10 2023-05-15 MOV 5 1 5.00
";

			dataTable = RunScript(companyX.PK.ToGuid(), startDate, endDate, 'Y', 'Y');
			AssertTableAsTextFromSQLServerManagenentStudio("No Main office Address", dataTable, expectedResult, new List<string>() { }, dateOnlyColumns, pkReplacment);
		}

		(GlbCompany companyX, List<Tuple<ZGuid, string>>) SetupData()
		{
			var companyX = TestObjectCreator.CreateNewCompany("XXX");
			var branchX = TestObjectCreator.CreateNewBranch(companyX, "X01");

			Factory.Save();

			List<Tuple<ZGuid, string>> pkReplacment;
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), branchX.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var org1 = TestObjectCreator.CreateOrgHeader("Org1", true, true, "AUSYD");
				org1.CompanyData.OB_GC = companyX.PK;
				org1.OH_Category = "BUS";

				var org2 = TestObjectCreator.CreateOrgHeader("Org2", true, true, "AUBNE");
				org2.CompanyData.OB_GC = companyX.PK;
				org2.OH_Category = "NAT";

				var org3 = TestObjectCreator.CreateOrgHeader("Org3", true, true, "AUMEL");
				org3.OH_IsActive = false;
				org3.CompanyData.OB_GC = companyX.PK;
				org3.OH_Category = "GOV";

				var org4 = TestObjectCreator.CreateOrgHeader("Org4", true, true, "AUPER");
				org4.OH_IsActive = false;
				org4.CompanyData.OB_GC = companyX.PK;
				org4.OH_Category = "NGO";

				var org5 = TestObjectCreator.CreateOrgHeader("Org5", true, true, "NZAKL");
				org5.CompanyData.OB_GC = companyX.PK;

				org1.MainAddress.OA_State = org2.MainAddress.OA_State = org3.MainAddress.OA_State = org4.MainAddress.OA_State = org5.MainAddress.OA_State = "NSW";

				var taxConfig1 = ObjectCreator.CreateTaxConfiguration(companyX, TaxConfigurationLedgers.AccountsReceivable.Code);
				taxConfig1.ETC_Code = "XTC1";
				taxConfig1.ETC_Description = "Test configuration XTC1";

				Factory.Save();

				var taxOrgConfig1 = ObjectCreator.CreateOrgTaxConfiguration(taxConfig1, org1.CompanyData, true); //AR - org1 - Active Org - Active Config
				ObjectCreator.AddOrgTaxRateItem(taxOrgConfig1, ZDate.Today.AddDays(20), ZDate.Today.AddDays(25), RateSourceMethods.ManualOverride.Code, ((ZInt)5, (ZInt)1));
				ObjectCreator.AddOrgTaxRateItem(taxOrgConfig1, ZDate.Today.AddDays(40), ZDate.Today.AddDays(45), RateSourceMethods.ManualOverride.Code, ((ZInt)10, (ZInt)1));

				var taxOrgConfig2 = ObjectCreator.CreateOrgTaxConfiguration(taxConfig1, org2.CompanyData, false); //AR - org2 - Active Org - InActive Config
				ObjectCreator.AddOrgTaxRateItem(taxOrgConfig2, ZDate.Today.AddDays(40), ZDate.Today.AddDays(45), RateSourceMethods.ManualOverride.Code, ((ZInt)15, (ZInt)1));
				ObjectCreator.AddOrgTaxRateItem(taxOrgConfig2, ZDate.Today.AddDays(60), ZDate.Today.AddDays(65), RateSourceMethods.ManualOverride.Code, ((ZInt)20, (ZInt)1));

				var taxOrgConfig3 = ObjectCreator.CreateOrgTaxConfiguration(taxConfig1, org3.CompanyData, true); //AR - org3 - InActive Org - Active Config
				ObjectCreator.AddOrgTaxRateItem(taxOrgConfig3, ZDate.Today.AddDays(60), ZDate.Today.AddDays(65), RateSourceMethods.ManualOverride.Code, ((ZInt)25, (ZInt)1));
				ObjectCreator.AddOrgTaxRateItem(taxOrgConfig3, ZDate.Today.AddDays(80), ZDate.Today.AddDays(85), RateSourceMethods.ManualOverride.Code, ((ZInt)30, (ZInt)1));

				var taxOrgConfig4 = ObjectCreator.CreateOrgTaxConfiguration(taxConfig1, org4.CompanyData, false); //AR - org4 - InActive Org - InActive Config
				ObjectCreator.AddOrgTaxRateItem(taxOrgConfig4, ZDate.Today.AddDays(70), ZDate.Today.AddDays(75), RateSourceMethods.ManualOverride.Code, ((ZInt)35, (ZInt)1));
				ObjectCreator.AddOrgTaxRateItem(taxOrgConfig4, ZDate.Today.AddDays(90), ZDate.Today.AddDays(95), RateSourceMethods.ManualOverride.Code, ((ZInt)40, (ZInt)1));

				var taxOrgConfig5 = ObjectCreator.CreateOrgTaxConfiguration(taxConfig1, org5.CompanyData, true); //AR - org5 - Active Org - Active Config

				Factory.Save();

				pkReplacment = new List<Tuple<ZGuid, string>>(new[]
				{
						new Tuple<ZGuid, string>(taxConfig1.PK, "taxConfig1"),

						new Tuple<ZGuid, string>(taxOrgConfig1.PK, "taxOrgConfig1"),
						new Tuple<ZGuid, string>(taxOrgConfig2.PK, "taxOrgConfig2"),
						new Tuple<ZGuid, string>(taxOrgConfig3.PK, "taxOrgConfig3"),
						new Tuple<ZGuid, string>(taxOrgConfig4.PK, "taxOrgConfig4"),
						new Tuple<ZGuid, string>(branchX.PK, "branchX")
				});

				return (companyX, pkReplacment);
			}
		}

		DataTable RunScript(ZGuid orgPK, ZDate startDate, ZDate endDate, char includeInactiveTaxConfigurationOrganization = 'N', char includeInactiveOrganization = 'N', string orgnizationCategory = "ALL")
		{
			return DataUtils.GetDataTableFromQuery(Db.Connection, string.Format(@"SELECT * FROM Report_ARTaxConfigurationOrgRatesAndNoOrgRates('{0}', '{1}', '{2}', '{3}', '{4}', '{5}') ORDER BY OH_CODE, OTR_STARTDATE", orgPK, startDate, endDate, includeInactiveTaxConfigurationOrganization, includeInactiveOrganization, orgnizationCategory));
		}

		DataTable RunScriptForOrgRates(ZGuid orgPK, ZDate startDate, ZDate endDate, char includeInactiveTaxConfigurationOrganization = 'N', char includeInactiveOrganization = 'N', string orgnizationCategory = "ALL")
		{
			return DataUtils.GetDataTableFromQuery(Db.Connection, string.Format(@"SELECT * FROM Report_ARTaxConfigurationOrgRatesAndNoOrgRates('{0}', '{1}', '{2}', '{3}', '{4}', '{5}') WHERE OTR_STARTDATE IS NOT NULL ORDER BY OH_CODE, OTR_STARTDATE", orgPK, startDate, endDate, includeInactiveTaxConfigurationOrganization, includeInactiveOrganization, orgnizationCategory));
		}

		DataTable RunScriptForNoOrgRates(ZGuid orgPK, ZDate startDate, ZDate endDate, char includeInactiveTaxConfigurationOrganization = 'N', char includeInactiveOrganization = 'N', string orgnizationCategory = "ALL")
		{
			return DataUtils.GetDataTableFromQuery(Db.Connection, string.Format(@"SELECT * FROM Report_ARTaxConfigurationOrgRatesAndNoOrgRates('{0}', '{1}', '{2}', '{3}', '{4}', '{5}') WHERE OTR_STARTDATE IS NULL ORDER BY OH_CODE, OTR_STARTDATE", orgPK, startDate, endDate, includeInactiveTaxConfigurationOrganization, includeInactiveOrganization, orgnizationCategory));
		}

		AccountingTestObjectCreator ObjectCreator => objectCreator ?? (objectCreator = new AccountingTestObjectCreator(Factory));
		AccountingTestObjectCreator objectCreator;
	}
}
