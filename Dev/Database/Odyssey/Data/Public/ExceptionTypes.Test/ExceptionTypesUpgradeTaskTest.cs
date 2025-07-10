using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.Testing
{
	sealed class ExceptionTypesUpgradeTaskTest : TransactionedTestCase
	{
		public void TestDataTables()
		{
			task.Run();

			var tempFile = new ExceptionTypesDataFileForTest();
			var data = tempFile.LoadDataFromDatabase();

			AssertEquals("Table Count", 1, data.Tables.Count);
			AssertEquals(ProcessWorkflowExceptionTypeSchema.Constants.TableName, data.Tables[0].TableName);
			AssertNotEquals(0, data.Tables[0].Rows.Count);
		}

		protected override void SetUp()
		{
			base.SetUp();

			DataHelpers.ClearTable(ProcessWorkflowExceptionTypeSchema.Constants.TableName);

			task = new ExceptionTypesUpgradeTask();
		}

		EmbeddedUpgradeTask task;
	}
}
