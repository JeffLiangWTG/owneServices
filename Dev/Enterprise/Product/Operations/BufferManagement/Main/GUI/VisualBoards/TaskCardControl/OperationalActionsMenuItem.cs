using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Services.OperationalActions.Business;
using Enterprise.Services.OperationalActions.Module;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.GUI
{
	class OperationalActionsMenuItem : ZLazyPopulatingMenuItem
	{
		public OperationalActionsMenuItem(ProcessHeader workflow, ProcessTask task = null)
			: base(Res.GetString("2A1548A2-44D6-4788-A3B5-E9DB58F7C10F", "Operational Actions"), MenuItemFunc(workflow, task))
		{
		}

		static Func<IEnumerable<ZMenuItem>> MenuItemFunc(ProcessHeader workflow, ProcessTask task)
		{
			return new Func<IEnumerable<ZMenuItem>>(() =>
			{
				var menuItems = new List<ZMenuItem>();

				if (task != null)
				{
					var taskMenu = new ZMenuItem(Res.GetString("8E72F8A2-A72B-4490-AFC2-2EF495DDD3E0", "Task"));
					TryAddSubMenu(taskMenu, ModuleIDs.ProcessTasks, task);
					menuItems.Add(taskMenu);
				}

				if (!(workflow is ProcessJobHeader))
				{
					var workflowMenu = new ZMenuItem(Res.GetString("EC1783B0-7DD2-4C72-AA4E-CB8A6B1AA757", "Workflow"));
					TryAddSubMenu(workflowMenu, ModuleIDs.ProcessHeader, workflow);
					menuItems.Add(workflowMenu);
				}

				var workflowProvider = workflow.Parent;

				if (workflowProvider != null)
				{
					var jobMenu = new ZMenuItem(Res.GetString("332D4945-0453-4838-8B48-F1B14C5C38DE", "Job"));
					var controller = WorkflowProviderHelper.GetControllerForWorkflowType(workflowProvider.WorkflowType);

					if (!TryAddSubMenu(jobMenu, controller?.ModuleID, workflowProvider as BusinessObject))
					{
						var disabledItem = new ZMenuItem(Res.GetString("3B52E56B-BFBB-4CEF-BC58-4B90FBEA8CE1", "Operational Actions are not enabled for Process Type '{0}'", workflowProvider.WorkflowType))
						{
							Enabled = false
						};

						jobMenu.MenuItems.Add(disabledItem);
					}

					menuItems.Add(jobMenu);
				}

				return menuItems;
			});
		}

		static bool TryAddSubMenu(ZMenuItem rootMenu, ModuleIdentifier moduleIdentifier, BusinessObject target)
		{
			if (target == null || moduleIdentifier == null)
			{
				return false;
			}

			OperationalActionContext context = null;

			using (var module = ZFilterModule.GetZFilterModule(moduleIdentifier) as ZFilterGridModule)
			{
				var operationalActionSupportable = module as IOperationalActionSupportable;

				if (operationalActionSupportable == null)
				{
					return false;
				}

				context = new OperationalActionContext(operationalActionSupportable.OperationalActionSupporter, module.ID.Description, module.WorkflowType);
			}

			var menuItemGenerator = new OperationalActionsMenuItemGenerator(new ReadOnlyBusinessObjectFactory(), new[] { target }, context);
			var items = menuItemGenerator.Generate(forRoot: false);

			if (items == null || items.Length == 0)
			{
				return false;
			}

			rootMenu.MenuItems.AddRange(items);
			return true;
		}
	}
}
