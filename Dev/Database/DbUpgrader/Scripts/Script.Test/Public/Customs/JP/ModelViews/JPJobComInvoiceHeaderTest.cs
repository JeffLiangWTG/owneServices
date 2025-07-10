using CargoWise.Data;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.JP.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.JP.ModelViews.JPJobComInvoiceHeader))]
	sealed class JPJobComInvoiceHeaderTest : DbCreateScriptTest
	{
		public void TestViewColumns()
		{
			TestDbViewHelper.AssertViewColumns(
				Db.Connection,
				"JPJobComInvoiceHeader",
				"JobComInvoiceHeader",
				new[]
				{
					new TestDbViewHelper.DbColumn("JZ_PK", UniqueIdentifier, -1),
					new TestDbViewHelper.DbColumn("JZ_ClusterKey", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("JZ_AdvanceRulingOnValuation1", VarChar, 7),
					new TestDbViewHelper.DbColumn("JZ_AdvanceRulingOnValuation2", VarChar, 7),
					new TestDbViewHelper.DbColumn("JZ_ComprehensiveInsuranceNumber", VarChar, 8),
					new TestDbViewHelper.DbColumn("JZ_ElectronicInvoiceReceiptNumber", VarChar, 10),
					new TestDbViewHelper.DbColumn("JZ_FreightType", VarChar, 1),
					new TestDbViewHelper.DbColumn("JZ_InsuranceType", VarChar, 1),
					new TestDbViewHelper.DbColumn("JZ_InvoiceType", VarChar, 1),
					new TestDbViewHelper.DbColumn("JZ_InvoiceAmountType", VarChar, 1)
				}
			);
		}
	}
}
