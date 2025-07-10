using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.IN.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.IN.ModelViews.INCusEntryInstruction))]
	class INCusEntryInstructionTest : DbCreateScriptTest
	{
		public void TestViewColumns()
		{
			TestDbViewHelper.AssertViewColumns(
				Db.Connection,
				"INCusEntryInstruction",
				"CusEntryInstruction",
				new[]
				{
					new TestDbViewHelper.DbColumn("CEI_PK", UniqueIdentifier, -1),
					new TestDbViewHelper.DbColumn("CEI_ClusterKey", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("CEI_WeightUQ", VarChar, 3),
					new TestDbViewHelper.DbColumn("CEI_TotalContainer", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("CEI_LoosePackages", Int, -1, 10, 0)
				}
			);
		}

		public void TestViewIndexes()
		{
			AssertEquals("INCusEntryInstruction doesn't require any indexes.", false, DbObjectCreator.ViewExists(Db.Connection, "INCusEntryInstruction_Idx"));
		}
	}
}
