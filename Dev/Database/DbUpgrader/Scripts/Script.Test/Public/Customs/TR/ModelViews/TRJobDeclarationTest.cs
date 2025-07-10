using System.Linq;
using CargoWise.Data;
using Enterprise.Build.Database.Script.Testing.Public.Customs.EU.ModelViews;
using Enterprise.DbUpgrader.Shared;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.TR.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.TR.ModelViews.TRJobDeclaration))]
	class TRJobDeclarationTest : DbCreateScriptTest
	{
		public void TestViewColumns()
		{
			TestDbViewHelper.AssertViewColumns(
				Db.Connection,
				"TRJobDeclaration",
				"JobDeclaration",
				new []
				{
					new TestDbViewHelper.DbColumn("JE_BankCode", VarChar, 12),
					new TestDbViewHelper.DbColumn("JE_CountryOfSupply", VarChar, 4),
					new TestDbViewHelper.DbColumn("JE_NumberOfDocs", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("JE_ShippingCountry", VarChar, 2),
					new TestDbViewHelper.DbColumn("JE_TradeType", VarChar, 3),
				}.Union(EUJobDeclarationTest.ExpectedColumns).ToArray()
			);
		}

		public void TestViewIndexes()
		{
			AssertEquals("TRJobDeclaration doesn't require any indexes.", false, DbObjectCreator.ViewExists(Db.Connection, "TRJobDeclaration_Idx"));
		}
	}
}
