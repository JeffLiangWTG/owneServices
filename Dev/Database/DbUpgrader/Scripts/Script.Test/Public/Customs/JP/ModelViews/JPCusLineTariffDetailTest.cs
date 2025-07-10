using CargoWise.Data;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.JP.ModelViews;

[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.JP.ModelViews.JPCusLineTariffDetail))]
sealed class JPCusLineTariffDetailTest : DbCreateScriptTest
{
	public void TestViewColumns()
	{
		TestDbViewHelper.AssertViewColumns(
			Db.Connection,
			"JPCusLineTariffDetail",
			"CusLineTariffDetail",
			new[]
			{
				new TestDbViewHelper.DbColumn("BZ_PK", UniqueIdentifier, -1),
				new TestDbViewHelper.DbColumn("BZ_ExemptionReductionCode", NVarChar, 3),
			}
		);
	}
}
