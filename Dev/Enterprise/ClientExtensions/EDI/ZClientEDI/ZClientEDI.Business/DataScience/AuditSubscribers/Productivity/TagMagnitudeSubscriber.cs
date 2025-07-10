using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Productivity
{
	public class TagMagnitudeSubscriber : DataScienceSubscriberToKafkaBase<KafkaRegistryConfigs.Productivity>
	{
		public override string Code => SubscriberCodes.TagMagnitudeSubscriberCode;
		public override int DataSchemaVersion => 1; // Bump this if the schema changes
		public override ITableSchema Table { get; } = TagMagnitudeSchema.Instance;
		public override IEnumerable<SchemaColumn> BizObjColumns => new SchemaColumn[]
		{
			TagMagnitudeSchema.PK,
			TagMagnitudeSchema.TGM_Code,
			TagMagnitudeSchema.TGM_Description,
			TagMagnitudeSchema.TGM_GG_OwnerGroup,
			TagMagnitudeSchema.TGM_IsActive,
			TagMagnitudeSchema.TGM_NudgeAmount,
			TagMagnitudeSchema.TGM_RuleRunSequence,
			TagMagnitudeSchema.TGM_SystemCreateTimeUtc,
			TagMagnitudeSchema.TGM_SystemCreateUser,
			TagMagnitudeSchema.TGM_SystemLastEditTimeUtc,
			TagMagnitudeSchema.TGM_SystemLastEditUser,
			TagMagnitudeSchema.TGM_TGD_Tag,
		};
	}
}
