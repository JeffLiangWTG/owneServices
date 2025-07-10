using Enterprise.Build.Database.Script.Testing.Public.Core.ModelViews;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.PL.ModelViews;

[TestedType(type: typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.PL.ModelViews.PLJobComInvoiceLine))]
sealed class PLJobComInvoiceLineTest : BaseModelViewScriptTest
{
	protected override string ViewName => "PLJobComInvoiceLine";

	protected override string UnderlyingTableName => "JobComInvoiceLine";

	protected override TestDbViewHelper.DbColumn[] ExpectedViewColumns => [
		new("JI_PK", UniqueIdentifier, characterMaximumLength: -1),
		new("JI_ClusterKey", Int, characterMaximumLength: -1, precision: 10, scale: 0),
		new("JI_TargetEntryLineNumber", SmallInt, characterMaximumLength: -1, precision: 5, scale: 0),
		new("JI_AIEMType", VarChar, characterMaximumLength: 6),
		new("JI_CommercialPaymentAmount", Decimal, characterMaximumLength: -1, precision: 12, scale: 3),
		new("JI_CommercialPaymentCode", VarChar, characterMaximumLength: 3),
		new("JI_CommercialPaymentDate", DateTime, characterMaximumLength: -1),
		new("JI_CommercialPaymentNumber", VarChar, characterMaximumLength: 20),
		new("JI_CommercialReference", VarChar, characterMaximumLength: 70),
		new("JI_CountryOfDestination", VarChar, characterMaximumLength: 2),
		new("JI_CountryOfDispatch", VarChar, characterMaximumLength: 2),
		new("JI_CountryOfSupply", VarChar, characterMaximumLength: 4),
		new("JI_CusNumber", VarChar, characterMaximumLength: 9),
		new("JI_EconomicConditions", VarChar, characterMaximumLength: 2),
		new("JI_EntryExitPurposeCode", VarChar, characterMaximumLength: 3),
		new("JI_EntryExitPurposeDetail", VarChar, characterMaximumLength: 100),
		new("JI_ExcessStock", Bit, characterMaximumLength: -1),
		new("JI_ExciseCode", VarChar, characterMaximumLength: 3),
		new("JI_ExciseExemption", VarChar, characterMaximumLength: 1),
		new("JI_ExportUnionAdditionalTariffCode", VarChar, characterMaximumLength: 15),
		new("JI_ExportUnionDeferredInstallment", VarChar, characterMaximumLength: 250),
		new("JI_ExportUnionEcological", Bit, characterMaximumLength: -1),
		new("JI_ExportUnionPackCode", VarChar, characterMaximumLength: 5),
		new("JI_ExportUnionProductionYear", SmallInt, characterMaximumLength: -1, precision: 5, scale: 0),
		new("JI_ExportUnionThreadCode", VarChar, characterMaximumLength: 5),
		new("JI_FecChallengeDST", Bit, characterMaximumLength: -1),
		new("JI_FecDST", Bit, characterMaximumLength: -1),
		new("JI_GlobalWarmingPotential", Decimal, characterMaximumLength: -1, precision: 12, scale: 2),
		new("JI_GoodsCategory", VarChar, characterMaximumLength: 1),
		new("JI_HadErrorInLastResponse", Bit, characterMaximumLength: -1),
		new("JI_HasNonRecycledPlastics", Bit, characterMaximumLength: -1),
		new("JI_IdentificationMeansType", VarChar, characterMaximumLength: 1),
		new("JI_InwardProcessingLicenseLineNumber", VarChar, characterMaximumLength: 14),
		new("JI_IsMainPack", Bit, characterMaximumLength: -1),
		new("JI_IsREADirectConsumption", Bit, characterMaximumLength: -1),
		new("JI_MethodOfPayment", VarChar, characterMaximumLength: 1),
		new("JI_MethodOfPayment2", VarChar, characterMaximumLength: 1),
		new("JI_Meursing", VarChar, characterMaximumLength: 4),
		new("JI_PortTaxRate", VarChar, characterMaximumLength: 2),
		new("JI_PriceType", VarChar, characterMaximumLength: 2),
		new("JI_PrincipalsRepresentativeName", VarChar, characterMaximumLength: 50),
		new("JI_ProcessingDescription", VarChar, characterMaximumLength: 300),
		new("JI_QuotaQty", Int, characterMaximumLength: -1, precision: 10, scale: 0),
		new("JI_QuotaUQ", VarChar, characterMaximumLength: 4),
		new("JI_REAProductCode", VarChar, characterMaximumLength: 4),
		new("JI_RegionOfDestination", VarChar, characterMaximumLength: 10),
		new("JI_RelatedIndicator2", VarChar, characterMaximumLength: 1),
		new("JI_RelatedIndicator3", VarChar, characterMaximumLength: 1),
		new("JI_RelatedIndicator4", VarChar, characterMaximumLength: 1),
		new("JI_ReturningGoodsReasonCode", VarChar, characterMaximumLength: 3),
		new("JI_ReturningGoodsReasonDetail", VarChar, characterMaximumLength: 100),
		new("JI_ReturnToOrigin", Bit, characterMaximumLength: -1),
		new("JI_RL_NKPrincipalsRepresentativeCity", VarChar, characterMaximumLength: 5),
		new("JI_RW_NKBorderTradeStateCode", VarChar, characterMaximumLength: 2),
		new("JI_SecondaryTreatedProduct", Bit, characterMaximumLength: -1),
		new("JI_SecondQuota", VarChar, characterMaximumLength: 15),
		new("JI_StatisticalValue", Decimal, characterMaximumLength: -1, precision: 13, scale: 2),
		new("JI_StatisticalValueManualOverride", Bit, characterMaximumLength: -1),
		new("JI_SteelType", VarChar, characterMaximumLength: 1),
		new("JI_T2LItemNumber", Int, characterMaximumLength: -1, precision: 10, scale: 0),
		new("JI_TotalRetailPrice", Decimal, characterMaximumLength: -1, precision: 15, scale: 2),
		new("JI_TransNature", VarChar, characterMaximumLength: 2),
		new("JI_UCRReference", VarChar, characterMaximumLength: 35),
		new("JI_UsedGoodsCode", VarChar, characterMaximumLength: 3),
		new("JI_ValueAdjustmentCode", VarChar, characterMaximumLength: 1),
	];

	protected override bool HasIndexes => false;
}
