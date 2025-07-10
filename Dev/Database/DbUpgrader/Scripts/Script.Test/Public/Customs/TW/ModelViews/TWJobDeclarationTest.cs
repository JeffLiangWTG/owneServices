using CargoWise.Data;
using Enterprise.Build.Database.Script.Testing;
using Enterprise.DbUpgrader.Shared;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Public.Customs.TW.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.TW.ModelViews.TWJobDeclaration))]
	sealed class TWJobDeclarationTest : DbCreateScriptTest
	{
		public void TestViewColumns()
		{
			TestDbViewHelper.AssertViewColumns(
				Db.Connection,
				"TWJobDeclaration",
				"JobDeclaration",
				new[]
				{
					new TestDbViewHelper.DbColumn("JE_PK", UniqueIdentifier, -1),
					new TestDbViewHelper.DbColumn("JE_ClusterKey", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("JE_VesselArrivalReg", VarChar, 6),
					new TestDbViewHelper.DbColumn("JE_SLD", VarChar, 4),
					new TestDbViewHelper.DbColumn("JE_SplitMark", Bit, -1),
					new TestDbViewHelper.DbColumn("JE_OtherBankAccount", VarChar, 16),
					new TestDbViewHelper.DbColumn("JE_Z99PortOfOrigin", VarChar, 256),
					new TestDbViewHelper.DbColumn("JE_Z99FinalDestination", VarChar, 256),
					new TestDbViewHelper.DbColumn("JE_DeclDocType", VarChar, 1),
				});
		}

		public void TestViewIndexes()
		{
			AssertEquals("TWJobDeclaration doesn't require any indexes.", false, DbObjectCreator.ViewExists(Db.Connection, "TWJobDeclaration_Idx"));
		}
	}
}
