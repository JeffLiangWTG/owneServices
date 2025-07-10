using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Billing
{
	public sealed class ClientLicencePriceHeaderSubscriber : DataScienceSubscriberToKafkaBase<KafkaRegistryConfigs.Billing>
	{
		public override string Code => SubscriberCodes.ClientLicencePriceHeaderSubscriberCode;
		public override int DataSchemaVersion => 1; // Bump this if the schema changes
		public override ITableSchema Table => ClientLicencePriceHeaderSchema.Instance;
		public override IEnumerable<SchemaColumn> BizObjColumns { get; } = new SchemaColumn[]
		{
			ClientLicencePriceHeaderSchema.PK,
			ClientLicencePriceHeaderSchema.L6_LC,
			ClientLicencePriceHeaderSchema.L6_RX_NKCurrency,
			ClientLicencePriceHeaderSchema.L6_ValidFrom,
			ClientLicencePriceHeaderSchema.L6_ValidTo,
			ClientLicencePriceHeaderSchema.L6_SystemCreateTimeUtc,
			ClientLicencePriceHeaderSchema.L6_SystemCreateUser,
			ClientLicencePriceHeaderSchema.L6_LicenceEdition,
			ClientLicencePriceHeaderSchema.L6_SystemCode,
			ClientLicencePriceHeaderSchema.L6_RN_NKCountry,
			ClientLicencePriceHeaderSchema.L6_IsStandard,
			ClientLicencePriceHeaderSchema.L6_LicenceUnitRate,
			ClientLicencePriceHeaderSchema.L6_DiscountCode,
			ClientLicencePriceHeaderSchema.L6_UseStandardDiscount,
			ClientLicencePriceHeaderSchema.L6_LiveMonthsUntilTestDbBilling,
			ClientLicencePriceHeaderSchema.L6_TestDbPriceCode,
			ClientLicencePriceHeaderSchema.L6_HasExchangeRates,
			ClientLicencePriceHeaderSchema.L6_Rounding,
			ClientLicencePriceHeaderSchema.L6_PricelistVersion,
		};
	}
}
