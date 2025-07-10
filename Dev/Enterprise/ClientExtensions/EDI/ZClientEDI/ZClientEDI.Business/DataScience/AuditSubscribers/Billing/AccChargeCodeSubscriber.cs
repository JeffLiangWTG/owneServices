using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Billing
{
	public sealed class AccChargeCodeSubscriber : DataScienceSubscriberToKafkaBase<KafkaRegistryConfigs.Billing>
	{
		public override string Code => SubscriberCodes.AccChargeCode;
		public override int DataSchemaVersion => 1; // Bump this if the schema changes
		public override ITableSchema Table => AccChargeCodeSchema.Instance;
		public override IEnumerable<SchemaColumn> BizObjColumns { get; } = new SchemaColumn[]
		{
			AccChargeCodeSchema.PK,
			AccChargeCodeSchema.AC_AC_RevenueChargeCode,
			AccChargeCodeSchema.AC_AG_AccrualAccount,
			AccChargeCodeSchema.AC_AG_CostAccount,
			AccChargeCodeSchema.AC_AG_CostClearingAccount,
			AccChargeCodeSchema.AC_AG_DisbursementShortfallAccount,
			AccChargeCodeSchema.AC_AG_DisbursementSurplusAccount,
			AccChargeCodeSchema.AC_AG_RevenueAccount,
			AccChargeCodeSchema.AC_AG_RevenueClearingAccount,
			AccChargeCodeSchema.AC_AG_WIPAccount,
			AccChargeCodeSchema.AC_AllowDescriptionOvertype,
			AccChargeCodeSchema.AC_AR_ExpenseGroup,
			AccChargeCodeSchema.AC_AR_SalesGroup,
			AccChargeCodeSchema.AC_AT_GSTRate,
			AccChargeCodeSchema.AC_AW_WithholdingTaxRate,
			AccChargeCodeSchema.AC_AX_TaxOverrideGroup,
			AccChargeCodeSchema.AC_ChargeGroup,
			AccChargeCodeSchema.AC_ChargeOtherGroups,
			AccChargeCodeSchema.AC_ChargeSubGroup,
			AccChargeCodeSchema.AC_ChargeType,
			AccChargeCodeSchema.AC_Code,
			AccChargeCodeSchema.AC_DefaultCommissionProduct,
			AccChargeCodeSchema.AC_DefaultCommissionService,
			AccChargeCodeSchema.AC_DefaultCommissionSubModule,
			AccChargeCodeSchema.AC_DepartmentFilterList,
			AccChargeCodeSchema.AC_Desc,
			AccChargeCodeSchema.AC_EnergySourceGroup,
			AccChargeCodeSchema.AC_ENettChargeCodeMap,
			AccChargeCodeSchema.AC_GC,
			AccChargeCodeSchema.AC_GoodsServiceType,
			AccChargeCodeSchema.AC_GovtChargeCode,
			AccChargeCodeSchema.AC_IATA_ChargeCodeMap,
			AccChargeCodeSchema.AC_InputGSTVATRecoverable,
			AccChargeCodeSchema.AC_IsActive,
			AccChargeCodeSchema.AC_IsAdhocServiceCharge,
			AccChargeCodeSchema.AC_IsCommissionable,
			AccChargeCodeSchema.AC_IsGroupageCharge,
			AccChargeCodeSchema.AC_LocalLanguageDescription,
			AccChargeCodeSchema.AC_MarginPercentage,
			AccChargeCodeSchema.AC_PrintSequence,
			AccChargeCodeSchema.AC_RateCalculator,
			AccChargeCodeSchema.AC_ShowOnQuotation,
			AccChargeCodeSchema.AC_SuppressOnQuoteIfZero,
			AccChargeCodeSchema.AC_SystemCreateTimeUtc,
			AccChargeCodeSchema.AC_SystemCreateUser,
			AccChargeCodeSchema.AC_SystemLastEditTimeUtc,
			AccChargeCodeSchema.AC_SystemLastEditUser,
		};
	}
}
