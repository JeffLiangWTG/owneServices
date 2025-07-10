using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Productivity
{
	public class GlbGroupLinkSubscriber : DataScienceSubscriberToKafkaBase<KafkaRegistryConfigs.Productivity>
	{
		public override string Code => SubscriberCodes.GlbGroupLinkSubscriberCode;
		public override int DataSchemaVersion => 1; // Bump this if the schema changes
		public override ITableSchema Table { get; } = GlbGroupLinkSchema.Instance;
		public override IEnumerable<SchemaColumn> BizObjColumns => new SchemaColumn[]
		{
			GlbGroupLinkSchema.PK,
			GlbGroupLinkSchema.GK_CapacityLimitPercent,
			GlbGroupLinkSchema.GK_GG,
			GlbGroupLinkSchema.GK_GS,
			GlbGroupLinkSchema.GK_IsValid,
			GlbGroupLinkSchema.GK_MembershipType,
			GlbGroupLinkSchema.GK_SkillLevel,
			GlbGroupLinkSchema.GK_SystemCreateTimeUtc,
			GlbGroupLinkSchema.GK_SystemCreateUser,
			GlbGroupLinkSchema.GK_SystemLastEditTimeUtc,
			GlbGroupLinkSchema.GK_SystemLastEditUser,
		};
	}
}
