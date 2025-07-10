using System;
using System.Collections.Generic;
using CargoWise.DbUpgrader.Foundation;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Schema.Testing
{
	sealed class IndexScriptRunnerWithPartitionedTableMockDbsTest : TestCaseWithMockMainDbAndTemplateDbTransactional
	{
		public void TestDropRecreateAndAddIndexesOnPartitionedTable()
		{
			IndexTestHelper.AssertIndexExistInDb(TestConnection, mockMainDb, "PartitionedTable", "IX_NoChanges", isUnique: false, isClustered: false, fillFactor: 0, filter: null, compression: null);
			IndexTestHelper.AssertIndexExistInDb(TestConnection, mockMainDb, "PartitionedTable", "IX_ToBeRemoved", isUnique: false, isClustered: false, fillFactor: 0, filter: null, compression: null);
			IndexTestHelper.AssertIndexNotExists(TestConnection, mockMainDb, "PartitionedTable", "IX_ToBeAdded");

			IndexTestHelper.AssertIndexExistInDb(TestConnection, mockTemplateDb, "PartitionedTable", "IX_NoChanges", isUnique: false, isClustered: false, fillFactor: 0, filter: null, compression: null);
			IndexTestHelper.AssertIndexNotExists(TestConnection, mockTemplateDb, "PartitionedTable", "IX_ToBeRemoved");
			IndexTestHelper.AssertIndexExistInDb(TestConnection, mockTemplateDb, "PartitionedTable", "IX_ToBeAdded", isUnique: false, isClustered: false, fillFactor: 0, filter: null, compression: null);

			var logger = new DummyLogger();
			var testScriptRunner = new IndexScriptRunner(TestConnection, mockMainDb, mockTemplateDb, logger);
			RunActionOnMockMainDb(testScriptRunner.DropOldIndexes);
			RunActionOnMockMainDb(testScriptRunner.RecreateModifiedIndexes);
			RunActionOnMockMainDb(testScriptRunner.CreateNewIndexes);

			// In Main DB and not in the Template - expect: removed
			IndexTestHelper.AssertIndexNotExists(TestConnection, mockMainDb, "PartitionedTable", "IX_ToBeRemoved");
			// In Template and not in the Main DB - expect: added
			IndexTestHelper.AssertIndexExistInDb(TestConnection, mockMainDb, "PartitionedTable", "IX_ToBeAdded", isUnique: false, isClustered: false, fillFactor: 0, filter: null, compression: null);
			// In both Main DB and Template - expect: not re-created
			IndexTestHelper.AssertIndexExistInDb(TestConnection, mockMainDb, "PartitionedTable", "IX_NoChanges", isUnique: false, isClustered: false, fillFactor: 0, filter: null, compression: null);

			// Assert LogsIX_ToBeRemoved
			AssertEquals("Logs\r\n" + String.Join("\r\n", logger.Logs), 2, logger.Logs.Count);

			AssertCollectionContains(
				"Logs should contain IX_ToBeRemoved but didn't.\r\n" + String.Join("\r\n", logger.Logs),
				"(-) [dbo].[PartitionedTable].[IX_ToBeRemoved]", logger.Logs);
			AssertCollectionContains(
				"Logs should contain IX_ToBeAdded but didn't.\r\n" + String.Join("\r\n", logger.Logs),
				"(+) [PartitionedTable].[IX_ToBeAdded]", logger.Logs);
			AssertCollectionNotContains(
				"Logs should not contain IX_NoChanges but did.",
				"(~) [PartitionedTable].[IX_NoChanges]", logger.Logs);
		}

		protected override IAuxiliaryDbCreator GetMockMainDbCreator()
		{
			return new AuxiliaryDbCreatorForTesting(mockMainDb, createTestMainDbObjectsScript);
		}

		protected override IAuxiliaryDbCreator GetMockTemplateDbCreator()
		{
			return new AuxiliaryDbCreatorForTesting(mockTemplateDb, createTestTemplateDbObjectsScript);
		}

		public class DummyLogger : IUpgradeTaskWorkflowLogger
		{
			public void ActivateSubtaskProgress(int numOfSubtasks) { }
			public void ActivateTaskProgress(int numOfTasks) { }
			public void ShowInfoMessage(string infoMessage) { Logs.Add(infoMessage.Trim()); }
			public void ShowTaskError(string errorMessage) { Logs.Add(errorMessage.Trim()); }
			public void StartSubtask(string subtask) { Logs.Add(subtask.Trim()); }
			public void StartTask(string task) { Logs.Add(task.Trim()); }

			public List<string> Logs = new List<string>();
		}

		#region Scripts

		#region dbBeingUpgraded

		const string createTestMainDbObjectsScript = @"
			CREATE PARTITION FUNCTION PF_NumberRange(INT) AS RANGE LEFT FOR VALUES ();
			CREATE PARTITION SCHEME PS_NumberRange AS PARTITION PF_NumberRange ALL TO ([PRIMARY]);

			-- This table is partitioned
			-- IX_ToNoChanges - will not be changed
			-- IX_ToBeRemoved - will be REMOVED
			-- IX_ToBeAdded   - will be ADDED
			CREATE TABLE PartitionedTable
			( 
				Col1 INT      NOT NULL,
				Col2 CHAR(1)  NOT NULL,
				Col3 INT      NULL
			) ON PS_NumberRange(Col1);

			ALTER TABLE PartitionedTable ADD CONSTRAINT PK_PartitionedTable PRIMARY KEY NONCLUSTERED (Col1);
			CREATE NONCLUSTERED INDEX IX_NoChanges ON PartitionedTable (Col2);
			CREATE NONCLUSTERED INDEX IX_ToBeRemoved ON PartitionedTable (Col3);
			";

		#endregion // dbBeingUpgraded

		#region templateDb

		const string createTestTemplateDbObjectsScript = @"
			-- This table is partitioned in the DB being upgraded
			-- IX_ToNoChanges - no changes
			-- IX_ToBeRemoved - REMOVED
			-- IX_ToBeAdded   - ADDED
			CREATE TABLE PartitionedTable
			( 
				Col1 INT      NOT NULL,
				Col2 CHAR(1)  NOT NULL,
				Col3 INT      NULL
			);

			ALTER TABLE PartitionedTable ADD CONSTRAINT PK_PartitionedTable PRIMARY KEY NONCLUSTERED (Col1);
			CREATE NONCLUSTERED INDEX IX_NoChanges ON PartitionedTable (Col2);
			CREATE NONCLUSTERED INDEX IX_ToBeAdded ON PartitionedTable (Col3);
			";

		#endregion // templateDb

		#endregion // Scripts
	}
}
