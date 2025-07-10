using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.GUI
{
	public class MultiJobHeaderEditorMenuItemProvider : IFilterGridMenuItemProvider
	{
		#region IFilterGridMenuItemProvider Members

		IEnumerable<MenuItem> IFilterGridMenuItemProvider.GetMenuItems(ZFilterGridModule module)
		{
			var isJobWorkflowsModule = module.ID == ModuleIDs.ProcessHeader;
			if ((module.SupportsWorkflow || isJobWorkflowsModule) && ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled)
			{
				var workflowType = isJobWorkflowsModule ? null : module.WorkflowType;

				if (!string.IsNullOrEmpty(workflowType) || isJobWorkflowsModule)
				{
					var factory = new BusinessObjectFactory { NameForDebugging = GetType().Name };
					factory.RefreshEnabled = false;
					if (BMSystem.GetAllApplicableBMSystems(workflowType, factory).Any())
					{
						yield return new ZMenuItem(Res.GetString("adddc5a9-7246-4b84-ae66-a9de94896aad", "Edit Job Schedules"), GetClickHandler(module));
					}
				}
			}
		}

		#endregion

		#region Implementation

		static EventHandler GetClickHandler(ZFilterGridModule module)
		{
			return (s, e) =>
			{
				var workflowProviders = module.GridCollection.Cast<BusinessObject>().Where(bizo => bizo is IWorkflowProvider || bizo is ProcessHeader);
				var factory = new BusinessObjectFactory { NameForDebugging = nameof(MultiJobHeaderEditorMenuItemProvider) + " GetClickHandler" };

				var form = MultiJobHeaderEditorForm.ShowFormIfAllowed(workflowProviders, factory);

				if (form != null)
				{
					var selectedItems = module.GetSelectedBusinessObjects().OfType<ProcessHeader>().ToArray();

					if (selectedItems.Length > 0)
					{
						var viewsDictionary = form.SchedulesGrid.List
							.Cast<JobHeaderView>()
							.Where(v => v.ProcessHeader != null)
							.Select((v, i) => new { Key = v.ProcessHeader.PK, Index = i })
							.ToDictionary(x => x.Key, x => x.Index);

						foreach (var item in selectedItems)
						{
							int index;
							if (viewsDictionary.TryGetValue(item.PK, out index))
							{
								form.SchedulesGrid.Select(index);
							}
						}
					}
				}
			};
		}

		#endregion
	}
}
