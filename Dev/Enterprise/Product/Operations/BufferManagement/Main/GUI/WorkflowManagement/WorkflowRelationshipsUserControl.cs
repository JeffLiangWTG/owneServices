using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.GUI
{
	public partial class WorkflowRelationshipsUserControl : ZUserControl
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1046", Justification = "The control is not a button")]
		public WorkflowRelationshipsUserControl()
		{
			InitializeComponent();

			ToolTipService.SetToolTip(prerequisitesPictureBox, Res.GetString("2fb5a896-7688-4d86-90ef-97752ed654a1", "Pre-requisites must be completed before the currently selected job or workflow can begin."));
			ToolTipService.SetToolTip(postrequisitesPictureBox, Res.GetString("7bc8a895-990b-46a2-bc4f-bffee8986778", "Post-requisites can only be started after the currently selected job or workflow is complete."));
			ToolTipService.SetToolTip(parentsPictureBox, Res.GetString("ccc0c5c2-d493-473b-b4c0-ea516560cc71", "Represents the parents of the currently selected job or workflow."));
			ToolTipService.SetToolTip(childrenPictureBox, Res.GetString("88abc2b7-db74-4936-8228-b245b1babd7c", "Represents the children of the currently selected job or workflow."));
			ToolTipService.SetToolTip(JobWorkflowsPictureBox, Res.GetString("2f7804c0-92c6-4420-a3d5-d545db8f0e8d", "Represents the workflows in the same job as the currently selected job or workflow."));
		}

		class OverridedZGrid : ZGrid
		{
			protected override int HandleDelete(int clickedRow)
			{
				var parent = this.GetParent<WorkflowRelationshipsUserControl>();
				if (parent != null)
				{
					parent.RemoveLinks(this, parent.ActiveProcessHeader.PrerequisiteLinks_ForBinding);
				}

				return 0;
			}
		}

		public void SetDataBinding(ProcessHeader processHeader, ProcessHeaderRelationshipsViewModel model)
		{
			viewModel = model;
			viewModel.NavigateTo(processHeader);
			SetDataBinding(viewModel, string.Empty);
		}

		ProcessHeaderRelationshipsViewModel viewModel;

		#region Workflow navigation

		ProcessHeader ActiveProcessHeader
		{
			get { return viewModel.ActiveProcessHeaders[0]; }
		}

#if DEBUG
		public
#endif
 void NavigateIfAllowed(Func<ProcessHeader> processHeaderGetter, int row)
		{
			if (row >= 0)
			{
				NavigateIfAllowed(processHeaderGetter);
			}
		}

		void NavigateIfAllowed(Func<ProcessHeader> processHeaderGetter)
		{
			if (ActiveProcessHeader.HasErrors)
			{
				Globals.Message.Show(Res.GetString("b6c08ed8-dee0-4807-be67-0071f06bea10", "Please fix the errors in the form before navigating the workflow relationships"), Res.GetString("ce6bc760-315c-4cb2-a32b-f507b1218fae", "Errors..."), MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
			else
			{
				var processHeader = processHeaderGetter();
				if (processHeader != null)
				{
					viewModel.NavigateTo(processHeader);
					SetDataBinding(null, string.Empty);
					SetDataBinding(viewModel, string.Empty);
					selectedWorkflowGroupBox.Text = Res.GetString("bdccdfcf-6427-4822-9324-95796fe7f994", "Selected {0}", processHeader.ProcessHeaderType);
					GoToJobButton.Visible = !(processHeader is ProcessJobHeader);
				}
			}
		}

		void NavigateIfAllowed(ZGrid grid, Func<ProcessHeaderLink, ProcessHeader> processHeaderGetter, MouseEventArgs e)
		{
			NavigateIfAllowed(() =>
			{
				var currentLink = GetCurrentlySelectedItem<ProcessHeaderLink>(grid);
				return currentLink != null ? processHeaderGetter(currentLink) : null;
			}, grid.GetRow(e));
		}

		void prerequisitesGrid_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			NavigateIfAllowed(prerequisitesGrid, link => link.HeaderFrom, e);
		}

		void postrequisitesGrid_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			NavigateIfAllowed(postrequisitesGrid, link => link.HeaderTo, e);
		}

		void childrenGrid_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			NavigateIfAllowed(childrenGrid, link => link.HeaderFrom, e);
		}

		void parentsGrid_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			NavigateIfAllowed(parentsGrid, link => link.HeaderTo, e);
		}

		void JobWorkflowsGrid_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			NavigateIfAllowed(() => GetCurrentlySelectedItem<ProcessHeader>(JobWorkflowsGrid), JobWorkflowsGrid.GetRow(e));
		}

#if DEBUG
		public
#endif
 static T GetCurrentlySelectedItem<T>(ZGrid grid)
			where T : class
		{
			return grid.ListManager.GetCurrent() as T;
		}

		#endregion

		#region Grids add / remove buttons

		void AddLink(ProcessHeaderLinkCollection collection, RelationshipDirection target)
		{
			var popupCollection = new ProcessHeaderLookups(ActiveProcessHeader).HeadersWithDefaultFilters;
			var recordChooser = new ZRecordChooser<ProcessHeader>(ModuleIDs.ProcessHeader, popupCollection);

			recordChooser.ShowModal(ParentForm, selectedProcessHeaders =>
			{
				viewModel.AddLinks(selectedProcessHeaders, collection, target);
			});
		}

		void RemoveLinks(ZGrid grid, ProcessHeaderLinkCollection collection)
		{
			var selectedLinks = grid.SelectedElements.Cast<ProcessHeaderLink>().ToArray();
			viewModel.RemoveLinks(selectedLinks, collection);
		}

		#region Grids Add/Remove click methods

		void AddPrerequisiteButton_Click(object sender, EventArgs e)
		{
			AddLink(ActiveProcessHeader.PrerequisiteLinks_ForBinding, RelationshipDirection.From);
		}

		void RemovePrerequisiteButton_Click(object sender, EventArgs e)
		{
			RemoveLinks(prerequisitesGrid, ActiveProcessHeader.PrerequisiteLinks_ForBinding);
		}

		void AddPostrequisiteButton_Click(object sender, EventArgs e)
		{
			AddLink(ActiveProcessHeader.PostrequisiteLinks_ForBinding, RelationshipDirection.To);
		}

		void RemovePostrequisiteButton_Click(object sender, EventArgs e)
		{
			RemoveLinks(postrequisitesGrid, ActiveProcessHeader.PostrequisiteLinks_ForBinding);
		}

		void AddParentButton_Click(object sender, EventArgs e)
		{
			AddLink(ActiveProcessHeader.ParentLinks_ForBinding, RelationshipDirection.To);
		}

		void RemoveParentButton_Click(object sender, EventArgs e)
		{
			RemoveLinks(parentsGrid, ActiveProcessHeader.ParentLinks_ForBinding);
		}

		void AddChildButton_Click(object sender, EventArgs e)
		{
			AddLink(ActiveProcessHeader.ChildLinks_ForBinding, RelationshipDirection.From);
		}

		void RemoveChildButton_Click(object sender, EventArgs e)
		{
			RemoveLinks(childrenGrid, ActiveProcessHeader.ChildLinks_ForBinding);
		}

		#endregion

		#endregion

		#region Job navigation

		void OpenJobFormButton_Click(object sender, EventArgs e)
		{
			WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(ActiveProcessHeader);
		}

		void GoToJobButton_Click(object sender, EventArgs e)
		{
			NavigateIfAllowed(() => ActiveProcessHeader != null ? ActiveProcessHeader.JobHeader : null);
		}

		#endregion
	}
}
