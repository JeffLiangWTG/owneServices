using NUnit.Framework;

namespace Enterprise.DataTools.DbBackupAndRestore.Business
{
	sealed class NumberFountainsToPreserveTest : TestCase
	{
		public void TestSchema()
		{
			Assert("Column SN_ID exists", SchemaTestHelper.ColumnExists("SN_ID"));
			Assert("Column SN_Name exists", SchemaTestHelper.ColumnExists("SN_Name"));
			Assert("Column SN_Owner exists", SchemaTestHelper.ColumnExists("SN_Owner"));
			Assert("Column SN_Value exists", SchemaTestHelper.ColumnExists("SN_Value"));

			Assert("Column SG_SN exists", SchemaTestHelper.ColumnExists("SG_SN"));
			Assert("Column SG_Value exists", SchemaTestHelper.ColumnExists("SG_Value"));
		}
	}
}
