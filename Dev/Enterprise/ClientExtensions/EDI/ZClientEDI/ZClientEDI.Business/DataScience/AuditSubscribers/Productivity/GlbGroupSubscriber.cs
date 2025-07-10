using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Productivity
{
	public class GlbGroupSubscriber : DataScienceSubscriberToKafkaBase<KafkaRegistryConfigs.Productivity>
	{
		public override string Code => SubscriberCodes.GlbGroupSubscriberCode;
		public override int DataSchemaVersion => 3; // Bump this if the schema changes
		public override ITableSchema Table { get; } = GlbGroupSchema.Instance;
		public override IEnumerable<SchemaColumn> BizObjColumns => new SchemaColumn[]
		{
			GlbGroupSchema.PK,
			GlbGroupSchema.GG_ActiveDirectoryObjectGuid,
			GlbGroupSchema.GG_Category,
			GlbGroupSchema.GG_Code,
			GlbGroupSchema.GG_Desc,
			GlbGroupSchema.GG_DomainName,
			GlbGroupSchema.GG_ExternalId,
			GlbGroupSchema.GG_GC,
			GlbGroupSchema.GG_GG_ParentGroup,
			GlbGroupSchema.GG_IsActive,
			GlbGroupSchema.GG_IsSales,
			GlbGroupSchema.GG_IsSecurityEnabled,
			GlbGroupSchema.GG_IsSystemDefined,
			GlbGroupSchema.GG_IsValid,
			GlbGroupSchema.GG_SystemCreateBranch,
			GlbGroupSchema.GG_SystemCreateDepartment,
			GlbGroupSchema.GG_SystemCreateTimeUtc,
			GlbGroupSchema.GG_SystemCreateUser,
			GlbGroupSchema.GG_SystemLastEditTimeUtc,
			GlbGroupSchema.GG_SystemLastEditUser,
			GlbGroupSchema.GG_Type,
		};
	}
}
