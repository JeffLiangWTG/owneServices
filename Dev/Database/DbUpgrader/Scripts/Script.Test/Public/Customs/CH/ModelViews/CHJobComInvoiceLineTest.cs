using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.CH.ModelViews;

[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.CH.ModelViews.CHJobComInvoiceLine))]
sealed class CHJobComInvoiceLineTest : DbCreateScriptTest
{
	public void TestViewColumns()
	{
		TestDbViewHelper.AssertViewColumns(
			Db.Connection,
			"CHJobComInvoiceLine",
			"JobComInvoiceLine",
			new[]
			{
					new TestDbViewHelper.DbColumn("JI_PK", UniqueIdentifier, -1),
					new TestDbViewHelper.DbColumn("JI_ClusterKey", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("JI_AdditionalUnitConfirmation", Bit, -1),
					new TestDbViewHelper.DbColumn("JI_CusNumber", VarChar, 10),
					new TestDbViewHelper.DbColumn("JI_GoodsReturned", Bit, -1),
					new TestDbViewHelper.DbColumn("JI_GrossMassConfirmation", Bit, -1),
					new TestDbViewHelper.DbColumn("JI_NetMassConfirmation", Bit, -1),
					new TestDbViewHelper.DbColumn("JI_NonCustomsLawObligation", VarChar, 1),
					new TestDbViewHelper.DbColumn("JI_NonTradingGoods", Bit, -1),
					new TestDbViewHelper.DbColumn("JI_OverriddenRate", Decimal, -1, 5, 2),
					new TestDbViewHelper.DbColumn("JI_PermitObligation", VarChar, 1),
					new TestDbViewHelper.DbColumn("JI_RateOverride", Bit, -1),
					new TestDbViewHelper.DbColumn("JI_RefundGoodsItemNumber", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("JI_RefundReason", VarChar, 512),
					new TestDbViewHelper.DbColumn("JI_RefundReferenceNumber", VarChar, 18),
					new TestDbViewHelper.DbColumn("JI_RefundType", VarChar, 1),
					new TestDbViewHelper.DbColumn("JI_StatisticalValueConfirmation", Bit, -1),
					new TestDbViewHelper.DbColumn("JI_StorageType", VarChar, 1),
					new TestDbViewHelper.DbColumn("JI_TareSupplementConfirmation", Bit, -1),
					new TestDbViewHelper.DbColumn("JI_TareSupplementPercentage", Decimal, -1, 4, 1),
					new TestDbViewHelper.DbColumn("JI_VATCodeConfirmation", Bit, -1),
					new TestDbViewHelper.DbColumn("JI_VATValueConfirmation", Bit, -1),
					new TestDbViewHelper.DbColumn("JI_WeightIncludingInnerPackage", Decimal, -1, 9, 3),
					new TestDbViewHelper.DbColumn("JI_WeightIncludingInnerPackageUQ", VarChar, 2),
			}
		);
	}

	public void TestViewIndexes()
	{
		AssertEquals("CHJobComInvoiceLine doesn't require any indexes.", false, DbObjectCreator.ViewExists(Db.Connection, "CHJobComInvoiceLine_Idx"));
	}
}
