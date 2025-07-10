using System;
using System.IO;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.Testing
{
	sealed class ProcessTaskTemplateDataFileTest : TransactionedTestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDataFileExists()
		{
			var dataFile = new ProcessTaskTemplateDataFile();
			Assert(File.Exists(dataFile.FileFullPath));
		}

		public void TestLoadDataFromDatabase()
		{
			ClearTables();
			InsertExistingData();

			using (Db.Connection.TrackExecutedCommands())
			{
				var data = DataFile.LoadDataFromDatabase();

				AssertEquals("Table count", 4, data.Tables.Count);
				AssertEquals("ProcessTaskTemplate row count (system tasks only)", 1, data.Tables[ProcessTaskTemplateSchema.Constants.TableName].Rows.Count);
				AssertEquals("ProcessTasks row count (system tasks only)", 2, data.Tables[ProcessTasksSchema.Constants.TableName].Rows.Count);

				Assert("ProcessTaskNotification data load query should have WITH(FORCESEEK) table hint.", SqlEventTracker.Instance.LastSqlQuery.Contains("dbo.ProcessTaskNotification WITH (FORCESEEK)"));
				Assert("ProcessTasks subquery in ProcoessTaskNotification data load query should have WITH(FORCESEEK) table hint and use NR_RC__P9_ParentID index", SqlEventTracker.Instance.LastSqlQuery.Contains("dbo.ProcessTasks WITH (FORCESEEK, INDEX = NR_RC__P9_ParentID)"));
			}
		}

		#region Implementation

		readonly Guid UserDefinedProcessTaskTemplateGuid = Guid.NewGuid();
		readonly Guid UserDefinedProcessTaskGuid1 = Guid.NewGuid();

		readonly Guid SystemProcessTaskTemplateGuid = Guid.NewGuid();
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

		internal static void ClearTables()
		{
			DataHelpers.ClearTable(ProcessTaskNotificationSchema.Constants.TableName);
			DataHelpers.ClearTable(ProcessTasksSchema.Constants.TableName);
			DataHelpers.ClearTable(ProcessJobTriggerLinkSchema.Constants.TableName);
			DataHelpers.ClearTable(ProcessTemplateTriggerSchema.Constants.TableName);
			DataHelpers.ClearTable(ProcessTaskTemplateSchema.Constants.TableName);
		}

		void InsertExistingData()
		{
			string processTaskTemplateInsertSql = String.Format(@"
				INSERT INTO dbo.ProcessTaskTemplate (P0_PK, P0_IsSystem, P0_IsActive, P0_Name) VALUES ('{0}', 0, 0, 'Nope');
				INSERT INTO dbo.ProcessTaskTemplate (P0_PK, P0_IsSystem, P0_IsActive, P0_Name) VALUES ('{1}', 1, 0, 'Yep');",
				UserDefinedProcessTaskTemplateGuid, SystemProcessTaskTemplateGuid);
			Db.Connection.ExecuteNonQuery(processTaskTemplateInsertSql);

			string processTaskInsertSql = String.Format(@"
				INSERT INTO dbo.ProcessTasks (P9_ParentID, P9_ParentTableCode, P9_PK) VALUES ('{0}', 'P0', '{2}');
				INSERT INTO dbo.ProcessTasks (P9_ParentID, P9_ParentTableCode, P9_PK) VALUES ('{1}', 'P0', '{3}');
				INSERT INTO dbo.ProcessTasks (P9_ParentID, P9_ParentTableCode, P9_PK) VALUES ('{1}', 'P0', '{4}');",
				UserDefinedProcessTaskTemplateGuid, SystemProcessTaskTemplateGuid,
				UserDefinedProcessTaskGuid1, SystemProcessTaskGuid1, SystemProcessTaskGuid2);
			Db.Connection.ExecuteNonQuery(processTaskInsertSql);
		}

		#endregion
	}
}
