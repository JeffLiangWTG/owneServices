using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Core
{
	public sealed class GlbStaffManagerSubscriber : DataScienceSubscriberToKafkaBase<KafkaRegistryConfigs.Core>
	{
		public override string Code => SubscriberCodes.GlbStaffManagerSubscriberCode;
		public override int DataSchemaVersion => 1; // Bump this if the schema changes

		public override ITableSchema Table { get; } = GlbStaffManagerSchema.Instance;

		public override IEnumerable<SchemaColumn> BizObjColumns => new SchemaColumn[]
		{
			GlbStaffManagerSchema.PK,
			GlbStaffManagerSchema.GSM_EffectiveDate,
			GlbStaffManagerSchema.GSM_EndDate,
			GlbStaffManagerSchema.GSM_GCR_ChangeRequest,
			GlbStaffManagerSchema.GSM_GS_Manager,
			GlbStaffManagerSchema.GSM_GS_Staff,
			GlbStaffManagerSchema.GSM_IsApproved,
			GlbStaffManagerSchema.GSM_ManagerType,
			GlbStaffManagerSchema.GSM_SystemCreateTimeUtc,
			GlbStaffManagerSchema.GSM_SystemCreateUser,
			GlbStaffManagerSchema.GSM_SystemLastEditTimeUtc,
			GlbStaffManagerSchema.GSM_SystemLastEditUser,
		};
	}
}
