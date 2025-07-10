using System.Data;
using Enterprise.Build.Database.Script.Testing.Public.Core.ModelViews;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.KR.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.KR.ModelViews.KRJobComInvoiceHeader))]
	sealed class KRJobComInvoiceHeaderTest : BaseModelViewScriptTest
	{
		#region Overrides of ModelViewScriptTestBase

		protected override string ViewName => "KRJobComInvoiceHeader";

		protected override string UnderlyingTableName => "JobComInvoiceHeader";

		protected override TestDbViewHelper.DbColumn[] ExpectedViewColumns => new[]
		{
			new TestDbViewHelper.DbColumn("JZ_PK", SqlDbType.UniqueIdentifier, -1),
			new TestDbViewHelper.DbColumn("JZ_ClusterKey", SqlDbType.Int, -1, 10, 0),
			new TestDbViewHelper.DbColumn("JZ_BlanketValuationDeclarationNumber", SqlDbType.VarChar, 12),
			new TestDbViewHelper.DbColumn("JZ_COOExemptionReason", SqlDbType.VarChar, 2),
			new TestDbViewHelper.DbColumn("JZ_COOLabelLocation", SqlDbType.VarChar, 1),
			new TestDbViewHelper.DbColumn("JZ_COOLabelType", SqlDbType.VarChar, 1),
			new TestDbViewHelper.DbColumn("JZ_COOStatus", SqlDbType.VarChar, 1),
			new TestDbViewHelper.DbColumn("JZ_DeductionRate", SqlDbType.Decimal, -1, 5, 2),
			new TestDbViewHelper.DbColumn("JZ_DeductionType", SqlDbType.VarChar, 1),
			new TestDbViewHelper.DbColumn("JZ_DRWApplicantType", SqlDbType.VarChar, 1),
			new TestDbViewHelper.DbColumn("JZ_DutyReductionRateRegulationCode", SqlDbType.VarChar, 9),
			new TestDbViewHelper.DbColumn("JZ_EstimatedDateOfFinalPrice", SqlDbType.DateTime, -1),
			new TestDbViewHelper.DbColumn("JZ_ImpContractExpiryDate", SqlDbType.DateTime, -1),
			new TestDbViewHelper.DbColumn("JZ_ImportCargoManagementNumber", SqlDbType.VarChar, 19),
			new TestDbViewHelper.DbColumn("JZ_InboundDate", SqlDbType.DateTime, -1),
			new TestDbViewHelper.DbColumn("JZ_JurisdictionalCusOffice", SqlDbType.VarChar, 3),
			new TestDbViewHelper.DbColumn("JZ_OnlineTradeType", SqlDbType.VarChar, 1),
			new TestDbViewHelper.DbColumn("JZ_ProvAdditionalAmount", SqlDbType.Decimal, -1,12,0),
			new TestDbViewHelper.DbColumn("JZ_ProvAdditionalRate", SqlDbType.Decimal, -1, 7 , 2),
			new TestDbViewHelper.DbColumn("JZ_ProvPricingYN", SqlDbType.VarChar, 1),
			new TestDbViewHelper.DbColumn("JZ_RN_NKReExportDestinationCountry", SqlDbType.VarChar, 2),
			new TestDbViewHelper.DbColumn("JZ_ScheduledReExportCustomsOffice", SqlDbType.VarChar, 3),
			new TestDbViewHelper.DbColumn("JZ_ScheduledReExportDate", SqlDbType.DateTime, -1),
			new TestDbViewHelper.DbColumn("JZ_SpecificUseCodeDescription", SqlDbType.VarChar, 30),
			new TestDbViewHelper.DbColumn("JZ_SpecificUseProductType", SqlDbType.VarChar, 2),
			new TestDbViewHelper.DbColumn("JZ_ValuationDecAttachCode", SqlDbType.VarChar, 1),
		};

		protected override bool HasIndexes => false;

		#endregion
	}
}
