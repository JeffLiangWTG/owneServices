using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Incidents
{
	public class IncidentManagementGroupSubscriber : DataScienceSubscriberToKafkaBase<KafkaRegistryConfigs.IncidentRelated>
	{
		public override string Code { get; } = SubscriberCodes.IncidentManagementGroupSubscriberCode;
		public override int DataSchemaVersion => 3; // Bump this if the schema changes
		public override ITableSchema Table { get; } = IncidentManagementGroupSchema.Instance;
		public override IEnumerable<SchemaColumn> BizObjColumns => new SchemaColumn[]
		{
			IncidentManagementGroupSchema.PK,
			IncidentManagementGroupSchema.ING_IncidentGroupNumber,
			IncidentManagementGroupSchema.ING_Type,
			IncidentManagementGroupSchema.ING_Description,
			IncidentManagementGroupSchema.ING_ServiceOutage,
			IncidentManagementGroupSchema.ING_BusinessImpact,
			IncidentManagementGroupSchema.ING_Urgency,
			IncidentManagementGroupSchema.ING_GS_NKGroupOwner,
			IncidentManagementGroupSchema.ING_Status,
			IncidentManagementGroupSchema.ING_Product,
			IncidentManagementGroupSchema.ING_ProductArea,
			IncidentManagementGroupSchema.ING_Priority,
			IncidentManagementGroupSchema.ING_SourceModuleId,
			IncidentManagementGroupSchema.ING_Category,
			IncidentManagementGroupSchema.ING_Module,
			IncidentManagementGroupSchema.ING_RN_NKCountry,
			IncidentManagementGroupSchema.ING_SystemCreateTimeUtc,
			IncidentManagementGroupSchema.ING_SystemCreateUser,
			IncidentManagementGroupSchema.ING_SystemCreateBranch,
			IncidentManagementGroupSchema.ING_SystemCreateDepartment,
			IncidentManagementGroupSchema.ING_SystemLastEditTimeUtc,
			IncidentManagementGroupSchema.ING_SystemLastEditUser,
			IncidentManagementGroupSchema.ING_GS_NKDefaultResponder,
			IncidentManagementGroupSchema.ING_IsAutoReply,
			IncidentManagementGroupSchema.ING_IsBroadcastToControlledOnly,
			IncidentManagementGroupSchema.ING_IsKnownIssue,
			IncidentManagementGroupSchema.ING_IMT_Triage,
			IncidentManagementGroupSchema.ING_ServiceType,
			IncidentManagementGroupSchema.ING_IsAutoReplyUnflagsCommunication,
			IncidentManagementGroupSchema.ING_IsInterimBroadcastUnflagsCommunication,
		};
	}
}
