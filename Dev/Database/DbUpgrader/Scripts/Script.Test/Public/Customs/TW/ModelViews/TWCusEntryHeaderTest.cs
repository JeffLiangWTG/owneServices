using CargoWise.Data;
using Enterprise.Build.Database.Script.Testing;
using Enterprise.DbUpgrader.Shared;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Public.Customs.TW.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.TW.ModelViews.TWCusEntryHeader))]
	sealed class TWCusEntryHeaderTest : DbCreateScriptTest
	{
		public void TestViewColumns()
		{
			TestDbViewHelper.AssertViewColumns(
				Db.Connection,
				"TWCusEntryHeader",
				"CusEntryHeader",
				new[]
				{
					new TestDbViewHelper.DbColumn("CH_PK", UniqueIdentifier, -1),
					new TestDbViewHelper.DbColumn("CH_ClusterKey", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("CH_DutyDueDate", DateTime, -1),
					new TestDbViewHelper.DbColumn("CH_DeclarationIncoterm", VarChar, 3),
				});
		}

		public void TestViewIndexes()
		{
			AssertEquals("TWCusEntryHeader doesn't require any indexes.", false, DbObjectCreator.ViewExists(Db.Connection, "TWCusEntryHeader_Idx"));
		}
	}
}
