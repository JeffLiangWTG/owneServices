using System;
using System.Data;
using CargoWise.Data;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.ProductionRules;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.ProductionRules
{
	[TestedType(typeof(GlowDeleteProductionRule))]
	class GlowDeleteProductionRuleTest : DbCreateScriptTest
	{
		public void TestGlowDeleteProductionRule()
		{
			var sql = new SqlQueryBuilder();
			var ruleSet = new ProductionRuleSet("PWP", "TEST") { PRS_IsLive = false }.AppendInsertAndReturnObject(sql);
			var rule1 = new ProductionRule(ruleSet, "TEST1", "{}").AppendInsertAndReturnObject(sql);
			var rule2 = new ProductionRule(ruleSet, "TEST2", "{}").AppendInsertAndReturnObject(sql);

			var rule1Schedule = new StmScheduleTask(rule1.PK, "PRL").AppendInsertAndReturnObject(sql);
			var rule2Schedule = new StmScheduleTask(rule2.PK, "PRL").AppendInsertAndReturnObject(sql);
			var rule1UnrelatedSchedule = new StmScheduleTask(rule1.PK, "AC").AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			RunStoredProcedure(rule1.PK, 0);

			AssertEquals("Should have deleted the rule.", 0, ProductionRule.CountInDB(TestConnection, prl => prl.PK == rule1.PK));
			AssertEquals("Should have deleted the schedule.", 0, StmScheduleTask.CountInDB(TestConnection, t => t.PK == rule1Schedule.PK));

			AssertEquals("Should *not* have deleted the other rule.", 1, ProductionRule.CountInDB(TestConnection, prl => prl.PK == rule2.PK));
			AssertEquals("Should *not* have deleted the other schedule.", 1, StmScheduleTask.CountInDB(TestConnection, t => t.PK == rule2Schedule.PK));
			AssertEquals("Should *not* have deleted the unrelated schedule.", 1, StmScheduleTask.CountInDB(TestConnection, t => t.PK == rule1UnrelatedSchedule.PK));
		}

		public void TestGlowDeleteProductionRule_Version()
		{
			var sql = new SqlQueryBuilder();
			var ruleSet = new ProductionRuleSet("PWP", "TEST") { PRS_IsLive = false }.AppendInsertAndReturnObject(sql);
			var rule1 = new ProductionRule(ruleSet, "TEST1", "{}").AppendInsertAndReturnObject(sql);
			var rule1Schedule = new StmScheduleTask(rule1.PK, "PRL").AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			RunStoredProcedure(rule1.PK, 1);
			AssertEquals("Should *not* have deleted the rule.", 1, ProductionRule.CountInDB(TestConnection, prl => prl.PK == rule1.PK));
			AssertEquals("Should *not* have deleted the schedule.", 1, StmScheduleTask.CountInDB(TestConnection, t => t.PK == rule1Schedule.PK));

			RunStoredProcedure(rule1.PK, 0);
			AssertEquals("Should have deleted the rule.", 0, ProductionRule.CountInDB(TestConnection, prl => prl.PK == rule1.PK));
			AssertEquals("Should have deleted the schedule.", 0, StmScheduleTask.CountInDB(TestConnection, t => t.PK == rule1Schedule.PK));
		}

		static void RunStoredProcedure(Guid pk, short version)
		{
			using (var command = Db.Connection.Command("GlowDeleteProductionRule"))
			{
				command.CommandType = CommandType.StoredProcedure;
				command.AddParameter("@ProductionRulePK", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@Version", SqlDbType.SmallInt, version);
				command.ExecuteScalar();
			}
		}
	}
}

