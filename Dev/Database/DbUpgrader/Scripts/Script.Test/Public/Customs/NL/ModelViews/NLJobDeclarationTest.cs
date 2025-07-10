using System.Linq;
using CargoWise.Data;
using Enterprise.Build.Database.Script.Testing.Public.Customs.EU.ModelViews;
using Enterprise.DbUpgrader.Shared;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.NL.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.NL.ModelViews.NLJobDeclaration))]
	class NLJobDeclarationTest : DbCreateScriptTest
	{
		public void TestViewColumns()
		{
			TestDbViewHelper.AssertViewColumns(
				Db.Connection,
				"NLJobDeclaration",
				"JobDeclaration",
				new []
				{
					new TestDbViewHelper.DbColumn("JE_PresentationEndDate", DateTime, -1),
					new TestDbViewHelper.DbColumn("JE_PresentationStartDate", DateTime, -1),
				}.Union(EUJobDeclarationTest.ExpectedColumns).ToArray()
			);
		}

		public void TestViewIndexes()
		{
			AssertEquals("NLJobDeclaration doesn't require any indexes.", false, DbObjectCreator.ViewExists(Db.Connection, "NLJobDeclaration_Idx"));
		}
	}
}
