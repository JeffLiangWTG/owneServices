using CargoWise.Data;
using Enterprise.Build.Database.Script.Testing;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Public.Customs.NO.Testing
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.NO.ModelViews.NOJobComInvoiceHeader))]
	class NOJobComInvoiceHeaderTest : DbCreateScriptTest
	{
		public void TestViewColumns()
		{
			TestDbViewHelper.AssertViewColumns(
				Db.Connection,
				"NOJobComInvoiceHeader",
				"JobComInvoiceHeader",
				new[]
				{
					new TestDbViewHelper.DbColumn("JZ_PK", UniqueIdentifier, -1),
					new TestDbViewHelper.DbColumn("JZ_ClusterKey", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("JZ_ValuationMethod", VarChar, 1)
				}
			);
		}
	}
}
