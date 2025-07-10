using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CargoWise.Common;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.PAVE.Common.Interfaces;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.BufferManagement.Business
{
	public static class IProcessHeaderExtensions
	{
		#region ProcessHeader

		[DebuggerStepThrough]
		public static BMSystem GetBMSystem(this IProcessHeader processHeader)
		{
			return ((ProcessHeader)processHeader).BMSystem;
		}

		[DebuggerStepThrough]
		public static bool HasOpenPrerequisites(this IProcessHeader processHeader)
		{
			return ((ProcessHeader)processHeader).HasOpenPrerequisites;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1113:Do Not Show Message Box From Business Layer", Justification = "This is a view model, there is going to be a GUI attached")]
		static ZDialogResult ShowConfirmationMessage(IProcessHeader[] workflows, BMComponent destinationComponent, string jobDescription = null)
		{
			var numberOfWorklowsMessage = workflows.Length > 1
				? (string.IsNullOrEmpty(jobDescription) ? Res.GetString("D572C36E-A605-4146-A7CC-B4F0489BDB9A", "{0} workflows", workflows.Length) : Res.GetString("b39a8a8a-61c0-449a-8782-66525d565bab", "{0} workflows in job [{1}]", workflows.Length, jobDescription))
				: Res.GetString("B8F69502-79BD-4974-8D57-D9F09A29C594", "workflow {0} from component [{1}]", workflows[0].Description, workflows[0].CurrentComponent?.FC_Name);

			var message = Res.GetString("553A5E59-E712-494F-B8B5-0E3BF8DF32C8", "Are you sure to move {0} to component [{1}]? Note that based on transfer rules, your workflows may move back to their original components.", numberOfWorklowsMessage, destinationComponent.FC_Name);
			return Globals.Message.Show(message, Res.GetString("CB7FDCC2-94C3-41C4-AA0B-03F43E174C22", "Move workflow"), ZMessageBoxButtons.YesNo, ZMessageBoxIcon.Question, ZDialogResult.Yes); // This is a view model, there is going to be a GUI attached
		}

		public static int GroupMoveToComponent(IProcessHeader[] workflows, BMComponent destinationComponent, string jobDescription = null)
		{
			var showConfirmationNow = workflows.Length > 1;
			var result = ZDialogResult.None;
			var movedCount = 0;
			var listOfTemplateWorkflows = new ZStringBuilder();

			if (showConfirmationNow)
			{
				result = ShowConfirmationMessage(workflows, destinationComponent, jobDescription);
			}

			if (result == ZDialogResult.Yes || !showConfirmationNow)
			{
				foreach (var workflow in workflows)
				{
					if (!workflow.FH_P0_Template.IsValid)
					{
						if (workflow.MoveToComponent(destinationComponent, !showConfirmationNow))
						{
							movedCount++;
						}
					}
					else
					{
						listOfTemplateWorkflows.AppendLine(workflow.Description);
					}
				}
			}

			if (!listOfTemplateWorkflows.IsEmpty)
			{
				ShowGroupTemplatesWorkflowError(listOfTemplateWorkflows.ToString());
			}

			return movedCount;
		}

		public static bool MoveToComponent(this IProcessHeader workflow, BMComponent destinationComponent, bool showConfirmationAndErrors = true)
		{
			if (!workflow.FH_P0_Template.IsValid)
			{
				if (workflow.FH_FC_CurrentComponent != destinationComponent.PK)
				{
					if (Env.Security.WorkflowTasksCurrentBufferManagementComponent.IsAllowed)
					{
						var result = ZDialogResult.None;
						if (showConfirmationAndErrors)
						{
							result = ShowConfirmationMessage(new[] { workflow }, destinationComponent);
						}

						if (result == ZDialogResult.Yes || !showConfirmationAndErrors)
						{
							workflow.FH_FC_CurrentComponent = destinationComponent.PK;

							return true;
						}
					}
					else
					{
						Env.Security.WorkflowTasksCurrentBufferManagementComponent.ShowError();
					}
				}
				else if (showConfirmationAndErrors)
				{
					ShowSameComponentError();
				}
			}
			else if (showConfirmationAndErrors)
			{
				ShowTemplateWorkflowError(workflow);
			}

			return false;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1113:Do Not Show Message Box From Business Layer", Justification = "This is a view model, there is going to be a GUI attached")]
		public static Tuple<int, IProcessHeader[]> MoveJobToComponent(this IProcessJobHeader jobHeader, ZGuid[] fromComponentPKs, BMComponent toComponent)
		{
			var movedCount = 0;
			var workflowsToMove = jobHeader.ProcessHeaders.Cast<IProcessHeader>().Where(x => fromComponentPKs.Contains(x.FH_FC_CurrentComponent)).ToArray();

			if (workflowsToMove.Any())
			{
				workflowsToMove = workflowsToMove.Where(x => x.FH_FC_CurrentComponent != toComponent.PK).ToArray();

				if (!workflowsToMove.Any())
				{
					ShowSameComponentError();
				}
				else
				{
					movedCount = GroupMoveToComponent(workflowsToMove, toComponent, jobHeader.Description);
				}
			}
			else
			{
				Globals.Message.Show(Res.GetString("dd2de38d-a5cb-4a47-8bef-e992312844cd", "[{0}] has no workflows in [{1}] to move.")); // This is a view model, there is going to be a GUI attached
			}

			return Tuple.Create(movedCount, workflowsToMove);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1113:Do Not Show Message Box From Business Layer", Justification = "This is a view model, there is going to be a GUI attached")]
		static void ShowSameComponentError()
		{
			Globals.Message.ShowError(Res.GetString("0fa94361-62bb-446c-9e8d-a84c2d90559e", "The destination component cannot be the same as the source component.")); // This is a view model, there is going to be a GUI attached
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1113:Do Not Show Message Box From Business Layer", Justification = "This is a view model, there is going to be a GUI attached")]
		static void ShowTemplateWorkflowError(IProcessHeader workflow)
		{
			Globals.Message.ShowError( // This is a view model, there is going to be a GUI attached
				Res.GetString("362531b5-f6ec-48cd-b933-e07ecfcef718", @"The following workflow was not able to be moved because Template workflows cannot be moved into a Buffer Management System Component:

{0}", workflow.Description)); // This is a view model, there is going to be a GUI attached
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1113:Do Not Show Message Box From Business Layer", Justification = "This is a view model, there is going to be a GUI attached")]
		static void ShowGroupTemplatesWorkflowError(string listOfTemplateWorkflows)
		{
			Globals.Message.ShowError( // This is a view model, there is going to be a GUI attached
				Res.GetString("{80c78531-303a-462c-8a84-8eaf545ce8d7}", @"The following workflows were not able to be moved because Template workflows cannot be moved into a Buffer Management System Component:

{0}", listOfTemplateWorkflows));
		}

		#endregion

		#region Network

		public static string GetNetworkStatusDescription(this ProcessHeader entity)
		{
			var status = GetStatusDescription(((IProposedNetworkEntity)entity).Status);
			return string.IsNullOrEmpty(status) ? string.Empty : Res.GetString("976c8ff2-3e10-4ae1-8b94-679f708d492e", "This {0} {1}", entity.ProcessHeaderType, status);
		}

		public static string GetNetworkStatusDescription(this IProposedNetworkEntity entity)
		{
			var status = GetStatusDescription(entity.Status);
			return string.IsNullOrEmpty(status) ? string.Empty : Res.GetString("812e9ce4-45e6-4c04-9b42-8443eec48bed", "This entity {1}", status);
		}

		static string GetStatusDescription(WorkStatus status)
		{
			switch (status)
			{
				case WorkStatus.Startable:
					return Res.GetString("7875270a-2c9a-4fe1-873d-f9261c6ed471", "is ready to start");

				case WorkStatus.Suspended:
					return Res.GetString("e3f68133-8bc3-4efc-b5ec-510490e4dea9", "has one or more suspended tasks");

				case WorkStatus.Working:
					return Res.GetString("058f5bf9-2af9-47f1-92b0-b356bbee57a5", "has one or more working tasks");

				case WorkStatus.Complete:
					return Res.GetString("16559eb3-82e6-46bf-b743-02bf7d5839a7", "is complete");

				case WorkStatus.Cancelled:
					return Res.GetString("861475dd-047a-40a9-a413-60611e755629", "has been canceled");

				default:
					return null;
			}
		}

		public static string GetStatusName(this IProposedNetworkEntity entity)
		{
			var status = entity.Status;
			return status == WorkStatus.Blocked
				? Res.GetString("bc73f951-3949-47bb-b175-1b5c4b27a304", "Has incomplete pre-requisites")
				: status.ToString();
		}

		#endregion
	}

	public static class ProcessTaskTemplateExtensions
	{
		public static ProcessJobHeader GetJobHeader(this ProcessTaskTemplate template)
		{
			var jobHeaders = template.ProcessHeaders.OfType<ProcessJobHeader>().ToArray();

			if (jobHeaders.Length > 1)
			{
				ReportMultipleJobLevelWorkflows(template, jobHeaders);

				var jobHeadersWithMostWorkflows = jobHeaders.CollectMaxBy(x => x.ProcessHeaders.Count);

				return jobHeadersWithMostWorkflows
					.OrderBy(x => x.FH_SystemCreateTimeUtc)
					.ThenBy(x => x.FH_CompletionStatement)
					.ThenBy(x => x.PK)
					.First();
			}

			return jobHeaders.SingleOrDefault();
		}

		#region SuppressResourceStringsCheckRegion

		static void ReportMultipleJobLevelWorkflows(ProcessTaskTemplate template, ProcessJobHeader[] jobHeaders)
		{
			var criteriaCodes = new[] { template.P0_SubType1, template.P0_SubType2, template.P0_SubType3, template.P0_SubType4, template.P0_SubType5 }.Where(x => !string.IsNullOrEmpty(x));
			var criteriaCodesString = string.Join(", ", criteriaCodes);
			var message = new ZStringBuilder(FormattableString.Invariant($@"Tried to apply a workflow template that has multiple job-level workflows. Each template may only have one. SAD!

Template details:
Name: {template.P0_Name}
Workflow Type: {template.P0_ProcessType}
Selection Criteria: {criteriaCodesString}
System: {template.P0_IsSystem}
Partial: {template.P0_IsPartialTemplate}
Universal: {template.P0_IsUniversal}"));

			foreach (var jobHeader in jobHeaders.OrderBy(x => x.FH_SystemCreateTimeUtc))
			{
				message.Append(FormattableString.Invariant($@"

Job-Level Workflow: {jobHeader.FH_CompletionStatement}
Created: {jobHeader.FH_SystemCreateTimeUtc.ToBestReadableDateTimeString()}
Created by: {jobHeader.FH_SystemCreateUser}
Workflows for this Job-Level Workflow:"));

				var workflows = jobHeader.ProcessHeaders.ToArray();

				if (workflows.Any())
				{
					for (var i = 0; i < workflows.Length; i++)
					{
						var workflow = workflows[i];
						var tasks = workflow.GetTasksWithoutAccessingWorkflowParent().Count;
						var taskWord = tasks == 1 ? "task" : "tasks";
						message.Append(FormattableString.Invariant($@"
{i + 1}. {workflow.FH_CompletionStatement} ({tasks} {taskWord})"));
					}
				}
				else
				{
					message.AppendLine("None");
				}
			}

			ErrorReporter.ReportOnce("TemplateAppliedWithMultipleJobLevelWorkflows", message.ToString());
		}

		#endregion

		public static IEnumerable<ProcessHeader> GetWorkflows(this ProcessTaskTemplate template)
		{
			return template.ProcessHeaders.Cast<ProcessHeader>().Where(w => w.IsWorkflow);
		}
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("ApiDesign", "RS0030:Do not used banned APIs", Justification = "Baseline")]
	[System.Runtime.InteropServices.Guid("4B6FC556-0E0A-4121-ACD0-B6F705FD8C14")]
	public static class GlbStaffExtensions
	{
		#region CCR

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public static bool IsOverCapacityConstrainedThreshold(this BMComponent component, decimal standardEstimateMinutes)
		{
			return component.IsBuffer && standardEstimateMinutes >= component.CCRThresholdMinutes;
		}

		public static IEnumerable<BMComponent> GetFeedingComponents(this BMComponent buffer)
		{
			return buffer.GetFeedingComponentLinks().Select(l => l.ComponentFrom);
		}

		public static IEnumerable<BMComponentLink> GetFeedingComponentLinks(this BMComponent buffer)
		{
			return buffer.FromOthersToMeLinks.Where(l => l.FL_IsReleaseGateRuleApplied);
		}

		public static bool IsCapacityConstrainedCandidate(this GlbStaff staff, BMComponent component)
		{
			if (component.IsBuffer)
			{
				var componentLink = component.GetResourceLink(staff.GS_Code);
				return componentLink != null
					&& componentLink.FD_CapacityConstraintDetectedUtc.IsValid
					&& componentLink.FD_IsPersistentlyOverloaded;
			}

			return false;
		}

		public static void DesignateAsCCR(this GlbStaff staff, BMComponent component)
		{
			MarkCCRStatus(staff, component, true);
		}

		public static void DesignateAsNonCCR(this GlbStaff staff, BMComponent component)
		{
			MarkCCRStatus(staff, component, false);
		}

		static void MarkCCRStatus(GlbStaff staff, BMComponent component, bool isCCR)
		{
			if (staff.GS_Code.IsEmpty)
			{
				throw new InvalidOperationException("It is invalid to provide a resource with no code since the code is needed for a natural key");
			}

			var componentLink = component.GetOrCreateResourceLink(staff.GS_Code);
			componentLink.FD_IsCapacityConstrained = isCCR;
			componentLink.FD_GS_NKDesignatedAsCapacityConstrainedBy = isCCR ? GlbStaff.CurrentUser.GS_Code : ZString.Empty;
		}

		#endregion
	}

	public enum CCRConstraintFilter
	{
		None = 0,
		Preconstraint,
		Postconstraint,
	}

	public enum CCRFilter
	{
		None = 0,
		CCR,
		NonCCR,
		InCCRWorkflow,
	}

	public enum CCRCandidacy
	{
		None = 0,
		Candidate,
		Designate,
	}
}
