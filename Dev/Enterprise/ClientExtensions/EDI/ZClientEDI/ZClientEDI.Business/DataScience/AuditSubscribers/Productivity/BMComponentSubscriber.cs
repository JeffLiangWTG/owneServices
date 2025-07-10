using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Productivity
{
	public class BMComponentSubscriber : DataScienceSubscriberToKafkaBase<KafkaRegistryConfigs.Productivity>
	{
		public override string Code => SubscriberCodes.BMComponentSubscriberCode;
		public override int DataSchemaVersion => 1; // Bump this if the schema changes
		public override ITableSchema Table { get; } = BMComponentSchema.Instance;
		public override IEnumerable<SchemaColumn> BizObjColumns => new SchemaColumn[]
		{
			BMComponentSchema.PK,
			BMComponentSchema.FC_AutoAssignTasksAge,
			BMComponentSchema.FC_BufferLoadLimitPercent,
			BMComponentSchema.FC_BufferTimeCapacityConstraintThresholdMultiple,
			BMComponentSchema.FC_BufferTimespanInMinutes,
			BMComponentSchema.FC_DisplaySequence,
			BMComponentSchema.FC_IsActive,
			BMComponentSchema.FC_NonCCRTemporaryOverloadLimitMultiplier,
			BMComponentSchema.FC_OffsetInMinutes,
			BMComponentSchema.FC_SystemCreateUser,
			BMComponentSchema.FC_Type,
			BMComponentSchema.FC_FC_ParentComponent,
			BMComponentSchema.FC_FS_System,
			BMComponentSchema.FC_GB_AgingBranch,
			BMComponentSchema.FC_GE_AgingDepartment,
			BMComponentSchema.FC_Name,
			BMComponentSchema.FC_SystemCreateTimeUtc,
			BMComponentSchema.FC_SystemLastEditTimeUtc,
			BMComponentSchema.FC_SystemLastEditUser
		};
	}
}
