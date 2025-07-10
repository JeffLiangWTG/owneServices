using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business
{
	class CapabilityChannelReassignmentStrategy : TicketReassignmentStrategy
	{
		internal CapabilityChannelReassignmentStrategy(TicketReassignmentParameters parameters)
			: base(parameters)
		{
		}

		#region TicketReassignmentStrategy Overrides

		protected override bool CanReassign()
		{
			return Parameters.CardContent.CardType == CardType.Task
				&& (Parameters.Task.P9_G4_RequiredCapability != Parameters.DestinationChannel.EntityPK || !Parameters.Task.P9_GS_NKAssignedStaffMember.IsEmpty);
		}

		protected override string GetReassignmentMessage()
		{
			var task = Parameters.Task;
			var channel = Parameters.DestinationChannel;

			var areWeReturningTaskToCapability = !task.P9_GS_NKAssignedStaffMember.IsEmpty && task.P9_G4_RequiredCapability == channel.EntityPK;
			var message = Res.GetString("64aaefc2-0164-4708-bda4-f60862b3d74b", "Would you like to {0} task '{1}' to the capability '{2}'?",
				/*0*/ areWeReturningTaskToCapability ? Res.GetString("47ad2a11-ebfa-4a79-b21d-f6ee1e84cb3a", "return") : Res.GetString("f5c42de9-69e6-46cf-9065-29f3f8ff7316", "assign"),
				/*1*/ task.P9_Description,
				/*2*/ channel.GetChannelName(DisplayNameType.FullName));

			var shouldRemoveResource = ShouldRemoveResourceAssignment(task, Parameters.SourceChannel) && task.AssignedStaffMember != null;
			var shouldChangeCapability = !task.P9_G4_RequiredCapability.IsEmpty && task.P9_G4_RequiredCapability != channel.EntityPK && !task.P9_GS_NKAssignedStaffMember.IsEmpty;

			if (shouldRemoveResource && shouldChangeCapability)
			{
				message += System.Environment.NewLine + System.Environment.NewLine +
					Res.GetString("c781c769-6418-452f-b9b9-e3666e6cd920", "This will {0}, and {1}.",
					GetResourceAssignmentMessage(task.AssignedStaffMember),
					GetCapabilityReplacementMessage(task.RequiredCapability));
			}
			else if (shouldRemoveResource)
			{
				message += System.Environment.NewLine + System.Environment.NewLine +
					Res.GetString("5be04a3f-be51-4e49-8e27-25350b734b80", "This will {0}.",
					GetResourceAssignmentMessage(task.AssignedStaffMember));
			}
			else if (shouldChangeCapability)
			{
				message += System.Environment.NewLine + System.Environment.NewLine +
					Res.GetString("5be04a3f-be51-4e49-8e27-25350b734b80", "This will {0}.",
					GetCapabilityReplacementMessage(task.RequiredCapability));
			}

			return message;
		}

		static bool ShouldCloseExistingTaskAndClone(ProcessTask task)
		{
			return task.P9_Status == ProcessTaskStatusCodeList.Codes.Suspended || task.P9_Status == ProcessTaskStatusCodeList.Codes.Working;
		}

		static bool ShouldNotUpdateTaskAssignment(ProcessTask task)
		{
			return task.P9_Status == ProcessTaskStatusCodeList.Codes.Cancelled || task.P9_Status == ProcessTaskStatusCodeList.Codes.Closed;
		}

		protected ProcessTask GetTaskToUpdateAssignment(ProcessTask task)
		{
			if (ShouldCloseExistingTaskAndClone(task))
			{
				var newTask = task.Clone() as ProcessTask;
				newTask.P9_FH_ProcessHeader = Parameters.Workflow.PK;
				Parameters.Workflow.TaskCollection.Add(newTask);
				task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				return newTask;
			}

			return task;
		}

		protected override void PerformReassignment(ProcessTask task)
		{
			if (ShouldNotUpdateTaskAssignment(task))
			{
				return;
			}

			var newTask = GetTaskToUpdateAssignment(task);

			if (ShouldRemoveResourceAssignment(task, Parameters.SourceChannel))
			{
				newTask.P9_GS_NKAssignedStaffMember = ZString.Empty;
			}

			newTask.P9_G4_RequiredCapability = Parameters.DestinationChannel.EntityPK;
		}

		protected override bool ShouldReturnTicketToOriginalLocation()
		{
			return Parameters.CardContent.CardType == CardType.Workflow || Parameters.Task.P9_G4_RequiredCapability == Parameters.DestinationChannel.EntityPK;
		}

		protected override CrossChannelTaskAssignments GetAllowedReassignmentOptionFlags()
		{
			return CrossChannelTaskAssignments.AllTasksInWorkflow
				| CrossChannelTaskAssignments.SelectedTask
				| CrossChannelTaskAssignments.TasksAssignedToCapability;
		}

		#endregion

		#region Implementation

		static bool ShouldRemoveResourceAssignment(ProcessTask task, IVisualBoardChannel sourceChannel)
		{
			return !task.P9_GS_NKAssignedStaffMember.IsEmpty && sourceChannel != null && sourceChannel.EntityType != ChannelTypeList.Codes.Capability;
		}

		static string GetResourceAssignmentMessage(GlbStaff resource)
		{
			return Res.GetString("c8328aa9-61b0-4d70-aef6-868fa3902a2d", "remove the assignment of {0} from the task", resource.GS_FullName);
		}

		static string GetCapabilityReplacementMessage(GlbCapability capability)
		{
			return Res.GetString("f7f70a50-a019-44d1-a4af-6f7667d9e093", "replace the existing required capability of '{0}'", capability.G4_Description);
		}

		#endregion
	}
}
