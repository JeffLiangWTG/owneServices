using System;
using System.Windows.Forms;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.NetworkVisualisation.GUI
{
	public class WorkflowRelatedDiagramsTabPage : ZTabPage, IWorkflowRelatedDiagramsTabPage
	{
		public WorkflowRelatedDiagramsTabPage(IProcessHeader workflow)
		{
			if (Controls.Count == 0)
			{
				Controls.Add(GetWorkflowRelatedDiagramsUserControl((ProcessHeader)workflow));
			}
		}

		public void UpdateDataBinding(IProcessHeader workflow)
		{
			if (workflowManagementUserControl != null)
			{
				workflowManagementUserControl.BeginInvoke(new Action(() => workflowManagementUserControl.SetDataBinding(workflow, string.Empty)));
			}
		}

		WorkflowRelatedDiagramsUserControl GetWorkflowRelatedDiagramsUserControl(ProcessHeader workflow)
		{
			if (workflowManagementUserControl == null)
			{
				workflowManagementUserControl = new WorkflowRelatedDiagramsUserControl(workflow);
				workflowManagementUserControl.Dock = DockStyle.Fill;
			}
			return workflowManagementUserControl;
		}

		WorkflowRelatedDiagramsUserControl workflowManagementUserControl;

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				workflowManagementUserControl = null;
			}

			base.Dispose(isNotFinalizing);
		}
	}
}
