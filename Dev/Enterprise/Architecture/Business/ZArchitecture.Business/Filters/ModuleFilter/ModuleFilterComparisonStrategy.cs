namespace Enterprise.ZArchitecture.Business
{
	public class ModuleFilterComparisonStrategy
	{
		public virtual int CompareModuleFilters(IModuleFilter filter1, IModuleFilter filter2)
		{
			return filter1.FilterPriority.CompareTo(filter2.FilterPriority);
		}
	}
}
