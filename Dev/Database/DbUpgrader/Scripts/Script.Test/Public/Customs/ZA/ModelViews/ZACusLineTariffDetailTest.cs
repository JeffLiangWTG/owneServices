using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.ZA.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.ZA.ModelViews.ZACusLineTariffDetail))]
	class ZACusLineTariffDetailTest : DbCreateScriptTest
	{
		public void TestViewColumns()
		{
			TestDbViewHelper.AssertViewColumns(
				Db.Connection,
				"ZACusLineTariffDetail",
				"CusLineTariffDetail",
				new []
				{
					new TestDbViewHelper.DbColumn("BZ_PK", UniqueIdentifier, -1),
					new TestDbViewHelper.DbColumn("BZ_CheckDigit", NVarChar, 2),
				}
			);
		}

		public void TestViewIndexes()
		{
			AssertEquals("ZACusLineTariffDetail doesn't require any indexes.", false, DbObjectCreator.ViewExists(Db.Connection, "ZACusLineTariffDetail_Idx"));
		}
	}
}
