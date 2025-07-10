using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformation.Common.Testing
{
	sealed class MoveAddInfoHelperTest : TransactionedTestCase
	{
		public void TestMoveAddInfoHelperMembers()
		{
			Assert(AddInfoHelper.OffLineUpdateTargetSqlText.Contains("= 1"));
			AssertEquals(AddInfoHelper.TempColumnNameForIndex, "JI_TMP_HasMoveColumnThirdUQ");
			AssertEquals(AddInfoHelper.TempIndexName, "JI_TMP_HasMoveColumnThirdUQ_Index");
			AssertEquals(AddInfoHelper.DropTempIndexSqlText, "DROP INDEX JI_TMP_HasMoveColumnThirdUQ_Index ON dbo.JobComInvoiceLine");
			AssertEquals(AddInfoHelper.DropTempColumnSqlText, "ALTER TABLE dbo.JobComInvoiceLine DROP COLUMN JI_TMP_HasMoveColumnThirdUQ");
		}

		MoveColumnFromAddInfoHelper AddInfoHelper
		{
			get { return new MoveColumnFromAddInfoHelper("dbo.JobComInvoiceLine", "JI", "JI_AddInfo", "ThirdUQ", "JI_CustomsThirdUnitQty", "", "ALTER TABLE [JobComInvoiceLine] ADD [JI_CustomsThirdUnitQty] [varchar](4) NOT NULL"); }
		}
	}
}
