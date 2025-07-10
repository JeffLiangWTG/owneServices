using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Productivity
{
	public class GlbResourceCapabilityPivotSubscriber : DataScienceSubscriberToKafkaBase<KafkaRegistryConfigs.Productivity>
	{
		public override string Code => SubscriberCodes.GlbResourceCapabilityPivotSubscriberCode;
		public override int DataSchemaVersion => 1; // Bump this if the schema changes
		public override ITableSchema Table { get; } = GlbResourceCapabilityPivotSchema.Instance;
		public override IEnumerable<SchemaColumn> BizObjColumns => new SchemaColumn[]
		{
			GlbResourceCapabilityPivotSchema.PK,
			GlbResourceCapabilityPivotSchema.G5_DateExperienceGained,
			GlbResourceCapabilityPivotSchema.G5_SkillLevel,
			GlbResourceCapabilityPivotSchema.G5_G4_Capability,
			GlbResourceCapabilityPivotSchema.G5_GS_RatingPerformedBy,
			GlbResourceCapabilityPivotSchema.G5_GS_Resource,
			GlbResourceCapabilityPivotSchema.G5_SystemCreateTimeUtc,
			GlbResourceCapabilityPivotSchema.G5_SystemCreateUser,
			GlbResourceCapabilityPivotSchema.G5_SystemLastEditTimeUtc,
			GlbResourceCapabilityPivotSchema.G5_SystemLastEditUser
		};
	}
}
