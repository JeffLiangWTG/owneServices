using static System.Data.SqlDbType;
using CargoWise.Data;
using Enterprise.Build.Database.Script.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.NO.Testing
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.NO.ModelViews.NOJobDeclaration))]
	sealed class NOJobDeclarationTest : DbCreateScriptTest
	{
		public void TestViewColumns() => TestDbViewHelper.AssertViewColumns(
			Db.Connection,
			"NOJobDeclaration",
			"JobDeclaration",
			new TestDbViewHelper.DbColumn("JE_PK", UniqueIdentifier, -1),
			new TestDbViewHelper.DbColumn("JE_ClusterKey", Int, -1, 10, 0),
			new TestDbViewHelper.DbColumn("JE_CustomsTransportMode", VarChar, 2)
		);

	public void TestViewIndexes() => TestDbViewHelper.AssertViewIndexes(
		Db.Connection,
		"NOJobDeclaration_Idx"
	);
}
}
