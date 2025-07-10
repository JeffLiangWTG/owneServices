using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Billing
{
	public sealed class ClientLicencePriceItemSubscriber : DataScienceSubscriberToKafkaBase<KafkaRegistryConfigs.Billing>
	{
		public override string Code => SubscriberCodes.ClientLicencePriceItemSubscriberCode;
		public override int DataSchemaVersion => 3; // Bump this if the schema changes
		public override ITableSchema Table => ClientLicencePriceItemSchema.Instance;
		public override IEnumerable<SchemaColumn> BizObjColumns { get; } = new SchemaColumn[]
		{
			ClientLicencePriceItemSchema.PK,
			ClientLicencePriceItemSchema.L7_L6,
			ClientLicencePriceItemSchema.L7_Code,
			ClientLicencePriceItemSchema.L7_Order,
			ClientLicencePriceItemSchema.L7_FeeType,
			ClientLicencePriceItemSchema.L7_Price,
			ClientLicencePriceItemSchema.L7_ParentCode,
			ClientLicencePriceItemSchema.L7_WebParentCode,
			ClientLicencePriceItemSchema.L7_LicenceUnits,
			ClientLicencePriceItemSchema.L7_UnitBreak,
			ClientLicencePriceItemSchema.L7_RX_NKCurrency,
			ClientLicencePriceItemSchema.L7_Ref4,
			ClientLicencePriceItemSchema.L7_Description,
			ClientLicencePriceItemSchema.L7_PGM_DiscountGroupCode,
			ClientLicencePriceItemSchema.L7_ChargeCode,
			ClientLicencePriceItemSchema.L7_DepositChargeCode,
			ClientLicencePriceItemSchema.L7_ChargeBasis,
			ClientLicencePriceItemSchema.L7_UnitBreakParentCode,
			ClientLicencePriceItemSchema.L7_Language,
			ClientLicencePriceItemSchema.L7_ExchangeRateGroupCode,
			ClientLicencePriceItemSchema.L7_IsVolumeAdjustmentEligible,
			ClientLicencePriceItemSchema.L7_Category,
			ClientLicencePriceItemSchema.L7_ParentCategory,
			ClientLicencePriceItemSchema.L7_ProductAvailability,
			ClientLicencePriceItemSchema.L7_ProductDisplayCategory,
			ClientLicencePriceItemSchema.L7_CountryTierCode,
			ClientLicencePriceItemSchema.L7_DisbursementDirection,
			ClientLicencePriceItemSchema.L7_RN_NKDisbursementCountry,
		};
	}
}
