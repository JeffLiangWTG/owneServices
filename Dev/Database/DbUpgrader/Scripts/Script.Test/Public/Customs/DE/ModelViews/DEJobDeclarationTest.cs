using System.Linq;
using CargoWise.Data;
using Enterprise.Build.Database.Script.Testing.Public.Customs.EU.ModelViews;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.DE.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.DE.ModelViews.DEJobDeclaration))]
	class DEJobDeclarationTest : DbCreateScriptTest
	{
		public void TestViewColumns()
		{
			TestDbViewHelper.AssertViewColumns(
				Db.Connection,
				"DEJobDeclaration",
				"JobDeclaration",
				new []
				{
					new TestDbViewHelper.DbColumn("JE_PrematureInputFlag", Bit, -1),
					new TestDbViewHelper.DbColumn("JE_PresentationEndDate", DateTime, -1),
					new TestDbViewHelper.DbColumn("JE_PresentationStartDate", DateTime, -1),
					new TestDbViewHelper.DbColumn("JE_StatisticsGoodsStatus", VarChar, 2),
					new TestDbViewHelper.DbColumn("JE_VATClaimBack", VarChar, 1),
				}.Union(EUJobDeclarationTest.ExpectedColumns).ToArray()
			);
		}

		public void TestViewIndexes()
		{
			TestDbViewHelper.AssertViewIndexes(
				Db.Connection,
				"DEJobDeclaration_Idx",
				[
					new TestDbViewHelper.DbIndex("NR_UC__JE_ClusterKey_JE_PK", "JE_ClusterKey,JE_PK"),
					new TestDbViewHelper.DbIndex("NR_UX__JE_PresentationEndDate", "JE_PresentationEndDate"),
				]
			);
		}
	}
}
