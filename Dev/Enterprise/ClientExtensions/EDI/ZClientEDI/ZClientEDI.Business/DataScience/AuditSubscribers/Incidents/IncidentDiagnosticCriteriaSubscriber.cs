using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Incidents
{
	public class IncidentDiagnosticCriteriaSubscriber : DataScienceSubscriberToKafkaBase<KafkaRegistryConfigs.IncidentRelated>
	{
		public override int DataSchemaVersion => 1;

		public override string Code => SubscriberCodes.IncidentDiagnosticCriteriaSubscriberCode;

		public override ITableSchema Table => IncidentDiagnosticCriteriaSchema.Instance;

		public override IEnumerable<SchemaColumn> BizObjColumns { get; } = new SchemaColumn[]
		{
			IncidentDiagnosticCriteriaSchema.PK,
			IncidentDiagnosticCriteriaSchema.IMD_Description,
			IncidentDiagnosticCriteriaSchema.IMD_FocusRelatedTriageNodesOnly,
			IncidentDiagnosticCriteriaSchema.IMD_InternalSupportNote,
			IncidentDiagnosticCriteriaSchema.IMD_IsActive,
			IncidentDiagnosticCriteriaSchema.IMD_Keywords,
			IncidentDiagnosticCriteriaSchema.IMD_Question,
			IncidentDiagnosticCriteriaSchema.IMD_SystemCreateTimeUtc,
			IncidentDiagnosticCriteriaSchema.IMD_SystemCreateUser,
			IncidentDiagnosticCriteriaSchema.IMD_SystemLastEditTimeUtc,
			IncidentDiagnosticCriteriaSchema.IMD_SystemLastEditUser,
			IncidentDiagnosticCriteriaSchema.IMD_Type
		};
	}
}
