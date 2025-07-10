using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public class PromoteWorkflowMenuItem : WorkflowProviderActionMenuItemBase
	{
		public PromoteWorkflowMenuItem(ZGrid workflowsGrid, ZString currentWorkflowType)
			: base(workflowsGrid, currentWorkflowType, Res.GetString("1138b3cc-9409-476c-9e2e-8274e13b9547", "Promote to Job"))
		{
		}

		protected override EventHandler MenuItemHandler(BMSystemWorkflowDeterminer workflowType)
		{
			return (e, s) =>
			{
				var workflow = (ProcessHeader)workflowsGrid.ListManager.GetCurrent();

				if (workflow == null)
				{
					Globals.Message.ShowWarning(Res.GetString("efad0814-9a47-46cb-b0ad-d6ce3b21ce1f", "Select a workflow to promote into a Job."));
				}
				else if (workflow is ProcessJobHeader)
				{
					Globals.Message.ShowWarning(Res.GetString("37e26b00-ef20-45bc-9099-917876de03ab", "Only workflows can be promoted."));
				}
				else
				{
					var result = Globals.Message.Show(
						Res.GetString("8d3c81e6-d5bf-41dd-8c8c-c13a4aa6b534", "Are you sure you want to promote this Workflow into a {0}? This will move all tasks and relationships with other workflows to the new Job.", workflowType.WorkflowTypeDescription),
						Res.GetString("5591fc6e-dec7-4cf6-ad4a-5e61e8998fe6", "Promote Workflow to {0}", workflowType.WorkflowTypeDescription),
						MessageBoxButtons.YesNo,
						DialogResult.Yes);

					if (result == DialogResult.Yes)
					{
						PromoteWorkflowInNewForm(workflowType, workflow);
					}
				}
			};
		}

		void PromoteWorkflowInNewForm(BMSystemWorkflowDeterminer workflowType, ProcessHeader workflow)
		{
			var jobCreator = new WorkflowParentJobCreator(workflowType.FSW_WorkflowType);
			jobCreator.CreateJob(new BusinessObjectFactory { NameForDebugging = GetType().Name });

			workflow.Promote(jobCreator.Job);

			var formSaveAction = new Action(() =>
			{
				workflow.Delete();
			});

			var form = jobCreator.ShowNewForm(formSaveAction);
			ZFormModaliser.Show(form, workflowsGrid.FindForm());
		}
	}
}
