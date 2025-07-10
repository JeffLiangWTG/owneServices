using Enterprise.ZArchitecture.Business;

namespace Enterprise.ServiceManager.Module
{
	public class ProcessControllerFilterBusinessObject : FilterStripBusinessObject
	{
		protected override bool IsActiveStatusFilterAlwaysApplied() => false;
		protected override bool ShouldAddCustomSqlFilter => false;
		protected override bool ShouldAddUserDefinedFiltersCore => false;

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			DisableFiltersMatchForAuditUserFilters(filters);
			return filters;
		}

		void DisableFiltersMatchForAuditUserFilters(ModuleFilterCollection filters)
		{
			filters.FilterAdded += (sender, args) =>
			{
				if (args.AddedFilter is ModuleNkFilter filter && (filter.Description == FilterDescriptions.CreatingUser || filter.Description == FilterDescriptions.LastEditUser))
				{
					filter.SupportsFiltersMatchComparisonOperator = false;
				}
			};
		}
	}
}
