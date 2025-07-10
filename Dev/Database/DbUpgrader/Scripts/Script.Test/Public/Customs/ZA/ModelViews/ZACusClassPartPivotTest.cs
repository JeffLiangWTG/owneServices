using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.ZA.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.ZA.ModelViews.ZACusClassPartPivot))]
	class ZACusClassPartPivotTest : DbCreateScriptTest
	{
		public void TestViewColumns()
		{
			TestDbViewHelper.AssertViewColumns(
				Db.Connection,
				"ZACusClassPartPivot",
				"CusClassPartPivot",
				new []
				{
					new TestDbViewHelper.DbColumn("CI_PK", UniqueIdentifier, -1),
					new TestDbViewHelper.DbColumn("CI_Colour", VarChar, 35),
					new TestDbViewHelper.DbColumn("CI_EngineCapacity", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("CI_NewUsed", VarChar, 1),
					new TestDbViewHelper.DbColumn("CI_ROOCert", VarChar, 35),
					new TestDbViewHelper.DbColumn("CI_VehicleFormat", VarChar, 5),
					new TestDbViewHelper.DbColumn("CI_VehicleType", VarChar, 17),
				}
			);
		}

		public void TestViewIndexes()
		{
			AssertEquals("ZACusClassPartPivot doesn't require any indexes.", false, DbObjectCreator.ViewExists(Db.Connection, "ZACusClassPartPivot_Idx"));
		}
	}
}
