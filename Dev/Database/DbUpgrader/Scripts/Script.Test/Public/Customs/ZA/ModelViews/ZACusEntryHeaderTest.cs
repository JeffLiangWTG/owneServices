using CargoWise.Data;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.ZA.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.ZA.ModelViews.ZACusEntryHeader))]
	class ZACusEntryHeaderTest : DbCreateScriptTest
	{
		public void TestViewColumns()
		{
			TestDbViewHelper.AssertViewColumns(
				Db.Connection,
				"ZACusEntryHeader",
				"CusEntryHeader",
				new []
				{
					new TestDbViewHelper.DbColumn("CH_PK", UniqueIdentifier, -1),
					new TestDbViewHelper.DbColumn("CH_ClusterKey", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("CH_EntryNumber", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("CH_Packages", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("CH_PaymentMethod", VarChar, 1),
					new TestDbViewHelper.DbColumn("CH_RelPrintInd", VarChar, 1),
					new TestDbViewHelper.DbColumn("CH_TotalEntries", Int, -1, 10, 0),
				}
			);
		}

		public void TestViewIndexes()
		{
			TestDbViewHelper.AssertViewIndexes(
				Db.Connection,
				"ZACusEntryHeader_Idx",
				new[]
				{
					new TestDbViewHelper.DbIndex("NR_UC__CH_ClusterKey_CH_PK", "CH_ClusterKey,CH_PK"),
					new TestDbViewHelper.DbIndex("NR_UX__CH_RelPrintInd", "CH_RelPrintInd"),
				}
			);
		}
	}
}
