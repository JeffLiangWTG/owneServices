using System.Linq;
using CargoWise.Data;
using Enterprise.Build.Database.Script.Testing.Public.Customs.EU.ModelViews;
using Enterprise.DbUpgrader.Shared;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.BE.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.BE.ModelViews.BEJobDeclaration))]
	class BEJobDeclarationTest : DbCreateScriptTest
	{
		public void TestViewColumns()
		{
			TestDbViewHelper.AssertViewColumns(
				Db.Connection,
				"BEJobDeclaration",
				"JobDeclaration",
				new []
				{
					new TestDbViewHelper.DbColumn("JE_PresentationStartDate", DateTime, -1),
				}.Union(EUJobDeclarationTest.ExpectedColumns).ToArray()
			);
		}

		public void TestViewIndexes()
		{
			AssertEquals("BEJobDeclaration doesn't require any indexes.", false, DbObjectCreator.ViewExists(Db.Connection, "BEJobDeclaration_Idx"));
		}
	}
}
