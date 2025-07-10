using System;
using System.Collections.Generic;
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
	public class TagilatorMenuItemProvider : IFilterGridTopLevelMenuItemProvider
	{
		#region Menu Items

		IEnumerable<MenuItem> IFilterGridMenuItemProvider.GetMenuItems(ZFilterGridModule module)
		{
			if (ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled)
			{
				Type bizoType = null;
				AddRemoveTagMenuItemStrategy strategy = null;

				if (module.ID == ModuleIDs.ProcessHeader)
				{
					bizoType = typeof(ProcessHeader);
					strategy = new AddRemoveTagMenuItemStrategy();
				}
				else if (module.ID == ModuleIDs.ProcessTasks)
				{
					bizoType = typeof(ProcessTask);
					strategy = new AddRemoveTagMenuItemStrategy();
				}
				else
				{
					var workflowType = module.WorkflowType;
					if (!string.IsNullOrEmpty(workflowType))
					{
						var factory = module.FilterBusinessObject.Factory ?? module.GridCollection.Factory ?? new BusinessObjectFactory { NameForDebugging = GetType().Name };
						var system = BMSystem.GetSystemForWorkflowType(workflowType, factory);

						if (system != null)
						{
							bizoType = module.GetElementType();
							strategy = new AddRemoveTagForRelatedModuleMenuItemStrategy();
						}
					}
				}

				if (bizoType != null)
				{
					foreach (var menuItem in strategy.GetAddAndRemoveTagMenuItems(module, bizoType))
					{
						yield return menuItem;
					}
				}
			}
		}

		public const string AddTagMenuItemName = "AddBMTag";
		public const string RemoveTagMenuItemName = "RemoveBMTag";

		bool IFilterGridTopLevelMenuItemProvider.TryGetButtonDetail(MenuItem item, ref IconTypes buttonImage, ref IconTypes buttonImageActive, ref string buttonToolTip)
		{
			if (item.Name == AddTagMenuItemName)
			{
				buttonToolTip = Res.GetString("7fd20f1a-a567-43ae-8f5f-aa8abca99370", "Add a tag to the selected rows.");
			}
			else if (item.Name == RemoveTagMenuItemName)
			{
				buttonToolTip = Res.GetString("3da5ae9b-80fb-40a6-925a-788d478d4742", "Remove a tag from the selected rows.");
			}
			else
			{
				return false;
			}

			buttonImage = buttonImageActive = IconTypes.Tag;
			return true;
		}

		#endregion
	}
}
