using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Productivity
{
	public class ProcessHeaderLinkSubscriber : DataScienceSubscriberToKafkaBase<KafkaRegistryConfigs.Productivity>
	{
		public override string Code => SubscriberCodes.ProcessHeaderLinkSubscriberCode;
		public override int DataSchemaVersion => 1; // Bump this if the schema changes
		public override ITableSchema Table { get; } = ProcessHeaderLinkSchema.Instance;
		public override IEnumerable<SchemaColumn> BizObjColumns => new SchemaColumn[]
		{
			ProcessHeaderLinkSchema.PK,
			ProcessHeaderLinkSchema.FP_FH_HeaderFrom,
			ProcessHeaderLinkSchema.FP_FH_HeaderTo,
			ProcessHeaderLinkSchema.FP_IsActive,
			ProcessHeaderLinkSchema.FP_LinkType,
			ProcessHeaderLinkSchema.FP_SynchroniseBufferPenetration,
			ProcessHeaderLinkSchema.FP_TimeDelayFactor,
			ProcessHeaderLinkSchema.FP_TimeDelayMinutes,
			ProcessHeaderLinkSchema.FP_SystemCreateTimeUtc,
			ProcessHeaderLinkSchema.FP_SystemCreateUser,
			ProcessHeaderLinkSchema.FP_SystemLastEditTimeUtc,
			ProcessHeaderLinkSchema.FP_SystemLastEditUser,
			ProcessHeaderLinkSchema.FP_IsValid,
		};
	}
}
