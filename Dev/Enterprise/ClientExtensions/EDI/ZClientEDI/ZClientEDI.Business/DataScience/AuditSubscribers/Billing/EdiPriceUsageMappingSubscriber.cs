using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Billing
{
	public sealed class EdiPriceUsageMappingSubscriber : DataScienceSubscriberToKafkaBase<KafkaRegistryConfigs.Billing>
	{
		public override string Code => SubscriberCodes.EdiPriceUsageMappingSubscriberCode;
		public override int DataSchemaVersion => 1; // Bump this if the schema changes
		public override ITableSchema Table => EdiPriceUsageMappingSchema.Instance;
		public override IEnumerable<SchemaColumn> BizObjColumns { get; } = new SchemaColumn[]
		{
			EdiPriceUsageMappingSchema.PK,
			EdiPriceUsageMappingSchema.PUM_L6,
			EdiPriceUsageMappingSchema.PUM_PriceCode,
			EdiPriceUsageMappingSchema.PUM_UsageCode,
			EdiPriceUsageMappingSchema.PUM_PriceCategory,
			EdiPriceUsageMappingSchema.PUM_UsageCategory,
		};
	}
}
