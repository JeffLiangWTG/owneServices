using System;
using System.Windows.Forms;
#if WINZOR
using CargoWise.Application;
#endif
using CargoWise.EntityFramework;
#if WINZOR
using CargoWise.NetworkVisualisation.GUI;
#endif
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
#if WINZOR
using Enterprise.BufferManagement.NetworkVisualisation.Business;
#endif
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.GUI
{
	public partial class WorkflowDetailsUserControl : ZUserControl, IWorkflowDetailsUserControl
	{
		public WorkflowDetailsUserControl()
		{
			InitializeComponent();
#if WINZOR
			if (DesignModeFinder.IsDesigning || BMSRegistryGUIHelper.IsPlanningManagementEnabled)
			{
				InitializeWorkflowRelationshipDesignerButton();
			}
#endif
			HideSequencingGroupBoxIfModuleNotEnabled();
			HideResponsiveReleaseGateControlsIfDisabledInRegistry();
		}

		void HideSequencingGroupBoxIfModuleNotEnabled()
		{
			if (!DesignModeFinder.IsDesigning)
			{
				if (!BMSRegistry.Instance.ReleaseSequencesModuleEnabled.Value)
				{
					SequencingGroupBox.Visible = false;
				}
			}
		}

		void HideResponsiveReleaseGateControlsIfDisabledInRegistry()
		{
			if (!DesignModeFinder.IsDesigning)
			{
				if (!BMSRegistry.Instance.DisplayResponsiveReleaseGateUiSettings.Value)
				{
					NewReleaseGateGroupBox.Visible = false;
				}
			}
		}

		ProcessHeader SelectedWorkflow => BindingSource.Current as ProcessHeader;

		#region ZUserControl Overrides

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			SetComponentDropEditWidth();
		}

		protected override void OnBindingContextChanged(EventArgs e)
		{
			base.OnBindingContextChanged(e);
			WorkflowRelationshipsButton.Enabled = SelectedWorkflow != null;
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			WorkflowRelationshipsButton.Enabled = SelectedWorkflow != null;
			var isSelectedWorkflowReadOnly = SelectedWorkflow != null && (!SelectedWorkflow.IsWorkflow || !SelectedWorkflow.HasSecurityRightsToChangeCurrentComponent);
			SystemNameFindBox.ReadOnly = isSelectedWorkflowReadOnly;
			ComponentDropEdit.ReadOnly = isSelectedWorkflowReadOnly;
			OpenSequenceButton.Enabled = SelectedWorkflow?.HighestReleaseSequence != null;
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);

			SetComponentDropEditWidth();
		}

		void SetComponentDropEditWidth()
		{
			((ZDropEditInternals)ComponentDropEdit).SetControlWidth(ReleaseGroupFindBox.Width);
		}

		#endregion

		#region TransferFailure

		void TransferFailureButton_Click(object sender, EventArgs e)
		{
			var workflow = SelectedWorkflow;
			if (workflow != null)
			{
				WorkflowTransferDiagnosisForm.Show(workflow);
			}
		}

		#endregion

		#region Workflow Relationships

		void WorkflowRelationshipsButton_Click(object sender, EventArgs e)
		{
			OpenWorkflowRelationshipsForm();
		}

		void OpenWorkflowRelationshipsForm()
		{
			bool showForm = false;

			var currentWorkflow = SelectedWorkflow;
			var parentJob = currentWorkflow != null ? currentWorkflow.Parent as BusinessObject : null;

			if (parentJob != null && (!parentJob.IsInDatabase || parentJob.HasChanges))
			{
				if (parentJob.ReadOnly)
				{
					Globals.Message.Show(Res.GetString("10b8e249-8058-4e7c-a436-d7eb788e3069", "Changes have been made to a read-only job. Please reload this job before viewing Workflow Relationships."));
				}
				else
				{
					var result = Globals.Message.Show(Res.GetString("623c6fd6-2774-4dfb-bb93-3f4d7a7f3fdd", "You must save this form before trying to edit this workflow's relationships. Would you like to save now?"), Res.GetString("b3f32a28-0720-4a9c-bec2-df3956349b15", "Cannot Edit"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
					if (result == DialogResult.Yes)
					{
						showForm = (((ZForm)ParentForm).FireSaveButton() == ContinueWithSave.Yes);
					}
				}
			}
			else if (currentWorkflow == null)
			{
				Globals.Message.Show(Res.GetString("d2990c57-d268-444e-8089-db1d22264094", "Please select a workflow."), Res.GetString("b3f32a28-0720-4a9c-bec2-df3956349b15", "Cannot Edit"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
			else
			{
				showForm = true;
			}

			if (showForm)
			{
				var factory = currentWorkflow.ReadOnly
					? new ReadOnlyBusinessObjectFactory { NameForDebugging = nameof(WorkflowRelationshipsForm) }
					: new BusinessObjectFactory { NameForDebugging = nameof(WorkflowRelationshipsForm) };
				var reloadedWorkflow = factory.Load<ProcessHeader>(currentWorkflow.PK);

				if (reloadedWorkflow == null)
				{
					Globals.Message.ShowError(Res.GetString("47cad683-8109-4a09-b2b0-402fc2e3c891", "Cannot show the Workflow Relationship Navigator because this workflow has been deleted."));
				}
				else
				{
					var parentForm = FindForm();
					var form = new WorkflowRelationshipsForm(reloadedWorkflow) { DisplayMode = (parentForm as ZForm)?.DisplayMode ?? ODisplayMode.Browse };
					ZFormModaliser.Show(form, parentForm);
				}
			}
		}

		#endregion

		#region Prerequisites List

		void PrereqListButton_Click(object sender, EventArgs e)
		{
			if (Env.Security.WorkflowDependencies.IsAllowed)
			{
				var workflow = SelectedWorkflow;

				if (workflow != null)
				{
					var prerequisiteLinkPKs = workflow.GetPKsOfPrerequisitesUpTheTree();

					if (prerequisiteLinkPKs.Length == 0)
					{
						Globals.Message.Show(Res.GetString("451f59ed-9346-4de0-a086-148e78f63fea", "The selected workflow has no prerequisites."));
					}
					else
					{
						var form = FindForm() as ZForm;

						if (form != null && ((IBusiness)DataSource).HasChanges)
						{
							var message = Res.GetString("46bc7f14-1f59-417f-aacf-87953084a9b4", "Please save the form before viewing prerequisites. Would you like to save now?");
							var caption = Res.GetString("45a61645-53f4-4397-ae6e-58c94bbc1c97", "Please save the form");

							var result = Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, DialogResult.Yes);

							if (result == DialogResult.Yes)
							{
								if (form.FireSaveButton() == ContinueWithSave.No)
								{
									return;
								}
							}
							else
							{
								return;
							}
						}

						ActuallyShowPrereqsList(workflow, prerequisiteLinkPKs);
					}
				}
				else
				{
					Globals.Message.Show(Res.GetString("5e4d7f6f-c0d4-4685-9fa1-a3aa211d9b2d", "Please select a workflow."));
				}
			}
			else
			{
				Env.Security.WorkflowDependencies.ShowError();
			}
		}

		static void ActuallyShowPrereqsList(ProcessHeader workflow, ZGuid[] prerequisiteLinkPKs)
		{
			var query = new ZQuery(ProcessHeaderLinkSchema.PK, prerequisiteLinkPKs);

			BusinessObjectModulePicker.ShowModuleScreen(ModuleIDs.ProcessHeaderLink, module =>
			{
				module.AddAdditionalDisplayFilter = q => q.AddToFilter(query);
			}, BusinessObjectModulePicker.FilterLayoutStrategy.EmptyLayoutStrategy, isReadOnly: false);

			workflow.RefreshOpenPrerequisiteStatus();
		}

		#endregion

		void OpenSequenceButton_Click(object sender, EventArgs e)
		{
			// BMReleaseSequence controller launches the GLOW URL
			ZControllerFactory.Create(ControllerIDs.BMReleaseSequence).ShowEditForm(SelectedWorkflow.HighestReleaseSequence as BusinessObject);
		}

		void SequenceParentJobLinkLabel_Click(object sender, EventArgs e)
		{
			var workflow = (((sender as Control)?
				.Parent?
				.Parent?
				.Parent as WorkflowDetailsUserControl)?
				.CurrentDataItem as ProcessHeader)?
				.HighestSequencedWorkflow;

			if (workflow != null)
			{
				var form = ZControllerFactory.Create(ControllerIDs.ProcessHeader).ShowEditForm(workflow);

				if (form != null)
				{
					form.Show();
				}
			}
		}

#if WINZOR

		ProcessJobHeader jobHeader;
		JobNetwork network;

		void WorkflowRelationshipDesignerButton_Click(object sender, EventArgs e)
		{
			InitialiseNetwork();
			PopOutRelationshipDesigner();
		}

		void InitialiseNetwork()
		{
			UnhookNetworkEvents();
			jobHeader = ProcessJobHeader.GetForParent(SelectedWorkflow.Parent, SelectedWorkflow.Factory);
			var defaultShape = jobHeader.GetDefaultDiagram();
			var refresher = new JobNetworkRefresher();
			var controller = ObjectFactory.Get<IBMNetworkEntityController>(nameof(IBMNetworkEntityController), refresher, FindForm());
			network = JobNetwork.Create(defaultShape, controller, refresher);
			refresher.AddedToNetwork(network);
			network.DeleteWorkflowDialogWrapper = new MultiActionButtonDialogWrapper<DeleteWorkflowOption>();
			network.FullRefresh();
			network.DiagramShape.DeletedByDataRefresh += DefaultShape_DeletedByDataRefresh;
		}

		void PopOutRelationshipDesigner()
		{
			if (IsJobHeaderValid() && IsNetworkValid())
			{
				var networkUserControl = new NetworkUserControl(network?.DiagramEntity, network?.Refresher, isWRDPopout: true);
				networkUserControl.SetDataContext(network, isReloading: false);
				networkUserControl?.PopOut();
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("a12af21a-f0ab-4a07-9ada-3fb5d654eb82", "The Workflow Relationship Designer cannot be shown since the network has an invalid relationship. Please correct this relationship and click this message to show the network."));
			}
		}

		void UnhookNetworkEvents()
		{
			if (network != null)
			{
				network.DiagramShape.DeletedByDataRefresh -= DefaultShape_DeletedByDataRefresh;
			}
		}

		void DefaultShape_DeletedByDataRefresh(object sender, EventArgs e)
		{
			InitialiseNetwork();
		}

		bool IsNetworkValid()
		{
			var graph = new NetworkDependencyGraph(network);
			return graph.IsDirectedAcyclicGraph();
		}

		bool IsJobHeaderValid()
		{
			return !(jobHeader?.IsDeleted) ?? false;
		}
#endif
	}
}
