using Enterprise.Build.Database.Script.Testing.Public.Core.ModelViews;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.KR.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.KR.ModelViews.KRCusEntryInstruction))]
	sealed class KRCusEntryInstructionTest : BaseModelViewScriptTest
	{
		#region Overrides of ModelViewScriptTestBase

		protected override string ViewName => "KRCusEntryInstruction";

		protected override string UnderlyingTableName => "CusEntryInstruction";

		protected override TestDbViewHelper.DbColumn[] ExpectedViewColumns => new[]
		{
			new TestDbViewHelper.DbColumn("CEI_PK", UniqueIdentifier, -1),
			new TestDbViewHelper.DbColumn("CEI_ClusterKey", Int, -1, 10, 0),
			new TestDbViewHelper.DbColumn("CEI_AgreedDutyRate", Decimal, -1, 12, 2),
			new TestDbViewHelper.DbColumn("CEI_AgreedDutyRatePreferenceCode", VarChar, 6),
			new TestDbViewHelper.DbColumn("CEI_AgreedRateApp", VarChar, 1),
			new TestDbViewHelper.DbColumn("CEI_ApplyDutyPenaltyReduction", VarChar, 1),
			new TestDbViewHelper.DbColumn("CEI_BondedFactoryArrivalDate", DateTime, -1),
			new TestDbViewHelper.DbColumn("CEI_BondedFactoryUseCode", VarChar, 1),
			new TestDbViewHelper.DbColumn("CEI_DutyPenaltyCause", VarChar, 2),
			new TestDbViewHelper.DbColumn("CEI_FTARelationArticleCode", VarChar, 1),
			new TestDbViewHelper.DbColumn("CEI_PackQty", Int, -1, 10, 0),
			new TestDbViewHelper.DbColumn("CEI_RefundCauseCode", VarChar, 2),
			new TestDbViewHelper.DbColumn("CEI_RefundReasonCode", VarChar, 3),
			new TestDbViewHelper.DbColumn("CEI_RefundType", VarChar, 1),
			new TestDbViewHelper.DbColumn("CEI_StatementNumber5WN", VarChar, 15),
			new TestDbViewHelper.DbColumn("CEI_TaxPenaltyCause", VarChar, 2),
		};

		protected override bool HasIndexes => false;

		#endregion
	}
}
