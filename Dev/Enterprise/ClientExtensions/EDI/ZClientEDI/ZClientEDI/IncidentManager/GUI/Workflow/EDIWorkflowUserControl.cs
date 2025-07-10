using System;
using Enterprise.BufferManagement.Integration;
using Enterprise.Client.EDI.IncidentManager.GUI.Workflow;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	partial class EDIWorkflowUserControl : ZWorkflowUserControl
	{
		public EDIWorkflowUserControl()
		{
			GetTaskDetailsMenuItemMethod = () => new GitPullRequestMenuItem();
		}

		protected override IWorkflowManagementTabPage GetNewWorkflowManagementTabPage(IWorkflowProvider provider)
		{
			var workFlowManagementTabPage = base.GetNewWorkflowManagementTabPage(provider);
			if (provider.WorkflowType == JobInvoicingConsumerTypes.WorkItem.Code && workFlowManagementTabPage is ITaskDetailsMenuStripHostControl menuStripHostControl)
			{
				menuStripHostControl.AddTaskDetailsMenuItem(GetTaskDetailsMenuItemMethod.Invoke());
			}

			return workFlowManagementTabPage;
		}

		protected override TaskWithDetailsAndFilterTab GetNewTaskWithDetailsAndFilterTab(IWorkflowProvider provider)
		{
			if (provider.WorkflowType == JobInvoicingConsumerTypes.WorkItem.Code)
			{
				return new EDITaskWithDetailsAndFilterTab();
			}
			else
			{
				return base.GetNewTaskWithDetailsAndFilterTab(provider);
			}
		}

		public Func<ITaskDetailsMenuItem> GetTaskDetailsMenuItemMethod { get; set; }
	}
}
