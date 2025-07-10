using System;
using System.Text;
using CargoWise.DbUpgrader.Scripts.Definitions.Workflow;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Workflow.Testing
{
	[TestedType(typeof(GetCurrentTask))]
	class GetCurrentTaskTest : DbCreateScriptTest
	{
		public void TestGetCurrentTask()
		{
			var parentPK = Guid.NewGuid();

			var assignedTaskPK = Guid.NewGuid();
			var suspendedTaskPK = Guid.NewGuid();
			var workingTaskPK = Guid.NewGuid();

			var insertSql = new StringBuilder();
			insertSql.AppendFormat(ProcessTasksInsertSql, suspendedTaskPK, "SUS", "test suspended task", "XYZ", 2, parentPK);
			insertSql.AppendFormat(ProcessTasksInsertSql, assignedTaskPK, "ASN", "test assigned task", "ABC", 3, parentPK);
			insertSql.AppendFormat(ProcessTasksInsertSql, workingTaskPK, "WRK", "test working task", "YUI", 4, parentPK);
			TestConnection.ExecuteNonQuery(insertSql.ToString());

			AssertEquals("Current Task should be the suspended task", suspendedTaskPK, GetCurrentTask(parentPK));

			var closedTaskPK = Guid.NewGuid();
			insertSql = new StringBuilder();
			insertSql.AppendFormat(ProcessTasksClosedInsertSql, closedTaskPK, "CLS", "test closed task", "CCC", 1, parentPK, "CAST(0 as SMALLDATETIME)");
			TestConnection.ExecuteNonQuery(insertSql.ToString());

			AssertEquals("Current Task should still be the suspended task", suspendedTaskPK, GetCurrentTask(parentPK));
		}

		Guid GetCurrentTask(Guid parentPK)
		{
			var sql = string.Format("SELECT P9_PK from dbo.GetCurrentTask('{0}')", parentPK);
			var result = Guid.Empty;
			using (var reader = TestConnection.Command(sql).ExecuteReader())
			{
				reader.Read();
				result = reader.GetGuid(0);
			}
			return result;
		}

		const string ProcessTasksInsertSql = @"
			INSERT dbo.ProcessTasks (
				P9_PK, P9_Status, P9_Description, P9_GS_NKAssignedStaffMember,P9_Sequence, P9_ParentID, P9_ParentTableCode
			) VALUES (
				'{0}', '{1}', '{2}', '{3}', {4}, '{5}', 'JS'
			)
			";

		const string ProcessTasksClosedInsertSql = @"
			INSERT dbo.ProcessTasks (
				P9_PK, P9_Status, P9_Description, P9_GS_NKAssignedStaffMember,P9_Sequence, P9_ParentID, P9_ParentTableCode, P9_CompletedTimeUtc
			) VALUES (
				'{0}', '{1}', '{2}', '{3}', {4}, '{5}', 'JS', {6}
			)
			";
	}
}

