using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.IN.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.IN.ModelViews.INJobComInvoiceHeader))]
	class INJobComInvoiceHeaderTest : DbCreateScriptTest
	{
		public void TestViewColumns()
		{
			TestDbViewHelper.AssertViewColumns(
				Db.Connection,
				"INJobComInvoiceHeader",
				"JobComInvoiceHeader",
				new[]
				{
					new TestDbViewHelper.DbColumn("JZ_PK", UniqueIdentifier, -1),
					new TestDbViewHelper.DbColumn("JZ_ClusterKey", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("JZ_AuthorizedEconomicOperatorRole", VarChar, 3),
					new TestDbViewHelper.DbColumn("JZ_ExporterContractNumber", VarChar, 30),
					new TestDbViewHelper.DbColumn("JZ_GSTPaymentStatus", VarChar, 3),
					new TestDbViewHelper.DbColumn("JZ_PaymentDays", Int, -1, 10, 0),
				}
			);
		}

		public void TestViewIndexes()
		{
			AssertEquals("INJobComInvoiceHeader doesn't require any indexes.", false, DbObjectCreator.ViewExists(Db.Connection, "INJobComInvoiceHeader_Idx"));
		}
	}
}
