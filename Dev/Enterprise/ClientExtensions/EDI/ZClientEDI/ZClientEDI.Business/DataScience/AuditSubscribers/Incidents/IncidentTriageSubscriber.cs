using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Incidents
{
	public class IncidentTriageSubscriber : DataScienceSubscriberToKafkaBase<KafkaRegistryConfigs.IncidentRelated>
	{
		public override int DataSchemaVersion => 3;

		public override string Code => SubscriberCodes.IncidentTriageSubscriberCode;

		public override ITableSchema Table => IncidentTriageSchema.Instance;

		public override IEnumerable<SchemaColumn> BizObjColumns { get; } = new SchemaColumn[]
		{
			IncidentTriageSchema.PK,
			IncidentTriageSchema.IMT_IsActive,
			IncidentTriageSchema.IMT_IsInternal,
			IncidentTriageSchema.IMT_IsPublished,
			IncidentTriageSchema.IMT_Level,
			IncidentTriageSchema.IMT_Module,
			IncidentTriageSchema.IMT_Product,
			IncidentTriageSchema.IMT_ProductArea,
			IncidentTriageSchema.IMT_SetProductAreaByMenuItem,
			IncidentTriageSchema.IMT_SupportDescription,
			IncidentTriageSchema.IMT_SystemCreateTimeUtc,
			IncidentTriageSchema.IMT_SystemCreateUser,
			IncidentTriageSchema.IMT_SystemCreateBranch,
			IncidentTriageSchema.IMT_SystemCreateDepartment,
			IncidentTriageSchema.IMT_SystemLastEditTimeUtc,
			IncidentTriageSchema.IMT_SystemLastEditUser,
			IncidentTriageSchema.IMT_TriageNumber,
			IncidentTriageSchema.IMT_Type,
			IncidentTriageSchema.IMT_IsPublishedToAssist,
		};
	}
}
