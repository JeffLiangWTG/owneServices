using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Billing
{
	public sealed class EdiBilledUsageSubscriber : DataScienceSubscriberToKafkaBase<KafkaRegistryConfigs.Billing>
	{
		public override string Code => SubscriberCodes.EdiBilledUsageSubscriberCode;
		public override int DataSchemaVersion => 2; // Bump this if the schema changes
		public override ITableSchema Table => EdiBilledUsageSchema.Instance;
		public override IEnumerable<SchemaColumn> BizObjColumns { get; } = new SchemaColumn[]
		{
			EdiBilledUsageSchema.PK,
			EdiBilledUsageSchema.BU9_LD,
			EdiBilledUsageSchema.BU9_PeriodStart,
			EdiBilledUsageSchema.BU9_UsageCode,
			EdiBilledUsageSchema.BU9_UsageSubCode,
			EdiBilledUsageSchema.BU9_UnitCount,
			EdiBilledUsageSchema.BU9_L7,
			EdiBilledUsageSchema.BU9_PriceCode,
			EdiBilledUsageSchema.BU9_LocalAmountPostDiscount,
			EdiBilledUsageSchema.BU9_LCC,
			EdiBilledUsageSchema.BU9_LC,
			EdiBilledUsageSchema.BU9_AH_Invoice,
			EdiBilledUsageSchema.BU9_BillingModel,
		};
	}
}
