using System;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business
{
	abstract class TicketReassignmentStrategy
	{
		internal static TicketReassignmentStrategy GetStrategy(IVisualBoardChannel channel, TicketReassignmentParameters parameters)
		{
			if (parameters.IsMovingWithinChannelToDifferentTimeSlot)
			{
				return new ChangeTimeSlotReassignmentStrategy(parameters);
			}
			else if (channel != null)
			{
				switch (channel.EntityType)
				{
					case ChannelTypeList.Codes.Resource:
						return new StaffChannelReassignmentStrategy(parameters);

					case ChannelTypeList.Codes.Group:
						return new GroupChannelReassignmentStrategy(parameters);

					case ChannelTypeList.Codes.Capability:
						return new CapabilityChannelReassignmentStrategy(parameters);

					case ChannelTypeList.Codes.Tag:
						return new TagChannelReassignmentStrategy(parameters);

					default:
						throw new ArgumentException("Invalid EntityType: " + channel.EntityType);
				}
			}
			else
			{
				return new NoOperationTicketReassignmentStrategy(parameters);
			}
		}

		protected TicketReassignmentStrategy(TicketReassignmentParameters parameters)
		{
			Parameters = parameters;
		}

		protected TicketReassignmentParameters Parameters { get; }

		internal bool TryReassign()
		{
			if (CanReassign())
			{
				var message = GetReassignmentMessage();
				var caption = GetReassignmentCaption();

				var result = ShowDialog(message, caption);

				if (result != CrossChannelTaskAssignments.None)
				{
					return true;
				}
			}

			var operationWasSuccessfulEvenIfNotReassigned = Parameters.Workflow.FH_FC_CurrentComponentInfo.HasChanges && ShouldReturnTicketToOriginalLocation();

			return operationWasSuccessfulEvenIfNotReassigned;
		}

		protected abstract bool CanReassign();

		protected abstract string GetReassignmentMessage();

		protected virtual string GetReassignmentCaption()
		{
			return Res.GetString("4c9af4c6-8472-45dd-85ee-ff48a5d2f353", "Reassign task");
		}

		protected virtual CrossChannelTaskAssignments ShowDialog(string message, string caption)
		{
			var sourceChannelStrategy = GetStrategy(Parameters.SourceChannel, Parameters);
			var assignmentOptions = sourceChannelStrategy.GetAllowedReassignmentOptionFlags();

			return ChannelReassignmentHandler.ShowDialog(Parameters.Task, Parameters.Workflow, Parameters.SourceChannel, assignmentOptions, Parameters.UserNotification, message, caption, t => PerformReassignment(t), Parameters.DestinationChannel);
		}

		protected abstract void PerformReassignment(ProcessTask task);

		protected abstract bool ShouldReturnTicketToOriginalLocation();

		protected abstract CrossChannelTaskAssignments GetAllowedReassignmentOptionFlags();
	}
}
