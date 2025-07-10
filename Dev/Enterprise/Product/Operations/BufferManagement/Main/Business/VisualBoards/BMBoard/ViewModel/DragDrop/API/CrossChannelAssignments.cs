using System;

namespace Enterprise.BufferManagement.Business
{
	[Flags]
	public enum CrossChannelTaskAssignments
	{
		None = 0,
		SelectedTask = 1 << 1,
		AllTasksInWorkflow = 1 << 2,
		TasksAssignedToStaff = 1 << 3,
		TasksAssignedToCapability = 1 << 4,
		TasksAssignedToGroup = 1 << 5,
		SelectedTaskForClaimOrAssign = 1 << 6,
		AllTasksInWorkfloWithForClaimOrAssign = 1 << 7,
		WorkflowBetweenTimeSlot = 1 << 8,
	}
}
