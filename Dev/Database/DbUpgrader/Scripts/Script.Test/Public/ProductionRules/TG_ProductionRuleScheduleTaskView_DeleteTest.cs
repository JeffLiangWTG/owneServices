using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.ProductionRules.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.ProductionRules
{
	[TestedType(typeof(TG_ProductionRuleScheduleTaskView_Delete))]
	class TG_ProductionRuleScheduleTaskView_DeleteTest : DbCreateScriptTest
	{
		public void TestTrigger()
		{
			var sql = new SqlQueryBuilder();

			var ruleSet = new ProductionRuleSet("PWP", "TEST") { PRS_IsLive = false }.AppendInsertAndReturnObject(sql);
			var rule1 = new ProductionRule(ruleSet, "TEST1", "{}").AppendInsertAndReturnObject(sql);
			var rule1Schedule = new StmScheduleTask(rule1.PK, "PRL").AppendInsertAndReturnObject(sql);
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertEquals("Precondition.", true, ProductionRuleScheduleTaskView.ExistsInDB(TestConnection, rule1Schedule.PK));
			AssertEquals("Precondition.", true, StmScheduleTask.ExistsInDB(TestConnection, rule1Schedule.PK));

			ProductionRuleScheduleTaskView.DeleteInDB(TestConnection, rule1Schedule.PK);
			AssertEquals("Should have deleted the task.", false, ProductionRuleScheduleTaskView.ExistsInDB(TestConnection, rule1Schedule.PK));
			AssertEquals("Should have deleted the task.", false, StmScheduleTask.ExistsInDB(TestConnection, rule1Schedule.PK));
		}
	}
}

