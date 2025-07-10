using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Productivity
{
	public class ProcessHeaderSubscriber : DataScienceSubscriberToKafkaBase<KafkaRegistryConfigs.Productivity>
	{
		public override string Code => SubscriberCodes.ProcessHeaderSubscriberCode;
		public override int DataSchemaVersion => 11; // Bump this if the schema changes
		public override ITableSchema Table { get; } = ProcessHeaderSchema.Instance;
		public override IEnumerable<SchemaColumn> BizObjColumns => new SchemaColumn[]
		{
			ProcessHeaderSchema.PK,
			ProcessHeaderSchema.FH_AgreedDeliveryDate,
			ProcessHeaderSchema.FH_AgreedDeliveryDateDefaultHoursOffset,
			ProcessHeaderSchema.FH_AgreedDeliveryDateDefaultsFrom,
			ProcessHeaderSchema.FH_AllowTaskAutoAssignment,
			ProcessHeaderSchema.FH_BufferPenetrationPercentWhenCompleted,
			ProcessHeaderSchema.FH_Category,
			ProcessHeaderSchema.FH_CompletionStatement,
			ProcessHeaderSchema.FH_DateAcceptability,
			ProcessHeaderSchema.FH_DeadlineType,
			ProcessHeaderSchema.FH_DoNotStartBeforeDate,
			ProcessHeaderSchema.FH_EarliestStartDateDefaultsFrom,
			ProcessHeaderSchema.FH_EarliestStartDefaultHoursOffset,
			ProcessHeaderSchema.FH_FC_CurrentComponent,
			ProcessHeaderSchema.FH_FH_ParentHeader,
			ProcessHeaderSchema.FH_GG_ReleaseGroup,
			ProcessHeaderSchema.FH_IsActive,
			ProcessHeaderSchema.FH_IsCriticalHandover,
			ProcessHeaderSchema.FH_IsReleasableUnitParent,
			ProcessHeaderSchema.FH_IsReleaseGroupSetByTemplate,
			ProcessHeaderSchema.FH_IsStandby,
			ProcessHeaderSchema.FH_JobCode,
			ProcessHeaderSchema.FH_JobDescription,
			ProcessHeaderSchema.FH_LastTransferType,
			ProcessHeaderSchema.FH_P0_Template,
			ProcessHeaderSchema.FH_ParentId,
			ProcessHeaderSchema.FH_ParentTableCode,
			ProcessHeaderSchema.FH_ParentTemplateId,
			ProcessHeaderSchema.FH_PlannedDurationInMinutes,
			ProcessHeaderSchema.FH_ReleaseDateTime,
			ProcessHeaderSchema.FH_RemainingMinutesToComplete,
			ProcessHeaderSchema.FH_StaggeredReleaseDelayExpiry,
			ProcessHeaderSchema.FH_Status,
			ProcessHeaderSchema.FH_SystemCreateTimeUtc,
			ProcessHeaderSchema.FH_SystemCreateUser,
			ProcessHeaderSchema.FH_SystemLastEditTimeUtc,
			ProcessHeaderSchema.FH_SystemLastEditUser,
			ProcessHeaderSchema.FH_TaskLowestOpenSequenceNumber,
			ProcessHeaderSchema.FH_TaskPenetrationResetDateTimeUtc,
			ProcessHeaderSchema.FH_TimeDelayFactor,
			ProcessHeaderSchema.FH_TimeDelayMinutes,
			ProcessHeaderSchema.FH_VoteUpDownAmount,
			ProcessHeaderSchema.FH_WorkflowType,
			ProcessHeaderSchema.FH_IsApproved,
			ProcessHeaderSchema.FH_EffectiveNudge,
			ProcessHeaderSchema.FH_FC_DedicatedBuffer,
			ProcessHeaderSchema.FH_GB_Branch,
			ProcessHeaderSchema.FH_GE_Department,
			ProcessHeaderSchema.FH_EffectiveAgreedDeliveryDateUtc,
			ProcessHeaderSchema.FH_ReleaseSequenceSortDateUtc,
			ProcessHeaderSchema.FH_LatestAcceptableReleaseDateUtc,
			ProcessHeaderSchema.FH_GB_EffectiveBranch,
			ProcessHeaderSchema.FH_GE_EffectiveDepartment,
			ProcessHeaderSchema.FH_BMT_BufferTimespan,
		};
	}
}
