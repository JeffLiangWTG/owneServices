namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class DummyFilterBusinessObjectWithForeignCollectionFilter : DummyFilterBusinessObject
	{
		public DummyFilterBusinessObjectWithForeignCollectionFilter(ModuleGuidForeignCollectionFilter filter)
		{
			foreignCollectionFilter = filter;
		}

		readonly ModuleGuidForeignCollectionFilter foreignCollectionFilter;

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = base.GetModuleFiltersCore();
			filters.AddFilter(foreignCollectionFilter);

			return filters;
		}
	}
}
