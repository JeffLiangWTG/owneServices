using CargoWise.Data;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.IL.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.IL.ModelViews.ILCusEntryInstruction))]
	sealed class ILCusEntryInstructionTest : DbCreateScriptTest
	{
		public void TestViewColumns() => TestDbViewHelper.AssertViewColumns(
			Db.Connection,
			"ILCusEntryInstruction",
			"CusEntryInstruction",
			new TestDbViewHelper.DbColumn("CEI_PK", UniqueIdentifier, -1),
			new TestDbViewHelper.DbColumn("CEI_ClusterKey", Int, -1, 10, 0),
			new TestDbViewHelper.DbColumn("CEI_AutonomyRegionType", VarChar, 2),
			new TestDbViewHelper.DbColumn("CEI_CustomsPackType", VarChar, 2)
		);

		public void TestViewIndexes() => TestDbViewHelper.AssertViewIndexes(
			Db.Connection,
			"ILCusEntryInstruction_Idx"
		);
	}
}
