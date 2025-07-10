using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Core
{
	public class LicenceDatabaseSubscriber : DataScienceSubscriberToKafkaBase<KafkaRegistryConfigs.Core>
	{
		public override string Code => SubscriberCodes.LicenceDatabaseSubscriberCode;
		public override int DataSchemaVersion => 5; // Bump this if the schema changes
		public override ITableSchema Table { get; } = LicenceDatabaseSchema.Instance;
		public override IEnumerable<SchemaColumn> BizObjColumns => new SchemaColumn[]
		{
			LicenceDatabaseSchema.PK,
			LicenceDatabaseSchema.LD_ServerCode,
			LicenceDatabaseSchema.LD_LicenceType,
			LicenceDatabaseSchema.LD_LicenceExpiry,
			LicenceDatabaseSchema.LD_LastHeartbeat,
			LicenceDatabaseSchema.LD_ReleaseRing,
			LicenceDatabaseSchema.LD_LE,
			LicenceDatabaseSchema.LD_OH_BillingParty,
			LicenceDatabaseSchema.LD_IsActive,
			LicenceDatabaseSchema.LD_DatabaseNumber,
			LicenceDatabaseSchema.LD_Status,
			LicenceDatabaseSchema.LD_Product,
			LicenceDatabaseSchema.LD_HostedLocation,
			LicenceDatabaseSchema.LD_LD_ParentDatabase,
			LicenceDatabaseSchema.LD_GS_NKOwner,
			LicenceDatabaseSchema.LD_Billable,
			LicenceDatabaseSchema.LD_TenantID,
			LicenceDatabaseSchema.LD_SystemCreateTimeUtc,
			LicenceDatabaseSchema.LD_SystemCreateUser,
			LicenceDatabaseSchema.LD_SystemLastEditTimeUtc,
			LicenceDatabaseSchema.LD_SystemLastEditUser
		};
	}
}
