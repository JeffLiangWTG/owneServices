using CargoWise.Data;
using Enterprise.Build.Database.Script.Testing;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Public.Customs.NO.Testing
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.NO.ModelViews.NOCusEntryHeader))]
	sealed class NOCusEntryHeaderTest : DbCreateScriptTest
	{
		public void TestViewColumns()
		{
			TestDbViewHelper.AssertViewColumns(
				Db.Connection,
				"NOCusEntryHeader",
				"CusEntryHeader",
				new TestDbViewHelper.DbColumn("CH_PK", UniqueIdentifier, -1),
				new TestDbViewHelper.DbColumn("CH_ClusterKey", Int, -1, 10, 0),
				new TestDbViewHelper.DbColumn("CH_PaymentMethod", VarChar, 1),
				new TestDbViewHelper.DbColumn("CH_ReCalcCaseCode", VarChar, 4),
				new TestDbViewHelper.DbColumn("CH_ReCalcDeclType", VarChar, 5),
				new TestDbViewHelper.DbColumn("CH_ReCalcOrigDecl", VarChar, 35),
				new TestDbViewHelper.DbColumn("CH_ReCalcReason", VarChar, 350),
				new TestDbViewHelper.DbColumn("CH_ReCalcReplyMessage", VarChar, 350),
				new TestDbViewHelper.DbColumn("CH_ToCustomsControllingUnit", VarChar, 10)
			);
		}

		public void TestViewIndexes()
		{
			TestDbViewHelper.AssertViewIndexes(
				Db.Connection,
				"NOCusEntryHeader_Idx"
			);
		}
	}
}
