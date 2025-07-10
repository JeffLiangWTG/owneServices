using System.Data;
using Enterprise.Build.Database.Script.Testing.Public.Core.ModelViews;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.KR.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.KR.ModelViews.KRJobComInvoiceLine))]
	sealed class KRJobComInvoiceLineTest : BaseModelViewScriptTest
	{
		protected override string ViewName => "KRJobComInvoiceLine";

		protected override string UnderlyingTableName => "JobComInvoiceLine";

		protected override TestDbViewHelper.DbColumn[] ExpectedViewColumns => new[]
		{
			new TestDbViewHelper.DbColumn("JI_PK", SqlDbType.UniqueIdentifier, -1),
			new TestDbViewHelper.DbColumn("JI_ClusterKey", SqlDbType.Int, -1, 10, 0),
			new TestDbViewHelper.DbColumn("JI_AdditionalDutyRate", SqlDbType.Decimal, -1, 12, 2),
			new TestDbViewHelper.DbColumn("JI_AdditionalDutyType", SqlDbType.VarChar, 1),
			new TestDbViewHelper.DbColumn("JI_BrandCode", SqlDbType.VarChar, 4),
			new TestDbViewHelper.DbColumn("JI_COOExemptionReason", SqlDbType.VarChar, 2),
			new TestDbViewHelper.DbColumn("JI_COOLabelLocation", SqlDbType.VarChar, 1),
			new TestDbViewHelper.DbColumn("JI_COOLabelType", SqlDbType.VarChar, 1),
			new TestDbViewHelper.DbColumn("JI_COOSupportingDocType", SqlDbType.VarChar, 1),
			new TestDbViewHelper.DbColumn("JI_CourierCargoSelectivityIndicator", SqlDbType.VarChar, 1),
			new TestDbViewHelper.DbColumn("JI_CoveredByCOOExporter", SqlDbType.Bit, -1),
			new TestDbViewHelper.DbColumn("JI_DomesticTaxCode", SqlDbType.VarChar, 8),
			new TestDbViewHelper.DbColumn("JI_DomesticTaxExemptionCode", SqlDbType.VarChar, 7),
			new TestDbViewHelper.DbColumn("JI_DrawbackQuantity", SqlDbType.Decimal, -1, 14, 4),
			new TestDbViewHelper.DbColumn("JI_DrawbackUQ", SqlDbType.VarChar, 3),
			new TestDbViewHelper.DbColumn("JI_DutyRateSelection", SqlDbType.VarChar, 3),
			new TestDbViewHelper.DbColumn("JI_DutyReductionRateRegulationCode", SqlDbType.VarChar, 9),
			new TestDbViewHelper.DbColumn("JI_InboundDate", SqlDbType.DateTime, -1),
			new TestDbViewHelper.DbColumn("JI_Ingredient", SqlDbType.NVarChar, 70),
			new TestDbViewHelper.DbColumn("JI_InstallationCost", SqlDbType.Decimal, -1, 18, 0),
			new TestDbViewHelper.DbColumn("JI_InstallmentCode", SqlDbType.VarChar, 12),
			new TestDbViewHelper.DbColumn("JI_IsSpecificUseCode", SqlDbType.Bit, -1),
			new TestDbViewHelper.DbColumn("JI_JurisdictionalCusOffice", SqlDbType.VarChar, 3),
			new TestDbViewHelper.DbColumn("JI_LotNumber", SqlDbType.NVarChar, 70),
			new TestDbViewHelper.DbColumn("JI_MightRequireInspection", SqlDbType.VarChar, 1),
			new TestDbViewHelper.DbColumn("JI_NoOfPacks", SqlDbType.Int, -1, 10, 0),
			new TestDbViewHelper.DbColumn("JI_OriginalStateDocType", SqlDbType.VarChar, 2),
			new TestDbViewHelper.DbColumn("JI_PackType", SqlDbType.VarChar, 2),
			new TestDbViewHelper.DbColumn("JI_PCProcedure", SqlDbType.VarChar, 1),
			new TestDbViewHelper.DbColumn("JI_PostClearanceProcedureGA1", SqlDbType.VarChar, 3),
			new TestDbViewHelper.DbColumn("JI_PostClearanceProcedureGA2", SqlDbType.VarChar, 3),
			new TestDbViewHelper.DbColumn("JI_PostClearanceProcedureGA3", SqlDbType.VarChar, 3),
			new TestDbViewHelper.DbColumn("JI_ProductTypeCode", SqlDbType.VarChar, 2),
			new TestDbViewHelper.DbColumn("JI_RN_NKReExportDestinationCountry", SqlDbType.VarChar, 2),
			new TestDbViewHelper.DbColumn("JI_RN_NKSecondCommercialInvoiceCountry", SqlDbType.VarChar, 2),
			new TestDbViewHelper.DbColumn("JI_ScheduledReExportCustomsOffice", SqlDbType.VarChar, 3),
			new TestDbViewHelper.DbColumn("JI_ScheduledReExportDate", SqlDbType.DateTime, -1),
			new TestDbViewHelper.DbColumn("JI_SequenceNumber", SqlDbType.SmallInt, -1, 5, 0),
			new TestDbViewHelper.DbColumn("JI_SkipManifestReport", SqlDbType.VarChar, 1),
			new TestDbViewHelper.DbColumn("JI_SpecificUseCodeDescription", SqlDbType.NVarChar, 30),
			new TestDbViewHelper.DbColumn("JI_SpecificUseCodeDutyRatePermitNo", SqlDbType.VarChar, 25),
			new TestDbViewHelper.DbColumn("JI_SpecificUseProductType", SqlDbType.VarChar, 2),
			new TestDbViewHelper.DbColumn("JI_UseCode", SqlDbType.VarChar, 1),
			new TestDbViewHelper.DbColumn("JI_VATReductionCode", SqlDbType.VarChar, 7),
		};

		protected override bool HasIndexes => false;
	}
}
