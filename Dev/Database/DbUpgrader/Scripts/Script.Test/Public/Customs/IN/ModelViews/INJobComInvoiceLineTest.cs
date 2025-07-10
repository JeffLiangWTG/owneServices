using Enterprise.Build.Database.Script.Testing.Public.Core.ModelViews;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.IN.ModelViews;

[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.IN.ModelViews.INJobComInvoiceLine))]
sealed class INJobComInvoiceLineTest : BaseModelViewScriptTest
{
	protected override string ViewName => "INJobComInvoiceLine";

	protected override string UnderlyingTableName => "JobComInvoiceLine";

	protected override TestDbViewHelper.DbColumn[] ExpectedViewColumns => new[]
	{
		new TestDbViewHelper.DbColumn("JI_PK", UniqueIdentifier, -1),
		new TestDbViewHelper.DbColumn("JI_ClusterKey", Int, -1, 10, 0),
		new TestDbViewHelper.DbColumn("JI_AccessoryStatus", VarChar, 1),
		new TestDbViewHelper.DbColumn("JI_DrawbackSerialNo", VarChar, 15),
		new TestDbViewHelper.DbColumn("JI_EndUse", VarChar, 20),
		new TestDbViewHelper.DbColumn("JI_GSTPayNotApplicable", Bit, -1),
		new TestDbViewHelper.DbColumn("JI_JobWorkNotificationNo", VarChar, 10),
		new TestDbViewHelper.DbColumn("JI_MPG_Code", VarChar, 17),
		new TestDbViewHelper.DbColumn("JI_MPG_CodeType", VarChar, 1),
		new TestDbViewHelper.DbColumn("JI_PMV", Decimal, -1, 15, 2),
		new TestDbViewHelper.DbColumn("JI_RewardItem", VarChar, 1),
		new TestDbViewHelper.DbColumn("JI_RN_NKCountryOfTransit", VarChar, 2),
		new TestDbViewHelper.DbColumn("JI_TotalPMV", Decimal, -1, 15, 2),
		new TestDbViewHelper.DbColumn("JI_UnitPrice", Decimal, -1, 16, 5),
		new TestDbViewHelper.DbColumn("JI_UnitQuantity", Int, -1, 10, 0),
		new TestDbViewHelper.DbColumn("JI_UnitUQ", VarChar, 3),
	};

	protected override bool HasIndexes => false;
}
