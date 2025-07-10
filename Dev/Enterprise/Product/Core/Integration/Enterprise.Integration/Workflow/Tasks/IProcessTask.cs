using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Integration
{
	public interface IProcessTask
	{
		ZGuid PK { get; }
		ZString P9_TaskID { get; }
		ZString P9_ParentTableCode { get; set; }
		ZGuid P9_ParentID { get; set; }
		ZGuid P9_GC { get; set; }

		ZString P9_Description { get; set; }
		ZPropertyInfo P9_DescriptionInfo { get; }

		ZInt P9_Sequence { get; set; }
		ZPropertyInfo P9_SequenceInfo { get; }

		ZString P9_Type { get; set; }
		ZPropertyInfo P9_TypeInfo { get; }

		ZBool P9_IsCalendarItem { get; set; }
		ZPropertyInfo P9_IsCalendarItemInfo { get; }

		ZString P9_GS_NKAssignedStaffMember { get; set; }
		ZPropertyInfo P9_GS_NKAssignedStaffMemberInfo { get; }

		ZGuid P9_GG_AssignedGroup { get; set; }

		ZPropertyInfo P9_GG_AssignedGroupInfo { get; }

		ZGuid P9_G4_RequiredCapability { get; set; }
		ZPropertyInfo P9_G4_RequiredCapabilityInfo { get; }

		ZDateTimeOffset P9_ScheduledDate { get; set; }
		ZPropertyInfo P9_ScheduledDateInfo { get; }

		ZDateTimeOffset P9_ActualDate { get; }
		ZPropertyInfo P9_ActualDateInfo { get; }

		ZString P9_SE_NKMilestoneEvent { get; set; }
		ZPropertyInfo P9_SE_NKMilestoneEventInfo { get; }

		ZString P9_Condition1 { get; set; }
		ZPropertyInfo P9_Condition1Info { get; }

		ZBlob P9_Notes { get; set; }
		ZPropertyInfo P9_NotesInfo { get; }

		ZString P9_ReferencedTableCode { get; set; }
		ZPropertyInfo P9_ReferencedTableCodeInfo { get; }

		ZDateTime P9_EstDuration { get; set; }
		ZPropertyInfo P9_EstDurationInfo { get; }

		ZString P9_EstimatedDefaultedFrom { get; set; }
		ZDateTime P9_EstimatedDefaultTimeDelta { get; set; }
		ZInt P9_EstimatedDefaultFromPredecessor { get; set; }

		ZBool P9_RespondToCascadedEvents { get; }
		ZBool P9_IsResetBeingAppliedToThisTask { get; }

		ZString P9_Status { get; set; }
		ZPropertyInfo P9_StatusInfo { get; }

		ZGuid P9_FH_ProcessHeader { get; set; }
		ZPropertyInfo P9_FH_ProcessHeaderInfo { get; }

		ZDateTimeOffset P9_CompletedTime { get; set; }
		ZPropertyInfo P9_CompletedTimeInfo { get; }

		ZString P9_LineTriggerType { get; set; }
		ZPropertyInfo P9_LineTriggerTypeInfo { get; }

		ZGuid P9_ParentTemplateID { get; }
		ZPropertyInfo P9_ParentTemplateIDInfo { get; }

		ZString P9_FormFlowType { get; set; }

		ZDateTime P9_SystemCreateTimeUtc { get; set; }

		void DefaultEstimateIfRequired();

		ITemplateConditional TemplateConditions { get; }

		ZDateTime TimeBecameStartable { get; }

		ZBool ShouldTriggerOnEstimateEvents { get; set; }
	}

	public interface IProcessTaskInternals
	{
		IDisposable SuspendUpdateEventLog();

		void SetActualDateWithoutFiringWorkflow(ZDateTimeOffset value);
	}
}
