using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Shared;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.GUI
{
	public partial class WorkflowManagementUserControl : ZUserControl, IRequiresWorkflowSecurity
	{
		public WorkflowManagementUserControl()
		{
			InitializeComponent();
			SelectionManager = new GridSelectionManager(WorkflowsGrid, () => ParentForm, false);
		}

		GridSelectionManager SelectionManager { get; }

		public ZGrid WorkflowsGrid
		{
			get { return WorkflowsControl.WorkflowsGrid; }
		}

		#region Life Cycle

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			var jobHeader = dataSource as ProcessJobHeader;

			if (jobHeader != null)
			{
				LoadAllProcessHeadersAtOnce(jobHeader);
				var viewModel = new WorkflowManagementViewModel(jobHeader);
				dataSource = viewModel;
				viewModel.SetDefaultProcessHeaderOnTasksIfRequired();
				SetupRelatedDiagrams();

				CheckWorkflowsHaveParentHeaderPropertySet(viewModel);

				jobHeader.OnDeletingToHandleDuplicateWorkflow += (sender, e) =>
				{
					SetDataBinding(e.ProcessJobHeaderInDatabase, dataMember);
					WorkflowsControl.WorkflowsGrid.ReadOnly = true;

					var duplicateJobLevelNotification = e.ProcessJobHeaderDeleted.ProcessHeaders.Count > 0
						? Res.GetString("73799c70-4907-4ee6-81ed-1e68a4395a94", "Another user has made changes that will override yours. You will need to refresh this form to create new workflows, but you can still make changes to other fields of the job. You can refer to the Job-Level Workflow's Job Notes to see the names of your created workflows that failed to save.")
						: Res.GetString("3e474161-cdd9-4384-b9e6-2b15a01ce2c7", "Another user has made changes that will override yours. You will need to refresh this form to create new workflows, but you can still make changes to other fields of the job.");

					Globals.Message.ShowInformation(duplicateJobLevelNotification);
				};
			}

			base.SetDataBinding(dataSource, dataMember);

			if (dataSource != null)
			{
				UpdateWorkflowNotesTabPageAppearance();
			}
		}

		static void LoadAllProcessHeadersAtOnce(ProcessJobHeader jobHeader)
		{
			var factory = jobHeader.Factory;
			var workflows = factory.Load<ProcessHeader>(jobHeader.GetAllProcessHeadersInJobQuery());
			var processHeaderPKs = workflows.Select(p => p.PK).ToArray();

			ProcessHeaderLink.LoadLinksIntoFactoryOptimisingForUnsavedProcessHeaders(factory, workflows);

			factory.AddFetchHint(TagLinkSchema.Instance, new ZQuery(TagLinkSchema.TGL_ParentId, processHeaderPKs));
			factory.AddFetchHint(StmNoteSchema.Instance, new ZQuery(StmNoteSchema.ST_ParentID, processHeaderPKs));
			factory.AddFetchHint(BMNCNShapeSchema.Instance, new ZQuery(BMNCNShapeSchema.BNS_RelatedEntityID, processHeaderPKs));
		}

		static void CheckWorkflowsHaveParentHeaderPropertySet(WorkflowManagementViewModel viewModel)
		{
			if (viewModel.AllProcessHeaders.Count(x => x.FH_FH_ParentHeader.IsEmpty) > 1) // one is the job-level workflow, which is okay.
			{
				Globals.Message.Show(Res.GetString("e370b738-ad27-41a0-92d2-7d2f00295da4", "At least one workflow does not have its Parent Header property set. Please ensure that the process that created this job's workflows sets all required properties correctly."));
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			this.CheckAndConsumeWorkflowLicenceIfAllowed();
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			if (WorkflowsGrid.ListManager != null)
			{
				WorkflowsGrid.ListManager.ListChanged += SelectionManager.ListChanged;
				WorkflowsGrid.ListManager.ListChanged += WorkflowsGrid_ListChanged;
				WorkflowsGrid.ListManager.CurrentChanged += WorkflowsGrid_CurrentChanged;
				WorkflowsGrid.ListManager.CurrentItemChanged += WorkflowsGrid_CurrentItemChanged;
				WorkflowsGrid.ListManager.PositionChanged += WorkflowsGrid_PositionChanged;
				WorkflowsGrid_CurrentChanged(this, new EventArgs());
			}

			UpdateApprovedScheduleVisibility();
		}

		void SetupRelatedDiagrams()
		{
			if (BMSRegistryGUIHelper.IsPlanningManagementEnabled && WorkflowTasksAndNotesTabControl.TabPages.Count < 3)
			{
				relatedDiagramsTabPage = ObjectFactory.Get<IWorkflowRelatedDiagramsTabPageProvider>().GetTabPage(SelectedWorkflow);
				var relatedDiagramsZTabPage = (ZTabPage)relatedDiagramsTabPage;

				if (relatedDiagramsZTabPage != null)
				{
					relatedDiagramsZTabPage.Name = "RelatedDiagrams";
					relatedDiagramsZTabPage.CaptionResourceString = Res.GetData("F8F9C202-69CE-4CCD-8586-231FB97E86A3", "Related Diagrams");
					relatedDiagramsZTabPage.TabIndex = 2;
					WorkflowTasksAndNotesTabControl.TabPages.Insert(relatedDiagramsZTabPage, 2);
				}
			}
		}

		IWorkflowRelatedDiagramsTabPage relatedDiagramsTabPage;

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing && components != null)
			{
				components.Dispose();
				components = null;
			}
		}

		#endregion

		#region Keeping the correct workflow selected

		ProcessHeader lastSelectedWorkflow;
		readonly ActionSuspender settingLastSelectedWorkflow = new ActionSuspender();

		void WorkflowsGrid_ListChanged(object sender, ListChangedEventArgs e)
		{
			if (e.ListChangedType == ListChangedType.ItemAdded && WorkflowsGrid.ListManager?.List != null && e.NewIndex >= 0 && e.NewIndex < WorkflowsGrid.ListManager.List.Count)
			{
				lastSelectedWorkflow = null;
			}
		}

		bool IsFormSaving()
		{
			var parentForm = ParentForm as ZForm;
			return parentForm != null && parentForm.IsSavingInProgress;
		}

		void WorkflowsGrid_PositionChanged(object sender, EventArgs e)
		{
			if (!settingLastSelectedWorkflow.IsSuspended && !IsFormSaving())
			{
				lastSelectedWorkflow = SelectedWorkflow;
			}
		}

		void WorkflowsGrid_CurrentChanged(object sender, EventArgs e)
		{
			UpdateWorkflowNotesTabPageAppearance();
			UpdateApprovedScheduleVisibility();
			relatedDiagramsTabPage?.UpdateDataBinding(SelectedWorkflow);
		}

		void WorkflowsGrid_CurrentItemChanged(object sender, EventArgs e)
		{
			if (!IsFormSaving() && lastSelectedWorkflow?.PK == SelectedWorkflow?.PK)
			{
				SelectedWorkflow?.RefreshOpenPrerequisiteStatus();
			}
		}

		#endregion

		#region Update Appearance

		void UpdateWorkflowNotesTabPageAppearance()
		{
			WorkflowNotesTabPage.UpdateNoteImage();

			var selectedWorkflow = SelectedWorkflow;
			if (selectedWorkflow != null)
			{
				WorkflowNotesTabPage.Text = selectedWorkflow.IsWorkflow
					? Res.GetString("5ac6e095-e161-47a8-ab95-ab7dd54c5a11", "Workflow Notes")
					: Res.GetString("ec80f757-4bd0-4cf9-ab6b-bdd429feab75", "Job Notes");
			}
		}

		void UpdateApprovedScheduleVisibility()
		{
			var selectedWorkflow = SelectedWorkflow;
			ApprovedShapeDetailsControl.Visible = selectedWorkflow != null && selectedWorkflow.ApprovedShape != null;
		}

		ProcessHeader SelectedWorkflow
		{
			get { return WorkflowsGrid.ListManager != null ? WorkflowsGrid.ListManager.GetCurrent() as ProcessHeader : null; }
		}

		#endregion

		#region NavigateToWorkflowItem

		public void NavigateToWorkflowItem(IProcessTask task)
		{
			Argument.NotNull(task, "task");

			var workflowsGrid = WorkflowsControl.WorkflowsGrid;
			if (workflowsGrid != null && workflowsGrid.ListManager != null)
			{
				var workflow = task.GetProcessHeader();

				for (int i = 0; i < workflowsGrid.ListManager.Count; i++)
				{
					var currentWorkflow = (ProcessHeader)workflowsGrid.List[i];

					if (workflow == null)
					{
						if (!currentWorkflow.IsWorkflow)
						{
							workflowsGrid.ListManager.Position = i;
							break;
						}
					}
					else if (workflow.PK == currentWorkflow.PK)
					{
						workflowsGrid.ListManager.Position = i;
						break;
					}
				}
			}

			var tasksGrid = TaskDetailsControl.TasksGrid;
			if (tasksGrid != null && tasksGrid.ListManager != null)
			{
				for (int i = 0; i < tasksGrid.ListManager.Count; i++)
				{
					var currentTask = (ProcessTask)tasksGrid.List[i];
					if (task.PK == currentTask.PK)
					{
						tasksGrid.ListManager.Position = i;
						break;
					}
				}
			}
		}

		public void NavigateToWorkflowItem(IProcessHeader workflow)
		{
			var workflowsGrid = WorkflowsControl.WorkflowsGrid;

			if (workflowsGrid?.ListManager != null)
			{
				for (var i = 0; i < workflowsGrid.ListManager.Count; i++)
				{
					var currentWorkflow = (BusinessObject)workflowsGrid.List[i];

					if (currentWorkflow.PK == workflow.PK)
					{
						workflowsGrid.ListManager.Position = i;
						break;
					}
				}
			}
		}

		#endregion

		#region For Test
#if DEBUG

		public ZGrid TasksGrid_ForTest => TaskDetailsControl.TasksGrid;

		public void WorkflowsGrid_ListChanged_ForTest(object sender, ListChangedEventArgs e)
		{
			WorkflowsGrid_ListChanged(sender, e);
		}

		public void SetLastSelectedWorkflow_ForTest(ProcessHeader workflow)
		{
			lastSelectedWorkflow = workflow;
		}

#endif
		#endregion

		#region IRequireWorkflowSecurity Members

		string IRequiresWorkflowSecurity.WorkflowItemEditCheckpointCode
		{
			get { return SecurityCore.WorkflowTasksAutoGeneratedCode; }
		}

		string IRequiresWorkflowSecurity.WorkflowItemJustViewCheckpointCode
		{
			get { return SecurityCore.WorkflowTasksJustViewAutoGeneratedCode; }
		}

		ModuleIdentifier IRequiresWorkflowSecurity.WorkflowProviderModuleId
		{
			get { return null; }
		}

		bool IRequiresWorkflowSecurity.ViewOnly { get; set; }

		#endregion
	}
}
