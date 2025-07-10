using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.PAVE.Common.Interfaces;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	class MakeSelectedWorkflowsPrerequisitesMenuItem : ZMenuItem
	{
		internal MakeSelectedWorkflowsPrerequisitesMenuItem(ZGrid parentGrid, DependencyDirection firstWorkflowDependencyDirection)
		{
			this.parentGrid = parentGrid;
			this.firstWorkflowDependencyDirection = firstWorkflowDependencyDirection;

			Click += MakePrerequisiteMenuItem_Click;
		}

		readonly ZGrid parentGrid;
		readonly DependencyDirection firstWorkflowDependencyDirection;

		internal void UpdateMenuItem()
		{
			if (parentGrid.SelectedElements.Length == 2)
			{
				var selectedItems = parentGrid.SelectedElements.Cast<ProcessHeader>().ToArray();

				Visible = true;
				fromWorkflow = firstWorkflowDependencyDirection == DependencyDirection.PreRequisite ? selectedItems[0] : selectedItems[1];
				toWorkflow = firstWorkflowDependencyDirection == DependencyDirection.PreRequisite ? selectedItems[1] : selectedItems[0];

				Text = ResString.GetMultilingualString("WorkflowManagementUserControl.WorkflowsGrid.ContextMenu.MakeDependency", "Make '{0}' a prerequisite of '{1}'", fromWorkflow.FH_CompletionStatement, toWorkflow.FH_CompletionStatement);
			}
			else
			{
				Visible = false;
				fromWorkflow = null;
				toWorkflow = null;
			}
		}

		ProcessHeader fromWorkflow;
		ProcessHeader toWorkflow;

		void MakePrerequisiteMenuItem_Click(object sender, EventArgs e)
		{
			if (!fromWorkflow.MakePrerequisiteOf(toWorkflow))
			{
				var caption = Res.GetString("b11405ef-1a7e-4135-9325-44b8be71501a", "Action failed");
				var message = Res.GetString("3a81c679-9c71-452c-bc53-51b78164d217", "This dependency already exists.");

				Globals.Message.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
		}
	}
}
