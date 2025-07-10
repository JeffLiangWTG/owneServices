using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.TW.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.TW.ModelViews.TWCusContainer))]
	sealed class TWCusContainerTest : DbCreateScriptTest
	{
		public void TestViewColumns()
		{
			TestDbViewHelper.AssertViewColumns(
				Db.Connection,
				"TWCusContainer",
				"CusContainer",
				new[]
				{
					new TestDbViewHelper.DbColumn("CO_PK", UniqueIdentifier, -1),
					new TestDbViewHelper.DbColumn("CO_ClusterKey", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("CO_IsPart", Bit, -1),
				}
			);
		}

		public void TestViewIndexes()
		{
			AssertEquals("TWCusContainer doesn't require any indexes.", false, DbObjectCreator.ViewExists(Db.Connection, "TWCusContainer_Idx"));
		}
	}
}
