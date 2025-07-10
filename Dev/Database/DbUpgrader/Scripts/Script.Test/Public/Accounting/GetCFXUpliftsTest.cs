using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting
{
	[TestedType(typeof(GetCFXUplifts))]
	class GetCFXUpliftsTest : DbCreateScriptTest
	{
		public void TestForNoRecordsExpectNullValues()
		{
			var helper = new TestDbHelper(TestConnection);

			var debtorOrg = helper.InsertOrgHeader("DEB", "Their Org");
			var currentCompany = helper.InsertCompany("CAU", "My Company", "AUD", "AU", isReciprocal: false, isGSTRegistered: false);
			var currentBranch = helper.InsertBranch("BAU", currentCompany, "My Branch");

			var query = QueryString(debtorOrg, currentCompany, currentBranch);
			var result = DataUtils.GetDataTableFromQuery(TestConnection, query);

			// Baseline Behaviour: GetCFXUplifts currently returns a row with null values in columns when no CFX Uplift records exist
			// This might not be ideal as Reports have to replace nulls with 0
			AssertEquals(DBNull.Value, result.Rows[0]["ImportAirCollectUplift"]);
			AssertEquals(DBNull.Value, result.Rows[0]["ExportAirCollectUplift"]);
			AssertEquals(DBNull.Value, result.Rows[0]["ImportSeaCollectUplift"]);
			AssertEquals(DBNull.Value, result.Rows[0]["ExportSeaCollectUplift"]);
			AssertEquals("GetCFXUplifts only returns 1 record", 1, result.Rows.Count);
		}

		public void TestPreferCFXOfOrgBeforeCurrentBranchBeforeCurrentCompany()
		{
			var expectedCFXForCompany =	new TestCaseCFXUpliftValues() { ImportAir = 1.1m, ExportAir = 1.2m, ImportSea = 1.3m, ExportSea = 1.4m };
			var expectedCFXForBranch =	new TestCaseCFXUpliftValues() { ImportAir = 2.1m, ExportAir = 2.2m, ImportSea = 2.3m, ExportSea = 2.4m };
			var expectedCFXForOrg =		new TestCaseCFXUpliftValues() { ImportAir = 3.1m, ExportAir = 3.2m, ImportSea = 3.3m, ExportSea = 3.4m };

			var helper = new TestDbHelper(TestConnection);
			var debtorOrg = helper.InsertOrgHeader("DEB", "Their Org");
			var currentCompany = helper.InsertCompany("CAU", "My Company", "AUD", "AU", isReciprocal: false, isGSTRegistered: false);
			var currentBranch = helper.InsertBranch("BAU", currentCompany, "My Branch");

			// Test company level
			InsertCFXConfig(helper, "IMP", "AIR", expectedCFXForCompany.ImportAir, parentTableCode: string.Empty, parentId: null, currentCompany);
			InsertCFXConfig(helper, "EXP", "AIR", expectedCFXForCompany.ExportAir, parentTableCode: string.Empty, parentId: null, currentCompany);
			InsertCFXConfig(helper, "IMP", "SEA", expectedCFXForCompany.ImportSea, parentTableCode: string.Empty, parentId: null, currentCompany);
			InsertCFXConfig(helper, "EXP", "SEA", expectedCFXForCompany.ExportSea, parentTableCode: string.Empty, parentId: null, currentCompany);

			var query = QueryString(debtorOrg, currentCompany, currentBranch);
			var result = DataUtils.GetDataTableFromQuery(TestConnection, query);

			AssertEquals("GetCFXUplifts should use company level config when no branch or org", expectedCFXForCompany.ImportAir, result.Rows[0]["ImportAirCollectUplift"]);
			AssertEquals("GetCFXUplifts should use company level config when no branch or org", expectedCFXForCompany.ExportAir, result.Rows[0]["ExportAirCollectUplift"]);
			AssertEquals("GetCFXUplifts should use company level config when no branch or org", expectedCFXForCompany.ImportSea, result.Rows[0]["ImportSeaCollectUplift"]);
			AssertEquals("GetCFXUplifts should use company level config when no branch or org", expectedCFXForCompany.ExportSea, result.Rows[0]["ExportSeaCollectUplift"]);
			AssertEquals("GetCFXUplifts only returns 1 record", 1, result.Rows.Count);

			// Test branch level
			InsertCFXConfig(helper, "IMP", "AIR", expectedCFXForBranch.ImportAir, parentTableCode: "GB", parentId: currentBranch, currentCompany);
			InsertCFXConfig(helper, "EXP", "AIR", expectedCFXForBranch.ExportAir, parentTableCode: "GB", parentId: currentBranch, currentCompany);
			InsertCFXConfig(helper, "IMP", "SEA", expectedCFXForBranch.ImportSea, parentTableCode: "GB", parentId: currentBranch, currentCompany);
			InsertCFXConfig(helper, "EXP", "SEA", expectedCFXForBranch.ExportSea, parentTableCode: "GB", parentId: currentBranch, currentCompany);

			query = QueryString(debtorOrg, currentCompany, currentBranch);
			result = DataUtils.GetDataTableFromQuery(TestConnection, query);

			AssertEquals("GetCFXUplifts should use branch level config when no org", expectedCFXForBranch.ImportAir, result.Rows[0]["ImportAirCollectUplift"]);
			AssertEquals("GetCFXUplifts should use branch level config when no org", expectedCFXForBranch.ExportAir, result.Rows[0]["ExportAirCollectUplift"]);
			AssertEquals("GetCFXUplifts should use branch level config when no org", expectedCFXForBranch.ImportSea, result.Rows[0]["ImportSeaCollectUplift"]);
			AssertEquals("GetCFXUplifts should use branch level config when no org", expectedCFXForBranch.ExportSea, result.Rows[0]["ExportSeaCollectUplift"]);
			AssertEquals("GetCFXUplifts only returns 1 record", 1, result.Rows.Count);

			// Test org level
			InsertCFXConfig(helper, "IMP", "AIR", expectedCFXForOrg.ImportAir, parentTableCode: "OH", parentId: debtorOrg, currentCompany);
			InsertCFXConfig(helper, "EXP", "AIR", expectedCFXForOrg.ExportAir, parentTableCode: "OH", parentId: debtorOrg, currentCompany);
			InsertCFXConfig(helper, "IMP", "SEA", expectedCFXForOrg.ImportSea, parentTableCode: "OH", parentId: debtorOrg, currentCompany);
			InsertCFXConfig(helper, "EXP", "SEA", expectedCFXForOrg.ExportSea, parentTableCode: "OH", parentId: debtorOrg, currentCompany);

			query = QueryString(debtorOrg, currentCompany, currentBranch);
			result = DataUtils.GetDataTableFromQuery(TestConnection, query);

			AssertEquals("GetCFXUplifts should use use org level", expectedCFXForOrg.ImportAir, result.Rows[0]["ImportAirCollectUplift"]);
			AssertEquals("GetCFXUplifts should use use org level", expectedCFXForOrg.ExportAir, result.Rows[0]["ExportAirCollectUplift"]);
			AssertEquals("GetCFXUplifts should use use org level", expectedCFXForOrg.ImportSea, result.Rows[0]["ImportSeaCollectUplift"]);
			AssertEquals("GetCFXUplifts should use use org level", expectedCFXForOrg.ExportSea, result.Rows[0]["ExportSeaCollectUplift"]);
			AssertEquals("GetCFXUplifts only returns 1 record", 1, result.Rows.Count);
		}

		public void TestForDifferentCompanyExpectNullValues()
		{
			var helper = new TestDbHelper(TestConnection);

			var debtorOrg = helper.InsertOrgHeader("DEB", "Their Org");
			var currentCompany = helper.InsertCompany("CAU", "My Company", "AUD", "AU", isReciprocal: false, isGSTRegistered: false);
			var currentBranch = helper.InsertBranch("BAU", currentCompany, "My Branch");

			var otherDebtorOrg = helper.InsertOrgHeader("DTH", "Other Org");
			var otherCompany = helper.InsertCompany("CTH", "Other Company", "AUD", "AU", isReciprocal: false, isGSTRegistered: false);
			var otherBranch = helper.InsertBranch("BTH", otherCompany, "Other Branch");

			var cfxForOtherCompany =	new TestCaseCFXUpliftValues() { ImportAir = 1.1m, ExportAir = 1.2m, ImportSea = 1.3m, ExportSea = 1.4m };
			var cfxForOtherBranch =		new TestCaseCFXUpliftValues() { ImportAir = 2.1m, ExportAir = 2.2m, ImportSea = 2.3m, ExportSea = 2.4m };
			var cfxForOtherOrg =		new TestCaseCFXUpliftValues() { ImportAir = 3.1m, ExportAir = 3.2m, ImportSea = 3.3m, ExportSea = 3.4m };

			InsertCFXConfig(helper, "IMP", "AIR", cfxForOtherCompany.ImportAir, parentTableCode: string.Empty, parentId: null, otherCompany);
			InsertCFXConfig(helper, "EXP", "AIR", cfxForOtherCompany.ExportAir, parentTableCode: string.Empty, parentId: null, otherCompany);
			InsertCFXConfig(helper, "IMP", "SEA", cfxForOtherCompany.ImportSea, parentTableCode: string.Empty, parentId: null, otherCompany);
			InsertCFXConfig(helper, "EXP", "SEA", cfxForOtherCompany.ExportSea, parentTableCode: string.Empty, parentId: null, otherCompany);

			InsertCFXConfig(helper, "IMP", "AIR", cfxForOtherBranch.ImportAir, parentTableCode: "GB", parentId: otherBranch, otherCompany);
			InsertCFXConfig(helper, "EXP", "AIR", cfxForOtherBranch.ExportAir, parentTableCode: "GB", parentId: otherBranch, otherCompany);
			InsertCFXConfig(helper, "IMP", "SEA", cfxForOtherBranch.ImportSea, parentTableCode: "GB", parentId: otherBranch, otherCompany);
			InsertCFXConfig(helper, "EXP", "SEA", cfxForOtherBranch.ExportSea, parentTableCode: "GB", parentId: otherBranch, otherCompany);

			InsertCFXConfig(helper, "IMP", "AIR", cfxForOtherOrg.ImportAir, parentTableCode: "OH", parentId: otherDebtorOrg, otherCompany);
			InsertCFXConfig(helper, "EXP", "AIR", cfxForOtherOrg.ExportAir, parentTableCode: "OH", parentId: otherDebtorOrg, otherCompany);
			InsertCFXConfig(helper, "IMP", "SEA", cfxForOtherOrg.ImportSea, parentTableCode: "OH", parentId: otherDebtorOrg, otherCompany);
			InsertCFXConfig(helper, "EXP", "SEA", cfxForOtherOrg.ExportSea, parentTableCode: "OH", parentId: otherDebtorOrg, otherCompany);

			var query = QueryString(debtorOrg, currentCompany, currentBranch);
			var result = DataUtils.GetDataTableFromQuery(TestConnection, query);

			// Baseline Behaviour: GetCFXUplifts currently returns a row with null values in columns when no CFX Uplift records exist
			// This might not be ideal as Reports have to replace nulls with 0
			AssertEquals(DBNull.Value, result.Rows[0]["ImportAirCollectUplift"]);
			AssertEquals(DBNull.Value, result.Rows[0]["ExportAirCollectUplift"]);
			AssertEquals(DBNull.Value, result.Rows[0]["ImportSeaCollectUplift"]);
			AssertEquals(DBNull.Value, result.Rows[0]["ExportSeaCollectUplift"]);
			AssertEquals("GetCFXUplifts only returns 1 record", 1, result.Rows.Count);
		}

		public void TestForSameCompanyDifferentBranchOrgExpectNullValues()
		{
			var helper = new TestDbHelper(TestConnection);

			var debtorOrg = helper.InsertOrgHeader("DEB", "Their Org");
			var currentCompany = helper.InsertCompany("CAU", "My Company", "AUD", "AU", isReciprocal: false, isGSTRegistered: false);
			var currentBranch = helper.InsertBranch("BAU", currentCompany, "My Branch");

			var otherDebtorOrg = helper.InsertOrgHeader("DTH", "Other Org");
			var otherBranch = helper.InsertBranch("BTH", currentCompany, "Other Branch");

			var cfxForOtherBranch = new TestCaseCFXUpliftValues() { ImportAir = 2.1m, ExportAir = 2.2m, ImportSea = 2.3m, ExportSea = 2.4m };
			var cfxForOtherOrg =	new TestCaseCFXUpliftValues() { ImportAir = 3.1m, ExportAir = 3.2m, ImportSea = 3.3m, ExportSea = 3.4m };

			InsertCFXConfig(helper, "IMP", "AIR", cfxForOtherBranch.ImportAir, parentTableCode: "GB", parentId: otherBranch, currentCompany);
			InsertCFXConfig(helper, "EXP", "AIR", cfxForOtherBranch.ExportAir, parentTableCode: "GB", parentId: otherBranch, currentCompany);
			InsertCFXConfig(helper, "IMP", "SEA", cfxForOtherBranch.ImportSea, parentTableCode: "GB", parentId: otherBranch, currentCompany);
			InsertCFXConfig(helper, "EXP", "SEA", cfxForOtherBranch.ExportSea, parentTableCode: "GB", parentId: otherBranch, currentCompany);

			InsertCFXConfig(helper, "IMP", "AIR", cfxForOtherOrg.ImportAir, parentTableCode: "OH", parentId: otherDebtorOrg, currentCompany);
			InsertCFXConfig(helper, "EXP", "AIR", cfxForOtherOrg.ExportAir, parentTableCode: "OH", parentId: otherDebtorOrg, currentCompany);
			InsertCFXConfig(helper, "IMP", "SEA", cfxForOtherOrg.ImportSea, parentTableCode: "OH", parentId: otherDebtorOrg, currentCompany);
			InsertCFXConfig(helper, "EXP", "SEA", cfxForOtherOrg.ExportSea, parentTableCode: "OH", parentId: otherDebtorOrg, currentCompany);

			var query = QueryString(debtorOrg, currentCompany, currentBranch);
			var result = DataUtils.GetDataTableFromQuery(TestConnection, query);

			// Baseline behaviour: GetCFXUplifts currently returns a row with null values in columns when no CFX Uplift records exist
			// This might not be ideal as Reports have to replace nulls with 0
			AssertEquals(DBNull.Value, result.Rows[0]["ImportAirCollectUplift"]);
			AssertEquals(DBNull.Value, result.Rows[0]["ExportAirCollectUplift"]);
			AssertEquals(DBNull.Value, result.Rows[0]["ImportSeaCollectUplift"]);
			AssertEquals(DBNull.Value, result.Rows[0]["ExportSeaCollectUplift"]);
			AssertEquals("GetCFXUplifts only returns 1 record", 1, result.Rows.Count);
		}

		public void TestGivenMultipleConfigJobTypesOfSameDirectionAndModeExpectFirstOfUndefinedOrder()
		{
			var jobTypeTestCases = new List<(string JobType, decimal Percent)>()
			{
				( JobType: "SHP", Percent: 10.5m),
				( JobType: "ALL", Percent: 20.5m),
				( JobType: "ACR", Percent: 30.5m),
			};
			var expectedPossiblePercents = jobTypeTestCases.Select(testcase => testcase.Percent);

			var helper = new TestDbHelper(TestConnection);

			var debtorOrg = helper.InsertOrgHeader("DEB", "Their Org");
			var currentCompany = helper.InsertCompany("CAU", "My Company", "AUD", "AU", isReciprocal: false, isGSTRegistered: false);
			var currentBranch = helper.InsertBranch("BAU", currentCompany, "My Branch");

			foreach (var jobTypeTestCase in jobTypeTestCases)
			{
				_ = helper.InsertAccJobConfig(jobType: jobTypeTestCase.JobType, percentage: jobTypeTestCase.Percent,
												companyPK: currentCompany, ledger: "AR", configType: "CFX", parentTableCode: "OH", parentId: debtorOrg,
												serviceDirection: "IMP", transportMode: "AIR");
			}

			var query = QueryString(debtorOrg, currentCompany, currentBranch);
			var result = DataUtils.GetDataTableFromQuery(TestConnection, query);

			var actualPercent = result.Rows[0]["ImportAirCollectUplift"];

			// Baseline behaviour: GetCFXUplifts isn't prioritizing JobType when picking a cfx uplift value
			AssertCollectionContains("Given multiple accJobConfig records exist for a direction/mode with different JCF_JobType values, GetCFXUplifts should the first record via undefined ordering",
										actualPercent, expectedPossiblePercents);
			AssertEquals("GetCFXUplifts only returns 1 record", 1, result.Rows.Count);
		}

		static void InsertCFXConfig(TestDbHelper helper, string serviceDirection, string transportMode, decimal percentage, string parentTableCode, Guid? parentId, Guid currentCompany)
		{
			var ledger = "AR";
			var configType = "CFX";

			helper.InsertAccJobConfig(companyPK: currentCompany, ledger: ledger, configType: configType, parentTableCode: parentTableCode, parentId: parentId,
										serviceDirection: serviceDirection, transportMode: transportMode, percentage: percentage);
		}

		static string QueryString(Guid orgPK, Guid companyPK, Guid branchPK)
			=> $"SELECT * FROM [GetCFXUplifts]('{orgPK}', '{companyPK}', '{branchPK}')";
		struct TestCaseCFXUpliftValues
		{
			public decimal ImportAir;
			public decimal ExportAir;
			public decimal ImportSea;
			public decimal ExportSea;
		}
	}
}

