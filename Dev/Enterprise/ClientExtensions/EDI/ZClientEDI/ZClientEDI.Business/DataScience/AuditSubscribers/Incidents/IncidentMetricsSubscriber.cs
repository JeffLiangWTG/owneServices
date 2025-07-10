using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Incidents
{
	public class IncidentMetricsSubscriber : DataScienceSubscriberToKafkaBase<KafkaRegistryConfigs.IncidentRelated>
	{
		public override string Code { get; } = SubscriberCodes.IncidentMetricsSubscriberCode;
		public override int DataSchemaVersion => 3; // Bump this if the schema changes
		public override ITableSchema Table { get; } = IncidentMetricsSchema.Instance;
		public override IEnumerable<SchemaColumn> BizObjColumns => new SchemaColumn[]
		{
			IncidentMetricsSchema.PK,
			IncidentMetricsSchema.IME_CalculatedMetric,
			IncidentMetricsSchema.IME_EndTimeUtc,
			IncidentMetricsSchema.IME_IncidentNumber,
			IncidentMetricsSchema.IME_MetricCode,
			IncidentMetricsSchema.IME_MetricCount,
			IncidentMetricsSchema.IME_StartTimeUtc,
			IncidentMetricsSchema.IME_SystemCreateTimeUtc,
			IncidentMetricsSchema.IME_SystemCreateUser,
			IncidentMetricsSchema.IME_SystemLastEditTimeUtc,
			IncidentMetricsSchema.IME_SystemLastEditUser,
		};
	}
}
