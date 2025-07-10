using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.PAVE.Common.Interfaces;
using Enterprise.BufferManagement.Business;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public partial class WorkflowsUserControl : ZUserControl
	{
		public WorkflowsUserControl()
		{
			InitializeComponent();
			RemoveReleaseSequenceColumnsIfModuleNotEnabled();
			RemoveNewReleaseGateColumnsIfDisabledInRegistry();
#if (DEBUG || !WINZOR)
			AddNCNTabIfWorkflowModeAllows();
#endif

			WorkflowsGrid.ContextMenu.Popup += ContextMenu_Popup;
		}

		void AddNCNTabIfWorkflowModeAllows()
		{
			if (DesignModeFinder.IsDesigning || BMSRegistryGUIHelper.IsPlanningManagementEnabled)
			{
				RelationshipDesignerUserControl = new WorkflowRelationshipDesignerUserControl(DeleteWorkflowDialogWrapper);
				RelationshipDesignerUserControl.Dock = DockStyle.Fill;
				RelationshipDesignerUserControl.Name = "RelationshipDesignerUserControl";
				RelationshipDesignerUserControl.SelectedWorkflowChanged += RelationshipDesignerUserControl_SelectedWorkflowChanged;

				NCNTabPage = new ZTabPage();
				NCNTabPage.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("b219c1d4-9ade-471a-887f-ae9264abbd72", "Workflow Relationship Designer");
				NCNTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
				NCNTabPage.Name = "NCNTabPage";
				NCNTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
				NCNTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(792, 273, true);
				NCNTabPage.TabIndex = 1;
				NCNTabPage.UseVisualStyleBackColor = true;
				NCNTabPage.Controls.Add(RelationshipDesignerUserControl);

				WorkflowNCNTabControl.Controls.Add(NCNTabPage);
			}
		}

		void RelationshipDesignerUserControl_SelectedWorkflowChanged(object sender, ProcessHeader selectedProcessHeader)
		{
			if (WorkflowsGrid?.ListManager != null)
			{
				var gridIndexToSelect = -1;

				if (selectedProcessHeader != null)
				{
					gridIndexToSelect = WorkflowsGrid.List.IndexOf(selectedProcessHeader);
				}

				if (gridIndexToSelect == -1 && DataSource?.JobHeader != null)
				{
					gridIndexToSelect = WorkflowsGrid.List.IndexOf(DataSource.JobHeader);
				}

				if (gridIndexToSelect >= 0)
				{
					WorkflowsGrid.ListManager.Position = gridIndexToSelect;
				}
			}
		}

#if DEBUG
		public
#endif
		WorkflowRelationshipDesignerUserControl RelationshipDesignerUserControl;

		void RemoveReleaseSequenceColumnsIfModuleNotEnabled()
		{
			if (!DesignModeFinder.IsDesigning)
			{
				if (!BMSRegistry.Instance.ReleaseSequencesModuleEnabled.Value)
				{
					var columnInfos = WorkflowsGrid.ColumnStyles.Cast<ZGridColumnInfo>().ToArray();
					WorkflowsGrid.ColumnStyles.Remove(columnInfos.Single(i => i.ColumnName == "ReleaseSequenceName"));
					WorkflowsGrid.ColumnStyles.Remove(columnInfos.Single(i => i.ColumnName == "ReleaseSequencePosition"));
					WorkflowsGrid.ColumnStyles.Remove(columnInfos.Single(i => i.ColumnName == "ReleaseSequenceWorkflow"));
					WorkflowsGrid.ColumnStyles.Remove(columnInfos.Single(i => i.ColumnName == "ReleaseSequenceParentJob"));

					var effectiveNudge = columnInfos.Single(i => i.ColumnName == "EffectiveNudge") as ZCalcEditColumnStyleInfo;
					effectiveNudge.Decimals = 2;
				}
			}
		}

		void RemoveNewReleaseGateColumnsIfDisabledInRegistry()
		{
			if (!DesignModeFinder.IsDesigning)
			{
				if (!BMSRegistry.Instance.DisplayResponsiveReleaseGateUiSettings.Value)
				{
					var columnInfos = WorkflowsGrid.ColumnStyles.Cast<ZGridColumnInfo>().ToArray();
					WorkflowsGrid.ColumnStyles.Remove(columnInfos.Single(i => i.ColumnName == "FH_IsApproved"));
					WorkflowsGrid.ColumnStyles.Remove(columnInfos.Single(i => i.ColumnName == "FH_EffectiveNudge"));
					WorkflowsGrid.ColumnStyles.Remove(columnInfos.Single(i => i.ColumnName == "DedicatedBufferName"));
					WorkflowsGrid.ColumnStyles.Remove(columnInfos.Single(i => i.ColumnName == "FH_EffectiveAgreedDeliveryDateUtc"));
					WorkflowsGrid.ColumnStyles.Remove(columnInfos.Single(i => i.ColumnName == "FH_ReleaseSequenceSortDateUtc"));
					WorkflowsGrid.ColumnStyles.Remove(columnInfos.Single(i => i.ColumnName == "FH_GB_Branch"));
					WorkflowsGrid.ColumnStyles.Remove(columnInfos.Single(i => i.ColumnName == "FH_GE_Department"));
					WorkflowsGrid.ColumnStyles.Remove(columnInfos.Single(i => i.ColumnName == "EffectiveBranchCode"));
					WorkflowsGrid.ColumnStyles.Remove(columnInfos.Single(i => i.ColumnName == "EffectiveDepartmentCode"));
					WorkflowsGrid.ColumnStyles.Remove(columnInfos.Single(i => i.ColumnName == "FH_DeadlineType"));
					WorkflowsGrid.ColumnStyles.Remove(columnInfos.Single(i => i.ColumnName == "FH_BMT_BufferTimespan"));
					WorkflowsGrid.ColumnStyles.Remove(columnInfos.Single(i => i.ColumnName == "EffectiveBufferDurationString"));
					WorkflowsGrid.ColumnStyles.Remove(columnInfos.Single(i => i.ColumnName == "LatestAcceptableReleaseDateUtcString"));
				}
			}
		}

		#region DataBinding

		public new WorkflowManagementViewModel DataSource
		{
			get { return (WorkflowManagementViewModel)base.DataSource; }
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			if (WorkflowsGrid.ListManager != null)
			{
				for (int i = 1; i < WorkflowsGrid.ListManager.Count; i++)
				{
					var workflow = WorkflowsGrid.List[i] as ProcessHeader;
					if (workflow.IsOpen)
					{
						WorkflowsGrid.ListManager.Position = i;
						break;
					}
				}
			}

			RelationshipDesignerUserControl?.SetDataBinding(dataSource, dataMember);
		}

		protected override void Dispose(bool disposing)
		{
			try
			{
				if (disposing)
				{
					if (components != null)
					{
						components.Dispose();
					}
				}
			}
			finally
			{
				base.Dispose(disposing);
			}
		}

		#endregion

		#region ContextMenu

		bool initialisedCustomMenuItems;

#if DEBUG
		public
#endif
		void ContextMenu_Popup(object sender, EventArgs e)
		{
			var menu = (ContextMenu)sender;

			if (!initialisedCustomMenuItems)
			{
				initialisedCustomMenuItems = true;

				menu.MenuItems.Add(new CloneWorkflowMenuItem(WorkflowsGrid, () => DataSource.AllProcessHeaders));

				var jobHeader = DataSource?.JobHeader;
				var provider = jobHeader?.Parent;
				var workflowType = provider?.WorkflowType;
				var parentJobHeaders = jobHeader.ParentLinks.Cast<ProcessHeaderLink>().Select(l => l.HeaderTo).OfType<ProcessJobHeader>();

				if (workflowType.HasValue)
				{
					menu.MenuItems.Add(new PromoteWorkflowMenuItem(WorkflowsGrid, workflowType.Value));
					menu.MenuItems.Add(new DemoteWorkflowMenuItem(WorkflowsGrid, workflowType.Value, parentJobHeaders));
				}

				menu.MenuItems.Add(new CancelWorkflowMenuItem(WorkflowsGrid));

				menu.MenuItems.Add(ZMenuItem.Separator);
				menu.MenuItems.Add(new PreRequisitesMenuItem(WorkflowsGrid));
				menu.MenuItems.Add(new DependentWorkflowsMenuItem(WorkflowsGrid));
				menu.MenuItems.Add(new ParentWorkflowsMenuItem(WorkflowsGrid));
				menu.MenuItems.Add(new ChildWorkflowsMenuItem(WorkflowsGrid));

				var dependencyShortcutMenu1 = new MakeSelectedWorkflowsPrerequisitesMenuItem(WorkflowsGrid, DependencyDirection.PreRequisite);
				var dependencyShortcutMenu2 = new MakeSelectedWorkflowsPrerequisitesMenuItem(WorkflowsGrid, DependencyDirection.PostRequisite);

				menu.MenuItems.Add(dependencyShortcutMenu1);
				menu.MenuItems.Add(dependencyShortcutMenu2);

				dependencyShortcutMenuItems.Add(dependencyShortcutMenu1);
				dependencyShortcutMenuItems.Add(dependencyShortcutMenu2);

				menu.MenuItems.Add(new ValidateWorkflowLoopsMenuItem(WorkflowsGrid));
			}

			foreach (var menuItem in dependencyShortcutMenuItems)
			{
				menuItem.UpdateMenuItem();
			}
		}

		readonly List<MakeSelectedWorkflowsPrerequisitesMenuItem> dependencyShortcutMenuItems = new List<MakeSelectedWorkflowsPrerequisitesMenuItem>();

		#endregion

		#region Delete

		void WorkflowsGrid_RowsDeleting(object sender, RowsDeletingEventArgs e)
		{
			var workflows = e.Objects.Cast<ProcessHeader>().ToArray();
			if (workflows.Length > 0)
			{
				var canDelete = WorkflowManagementViewModel.CanDeleteEntities(workflows, DeleteWorkflowDialogWrapper);

				if (canDelete.Result)
				{
					var workflowsToDelete = canDelete.WorkflowsToDelete.Where(w => w.IsInDatabase).ToArray(); // We don't delete entities not in the database. The grid can handle this for us.

					WorkflowManagementViewModel.DeleteEntities(workflowsToDelete);

					RelationshipDesignerUserControl?.RefreshNetwork();
				}
				else
				{
					e.Cancel = true;
				}
			}
		}

		public MultiActionButtonDialogWrapper<DeleteWorkflowOption> DeleteWorkflowDialogWrapper
		{
			get { return deleteWorkflowDialogWrapper ?? (deleteWorkflowDialogWrapper = new MultiActionButtonDialogWrapper<DeleteWorkflowOption>()); }
		}

		MultiActionButtonDialogWrapper<DeleteWorkflowOption> deleteWorkflowDialogWrapper;

		#endregion

		#region For Test
#if DEBUG

		public ZTabControl WorkflowNCNTabControl_Exposed
		{
			get { return WorkflowNCNTabControl; }
		}

		public ZTabPage WorkflowsTabPage_Exposed
		{
			get { return WorkflowsTabPage; }
		}

		public ZTabPage NCNTabPage_Exposed
		{
			get { return NCNTabPage; }
		}

#endif
		#endregion
	}
}
