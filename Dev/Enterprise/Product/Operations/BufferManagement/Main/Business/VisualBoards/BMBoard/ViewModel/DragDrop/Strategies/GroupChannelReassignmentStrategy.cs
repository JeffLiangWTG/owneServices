using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business
{
	class GroupChannelReassignmentStrategy : TicketReassignmentStrategy
	{
		internal GroupChannelReassignmentStrategy(TicketReassignmentParameters parameters)
			: base(parameters)
		{
		}

		protected override bool CanReassign()
		{
			return Parameters.CardContent.CardType == CardType.Task && Parameters.Task.P9_GG_AssignedGroup != Parameters.DestinationChannel.EntityPK;
		}

		protected override string GetReassignmentMessage()
		{
			return Res.GetString("0f33a7ae-3306-4b82-b991-be41711b0101", "Would you like to assign tasks related to '{0}' to the group '{1}'?",
				Parameters.Task.P9_Description,
				Parameters.DestinationChannel.GetChannelName(DisplayNameType.FullName));
		}

		protected override void PerformReassignment(ProcessTask task)
		{
			task.P9_GG_AssignedGroup = Parameters.DestinationChannel.EntityPK;
		}

		protected override bool ShouldReturnTicketToOriginalLocation()
		{
			return Parameters.CardContent.CardType == CardType.Workflow || Parameters.Task.P9_GG_AssignedGroup == Parameters.DestinationChannel.EntityPK;
		}

		protected override CrossChannelTaskAssignments GetAllowedReassignmentOptionFlags()
		{
			return CrossChannelTaskAssignments.AllTasksInWorkflow
				| CrossChannelTaskAssignments.SelectedTask
				| CrossChannelTaskAssignments.TasksAssignedToGroup;
		}
	}
}
