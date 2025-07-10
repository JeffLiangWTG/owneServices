using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Application;
using Enterprise.BufferManagement.Integration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.GUI
{
	public class MoveToComponentMenuItemProvider : IFilterGridMenuItemProvider
	{
		public const string MoveToComponentMenuItemName = "MoveToComponent";

		IEnumerable<MenuItem> IFilterGridMenuItemProvider.GetMenuItems(ZFilterGridModule module)
		{
			var isJobWorkflowsModule = module.ID == ModuleIDs.ProcessHeader;
			if (isJobWorkflowsModule && ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled)
			{
				var strategy = new MoveToComponentMenuItemStrategy();
				foreach (var menuItem in strategy.GetMoveToComponentMenuItems(module))
				{
					yield return menuItem;
				}
			}
		}
	}
}
