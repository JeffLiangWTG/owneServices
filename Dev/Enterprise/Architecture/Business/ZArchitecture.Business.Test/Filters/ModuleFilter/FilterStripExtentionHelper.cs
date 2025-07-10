
namespace Enterprise.ZArchitecture.Business.Testing
{
	public static class FilterStripExtentionHelper
	{
		public static ModuleFilter CreateDuplicateFor(this FilterStripBusinessObject filterBizo, string description)
		{
			var duplicateFilter = filterBizo.ModuleFilters.GetVisibleModuleFilterAndDuplicateAndDeactivateIfActive(description);
			duplicateFilter.IsActive = true;

			return duplicateFilter;
		}
	}
}
