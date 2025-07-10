using NUnit.Framework;

namespace Enterprise.DataTools.DbBackupAndRestore.Business
{
	sealed class StmScheduleTaskToPreserveTest : TestCase
	{
		public void TestSchema()
		{
			StmScheduleTaskToPreserveForTesting stmScheduleTaskToPreserve = new StmScheduleTaskToPreserveForTesting();
			Assert("StmScheduleTask exists", SchemaTestHelper.TableExists(stmScheduleTaskToPreserve.MainTableName_Exposed));
			Assert("Column S5_ParentID exists", SchemaTestHelper.ColumnExistsAndIsNullable("S5_ParentID"));
			Assert("StmServiceHost exists", SchemaTestHelper.TableExists("StmServiceHost"));
			Assert("Column SH_PK exists", SchemaTestHelper.ColumnExists("SH_PK"));
			Assert("Contraint 'Constraint_S5_ParentID' exists", SchemaTestHelper.ConstraintExists("Constraint_S5_ParentID", "([S5_ParentTableCode]='SH' AND [S5_ParentID] IS NULL OR [S5_ParentTableCode]<>'SH' AND [S5_ParentID] IS NOT NULL)"));
		}
	}
}
