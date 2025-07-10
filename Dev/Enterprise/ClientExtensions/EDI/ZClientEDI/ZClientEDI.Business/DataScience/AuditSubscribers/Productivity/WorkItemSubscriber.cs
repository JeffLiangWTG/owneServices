using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Productivity
{
	public class WorkItemSubscriber : DataScienceSubscriberToKafkaBase<KafkaRegistryConfigs.Productivity>
	{
		public override string Code => SubscriberCodes.WorkItemSubscriberCode;
		public override int DataSchemaVersion => 4; // Bump this if the schema changes
		public override ITableSchema Table { get; } = WorkItemSchema.Instance;
		public override IEnumerable<SchemaColumn> BizObjColumns => new SchemaColumn[]
		{
			WorkItemSchema.PK,
			WorkItemSchema.WKI_ActivitySubtype,
			WorkItemSchema.WKI_ActivityType,
			WorkItemSchema.WKI_DateOfChange,
			WorkItemSchema.WKI_GB_AssignedBranch,
			WorkItemSchema.WKI_GC_AssignedCompany,
			WorkItemSchema.WKI_GE_AssignedDepartment,
			WorkItemSchema.WKI_P9_DefectCausedByTask,
			WorkItemSchema.WKI_P9_DefectFirstMissedInTask,
			WorkItemSchema.WKI_PortOrCountry,
			WorkItemSchema.WKI_Priority,
			WorkItemSchema.WKI_Risk,
			WorkItemSchema.WKI_Status,
			WorkItemSchema.WKI_Summary,
			WorkItemSchema.WKI_SystemCreateBranch,
			WorkItemSchema.WKI_SystemCreateDepartment,
			WorkItemSchema.WKI_SystemCreateTimeUtc,
			WorkItemSchema.WKI_SystemCreateUser,
			WorkItemSchema.WKI_SystemLastEditTimeUtc,
			WorkItemSchema.WKI_SystemLastEditUser,
			WorkItemSchema.WKI_WorkItemArea,
			WorkItemSchema.WKI_WorkItemNumber,
			WorkItemSchema.WKI_WorkItemType,
		};
	}
}
