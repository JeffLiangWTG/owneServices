using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.Testing
{
	sealed class ProcessTaskTemplateUpgradeTaskTest : TransactionedTestCase
	{
		public void TestRunUpgradeTask()
		{
			ProcessTaskTemplateDataFileTest.ClearTables();
			InsertPreExistingData();

			UpgradeTask.Run();
			var data = DataFile.LoadDataFromDatabase();
			DataTable processTaskTemplateTable = data.Tables[ProcessTaskTemplateSchema.Constants.TableName];
			DataTable processTasksTable = data.Tables[ProcessTasksSchema.Constants.TableName];

			AssertEquals("ProcessTaskTemplate row count", 39, processTaskTemplateTable.Rows.Count);
			AssertEquals("Pre-existing user-defined template retained", true, ExistsProcessTaskTemplate(UserDefinedProcessTaskTemplateGuid));
			AssertEquals("Pre-existing system-defined template that no longer exists deleted", false, processTaskTemplateTable.Rows.Contains(ToBeDeletedSystemProcessTaskTemplateGuid));

			AssertEquals("New system-defined template added", true, processTaskTemplateTable.Rows.Contains(NewlyAddedSystemProcessTaskTemplateGuid));
			AssertEquals("New system-defined template task added", true, processTasksTable.Rows.Contains(NewlyAddedSystemProcessTaskGuid));
			AssertEquals("Existing system-defined template updated", true, processTaskTemplateTable.Rows.Contains(InactiveSystemProcessTaskTemplateGuid));
			AssertEquals("Existing system-defined template updated", "BRK", processTaskTemplateTable.Rows.Find(InactiveSystemProcessTaskTemplateGuid)[ProcessTaskTemplateSchema.P0_ProcessType.Name]);
			AssertEquals("Existing system-defined template inactive state retained", false, processTaskTemplateTable.Rows.Find(InactiveSystemProcessTaskTemplateGuid)[ProcessTaskTemplateSchema.P0_IsActive.Name]);
		}

		public void TestRunUpgradeTask_TaskTemplatesSystemAndActive()
		{
			ProcessTaskTemplateDataFileTest.ClearTables();
			UpgradeTask.Run();
			var data = DataFile.LoadDataFromDatabase();
			var leaveCancelledWorkflow = new Guid("fac4d524-a5d9-492c-8878-939611210673");
			var leaveRequestedWithFallbackApprovalWorkflow = new Guid("4945765E-B221-4E50-BB71-8B2D8E32F6F5");
			var startsInActive = new Guid[] { leaveCancelledWorkflow, leaveRequestedWithFallbackApprovalWorkflow };
			foreach (DataRow row in data.Tables[ProcessTaskTemplateSchema.Constants.TableName].Rows)
			{
				AssertEquals("P0_IsSystem should be true", true, row[ProcessTaskTemplateSchema.P0_IsSystem.Name]);
				if (startsInActive.Contains((Guid)row[ProcessTaskTemplateSchema.PK.Name]))
				{
					AssertEquals("P0_IsActive should be true", false, row[ProcessTaskTemplateSchema.P0_IsActive.Name]);
				}
				else
				{
					AssertEquals("P0_IsActive should be true", true, row[ProcessTaskTemplateSchema.P0_IsActive.Name]);
				}
			}
		}

		#region Implementation

		readonly Guid UserDefinedProcessTaskTemplateGuid = Guid.NewGuid();
		readonly Guid UserDefinedProcessTaskGuid1 = Guid.NewGuid();

		readonly Guid NewlyAddedSystemProcessTaskTemplateGuid = new Guid("49765a1e-fe40-4267-85c2-10885f835cbe");
		readonly Guid NewlyAddedSystemProcessTaskGuid = new Guid("cc2c282c-bc4c-40e9-9f2b-ea0ba9032c44");
		readonly Guid InactiveSystemProcessTaskTemplateGuid = new Guid("9d777e74-fea9-4227-b81c-9e9cf351a62f");
		readonly Guid ToBeDeletedSystemProcessTaskTemplateGuid = Guid.NewGuid();
		readonly Guid SystemProcessTaskGuid1 = Guid.NewGuid();
		readonly Guid SystemProcessTaskGuid2 = Guid.NewGuid();

		ProcessTaskTemplateDataFile DataFile
		{
			get
			{
				if (dataFile == null)
				{
					dataFile = new ProcessTaskTemplateDataFile();
				}
				return dataFile;
			}
		}
		ProcessTaskTemplateDataFile dataFile;

		ProcessTaskTemplateUpgradeTask UpgradeTask
		{
			get
			{
				if (upgradeTask == null)
				{
					upgradeTask = new ProcessTaskTemplateUpgradeTask();
				}
				return upgradeTask;
			}
		}
		ProcessTaskTemplateUpgradeTask upgradeTask;

		void InsertPreExistingData()
		{
			string processTaskTemplateInsertSql = String.Format(@"
				INSERT INTO dbo.ProcessTaskTemplate (P0_PK, P0_IsSystem, P0_IsActive, P0_Name) VALUES ('{0}', 0, 1, 'One');
				INSERT INTO dbo.ProcessTaskTemplate (P0_PK, P0_IsSystem, P0_IsActive, P0_Name) VALUES ('{1}', 1, 1, 'Two');
				INSERT INTO dbo.ProcessTaskTemplate (P0_PK, P0_IsSystem, P0_IsActive, P0_Name) VALUES ('{2}', 1, 0, 'Three');",
				UserDefinedProcessTaskTemplateGuid, ToBeDeletedSystemProcessTaskTemplateGuid, InactiveSystemProcessTaskTemplateGuid);
			Db.Connection.ExecuteNonQuery(processTaskTemplateInsertSql);

			string processTaskInsertSql = String.Format(@"
				INSERT INTO dbo.ProcessTasks (P9_ParentID, P9_ParentTableCode, P9_PK) VALUES ('{0}', 'P0', '{2}');
				INSERT INTO dbo.ProcessTasks (P9_ParentID, P9_ParentTableCode, P9_PK) VALUES ('{1}', 'P0', '{3}');
				INSERT INTO dbo.ProcessTasks (P9_ParentID, P9_ParentTableCode, P9_PK) VALUES ('{1}', 'P0', '{4}');",
				UserDefinedProcessTaskTemplateGuid, ToBeDeletedSystemProcessTaskTemplateGuid,
				UserDefinedProcessTaskGuid1, SystemProcessTaskGuid1, SystemProcessTaskGuid2);
			Db.Connection.ExecuteNonQuery(processTaskInsertSql);
		}

		bool ExistsProcessTaskTemplate(Guid processTaskTemplate)
		{
			DbCommand command = Db.Connection.Command("SELECT 'Y' FROM dbo.ProcessTaskTemplate WHERE P0_PK='" + processTaskTemplate + "'");
			return command.ExecuteScalar() as string == "Y";
		}

		#endregion
	}
}
