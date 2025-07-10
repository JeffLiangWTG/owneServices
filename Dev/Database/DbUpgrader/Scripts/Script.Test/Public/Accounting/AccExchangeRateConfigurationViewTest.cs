using System;
using System.Linq;
using CargoWise.Data.Testing;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Testing
{
	[TestedType(typeof(AccExchangeRateConfigurationView))]
	class AccExchangeRateConfigurationViewTest : DbCreateScriptTest
	{
		public void TestQueriesWithoutGCDoNotDoTableScan()
		{
			var helper = new TestDbHelper(TestConnection);
			var orgPK = Guid.Empty;
			for (int i = 0; i < 1000; i++)
			{
				orgPK = helper.InsertOrgHeader("ORG" + i, "ORGHEADER" + i);
				helper.InsertAccJobConfig("ERT", parentTableCode: "OH", parentId: orgPK, code: "BUY", code2: "TDR");
			}
			TestConnection.ExecuteNonQuery($"UPDATE STATISTICS AccJobConfig WITH FULLSCAN");

			// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538 
			using (TestConnection.TrackExecutedCommands(includeQueryPlansForExecuteReaderCommands: true))
			{
				var totalRows = 0;
				TestConnection.ExecuteReader($"SELECT * FROM dbo.AccExchangeRateConfigurationView WHERE JCE_ParentId = '{orgPK}' AND JCE_ParentTableCode = 'OH'",
					_ => totalRows++);
				AssertEquals(1, totalRows);
				var queryPlan = TestConnection.ExecutedCommandsAndQueryPlans.First(t => t.Item1.Contains("AccExchangeRateConfigurationView"));
				var queryPlanAnalyzer = new QueryPlanalyzer(queryPlan.Item2.First());
				Assert("Non clustered index on NR_RX__JCF_ParentId_JCF_ConfigType must be used.", queryPlanAnalyzer.IndexSeeks.Any(x => x.IndexName == "NR_RX__JCF_ParentId_JCF_ConfigType"));
				Assert("There should be no table scans.", !queryPlanAnalyzer.TableScans.Any());
				Assert("There should be no index scans.", !queryPlanAnalyzer.IndexScans.Any());
				Assert("There should be no clustered index seeks as NR_RX__JCF_ParentId_JCF_ConfigType is covering.", !queryPlanAnalyzer.IndexSeeks.Any(x => x.IndexKind == "Clustered"));
			}
		}
	}
}

