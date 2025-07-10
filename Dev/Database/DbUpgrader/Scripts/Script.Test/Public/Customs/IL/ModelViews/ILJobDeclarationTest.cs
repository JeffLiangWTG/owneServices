using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.IL.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.IL.ModelViews.ILJobDeclaration))]
	class ILJobDeclarationTest : DbCreateScriptTest
	{
		public void TestViewColumns()
		{
			TestDbViewHelper.AssertViewColumns(
				Db.Connection,
				"ILJobDeclaration",
				"JobDeclaration",
				new[]
				{
					new TestDbViewHelper.DbColumn("JE_PK", UniqueIdentifier, -1),
					new TestDbViewHelper.DbColumn("JE_ClusterKey", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("JE_MasterBillIssuedDate", DateTime, -1),
				}
			);
		}

		public void TestViewIndexes()
		{
			AssertEquals("ILJobDeclaration doesn't require any indexes.", false, DbObjectCreator.ViewExists(Db.Connection, "ILJobDeclaration_Idx"));
		}
	}
}
