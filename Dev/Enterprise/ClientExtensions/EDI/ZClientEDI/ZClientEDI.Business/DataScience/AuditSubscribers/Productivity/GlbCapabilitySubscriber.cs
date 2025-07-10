using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Productivity
{
	public class GlbCapabilitySubscriber : DataScienceSubscriberToKafkaBase<KafkaRegistryConfigs.Productivity>
	{
		public override string Code => SubscriberCodes.GlbCapabilitySubscriberCode;
		public override int DataSchemaVersion => 1; // Bump this if the schema changes
		public override ITableSchema Table { get; } = GlbCapabilitySchema.Instance;
		public override IEnumerable<SchemaColumn> BizObjColumns => new SchemaColumn[]
		{
			GlbCapabilitySchema.PK,
			GlbCapabilitySchema.G4_AllowTaskAutoAssignment,
			GlbCapabilitySchema.G4_AutoAssignTasksAge,
			GlbCapabilitySchema.G4_CapacityScope,
			GlbCapabilitySchema.G4_Code,
			GlbCapabilitySchema.G4_Description,
			GlbCapabilitySchema.G4_IsActive,
			GlbCapabilitySchema.G4_MembershipRequirements,
			GlbCapabilitySchema.G4_SystemCreateTimeUtc,
			GlbCapabilitySchema.G4_SystemCreateUser,
			GlbCapabilitySchema.G4_SystemLastEditTimeUtc,
			GlbCapabilitySchema.G4_SystemLastEditUser,
		};
	}
}
