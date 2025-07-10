using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.GUI.Test
{
	public static class CurrentTaskOnlyFilterTestHelper
	{
		#region Task Filter

		public static void AddFilterToTaskFilter(StmModuleFilter taskFilter)
		{
			FilterStripsTestHelper.AddFilterStrips(taskFilter, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Current Task Only",
				FilterStripValueSetter = (filter) => ((ModuleFlagsFilter)filter).Property0 = true
			});
		}

		public static void AddNestedFilterToTaskFilter(StmModuleFilter taskFilter)
		{
			FilterStripsTestHelper.AddFilterStrips(taskFilter, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Parent Job",
				FilterStripValueSetter = filter =>
				{
					var parentJobFilter = (ModuleGuidModuleSpecifiedFilter)filter;
					parentJobFilter.SelectedModule = ModuleIDs.Organisation.Name;

					var tasksModuleFilter = parentJobFilter.SelectedFilters.AddFilterStrip<ModuleGuidForeignCollectionFilter>("Tasks");
					tasksModuleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;

					var currentTaskOnlyFilter = tasksModuleFilter.SelectedFilters.AddFilterStrip<ModuleFlagsFilter>("Current Task Only");
					currentTaskOnlyFilter.Property0 = true;
				},
				ComparisonOperatorSetter = filter => ((ModuleGuidFilter)filter).ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch
			});
		}

		#endregion

		#region Workflow Filter

		public static void AddUserDefinedFilterToWorkflowFilter(StmModuleFilter workflowFilter)
		{
			using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.ProcessHeader))
			{
				var filterBizO = module.FilterBusinessObject;
				var tasksFilter = filterBizO.AddFilterStrip<ModuleGuidForeignCollectionFilter>("Tasks");
				tasksFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;
				var currentTaskOnlyFilter = tasksFilter.SelectedFilters.AddFilterStrip<ModuleFlagsFilter>("Current Task Only");
				currentTaskOnlyFilter.Property0 = true;

				FilterStripsTestHelper.SaveFilterLayout(module.FilterBusinessObject, "User Defined Filter X", isPublished: true, isPublishedGlobal: true, isUserDefinedFilter: true);
			}

			FilterStripsTestHelper.AddFilterStrip<ModuleUserDefinedFilter>(workflowFilter, "[USR]User Defined Filter X");
		}

		public static void AddFilterToWorkflowFilter(StmModuleFilter workflowFilter)
		{
			FilterStripsTestHelper.AddFilterStrips(workflowFilter, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Tasks",
				FilterStripValueSetter = (filter) =>
				{
					var tasksFilter = (ModuleGuidForeignCollectionFilter)filter;
					var currentTaskOnlyFilter = tasksFilter.SelectedFilters.AddFilterStrip<ModuleFlagsFilter>("Current Task Only");
					currentTaskOnlyFilter.Property0 = true;
				},
				ComparisonOperatorSetter = filter => ((ModuleGuidForeignCollectionFilter)filter).ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch,
			});
		}

		#endregion
	}
}
