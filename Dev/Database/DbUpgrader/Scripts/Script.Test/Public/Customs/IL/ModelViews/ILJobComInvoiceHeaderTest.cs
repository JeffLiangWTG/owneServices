using CargoWise.Data;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.IL.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.IL.ModelViews.ILJobComInvoiceHeader))]
	sealed class ILJobComInvoiceHeaderTest : DbCreateScriptTest
	{
		public void TestViewColumns() => TestDbViewHelper.AssertViewColumns(
			Db.Connection,
			"ILJobComInvoiceHeader",
			"JobComInvoiceHeader",
			new[]
			{
				new TestDbViewHelper.DbColumn("JZ_PK", UniqueIdentifier, -1),
				new TestDbViewHelper.DbColumn("JZ_ClusterKey", Int, -1, 10, 0),
				new TestDbViewHelper.DbColumn("JZ_InvoiceType", VarChar, 3),
				new TestDbViewHelper.DbColumn("JZ_PreferenceDocumentType", VarChar, 10)
			}
		);

		public void TestViewIndexes() => TestDbViewHelper.AssertViewIndexes(
			Db.Connection,
			"ILJobComInvoiceHeader_Idx"
		);
	}
}
