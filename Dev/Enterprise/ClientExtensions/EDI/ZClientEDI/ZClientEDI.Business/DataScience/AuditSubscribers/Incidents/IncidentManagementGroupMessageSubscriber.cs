using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Incidents
{
	public class IncidentManagementGroupMessageSubscriber : DataScienceSubscriberToKafkaBase<KafkaRegistryConfigs.IncidentRelated>
	{
		public override string Code { get; } = SubscriberCodes.IncidentManagementGroupMessageSubscriberCode;
		public override int DataSchemaVersion => 1; // Bump this if the schema changes
		public override ITableSchema Table { get; } = IncidentManagementGroupMessageSchema.Instance;
		public override IEnumerable<SchemaColumn> BizObjColumns => new SchemaColumn[]
		{
			IncidentManagementGroupMessageSchema.PK,
			IncidentManagementGroupMessageSchema.IGM_ING_Group,
			IncidentManagementGroupMessageSchema.IGM_IsPublished,
			IncidentManagementGroupMessageSchema.IGM_Type,
			IncidentManagementGroupMessageSchema.IGM_Message,
			IncidentManagementGroupMessageSchema.IGM_SystemCreateTimeUtc,
			IncidentManagementGroupMessageSchema.IGM_SystemCreateUser,
			IncidentManagementGroupMessageSchema.IGM_SystemLastEditTimeUtc,
			IncidentManagementGroupMessageSchema.IGM_SystemLastEditUser,
			IncidentManagementGroupMessageSchema.IGM_BroadcastDateUtc,
		};
	}
}
