using System;
using System.Globalization;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.GUI
{
	public class DemoteWorkflowByJobTypeMenuItem : WorkflowProviderActionMenuItemBase
	{
		public DemoteWorkflowByJobTypeMenuItem(ZGrid workflowsGrid, ZString currentWorkflowType)
			: base(workflowsGrid, currentWorkflowType, Res.GetString("a6437e65-3b7c-46e5-9562-7701400115ea", "Into Other Job"))
		{
		}

		public static ProcessJobHeader GetJobHeaderToDemote(ZGrid workflowsGrid)
		{
			var selectedJobHeader = workflowsGrid.ListManager.GetCurrent() as ProcessJobHeader;

			if (selectedJobHeader == null)
			{
				Globals.Message.ShowWarning(Res.GetString("a473d381-4a87-46cd-a514-bdd3b24daa79", "Only Jobs can be demoted."));
			}
			else
			{
				var result = Globals.Message.Show(
								 Res.GetString("e58fb7e6-917a-4bb7-a1ff-3e8e94ebc10d", "Are you sure you want to demote this Job? This will collapse all workflows into the selected Job."),
								 Res.GetString("5551282a-fdcc-4c02-a4d5-bd052b100fef", "Demote Job"),
								 MessageBoxButtons.YesNo,
								 DialogResult.Yes);

				if (result != DialogResult.Yes)
				{
					selectedJobHeader = null;
				}
			}

			return selectedJobHeader;
		}

		protected override EventHandler MenuItemHandler(BMSystemWorkflowDeterminer workflowType)
		{
			return (s, e) =>
			{
				var jobHeader = GetJobHeaderToDemote(workflowsGrid);
				if (jobHeader != null)
				{
					DemoteWorkflowInNewForm(workflowType, jobHeader);
				}
			};
		}

		void DemoteWorkflowInNewForm(BMSystemWorkflowDeterminer workflowType, ProcessJobHeader jobHeader)
		{
			var controller = WorkflowProviderHelper.GetControllerForWorkflowType(workflowType.FSW_WorkflowType);
			var moduleId = controller.ModuleID;

			if (!ZModuleFactory.Instance.IsZFilterModule(moduleId))
			{
				throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Could not find usable ZFilterModule for {0}", workflowType.WorkflowTypeDescription));
			}

			using (var module = (ZFilterModule)ZModuleFactory.Instance.Create(moduleId))
			{
				var gridCollection = module.GridCollection;
				var provider = BusinessObjectModulePicker.PickOneRecordFromModuleScreen<BusinessObject>(gridCollection, moduleId) as IWorkflowProvider;

				if (provider != null)
				{
					jobHeader.Demote(provider, jobHeader.Factory);
				}
			}
		}
	}
}
