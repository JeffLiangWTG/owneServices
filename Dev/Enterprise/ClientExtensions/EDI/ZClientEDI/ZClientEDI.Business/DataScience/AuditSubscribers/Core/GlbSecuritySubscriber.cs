using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Core
{
	public class GlbSecuritySubscriber : DataScienceSubscriberToKafkaBase<KafkaRegistryConfigs.Core>
	{
		public override string Code => SubscriberCodes.GlbSecuritySubscriberCode;
		public override int DataSchemaVersion => 1; // Bump this if the schema changes
		public override ITableSchema Table { get; } = GlbSecuritySchema.Instance;
		public override IEnumerable<SchemaColumn> BizObjColumns => new SchemaColumn[]
		{
			GlbSecuritySchema.PK,
			GlbSecuritySchema.GU_GB,
			GlbSecuritySchema.GU_GC,
			GlbSecuritySchema.GU_GE,
			GlbSecuritySchema.GU_GG,
			GlbSecuritySchema.GU_GS,
			GlbSecuritySchema.GU_IsValid,
			GlbSecuritySchema.GU_ItemGUID,
			GlbSecuritySchema.GU_SecurityItemIsAllowed,
			GlbSecuritySchema.GU_SecurityRight,
			GlbSecuritySchema.GU_SystemCreateTimeUtc,
			GlbSecuritySchema.GU_SystemCreateUser,
			GlbSecuritySchema.GU_SystemLastEditTimeUtc,
			GlbSecuritySchema.GU_SystemLastEditUser
		};
	}
}
