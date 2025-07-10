using CargoWise.Database.Abstractions.Extensions;
using CargoWise.Database.Shared;

namespace ZClientEDI.Business
{
	public static class ViewClientProcessHeaderCreateScript
	{
		public static DatabaseViewAndRoutineCreateScript Create() => new DatabaseViewAndRoutineCreateScript("ViewClientProcessHeader",
@"CREATE VIEW ViewClientProcessHeader
WITH SCHEMABINDING
as
SELECT
	ProcessHeader.FH_PK as VFH_PK,
	ProcessHeader.FH_IsActive as VFH_IsActive,
	ProcessHeader.FH_FC_CurrentComponent as VFH_FC_CurrentComponent,
	ProcessHeader.FH_FC_DedicatedBuffer as VFH_FC_DedicatedBuffer,
	ProcessHeader.FH_ParentTemplateId as VFH_ParentTemplateId,
	ProcessHeader.FH_ReleaseDateTime as VFH_ReleaseDateTime,
	ProcessHeader.FH_GG_ReleaseGroup as VFH_GG_ReleaseGroup,
	ProcessHeader.FH_VoteUpDownAmount as VFH_VoteUpDownAmount,
	ProcessHeader.FH_P0_Template as VFH_P0_Template,
	ProcessHeader.FH_CompletionStatement as VFH_CompletionStatement,
	ProcessHeader.FH_AgreedDeliveryDate as VFH_AgreedDeliveryDate,
	ProcessHeader.FH_DoNotStartBeforeDate as VFH_DoNotStartBeforeDate,
	jobHeader.FH_DoNotStartBeforeDate as VFH_JobDoNotStartBeforeDate,
	ProcessHeader.FH_Status as VFH_Status,
	ProcessHeader.FH_StaggeredReleaseDelayExpiry as VFH_StaggeredReleaseDelayExpiry,
	ProcessHeader.FH_TimeDelayFactor as VFH_TimeDelayFactor,
	ProcessHeader.FH_TimeDelayMinutes as VFH_TimeDelayMinutes,
	ProcessHeader.FH_FH_ParentHeader as VFH_FH_ParentHeader,
	ProcessHeader.FH_ParentId as VFH_ParentId,
	ProcessHeader.FH_ParentTableCode as VFH_ParentTableCode,
	ProcessHeader.FH_AllowTaskAutoAssignment as VFH_AllowTaskAutoAssignment,
	ProcessHeader.FH_IsStandby as VFH_IsStandby,
	ProcessHeader.FH_IsCriticalHandover as VFH_IsCriticalHandover,
	ProcessHeader.FH_SystemCreateTimeUtc as VFH_SystemCreateTimeUtc,
	ProcessHeader.FH_SystemCreateUser as VFH_SystemCreateUser,
	ProcessHeader.FH_SystemLastEditTimeUtc as VFH_SystemLastEditTimeUtc,
	ProcessHeader.FH_SystemLastEditUser as VFH_SystemLastEditUser,
	ProcessHeader.FH_PlannedDurationInMinutes as VFH_PlannedDurationInMinutes,
	ProcessHeader.FH_BufferPenetrationPercentWhenCompleted as VFH_BufferPenetrationPercentWhenCompleted,
	ProcessHeader.FH_IsReleasableUnitParent as VFH_IsReleasableUnitParent,
	ProcessHeader.FH_DateAcceptability as VFH_DateAcceptability,
	ProcessHeader.FH_WorkflowType as VFH_WorkflowType,
	IM_IncidentNumber as VFH_JobCode,
	ProcessHeader.FH_LastTransferType as VFH_LastTransferType,
	IM_Description as VFH_Description
FROM dbo.ProcessHeader
LEFT JOIN dbo.ProcessHeader jobHeader on ProcessHeader.FH_FH_ParentHeader = jobHeader.FH_PK
LEFT JOIN dbo.IncidentMain on IM_PK = ProcessHeader.FH_ParentId
where ProcessHeader.FH_ParentTableCode = 'IM' or ProcessHeader.FH_WorkflowType = 'INC'
", "drop view ViewClientProcessHeader", DbRoutineType.SqlViewTypeDesc);
	}
}
