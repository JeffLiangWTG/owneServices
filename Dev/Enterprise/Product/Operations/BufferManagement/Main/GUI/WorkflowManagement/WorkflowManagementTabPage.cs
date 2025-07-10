using System;
using System.Windows.Forms;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public class WorkflowManagementTabPage : ZBindingTabPage, IWorkflowManagementTabPage, ITaskDetailsMenuStripHostControl
	{
		WorkflowManagementUserControl GetWorkflowManagementUserControl()
		{
			if (workflowManagementUserControl == null)
			{
				workflowManagementUserControl = new WorkflowManagementUserControl();
				workflowManagementUserControl.Dock = DockStyle.Fill;

				taskDetailsMenuItem?.AttachToTaskDetailsControl(workflowManagementUserControl.TaskDetailsControl);
			}
			return workflowManagementUserControl;
		}

		WorkflowManagementUserControl workflowManagementUserControl;

		protected override void SetDataBindingCore(object dataSource, string dataMember)
		{
			if (!string.IsNullOrEmpty(dataMember))
			{
				throw new ArgumentException("dataMember is not supported for this control", nameof(dataMember));
			}

			workflowProvider = (IWorkflowProvider)dataSource;

			var processJobHeader = ProcessJobHeader.GetForParent(workflowProvider, workflowProvider.WorkflowItems.Factory, addDefaultProcessHeaderIfNone: false, shouldRecheckDatabase: true);

			if (Controls.Count == 0)
			{
				Controls.Add(GetWorkflowManagementUserControl());
			}

			base.SetDataBindingCore(processJobHeader, string.Empty);
		}

		IWorkflowProvider workflowProvider;
		ITaskDetailsMenuItem taskDetailsMenuItem { get; set; }
		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				workflowProvider = null;
				taskDetailsMenuItem?.Dispose();
			}

			base.Dispose(isNotFinalizing);
		}

		public void NavigateToWorkflowItem(IProcessTask task)
		{
			workflowManagementUserControl?.NavigateToWorkflowItem(task);
		}

		public void NavigateToWorkflowItem(IProcessHeader workflow)
		{
			workflowManagementUserControl?.NavigateToWorkflowItem(workflow);
		}

		public void AddTaskDetailsMenuItem(ITaskDetailsMenuItem menuItem)
		{
			taskDetailsMenuItem = menuItem;

			if (workflowManagementUserControl != null)
			{
				taskDetailsMenuItem?.AttachToTaskDetailsControl(workflowManagementUserControl.TaskDetailsControl);
			}
		}
	}
}
