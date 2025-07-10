using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Core
{
	public class OrgStaffAssignmentsSubscriber : DataScienceSubscriberToKafkaBase<KafkaRegistryConfigs.Core>
	{
		public override string Code => SubscriberCodes.OrgStaffAssignmentsSubscriberCode;
		public override int DataSchemaVersion => 2; // Bump this if the schema changes
		public override ITableSchema Table { get; } = OrgStaffAssignmentsSchema.Instance;
		public override IEnumerable<SchemaColumn> BizObjColumns => new SchemaColumn[]
		{
			OrgStaffAssignmentsSchema.PK,
			OrgStaffAssignmentsSchema.O8_Department,
			OrgStaffAssignmentsSchema.O8_GC,
			OrgStaffAssignmentsSchema.O8_GS_NKPersonResponsible,
			OrgStaffAssignmentsSchema.O8_OH,
			OrgStaffAssignmentsSchema.O8_Role,
			OrgStaffAssignmentsSchema.O8_Product,
			OrgStaffAssignmentsSchema.O8_SystemCreateUser,
			OrgStaffAssignmentsSchema.O8_SystemCreateTimeUtc,
			OrgStaffAssignmentsSchema.O8_SystemLastEditUser,
			OrgStaffAssignmentsSchema.O8_SystemLastEditTimeUtc,
		};
	}
}
