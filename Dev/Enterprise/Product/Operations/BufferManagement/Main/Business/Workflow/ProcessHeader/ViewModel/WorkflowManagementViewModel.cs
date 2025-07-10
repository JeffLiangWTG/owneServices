using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.PAVE.Common.Interfaces;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class WorkflowManagementViewModel : NonPersistentBusinessObject, IObsoleteValidation
	{
		public WorkflowManagementViewModel(ProcessJobHeader jobHeader)
			: base(jobHeader.Factory)
		{
			this.jobHeader = jobHeader;
		}

		#region Related Business Objects

		public ProcessJobHeader JobHeader
		{
			get { return jobHeader; }
			protected set
			{
				jobHeader = value;
				allProcessHeaders = null;
			}
		}

		ProcessJobHeader jobHeader;

		[ChildEditable]
		public ProcessHeaderCollection AllProcessHeaders
		{
			get
			{
				if (allProcessHeaders == null)
				{
					allProcessHeaders = ProcessHeaderCollection.GetCollectionForJobIncludingJobLevelWorkflow(jobHeader.Parent, jobHeader);
					allProcessHeaders.ApplySort((NoResString)"Sequence", ListSortDirection.Ascending); // Property name
					RegisterEditableChildObject(allProcessHeaders);
				}

				return allProcessHeaders;
			}
		}

		ProcessHeaderCollection allProcessHeaders;

		public virtual IEnumerable<ProcessHeader> Workflows
		{
			get { return jobHeader.ProcessHeaders; }
		}

		#endregion

		#region Delete

#if DEBUG
		public static bool TryDelete_ForTest(params ProcessHeader[] processHeaders)
		{
			return TryDelete(null, processHeaders);
		}
#endif

		public static bool TryDelete(IMultiActionButtonDialogWrapper<DeleteWorkflowOption> deleteWorkflowDialogWrapper, params ProcessHeader[] processHeaders)
		{
			var canDeleteResult = CanDeleteEntities(processHeaders, deleteWorkflowDialogWrapper);

			if (canDeleteResult.Result)
			{
				DeleteEntities(canDeleteResult.WorkflowsToDelete);

				return true;
			}

			return false;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1113:Do Not Show Message Box From Business Layer", Justification = "This is a view model")]
		public static CanDeleteWorkflowsResult CanDeleteEntities(ProcessHeader[] processHeaders, IMultiActionButtonDialogWrapper<DeleteWorkflowOption> deleteWorkflowDialogWrapper)
		{
			processHeaders = processHeaders.WhereNotNull().ToArray();

			if (processHeaders.OfType<ProcessJobHeader>().Any())
			{
				Globals.Message.ShowInformation(Res.GetString("603feeda-b5bd-4f6c-8636-4f0cdd049893", "The Job-level workflow cannot be deleted."), CannotDeleteCaption); // This is a view model

				return CanDeleteWorkflowsResult.CannotDelete;
			}
			else
			{
				if (processHeaders.Length > 0)
				{
					var workflowsWithApprovedSchedules = new List<ProcessHeader>();
					var workflowsWithFormFlowTypeTasks = new List<ProcessHeader>();
					foreach (var processHeader in processHeaders)
					{
						if (processHeader.DirectlyApprovedShape != null)  // directly approved shape
						{
							workflowsWithApprovedSchedules.Add(processHeader);
						}

						if (!processHeader.IsTemplate
							&& processHeader.SupportsFormFlowTypeTasks
							&& processHeader.Tasks.Any(t => !t.P9_FormFlowType.IsEmpty))
						{
							workflowsWithFormFlowTypeTasks.Add(processHeader);
						}
					}

					if (workflowsWithApprovedSchedules.Count != 0)
					{
						var message = Res.GetString("e7f59cb9-aff9-44f0-b2bc-c123db888545", "The following workflows are part of an approved project plan and cannot be deleted:")
							+ System.Environment.NewLine + "\t"
							+ string.Join(System.Environment.NewLine + "\t", workflowsWithApprovedSchedules.Select(w => w.FH_CompletionStatement));

						Globals.Message.Show(message, CannotDeleteCaption, ZMessageBoxButtons.OK, ZDialogResult.OK); // This is a viewmodel

						return CanDeleteWorkflowsResult.CannotDelete;
					}

					if (workflowsWithFormFlowTypeTasks.Count != 0)
					{
						var message = Res.GetString("5ba3ef08-156e-4b2b-a72e-2f9278fd5d48", "The following workflow(s) have a system maintained task linked to jobs and cannot be deleted:")
							+ System.Environment.NewLine + "\t"
							+ string.Join(System.Environment.NewLine + "\t", workflowsWithFormFlowTypeTasks.Select(w => w.FH_CompletionStatement));

						Globals.Message.Show(message, CannotDeleteCaption, ZMessageBoxButtons.OK, ZDialogResult.OK); // This is a viewmodel

						return CanDeleteWorkflowsResult.CannotDelete;
					}

					var shouldDelete = true;

					processHeaders = GetWorkflowsToDelete(processHeaders);
					if (processHeaders.SelectMany(w => w.Tasks).Any())
					{
						var message = Res.GetString("ce4be320-1054-4433-a7a0-9b9545d860d0", "Delete workflow '{0}' and all associated tasks?", string.Join("', '", processHeaders.Select(h => h.FH_CompletionStatement)));
						shouldDelete = Globals.Message.Show(message, ConfirmDeleteCaption, ZMessageBoxButtons.OKCancel, ZDialogResult.OK) == ZDialogResult.OK; // This is a viewmodel
					}

					if (shouldDelete)
					{
						if (deleteWorkflowDialogWrapper != null)
						{
							var workflowsAndLinkedShapeOptions = processHeaders.Select(w => new { Workflow = w, Option = DeleteWorkflowDialog.GetDeleteOption(w, deleteWorkflowDialogWrapper) }).ToArray();

							if (workflowsAndLinkedShapeOptions.Any(x => x.Option == DeleteWorkflowOption.CancelDelete || x.Option == DeleteWorkflowOption.OpenDiagramsAndCancelDelete))
							{
								shouldDelete = false;
							}
							else
							{
								foreach (var x in workflowsAndLinkedShapeOptions)
								{
									x.Workflow.UpdateLinkedShapesAndArrows(x.Option);
								}
							}
						}

						foreach (var processHeader in processHeaders)
						{
							processHeader.Factory.AddFetchHint(ProcessTaskIterationLinkSchema.P9I_FH_IterationWorkflow, processHeader.PK);
						}
					}

					if (shouldDelete)
					{
						return CanDeleteWorkflowsResult.CanDelete(processHeaders);
					}
				}
			}

			return CanDeleteWorkflowsResult.CannotDelete;
		}

		public class CanDeleteWorkflowsResult
		{
			internal static CanDeleteWorkflowsResult CannotDelete => new CanDeleteWorkflowsResult();

			internal static CanDeleteWorkflowsResult CanDelete(ICollection<ProcessHeader> workflowsToDelete) => new CanDeleteWorkflowsResult(workflowsToDelete);

			CanDeleteWorkflowsResult(ICollection<ProcessHeader> workflowsToDelete = null)
			{
				WorkflowsToDelete = workflowsToDelete;
			}

			public bool Result => WorkflowsToDelete != null;

			public ICollection<ProcessHeader> WorkflowsToDelete { get; }
		}

		public static void DeleteEntities(ICollection<ProcessHeader> processHeaders)
		{
			foreach (var workflow in processHeaders.WhereNotNull().ToArray())
			{
				workflow.Delete();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1113:Do Not Show Message Box From Business Layer", Justification = "This is a view model")]
		static ProcessHeader[] GetWorkflowsToDelete(ProcessHeader[] workflows)
		{
			var strategy = new ProcessHeaderDescendantsStrategy();
			var descendantWorkflowGroups =
				(from w in workflows
				 let descendants = w.Descendants(strategy).Cast<ProcessHeader>().ToArray()
				 where descendants.Length > 0
				 select Tuple.Create(w, descendants)).ToArray();

			if (descendantWorkflowGroups.Length > 0)
			{
				var message = GetDescendantWorkflowsMessage(descendantWorkflowGroups);
				var shouldDeleteDescendants = Globals.Message.Show(message, ConfirmDeleteCaption, ZMessageBoxButtons.YesNo, ZDialogResult.Yes) == ZDialogResult.Yes; // This is a viewmodel

				if (shouldDeleteDescendants)
				{
					return workflows.Concat(descendantWorkflowGroups.SelectMany(x => x.Item2)).ToArray();
				}
			}

			return workflows;
		}

		static string GetDescendantWorkflowsMessage(IEnumerable<Tuple<ProcessHeader, ProcessHeader[]>> descendantWorkflowGroups)
		{
			var message = new StringBuilder(Res.GetString("48cb13b0-57d9-4b4d-9282-d6435b2f7d11", "The following workflows have descendant workflows. Would you like to delete the descendants also?", System.Environment.NewLine));

			foreach (var tuple in descendantWorkflowGroups)
			{
				message.AppendLine();
				message.AppendFormat(CultureInfo.InvariantCulture, "{0}:", tuple.Item1.FH_CompletionStatement);
				message.AppendLine();
				message.Append("\t");
				message.Append(BMConstants.BulletPointCharacter);
				message.Append(" ");
				message.Append(string.Join("\r\n\t" + BMConstants.BulletPointCharacter + " ", tuple.Item2.Select(d => d.FH_CompletionStatement)));
			}

			return message.ToString();
		}

		static string CannotDeleteCaption
		{
			get { return Res.GetString("874a7765-ae26-4841-92d6-2a0005dcf58c", "Cannot Delete"); }
		}

		static string ConfirmDeleteCaption
		{
			get { return Res.GetString("fcad8f57-66c5-451a-b8ef-313f87ce13a5", "Confirm Delete"); }
		}

		#endregion

		#region Tasks

		public void SetDefaultProcessHeaderOnTasksIfRequired()
		{
			var parent = jobHeader.Parent;

			if (parent == null)
			{
				return;
			}

			var defaultProcessHeaderPk = ZGuid.Empty;
			var tasksWithoutWorkflows = parent.WorkflowItems.Tasks.Cast<ProcessTask>().Where(x => !x.P9_FH_ProcessHeader.IsValid);

			foreach (var task in tasksWithoutWorkflows)
			{
				if (defaultProcessHeaderPk.IsEmpty)
				{
					defaultProcessHeaderPk = MasterFiles.Business.ProcessJobHeaderProvider.GetDefaultWorkflowForTask(task)?.PK ?? ZGuid.Empty;
				}

				using (task.SuspendSettingHasChanges())
				{
					task.P9_FH_ProcessHeader = defaultProcessHeaderPk;
				}
			}
		}

		#endregion
	}
}
