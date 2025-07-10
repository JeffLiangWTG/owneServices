using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Core
{
	public sealed class LicenceHeaderSubscriber : DataScienceSubscriberToKafkaBase<KafkaRegistryConfigs.Core>
	{
		public override string Code => SubscriberCodes.LicenceHeaderSubscriberCode;
		public override int DataSchemaVersion => 1;
		public override ITableSchema Table => LicenceHeaderSchema.Instance;
		public override IEnumerable<SchemaColumn> BizObjColumns { get; } = new SchemaColumn[]
		{
			LicenceHeaderSchema.PK,
			LicenceHeaderSchema.LA_LD,
			LicenceHeaderSchema.LA_LC,
			LicenceHeaderSchema.LA_IsActive,
			LicenceHeaderSchema.LA_EstimatedLiveDate,
			LicenceHeaderSchema.LA_AgreedLiveDate,
		};
	}
}
