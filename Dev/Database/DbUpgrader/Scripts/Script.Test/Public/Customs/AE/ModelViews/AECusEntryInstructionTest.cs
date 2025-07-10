using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.AE.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.AE.ModelViews.AECusEntryInstruction))]
	class AECusEntryInstructionTest : DbCreateScriptTest
	{
		public void TestViewColumns()
		{
			TestDbViewHelper.AssertViewColumns(
				Db.Connection,
				"AECusEntryInstruction",
				"CusEntryInstruction",
				[
					new TestDbViewHelper.DbColumn("CEI_PK", UniqueIdentifier, -1),
					new TestDbViewHelper.DbColumn("CEI_ClusterKey", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("CEI_DeclarationPurpose", VarChar, 3),
					new TestDbViewHelper.DbColumn("CEI_DeclarationPurposeDetails", VarChar, 255),
					new TestDbViewHelper.DbColumn("CEI_TradeType", VarChar, 1)
				]
			);
		}

		public void TestViewIndexes()
		{
			AssertEquals("AECusEntryInstruction doesn't require any indexes.", expected: false, DbObjectCreator.ViewExists(Db.Connection, "AECusEntryInstruction_Idx"));
		}
	}
}
