using NUnit.Framework;

namespace Enterprise.DataTools.DbBackupAndRestore.Business.Testing
{
	public class StorageDocsToDeleteToPreserveTest : TestCase
	{
		public void TestSchema()
		{
			var storageDocsToDeleteToPreserve = new StorageDocsToDeleteToPreserveForTesting();
			Assert("StorageDocsToDelete Table exists", SchemaTestHelper.TableExists(storageDocsToDeleteToPreserve.MainTableName_Exposed));
		}
	}
	class StorageDocsToDeleteToPreserveForTesting : StorageDocsToDeleteToPreserve
	{
		public string MainTableName_Exposed
		{
			get { return TargetTableName; }
		}
	}
}
