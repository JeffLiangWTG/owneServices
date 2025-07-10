using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.BufferManagement.Business
{
	public delegate void HandleTaskAssignment(ProcessTask task);

	public static class ChannelReassignmentHandler
	{
		internal static CrossChannelTaskAssignments ShowDialog(IProcessTask task, IProcessHeader workflow, IVisualBoardChannel currentChannel, CrossChannelTaskAssignments assignmentFlags, IMultiActionButtonDialogWrapper<CrossChannelTaskAssignments> userNotificationProvider, string message, string caption, HandleTaskAssignment handleTaskAssignment, IVisualBoardChannel newChannel)
		{
			var concreteTask = (ProcessTask)task;
			var concreteWorkflow = (ProcessHeader)workflow;

			var buttons = GetAssignmentButtons(concreteTask, concreteWorkflow, currentChannel, handleTaskAssignment, newChannel)
				.Where(b => assignmentFlags.HasFlag(b.Response) || b.Response == CrossChannelTaskAssignments.None)
				.ToArray();

			if (buttons.Length == 1 && buttons[0].Response == CrossChannelTaskAssignments.None)
			{
				return CrossChannelTaskAssignments.None;
			}
			else
			{
				return userNotificationProvider.ShowDialog(message, caption, buttons);
			}
		}

		public static CrossChannelTaskAssignments GetCrossChannelTaskAssignmentType(ProcessTask task, IVisualBoardChannel currentChannel, BMBoardSectionViewModel sectionViewModel, IMultiActionButtonDialogWrapper<CrossChannelTaskAssignments> userNotificationProvider, string message, HandleTaskAssignment handleTaskAssignment, IVisualBoardChannel newChannel)
		{
			var workflow = (ProcessHeader)task.ProcessHeader;
			var factory = workflow.Factory;
			var staff = GetRelevantResource(factory, currentChannel);

			if (workflow.Tasks.Count(t => t.IsOpen && t.RequiresResourceWithCapability && t.P9_G4_RequiredCapability == task.P9_G4_RequiredCapability) == 1)
			{
				MoveTasks(staff, handleTaskAssignment, newChannel, task);
				return CrossChannelTaskAssignments.SelectedTaskForClaimOrAssign;
			}

			var caption = Res.GetString("4c9af4c6-8472-45dd-85ee-ff48a5d2f353", "Reassign task");
			var buttons = GetCapabilityAssignmentButtons(task, workflow, sectionViewModel, currentChannel, handleTaskAssignment, newChannel).ToArray();

			if (buttons.Length == 1 && buttons[0].Response == CrossChannelTaskAssignments.None)
			{
				return CrossChannelTaskAssignments.None;
			}
			else
			{
				return userNotificationProvider.ShowDialog(message, caption, buttons);
			}
		}

		static IEnumerable<ButtonStripAction<CrossChannelTaskAssignments>> GetAssignmentButtons(ProcessTask task, ProcessHeader workflow, IVisualBoardChannel currentChannel, HandleTaskAssignment handleTaskAssignment, IVisualBoardChannel newChannel)
		{
			var openTasks = workflow.Tasks.Where(t => t.IsOpenOrAssigned).ToArray();
			var factory = workflow.Factory;
			var staff = GetRelevantResource(factory, newChannel);

			if (task.IsOpenOrAssigned)
			{
				yield return new ButtonStripAction<CrossChannelTaskAssignments>
				{
					FireAction = (s, e) =>
					{
						MoveTasks(staff, handleTaskAssignment, newChannel, task);
					},
					Response = CrossChannelTaskAssignments.SelectedTask,
					Text = Res.GetString("b731188b-1efd-482f-b7a8-a7885437b1c9", "Selected Task"),
					ToolTip = ResString.GetMultilingualString("145b0c9d-1d79-46e8-bcd4-1d91c334bb6f", "Move only the selected task into another channel.")
				};
			}

			if (ShouldAddAssignmentButtons(task, openTasks.Count(t => t.P9_GS_NKAssignedStaffMember == task.P9_GS_NKAssignedStaffMember)))
			{
				yield return new ButtonStripAction<CrossChannelTaskAssignments>
				{
					FireAction = (s, e) =>
					{
						MoveTasks(staff, handleTaskAssignment, newChannel, workflow.Tasks.Where(t => string.Equals(t.P9_GS_NKAssignedStaffMember, task.P9_GS_NKAssignedStaffMember, StringComparison.CurrentCultureIgnoreCase)).ToArray());
					},
					HideFormBeforeFireAction = true,
					Response = CrossChannelTaskAssignments.TasksAssignedToStaff,
					Text = Res.GetString("d97a2d8c-1c71-478e-9f33-582e0b971646", "Tasks Assigned to Resource"),
					ToolTip = ResString.GetMultilingualString("555294bc-88b7-4c8a-995b-beb413949742", "Move tasks from this workflow that are not yet started assigned to the resource [{0}] into this channel.", task.StaffName)
				};
			}

			if (ShouldAddAssignmentButtons(task, openTasks.Count(t => t.RequiresResourceWithCapability && t.P9_G4_RequiredCapability == task.P9_G4_RequiredCapability)))
			{
				var capabilityName = task.CapabilityName;

				yield return new ButtonStripAction<CrossChannelTaskAssignments>
				{
					FireAction = (s, e) =>
					{
						MoveTasks(staff, handleTaskAssignment, newChannel, workflow.Tasks.Where(t => t.P9_G4_RequiredCapability == task.P9_G4_RequiredCapability).ToArray());
					},
					Response = CrossChannelTaskAssignments.TasksAssignedToCapability,
					Text = !string.IsNullOrEmpty(capabilityName) ? Res.GetString("32ab8550-aa6a-4f6e-b67c-8e136f56c391", "Tasks Assigned to Capability")
						: Res.GetString("55c05007-0793-4a01-b9bb-af08f87de092", "Tasks with No Capability"),
					ToolTip = string.IsNullOrEmpty(capabilityName) ? ResString.GetMultilingualString("364f39b6-1415-44f5-93bb-97bbd7c7ea4d", "Move all tasks from this workflow assigned to the capability [{0}] into this channel.", capabilityName)
						: ResString.GetMultilingualString("575f6246-1453-4c27-aa51-19597ab802f4", "Move all tasks from this workflow with no capability into this channel.")
				};
			}

			if (ShouldAddAssignmentButtons(task, openTasks.Count(t => t.RequiresResourceWithinGroup && t.P9_GG_AssignedGroup == task.P9_GG_AssignedGroup)))
			{
				var groupName = task.GroupName;

				yield return new ButtonStripAction<CrossChannelTaskAssignments>
				{
					FireAction = (s, e) =>
					{
						MoveTasks(staff, handleTaskAssignment, newChannel, workflow.Tasks.Where(t => t.P9_GG_AssignedGroup == task.P9_GG_AssignedGroup).ToArray());
					},
					Response = CrossChannelTaskAssignments.TasksAssignedToGroup,
					Text = !string.IsNullOrEmpty(groupName) ? Res.GetString("56240c1a-87de-4408-ab1d-f82c1da3b779", "Tasks Assigned to Group")
					: Res.GetString("19735543-4d2c-46ad-9208-1dedc016fbc1", "Tasks with No Group"),
					ToolTip = !string.IsNullOrEmpty(groupName) ? ResString.GetMultilingualString("7c2053b6-367a-4451-bf16-85e0e1f7f1cd", "Move all tasks from this workflow assigned to the group [{0}] into this channel.", groupName)
					: ResString.GetMultilingualString("3109fe52-5719-490b-b3fd-b5cd132e7a1f", "Move all tasks from this workflow with no group into this channel.")
				};
			}

			if (ShouldAddAssignmentButtons(task, openTasks.Length))
			{
				yield return new ButtonStripAction<CrossChannelTaskAssignments>
				{
					FireAction = (s, e) =>
					{
						MoveTasks(staff, handleTaskAssignment, newChannel, workflow.Tasks.ToArray());
					},
					HideFormBeforeFireAction = true,
					Response = CrossChannelTaskAssignments.AllTasksInWorkflow,
					Text = Res.GetString("151fe210-bc80-4dce-a040-798114a599e7", "Entire Workflow"),
					ToolTip = ResString.GetMultilingualString("c645fd2e-9f24-4049-a3f9-4a3a4a41a8dd", "Move all tasks in the workflow [{0}] to the selected channel.", workflow.FH_CompletionStatement)
				};
			}

			yield return new ButtonStripAction<CrossChannelTaskAssignments>
			{
				Response = CrossChannelTaskAssignments.None,
				Text = Res.GetString("a53c309a-5b17-4fca-95ab-c58177c9a554", "Cancel"),
				ToolTip = ResString.GetMultilingualString("8d4f6011-c5e4-456a-b3cd-52e562936c85", "Don't move anything.")
			};
		}

		static IEnumerable<ButtonStripAction<CrossChannelTaskAssignments>> GetCapabilityAssignmentButtons(ProcessTask task, ProcessHeader workflow, BMBoardSectionViewModel sectionViewModel, IVisualBoardChannel currentChannel, HandleTaskAssignment handleTaskAssignment, IVisualBoardChannel newChannel)
		{
			MultilingualString taskToolTip, capabilityToolTip;
			var factory = workflow.Factory;
			var staff = GetRelevantResource(factory, newChannel);
			var section = factory.Load<BMBoardSection>(sectionViewModel.SectionPK);
			var isCurrentUser = staff.PK == GlbStaff.CurrentUser.PK;
			var actionDescriptionTask = isCurrentUser ?
				Res.GetString("D8DD2FE5-11A6-4C80-990B-8504C88AA1C9", "Claim this task together with all tasks bound by task auto assignment restrictions of the SAM type") :
				Res.GetString("543B0022-F130-42EF-8711-FDEB49E3AB7A", "Assign this task together with all tasks bound by task auto assignment restrictions of the SAM type");
			var capabilityName = task.CapabilityName;
			var allCapabilityTasks = workflow.Tasks.Where(t => t.P9_G4_RequiredCapability == task.P9_G4_RequiredCapability && t.IsOpenOrAssigned).ToArray();
			var actionDescriptionCapability = isCurrentUser ?
				Res.GetString("EE4EDA12-86F3-467A-A3F7-AB6A8158ACDB", "Claim all tasks requiring the {0} capability together with all tasks bound by task auto assignment restrictions of the SAM type", capabilityName) :
				Res.GetString("D01ECA5B-E2CA-4D83-9425-B92FE164CB5A", "Assign all tasks requiring the {0} capability together with all tasks bound by task auto assignment restrictions of the SAM type", capabilityName);

			if (!BMSRegistry.Instance.DisableCapacityCalculations.Value)
			{
				var capacityBreakdown = CapacityCalculator.GetUtilisedCapacityBreakdown(staff, section.Component);
				var staffCapacity = string.Format("{0} {1}", capacityBreakdown.AvailableCapacity.FormatWithNoMoreThanTwoDecimalPlaces(), TimeConstants.TimeStrings.Hours);
				var taskAndRelatedDurationInHours = TaskAssignmentHelper.GetCapacityRequiredByTasksRespectingSAMRestrictions(task.WrapWithEnumerable()) / 60;
				var taskAndRelatedDurationStr = string.Format("{0} {1}", taskAndRelatedDurationInHours.FormatWithNoMoreThanTwoDecimalPlaces(), TimeConstants.TimeStrings.Hours);
				taskToolTip = ResString.GetMultilingualString("a49cf091-1195-4a6f-9058-c645013955e3", @"{0}. This will consume {1} of capacity.
Resource [{2}] has {3} of available capacity in this buffer. Move this selected task into this channel.", actionDescriptionTask, taskAndRelatedDurationStr, staff.GS_FullName, staffCapacity);
				var allCapabilityTasksAndRelatedDurationInHours = TaskAssignmentHelper.GetCapacityRequiredByTasksRespectingSAMRestrictions(allCapabilityTasks) / 60;
				var allCapabilityAndRelatedTaskDurationStr = string.Format("{0} {1}", allCapabilityTasksAndRelatedDurationInHours.FormatWithNoMoreThanTwoDecimalPlaces(), TimeConstants.TimeStrings.Hours);
				capabilityToolTip = ResString.GetMultilingualString("799a64c7-386c-4a6d-a28e-ca137f14cbc8", @"{0}. This will consume {1} of capacity.
Resource [{2}] has {3} of available capacity in this buffer. Move all tasks from this workflow assigned to the capability [{4}] that are not yet started into this channel", actionDescriptionCapability, allCapabilityAndRelatedTaskDurationStr, staff.GS_FullName, staffCapacity, capabilityName);
			}
			else
			{
				taskToolTip = ResString.GetMultilingualString("56d133da-24ef-4b04-a250-f354e5bb913d", @"{0}. Move this selected task into this channel.", actionDescriptionTask);
				capabilityToolTip = ResString.GetMultilingualString("48849e0b-8d29-4842-b6a8-31c75681c2a9", @"{0}. Move all tasks from this workflow assigned to the capability [{1}] that are not yet started into this channel", actionDescriptionCapability, capabilityName);
			}

			yield return new ButtonStripAction<CrossChannelTaskAssignments>
			{
				FireAction = (s, e) =>
				{
					MoveTasks(staff, handleTaskAssignment, newChannel, task);
				},
				Response = CrossChannelTaskAssignments.SelectedTaskForClaimOrAssign,
				Text = isCurrentUser ? Res.GetString("70276628-48fd-4b67-bd8b-9ad883addd00", "Claim this task") : Res.GetString("099b109b-0811-4d9e-b134-399fcffe38d3", "Assign this task"),
				ToolTip = taskToolTip
			};

			yield return new ButtonStripAction<CrossChannelTaskAssignments>
			{
				FireAction = (s, e) =>
				{
					MoveTasks(staff, handleTaskAssignment, newChannel, allCapabilityTasks);
				},
				Response = CrossChannelTaskAssignments.AllTasksInWorkfloWithForClaimOrAssign,
				Text = isCurrentUser ? Res.GetString("eb1dfb2b-d6b6-4755-9585-8bf4f0d207dd", "Claim tasks requiring {0} capability", capabilityName)
				: Res.GetString("32f558a2-5872-4ffe-8a21-347b7ffa7e42", "Assign tasks requiring {0} capability", capabilityName),
				ToolTip = capabilityToolTip
			};

			yield return new ButtonStripAction<CrossChannelTaskAssignments>
			{
				Response = CrossChannelTaskAssignments.None,
				Text = Res.GetString("90298eef-e8af-453d-9c3a-96879d52d7d1", "Cancel"),
				ToolTip = ResString.GetMultilingualString("5fe9223f-9316-4d00-a7e9-7f47d446b69c", "Don't move anything.")
			};
		}

		static void MoveTasks(GlbStaff staff, HandleTaskAssignment handleTaskAssignment, IVisualBoardChannel newChannel, params ProcessTask[] tasks)
		{
			if (newChannel?.EntityType == ChannelTypeList.Codes.Resource && WorkflowDataRegistry.Instance.RequireResourceToHaveCapability.Value)
			{
				tasks = CheckResourceCapability(staff, tasks);
			}

			tasks.ForEach(t => handleTaskAssignment(t));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1113:DoNotShowMessageBoxFromBusinessLayer", Justification = "Showing dialogs always happens when gui is present")]
		static ProcessTask[] CheckResourceCapability(GlbStaff staff, ProcessTask[] tasks)
		{
			var tasksThatResourceDoesntHaveCapabilityFor = new List<ProcessTask>();

			foreach (var task in tasks)
			{
				if (task.P9_G4_RequiredCapability != ZGuid.Empty && !staff.Capabilities.Contains(task.RequiredCapability))
				{
					tasksThatResourceDoesntHaveCapabilityFor.Add(task);
				}
			}

			if (tasksThatResourceDoesntHaveCapabilityFor.Count != 0)
			{
				var sb = new StringBuilder(Res.GetString("3421467a-c2e5-4c73-9e76-b5ab67e8b7b3", "{0} does not have the capability to do the following tasks, and therefore they have not been reassigned:", staff.GS_FullName));
				sb.AppendLine();
				tasksThatResourceDoesntHaveCapabilityFor.ForEach(t => sb.AppendLine(t.HumanReadableName));
				Globals.Message.Show(sb.ToString().Trim()); // Showing dialogs always happens when gui is present
			}

			return tasks.Except(tasksThatResourceDoesntHaveCapabilityFor).ToArray();
		}

		static GlbStaff GetRelevantResource(BusinessObjectFactory factory, IVisualBoardChannel currentChannel)
		{
			return (currentChannel?.EntityType == ChannelTypeList.Codes.Resource ? factory.Load<GlbStaff>(currentChannel.EntityPK) : null) ?? GlbStaff.CurrentUser;
		}

		static bool ShouldAddAssignmentButtons(ProcessTask task, int count)
		{
			return count > 0 && !(task.IsOpenOrAssigned && count == 1);
		}
	}
}
