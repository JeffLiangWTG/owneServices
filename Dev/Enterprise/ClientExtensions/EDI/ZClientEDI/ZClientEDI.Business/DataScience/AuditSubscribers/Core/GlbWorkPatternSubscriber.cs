using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Core
{
	public sealed class GlbWorkPatternSubscriber : DataScienceSubscriberToKafkaBase<KafkaRegistryConfigs.Core>
	{
		public override string Code => SubscriberCodes.GlbWorkPatternSubscriberCode;
		public override int DataSchemaVersion => 1; // Bump this if the schema changes
		public override ITableSchema Table { get; } = GlbWorkPatternSchema.Instance;

		public override IEnumerable<SchemaColumn> BizObjColumns => new SchemaColumn[]
		{
			GlbWorkPatternSchema.PK,
			GlbWorkPatternSchema.GWP_AutoEffectiveEndDate,
			GlbWorkPatternSchema.GWP_Comment,
			GlbWorkPatternSchema.GWP_EffectiveDate,
			GlbWorkPatternSchema.GWP_GCR_ChangeRequest,
			GlbWorkPatternSchema.GWP_GS_Staff,
			GlbWorkPatternSchema.GWP_IsApproved,
			GlbWorkPatternSchema.GWP_Name,
			GlbWorkPatternSchema.GWP_StandardDuration,
			GlbWorkPatternSchema.GWP_SystemCreateTimeUtc,
			GlbWorkPatternSchema.GWP_SystemCreateUser,
			GlbWorkPatternSchema.GWP_SystemLastEditTimeUtc,
			GlbWorkPatternSchema.GWP_SystemLastEditUser,
		};
	}
}
