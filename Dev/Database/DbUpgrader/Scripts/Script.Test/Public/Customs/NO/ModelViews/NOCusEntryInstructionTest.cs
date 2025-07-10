using CargoWise.Data;
using Enterprise.Build.Database.Script.Testing;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Public.Customs.NO.Testing
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.NO.ModelViews.NOCusEntryInstruction))]
	sealed class NOCusEntryInstructionTest : DbCreateScriptTest
	{
		public void TestViewColumns() => TestDbViewHelper.AssertViewColumns(
			Db.Connection,
			"NOCusEntryInstruction",
			"CusEntryInstruction",
			new TestDbViewHelper.DbColumn("CEI_PK", UniqueIdentifier, -1),
			new TestDbViewHelper.DbColumn("CEI_ClusterKey", Int, -1, 10, 0),
			new TestDbViewHelper.DbColumn("CEI_PackageCount", Decimal, -1, 18, 0)
		);

		public void TestViewIndexes() => TestDbViewHelper.AssertViewIndexes(
			Db.Connection,
			"NOCusEntryInstruction_Idx"
		);
	}
}
