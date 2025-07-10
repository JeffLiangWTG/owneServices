using System;
using System.Text;
using Enterprise.DbUpgrader.Shared;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.Testing
{
	sealed class ProcessTaskTemplateUpgradeTaskForUniversalTriggersTest : TransactionedTestCase
	{
		public void TestRunUpgradeTask_WhenExistingUniversalTriggersPresent_ShouldNotThrowException()
		{
			ProcessTaskTemplateDataFileTest.ClearTables();

			var universalTemplatePK = Guid.NewGuid();
			var nonUniversalTemplatePK = Guid.NewGuid();

			var universalTriggerPK = Guid.NewGuid();
			var nonUniversalTriggerPK = Guid.NewGuid();

			var insertSql = new StringBuilder();

			insertSql.Append(BMDbTestHelper.GetWorkflowTemplateInsertSql(universalTemplatePK, "Universemplate", isUniversal: true, isSystem: true));
			insertSql.Append(WorkflowDbTestHelper.GetProcessTemplateTriggerInsertSql(universalTriggerPK, universalTemplatePK, "Frack", "Z69", 1));
			insertSql.Append(WorkflowDbTestHelper.GetUniversalCompletionTriggerActionInsertSql(Guid.NewGuid(), universalTriggerPK, "FLD"));

			insertSql.Append(BMDbTestHelper.GetWorkflowTemplateInsertSql(nonUniversalTemplatePK, "Non-Universemplate", isUniversal: false, isSystem: true));
			insertSql.Append(WorkflowDbTestHelper.GetTriggerInsertSql(nonUniversalTriggerPK, nonUniversalTemplatePK, "P0", "Frack", "Z69"));
			insertSql.Append(WorkflowDbTestHelper.GetCompletionTriggerActionInsertSql(Guid.NewGuid(), nonUniversalTriggerPK, "FLD"));

			TestConnection.ExecuteNonQuery(insertSql.ToString());

			AssertNoExceptionThrown(new ProcessTaskTemplateUpgradeTask().Run);
		}
	}
}
