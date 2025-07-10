using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Core
{
	public class GlbWorkTimeSubscriber : DataScienceSubscriberToKafkaBase<KafkaRegistryConfigs.Core>
	{
		public override string Code => SubscriberCodes.GlbWorkTimeSubscriberCode;
		public override int DataSchemaVersion => 1; // Bump this if the schema changes
		public override ITableSchema Table { get; } = GlbWorkTimeSchema.Instance;
		public override IEnumerable<SchemaColumn> BizObjColumns => new SchemaColumn[]
		{
			GlbWorkTimeSchema.PK,
			GlbWorkTimeSchema.GW_IsValid,
			GlbWorkTimeSchema.GW_DayOfWeek,
			GlbWorkTimeSchema.GW_StartTime,
			GlbWorkTimeSchema.GW_EndTime,
			GlbWorkTimeSchema.GW_ParentTableCode,
			GlbWorkTimeSchema.GW_ParentID,
			GlbWorkTimeSchema.GW_SystemCreateTimeUtc,
			GlbWorkTimeSchema.GW_SystemCreateUser,
			GlbWorkTimeSchema.GW_SystemLastEditTimeUtc,
			GlbWorkTimeSchema.GW_SystemLastEditUser,
		};
	}
}
