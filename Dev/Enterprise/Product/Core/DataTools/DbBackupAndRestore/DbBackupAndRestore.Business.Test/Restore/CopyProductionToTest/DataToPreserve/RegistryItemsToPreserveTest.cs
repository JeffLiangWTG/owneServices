using NUnit.Framework;

namespace Enterprise.DataTools.DbBackupAndRestore.Business
{
	sealed class RegistryItemsToPreserveTest : TestCase
	{
		public void TestSchema()
		{
			RegistryItemsToPreserveForTesting registryItemsToPreserve = new RegistryItemsToPreserveForTesting();
			Assert("StmData exists", SchemaTestHelper.TableExists(registryItemsToPreserve.MainTableName_Exposed));
			Assert("Column SD_Name exists", SchemaTestHelper.ColumnExists("SD_Name"));
			Assert("Column SD_PreserveTestValue exists", SchemaTestHelper.ColumnExists("SD_PreserveTestValue"));
		}
	}
}
