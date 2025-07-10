using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Issues
{
	public sealed class HelpErrorLogKeySubscriber : DataScienceSubscriberToKafkaBase<KafkaRegistryConfigs.Issues>
	{
		public override string Code => SubscriberCodes.HelpErrorLogKeySubscriberCode;
		public override int DataSchemaVersion => 1; // Bump this if the schema changes
		public override ITableSchema Table => HelpErrorLogKeySchema.Instance;
		public override IEnumerable<SchemaColumn> BizObjColumns { get; } = new SchemaColumn[]
		{
			HelpErrorLogKeySchema.PK,
			HelpErrorLogKeySchema.HK_HashCode,
			HelpErrorLogKeySchema.HK_HE,
			HelpErrorLogKeySchema.HK_Key
		};
	}
}
