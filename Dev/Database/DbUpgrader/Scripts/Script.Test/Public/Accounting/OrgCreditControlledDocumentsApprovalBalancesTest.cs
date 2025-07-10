using System;
using System.Linq;
using CargoWise.Data.Testing;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Testing
{
	[TestedType(typeof(OrgCreditControlledDocumentsApprovalBalances))]
	class OrgCreditControlledDocumentsApprovalBalancesTest : DbCreateScriptTest
	{
		public void TestIndexUsedWithoutKeyLookups()
		{
			var helper = new TestDbHelper(TestConnection);
			var now = new DateTime(2020, 5, 12);
			var orgPK = Guid.Empty;
			for (int i = 0; i < 100; i++)
			{
				orgPK = helper.InsertOrgHeader("ORG" + i, "ORGHEADER" + i);
				var category = i % 2 == 0 ? "DCU" : "XXX";
				helper.InsertTransactionHeader("AR", "INV", "INV" + i, 100, now, helper.DefaultBranchPK, helper.DefaultDepartmentPK, org: orgPK, dueDate: now.AddDays(-i), category: category);
				helper.InsertTransactionHeader("AP", "INV", "INV" + i, 100, now, helper.DefaultBranchPK, helper.DefaultDepartmentPK, org: orgPK, dueDate: now.AddDays(-i), category: category);
			}

			// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538 
			using (TestConnection.TrackExecutedCommands(includeQueryPlansForExecuteReaderCommands: true))
			{
				var totalRows = 0;
				TestConnection.ExecuteReader($"SELECT * FROM OrgCreditControlledDocumentsApprovalBalances ('{orgPK}', '{TestDbHelper.DefaultCompanyPK}', '{now:yyyy-MM-dd}')",
					_ => totalRows++);
				AssertEquals(1, totalRows);
				var queryPlan = TestConnection.ExecutedCommandsAndQueryPlans.First(t => t.Item1.Contains("OrgCreditControlledDocumentsApprovalBalances"));
				var queryPlanAnalyzer = new QueryPlanalyzer(queryPlan.Item2.First());
				Assert("Non clustered index on NR_RX__AH_GC_AH_Ledger_AH_OH_AH_TransactionType_AH_DueDate must be used.", queryPlanAnalyzer.IndexSeeks.Any(x => x.IndexName == "NR_RX__AH_GC_AH_Ledger_AH_OH_AH_TransactionType_AH_DueDate"));
				Assert("There should be no clustered index seeks; only the non clustered index should be used", !queryPlanAnalyzer.IndexSeeks.Any(x => x.IndexKind == "Clustered"));
			}
		}
	}
}

