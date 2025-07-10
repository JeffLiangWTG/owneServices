using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Incidents
{
	public class IncidentManagementLinkSubscriber : DataScienceSubscriberToKafkaBase<KafkaRegistryConfigs.IncidentRelated>
	{
		public override string Code { get; } = SubscriberCodes.IncidentManagementLinkSubscriberCode;
		public override int DataSchemaVersion => 1; // Bump this if the schema changes
		public override ITableSchema Table { get; } = IncidentManagementLinkSchema.Instance;
		public override IEnumerable<SchemaColumn> BizObjColumns => new SchemaColumn[]
		{
			IncidentManagementLinkSchema.PK,
			IncidentManagementLinkSchema.INL_IsGroupControlled,
			IncidentManagementLinkSchema.INL_GS_NKResponder,
			IncidentManagementLinkSchema.INL_ING_Group,
			IncidentManagementLinkSchema.INL_IM_Incident,
			IncidentManagementLinkSchema.INL_SystemCreateTimeUtc,
			IncidentManagementLinkSchema.INL_SystemCreateUser,
		};
	}
}
