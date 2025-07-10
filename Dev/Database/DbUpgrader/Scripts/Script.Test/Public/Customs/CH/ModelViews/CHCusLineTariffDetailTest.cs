using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.CH.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.CH.ModelViews.CHCusLineTariffDetail))]
	sealed class CHCusLineTariffDetailTest : DbCreateScriptTest
	{
		public void TestViewColumns()
		{
			TestDbViewHelper.AssertViewColumns(
				Db.Connection,
				"CHCusLineTariffDetail",
				"CusLineTariffDetail",
				new[]
				{
					new TestDbViewHelper.DbColumn("BZ_PK", UniqueIdentifier, -1),
					new TestDbViewHelper.DbColumn("BZ_TaxType", NVarChar, 3),
					new TestDbViewHelper.DbColumn("BZ_AlcoholPercentage", Decimal, -1, 4, 1),
					new TestDbViewHelper.DbColumn("BZ_BaseValue", Decimal, -1, 11, 2),
					new TestDbViewHelper.DbColumn("BZ_ManualRate", Decimal, -1, 7, 2),
				}
			);
		}

		public void TestViewIndexes()
		{
			AssertEquals("CHCusLineTariffDetail doesn't require any indexes.", false, DbObjectCreator.ViewExists(Db.Connection, "CHCusLineTariffDetail_Idx"));
		}
	}
}
