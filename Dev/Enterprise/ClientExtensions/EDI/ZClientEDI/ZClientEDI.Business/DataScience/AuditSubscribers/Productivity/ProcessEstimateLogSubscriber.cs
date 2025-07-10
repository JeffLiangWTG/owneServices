using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Productivity
{
	public class ProcessEstimateLogSubscriber : DataScienceSubscriberToKafkaBase<KafkaRegistryConfigs.Productivity>
	{
		public override string Code => SubscriberCodes.ProcessEstimateLogSubscriberCode;
		public override int DataSchemaVersion => 1; // Bump this if the schema changes
		public override ITableSchema Table { get; } = ProcessEstimateLogSchema.Instance;
		public override IEnumerable<SchemaColumn> BizObjColumns => new SchemaColumn[]
		{
			ProcessEstimateLogSchema.PK,
			ProcessEstimateLogSchema.P9E_GS_NKUser,
			ProcessEstimateLogSchema.P9E_HasWorkStarted,
			ProcessEstimateLogSchema.P9E_LogDateTime,
			ProcessEstimateLogSchema.P9E_NewHighEstimateMinutes,
			ProcessEstimateLogSchema.P9E_NewLowEstimateMinutes,
			ProcessEstimateLogSchema.P9E_ParentId,
			ProcessEstimateLogSchema.P9E_ParentTableCode,
			ProcessEstimateLogSchema.P9E_PreviousHighEstimateMinutes,
			ProcessEstimateLogSchema.P9E_PreviousLowEstimateMinutes,
			ProcessEstimateLogSchema.P9E_SystemCreateTimeUtc,
			ProcessEstimateLogSchema.P9E_SystemCreateUser,
			ProcessEstimateLogSchema.P9E_SystemLastEditTimeUtc,
			ProcessEstimateLogSchema.P9E_SystemLastEditUser,
			ProcessEstimateLogSchema.P9E_WasWorkPreviouslyStarted,
		};
	}
}
