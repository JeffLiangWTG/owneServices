using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	abstract class DependencyMenuItemBase : ZLazyPopulatingMenuItem
	{
		protected DependencyMenuItemBase(ZGrid parentGrid, DependencyMenuItemStrategy strategy)
			: this(() => (ProcessHeader)parentGrid.ListManager.GetCurrent(), strategy)
		{
		}

		protected DependencyMenuItemBase(ProcessHeader workflow, DependencyMenuItemStrategy strategy)
			: this(() => workflow, strategy)
		{
		}

		DependencyMenuItemBase(Func<ProcessHeader> sourceWorkflowGetter, DependencyMenuItemStrategy strategy)
			: base(strategy.RootMenuItemText, MenuItemFunc(sourceWorkflowGetter, strategy))
		{
		}

		static Func<IEnumerable<ZMenuItem>> MenuItemFunc(Func<ProcessHeader> sourceWorkflowGetter, DependencyMenuItemStrategy strategy)
		{
			return new Func<IEnumerable<ZMenuItem>>(() =>
			{
				var workflow = sourceWorkflowGetter();
				var result = new List<ZMenuItem>();

				if (workflow == null)
				{
					result.Add(new ZMenuItem(Res.GetString("2b0e0959-8335-46b3-bce1-f2f3b896125c", "Please select a workflow"))
					{
						Enabled = false,
					});
				}
				else
				{
					result.Add(new AddNewDependencyMenuItem(workflow, strategy));

					AddNetworkDependenciesMenuItems(workflow, result, strategy);
				}

				return result;
			});
		}

		static void AddNetworkDependenciesMenuItems(ProcessHeader workflow, List<ZMenuItem> result, DependencyMenuItemStrategy strategy)
		{
			result.Add(new ZMenuItem(Separator));

			var removeMenuItem = new ZMenuItem(Res.GetString("0bcfff4f-be13-4fac-91bb-6c268add6624", "Remove"));
			var openMenuItem = new ZMenuItem(strategy.OpenItemMenuItemText);
			var editMenuItem = new ZMenuItem(strategy.EditItemMenuItemText);

			result.Add(removeMenuItem);
			result.Add(openMenuItem);
			result.Add(editMenuItem);

			var dependencies = strategy.GetFullNetworkDependencies(workflow).ToArray();

			if (dependencies.Length > 0)
			{
				var dependenciesWithinJob = dependencies.Where(p => p.ProcessHeader.IsInSameJob(workflow)).ToArray();
				AddAllDependencySubMenuItems(dependenciesWithinJob, workflow, strategy, removeMenuItem, openMenuItem, editMenuItem);

				var dependenciesOutsideJob = dependencies.Where(p => !p.ProcessHeader.IsInSameJob(workflow)).ToArray();
				AddAllDependencySubMenuItems(dependenciesOutsideJob, workflow, strategy, removeMenuItem, openMenuItem, editMenuItem);
			}
			else
			{
				var noDependenciesMenuItemCreator = new Func<ZMenuItem>(() => new ZMenuItem(strategy.NoDependenciesMenuItemText)
				{
					Enabled = false,
				});

				removeMenuItem.MenuItems.Add(noDependenciesMenuItemCreator());
				openMenuItem.MenuItems.Add(noDependenciesMenuItemCreator());
				editMenuItem.MenuItems.Add(noDependenciesMenuItemCreator());
			}
		}

		static void AddAllDependencySubMenuItems(LinkedProcessHeader[] dependencies, ProcessHeader sourceWorkflow, DependencyMenuItemStrategy strategy,
			ZMenuItem removeMenuItem, ZMenuItem openMenuItem, ZMenuItem editMenuItem)
		{
			if (dependencies.Length > 0 && removeMenuItem.MenuItems.Count > 0)
			{
				removeMenuItem.MenuItems.Add(Separator);
				openMenuItem.MenuItems.Add(Separator);
				editMenuItem.MenuItems.Add(Separator);
			}

			foreach (var prereq in dependencies)
			{
				removeMenuItem.MenuItems.Add(new RemoveDependencyMenuItem(prereq, sourceWorkflow, strategy));
				openMenuItem.MenuItems.Add(new OpenDependencyMenuItem(prereq, sourceWorkflow, strategy));
				editMenuItem.MenuItems.Add(new EditDependencyMenuItem(prereq, sourceWorkflow, strategy));
			}
		}
	}
}
