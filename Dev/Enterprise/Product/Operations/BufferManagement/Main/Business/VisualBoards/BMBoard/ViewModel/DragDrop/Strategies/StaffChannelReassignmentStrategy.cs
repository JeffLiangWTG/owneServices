using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.BufferManagement.Business
{
	class StaffChannelReassignmentStrategy : TicketReassignmentStrategy
	{
		internal StaffChannelReassignmentStrategy(TicketReassignmentParameters parameters)
			: base(parameters)
		{
			channel = parameters.DestinationChannel;
		}

		readonly IVisualBoardChannel channel;

		#region TicketReassignmentStrategy Overrides

		protected override bool CanReassign()
		{
			var task = Parameters.Task;

			return Parameters.CardContent.CardType == CardType.Task
				&& !string.Equals(task.P9_GS_NKAssignedStaffMember, channel.ChannelEntityCode, StringComparison.CurrentCultureIgnoreCase)
				&& channel.TryGetCapabilityPKs(task.Factory, out var capabilityPKs)
				&& ResourceIsAllowedToWorkOnCapabilityTask(channel, capabilityPKs, task);
		}

		protected override string GetReassignmentMessage()
		{
			var task = Parameters.Task;
			var capability = task.RequiredCapability;
			var openTasks = Parameters.Workflow.Tasks.Where(t => t.IsOpenOrAssigned).ToArray();

			if (openTasks.Length > 0)
			{
				if (!channel.TryGetCapabilityPKs(task.Factory, out var capabilityPks))
				{
					throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Oddly, we couldn't find the target staff member {0}.", channel.GetChannelName(DisplayNameType.FullName)));
				}
				else if (task.IsOpenOrAssigned)
				{
					if (capability != null && !capabilityPks.Contains(capability.PK))
					{
						return Res.GetString("0f2e4633-eb60-4468-a783-4019b62180a6", "This task requires {0} capability, which {1} does not have. Are you sure you want to assign tasks that have not yet been started to {1}?", capability.G4_Description, channel.GetChannelName(DisplayNameType.FullName));
					}
					else
					{
						return Res.GetString("22dd1f40-1e6e-4548-8768-915e289c291b", "Would you like to assign tasks related to '{0}' that are not yet started to {1}?", task.P9_Description, channel.GetChannelName(DisplayNameType.FullName));
					}
				}
				else
				{
					return Res.GetString("8dd556eb-094f-4633-b91c-42bea0676d6f", "'{0}' is currently in progress with its status being '{1}'. It cannot be assigned to {2}. Would you like to assign tasks related to '{0}' that are not yet started to {2}?", task.P9_Description, task.P9_Status, channel.GetChannelName(DisplayNameType.FullName));
				}
			}
			else
			{
				return Res.GetString("407ecf91-a0c7-48c6-b00e-928b2b6985b7", "'{0}' is currently in progress with its status being '{1}'. Currently, no tasks can be assigned to {2}. ", task.P9_Description, task.P9_Status, channel.GetChannelName(DisplayNameType.FullName));
			}
		}

		protected override void PerformReassignment(ProcessTask task)
		{
			if (task.IsOpenOrAssigned)
			{
				task.P9_GS_NKAssignedStaffMember = channel.ChannelEntityCode;
			}
		}

		protected override bool ShouldReturnTicketToOriginalLocation()
		{
			return Parameters.CardContent.CardType == CardType.Workflow || string.Equals(Parameters.Task.P9_GS_NKAssignedStaffMember, Parameters.DestinationChannel.ChannelEntityCode, StringComparison.CurrentCultureIgnoreCase);
		}

		protected override CrossChannelTaskAssignments GetAllowedReassignmentOptionFlags()
		{
			var assignmentTypes = CrossChannelTaskAssignments.AllTasksInWorkflow
					| CrossChannelTaskAssignments.SelectedTask
					| CrossChannelTaskAssignments.TasksAssignedToCapability;

			if (!Parameters.Task.P9_GS_NKAssignedStaffMember.IsEmpty)
			{
				assignmentTypes |= CrossChannelTaskAssignments.TasksAssignedToStaff;
			}

			return assignmentTypes;
		}

		#endregion

		#region Implementation

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1113:DoNotShowMessageBoxFromBusinessLayer", Justification = "Showing dialogs always happens when gui is present")]
		static bool ResourceIsAllowedToWorkOnCapabilityTask(IVisualBoardChannel channel, IEnumerable<ZGuid> capabilityPKs, ProcessTask task)
		{
			var result = task.P9_G4_RequiredCapability != ZGuid.Empty && !capabilityPKs.Contains(task.P9_G4_RequiredCapability) && WorkflowDataRegistry.Instance.RequireResourceToHaveCapability.Value;
			if (result)
			{
				Globals.Message.Show(Res.GetString("FBC1039C-75F7-4D2E-BC23-11E56DC9CC41", "{0} does not have the capability to work on '{1}' tasks", channel.GetChannelName(DisplayNameType.FullName), task.RequiredCapability.G4_Description)); // Showing dialogs always happens when gui is present
			}

			return !result;
		}

		#endregion
	}
}
