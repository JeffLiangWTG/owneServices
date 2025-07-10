using System.Linq;
using CargoWise.Data;
using Enterprise.Build.Database.Script.Testing.Public.Customs.EU.ModelViews;
using Enterprise.DbUpgrader.Shared;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.IE.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.IE.ModelViews.IEJobDeclaration))]
	class IEJobDeclarationTest : DbCreateScriptTest
	{
		public void TestViewColumns()
		{
			TestDbViewHelper.AssertViewColumns(
				Db.Connection,
				"IEJobDeclaration",
				"JobDeclaration",
				new []
				{
					new TestDbViewHelper.DbColumn("JE_PresentationStartDate", DateTime, -1),
				}.Union(EUJobDeclarationTest.ExpectedColumns).ToArray()
			);
		}

		public void TestViewIndexes()
		{
			AssertEquals("IEJobDeclaration doesn't require any indexes.", false, DbObjectCreator.ViewExists(Db.Connection, "IEJobDeclaration_Idx"));
		}
	}
}
