using CargoWise.Types;

namespace Enterprise.Integration
{
	public interface IWorkflowTask : IWorkflowItem
	{
		ZGuid PK { get; }
		ZString P9_TaskID { get; }
		ZInt P9_Sequence { get; set; }
		ZString P9_Type { get; set; }
		ZString P9_Status { get; set; }

		ZString P9_CardNote { get; set; }
		ZString P9_Description { get; set; }

		ZDateTimeOffset P9_ScheduledDate { get; set; }
		ZDateTime P9_EstDuration { get; set; }
		ZDateTimeOffset P9_ActualDate { get; }
		ZDateTime P9_ActualDuration { get; set; }
		ZBlob P9_Notes { get; set; }
		ZString P9_GS_NKAssignedStaffMember { get; set; }
		ZGuid P9_GG_AssignedGroup { get; set; }
		ZGuid P9_OA { get; set; }
		ZGuid P9_OC { get; set; }
		ZGuid P9_ParentID { get; set; }
		ZString P9_ParentTableCode { get; set; }
		ZBool P9_IsCalendarItem { get; set; }
		ZDateTimeOffset P9_SuspendedAt { get; set; }
		ZDateTime P9_TotalSuspendedDuration { get; set; }
		ZBool P9_TaskCannotBeDeleted { get; set; }
		ZBool P9_IsPublished { get; set; }
		ZGuid P9_GC { get; set; }
		ZGuid P9_ParentTemplateID { get; set; }
		ZBool P9_IsInterruptable { get; set; }
		ZDecimal P9_EstimateVariationFactor { get; set; }
		ZDateTime P9_EstimatedTimeToComplete { get; set; }
		ZDateTimeOffset P9_CompletedTime { get; set; }
		ZGuid P9_FH_ProcessHeader { get; set; }
		ZGuid P9_G4_RequiredCapability { get; set; }

		ZDecimal RelevantEstimateHours { get; }
	}
}
