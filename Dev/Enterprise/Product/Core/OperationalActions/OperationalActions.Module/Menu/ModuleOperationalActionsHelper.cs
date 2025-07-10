using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Services.OperationalActions.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Services.OperationalActions.Module
{
	public class ModuleOperationalActionsHelper : IModuleOperationalActionsHelper
	{
		bool IModuleOperationalActionsHelper.SupportsOperationalActions(IZFilterGridModule module)
		{
			return module is IOperationalActionSupportable;
		}

		IEnumerable<object> IModuleOperationalActionsHelper.OperationalActionNames(IZFilterGridModule module, BusinessObjectFactory factory)
		{
			var zModule = (ZFilterGridModule)module;
			var context = new OperationalActionContext(((IOperationalActionSupportable)module).OperationalActionSupporter, zModule.ID.Description, zModule.WorkflowType);
			var result = OperationalActionsMenuItemGenerator.LoadApplicableActions(context, factory);
			return result.Select(x => x.MenuNameMultilingual);
		}

		void IModuleOperationalActionsHelper.AddOperationalActionsIntoActionsMenu(IComponent component)
		{
			if (component is ZForm zform)
			{
				AddOperationalActionsMenu(zform.GetModule(), new BusinessObjectSelector(new[] { zform.BusinessEntity as BusinessObject }), (menu) => ZFormMenuStrategy.AddActionsMenuItem(zform, menu));
			}
			else if (component is ZGrid zGrid && ShouldAddOperationalActionsIntoGridActionsMenu(zGrid.Parent) && zGrid.ListManager?.List != null)
			{
				var moduleId = ZMetaData.GetModuleId(zGrid.ListManager.List);
				if (moduleId != ModuleIDs.NotAssigned)
				{
					using (var module = ZModuleFactory.Instance.Create(moduleId))
					{
						AddOperationalActionsMenu(module, new GridSelection(zGrid), (menu) => zGrid.ContextMenu.MenuItems.Add(menu));
					}
				}
			}
		}

		protected bool ShouldAddOperationalActionsIntoGridActionsMenu(Control parent)
		{
			while (parent != null)
			{
				if (parent is ZFilterStripControl || parent is ZModuleButtonGrid)
				{
					return false;
				}
				parent = parent.Parent;
			}
			return true;
		}

		void AddOperationalActionsMenu(ZModule module, ITargetRecordSelection selection, Action<MenuItem> action)
		{
			if (module is ZFilterGridModule gridModule && gridModule is IOperationalActionSupportable supportable && supportable.OperationalActionSupporter != null)
			{
				var context = new OperationalActionContext(supportable.OperationalActionSupporter, gridModule.ID.Description, gridModule.WorkflowType);
				var menuFactory = new BusinessObjectFactory { NameForDebugging = "Operational Actions Menu" };
				var generator = new OperationalActionsMenuItemGenerator(menuFactory, selection, context);

				var result = GetOperationalActionTopLevelMenuItem(generator);
				action.Invoke(result);
			}
		}

		internal static MenuItem GetOperationalActionTopLevelMenuItem(OperationalActionsMenuItemGenerator generator)
		{
			var result = new ZMenuItem(ResString.GetMultilingualString("7D72BE03-888A-4ebc-874B-92AF101745E0", "Operational Actions"));
			result.MenuItems.Add((NoResString)"<placeholder for popup event>");

			result.Popup += delegate
			{
				result.MenuItems.Clear();
				result.MenuItems.AddRange(generator.Generate(false));
			};

			return result;
		}
	}
}
