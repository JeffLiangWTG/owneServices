using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Core
{
	public class GlbBranchSubscriber : DataScienceSubscriberToKafkaBase<KafkaRegistryConfigs.Core>
	{
		public override string Code => SubscriberCodes.GlbBranchSubscriberCode;
		public override int DataSchemaVersion => 1; // Bump this if the schema changes
		public override ITableSchema Table { get; } = GlbBranchSchema.Instance;
		public override IEnumerable<SchemaColumn> BizObjColumns => new SchemaColumn[]
		{
			GlbBranchSchema.PK,
			GlbBranchSchema.GB_BranchName,
			GlbBranchSchema.GB_City,
			GlbBranchSchema.GB_Code,
			GlbBranchSchema.GB_GC,
			GlbBranchSchema.GB_IsActive,
			GlbBranchSchema.GB_IsValid,
			GlbBranchSchema.GB_LocalDocLanguage,
			GlbBranchSchema.GB_OA_AddressProxy,
			GlbBranchSchema.GB_OH_OrgProxy,
			GlbBranchSchema.GB_RL_NKHomePort,
			GlbBranchSchema.GB_RN_NKCountryCode,
			GlbBranchSchema.GB_State,
			GlbBranchSchema.GB_ValidationStatus,
			GlbBranchSchema.GB_SystemCreateTimeUtc,
			GlbBranchSchema.GB_SystemCreateUser,
			GlbBranchSchema.GB_SystemLastEditTimeUtc,
			GlbBranchSchema.GB_SystemLastEditUser,
		};
	}
}
