using System.Linq;
using CargoWise.Data;
using Enterprise.Build.Database.Script.Testing.Public.Customs.EU.ModelViews;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.FR.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.FR.ModelViews.FRJobDeclaration))]
	class FRJobDeclarationTest : DbCreateScriptTest
	{
		public void TestViewColumns()
		{
			TestDbViewHelper.AssertViewColumns(
				Db.Connection,
				"FRJobDeclaration",
				"JobDeclaration",
				new []
				{
					new TestDbViewHelper.DbColumn("JE_AirRouteType", VarChar, 2),
					new TestDbViewHelper.DbColumn("JE_CustomsGuaranteeNumber", VarChar, 35),
					new TestDbViewHelper.DbColumn("JE_ExportExitType", VarChar, 3),
					new TestDbViewHelper.DbColumn("JE_ExportExitTypeReason", VarChar, 260),
					new TestDbViewHelper.DbColumn("JE_RegionOrTerritoryOfDestination", VarChar, 5),
					new TestDbViewHelper.DbColumn("JE_TariffType", VarChar, 3),
				}.Union(EUJobDeclarationTest.ExpectedColumns).ToArray()
			);
		}

		public void TestViewIndexes()
		{
			TestDbViewHelper.AssertViewIndexes(
				Db.Connection,
				"FRJobDeclaration_Idx",
				[
					new TestDbViewHelper.DbIndex("NR_UC__JE_ClusterKey_JE_PK", "JE_ClusterKey,JE_PK"),
					new TestDbViewHelper.DbIndex("NR_UX__JE_ExportExitType", "JE_ExportExitType"),
				]
			);
		}
	}
}
