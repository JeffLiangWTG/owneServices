using CargoWise.Data;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.IL.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.IL.ModelViews.ILJobComInvoiceLine))]
	sealed class ILJobComInvoiceLineTest : DbCreateScriptTest
	{
		public void TestViewColumns()
			=> TestDbViewHelper.AssertViewColumns(
				Db.Connection,
				"ILJobComInvoiceLine",
				"JobComInvoiceLine",
				new[]
				{
					new TestDbViewHelper.DbColumn("JI_PK", UniqueIdentifier, -1),
					new TestDbViewHelper.DbColumn("JI_ClusterKey", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("JI_PreferenceDocNumber", VarChar, 35),
				}
			);

		public void TestViewIndexes() => TestDbViewHelper.AssertViewIndexes(
			Db.Connection,
			"ILJobComInvoiceLine_Idx"
		);
	}
}
