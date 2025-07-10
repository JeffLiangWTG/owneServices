using CargoWise.Data;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.JP.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.JP.ModelViews.JPJobComInvoiceLine))]
	sealed class JPJobComInvoiceLineTest : DbCreateScriptTest
	{
		public void TestViewColumns()
		{
			TestDbViewHelper.AssertViewColumns(
				Db.Connection,
				"JPJobComInvoiceLine",
				"JobComInvoiceLine",
				new[]
				{
					new TestDbViewHelper.DbColumn("JI_PK", UniqueIdentifier, -1),
					new TestDbViewHelper.DbColumn("JI_ClusterKey", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("JI_AdvanceRulingOnClassification", VarChar, 9),
					new TestDbViewHelper.DbColumn("JI_AdvanceRulingOnOrigin", VarChar, 7),
					new TestDbViewHelper.DbColumn("JI_BondedDate", DateTime, -1 ),
					new TestDbViewHelper.DbColumn("JI_DomesticConsumptionTaxExemptionCode", VarChar, 1),
					new TestDbViewHelper.DbColumn("JI_DomesticConsumptionTaxExemptionIsPartial", Bit, -1),
					new TestDbViewHelper.DbColumn("JI_DutyReductionAmount", Decimal, -1, 11, 0),
					new TestDbViewHelper.DbColumn("JI_DutyReductionExemptionRefundCode", VarChar, 5),
					new TestDbViewHelper.DbColumn("JI_FEFTAArticle48", VarChar, 1),
					new TestDbViewHelper.DbColumn("JI_NACCSCode", VarChar, 1),
					new TestDbViewHelper.DbColumn("JI_StorageType", VarChar, 1),
					new TestDbViewHelper.DbColumn("JI_TradeControlOrderAppendix", VarChar, 5)
				}
			);
		}
	}
}
