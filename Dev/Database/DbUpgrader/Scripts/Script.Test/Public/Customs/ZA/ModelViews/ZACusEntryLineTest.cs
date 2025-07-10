using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.ZA.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.ZA.ModelViews.ZACusEntryLine))]
	class ZACusEntryLineTest : DbCreateScriptTest
	{
		public void TestViewColumns()
		{
			TestDbViewHelper.AssertViewColumns(
				Db.Connection,
				"ZACusEntryLine",
				"CusEntryLine",
				new []
				{
					new TestDbViewHelper.DbColumn("CL_PK", UniqueIdentifier, -1),
					new TestDbViewHelper.DbColumn("CL_ClusterKey", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("CL_VPBAmount", Decimal, -1, 12, 0),
				}
			);
		}

		public void TestViewIndexes()
		{
			AssertEquals("ZACusEntryLine doesn't require any indexes.", false, DbObjectCreator.ViewExists(Db.Connection, "ZACusEntryLine_Idx"));
		}
	}
}
