using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Incidents
{
	public class IncidentTriageDiagnosticCriteriaPivotSubscriber : DataScienceSubscriberToKafkaBase<KafkaRegistryConfigs.IncidentRelated>
	{
		public override int DataSchemaVersion => 1;

		public override string Code => SubscriberCodes.IncidentTriageDiagnosticCriteriaPivotSubscriberCode;

		public override ITableSchema Table => IncidentTriageDiagnosticCriteriaPivotSchema.Instance;

		public override IEnumerable<SchemaColumn> BizObjColumns { get; } = new SchemaColumn[]
		{
			IncidentTriageDiagnosticCriteriaPivotSchema.PK,
			IncidentTriageDiagnosticCriteriaPivotSchema.IMO_IMD_DiagnosticCriteria,
			IncidentTriageDiagnosticCriteriaPivotSchema.IMO_IMT_Triage,
			IncidentTriageDiagnosticCriteriaPivotSchema.IMO_SystemCreateTimeUtc,
			IncidentTriageDiagnosticCriteriaPivotSchema.IMO_SystemCreateUser,
			IncidentTriageDiagnosticCriteriaPivotSchema.IMO_SystemLastEditTimeUtc,
			IncidentTriageDiagnosticCriteriaPivotSchema.IMO_SystemLastEditUser,
		};
	}
}
