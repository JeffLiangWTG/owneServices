using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Incidents
{
	public class IncidentDiagnosticCriteriaPivotSubscriber : DataScienceSubscriberToKafkaBase<KafkaRegistryConfigs.IncidentRelated>
	{
		public override int DataSchemaVersion => 1;

		public override string Code => SubscriberCodes.IncidentDiagnosticCriteriaPivotSubscriberCode;

		public override ITableSchema Table => IncidentDiagnosticCriteriaPivotSchema.Instance;

		public override IEnumerable<SchemaColumn> BizObjColumns { get; } = new SchemaColumn[]
		{
			IncidentDiagnosticCriteriaPivotSchema.PK,
			IncidentDiagnosticCriteriaPivotSchema.IMV_IMD_DiagnosticCriteria,
			IncidentDiagnosticCriteriaPivotSchema.IMV_ParentID,
			IncidentDiagnosticCriteriaPivotSchema.IMV_ParentTableCode,
			IncidentDiagnosticCriteriaPivotSchema.IMV_Status,
			IncidentDiagnosticCriteriaPivotSchema.IMV_SystemCreateTimeUtc,
			IncidentDiagnosticCriteriaPivotSchema.IMV_SystemCreateUser,
			IncidentDiagnosticCriteriaPivotSchema.IMV_SystemLastEditTimeUtc,
			IncidentDiagnosticCriteriaPivotSchema.IMV_SystemLastEditUser
		};
	}
}
