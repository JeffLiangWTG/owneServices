using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	public sealed class DummyFilterStripBusinessObjectForDefaultsTesting : DummyFilterStripBusinessObject
	{
		public DummyFilterStripBusinessObjectForDefaultsTesting()
		{
			((IFilterStripBusinessObjectInternals)this).LayoutContext = LayoutsTestDataHelper.TestModuleID;
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var collection = new ModuleFilterCollection();

			var someFilter = new ModuleTextFilter("someFilter", DummyBizoSchema.GenericStringSchemaColumn);
			var anotherSomeFilter = new ModuleTextFilter("anotherSomeFilter", DummyBizoSchema.GenericStringSchemaColumn);
			var nVarCharMaxFilter = new ModuleTextFilter("nVarCharMaxFilter", DummyBizoSchema.Z0_NVarCharMax);

			var hasInitialCodeForSearchFilter = new ModuleTextFilter("hasInitialCodeForSearchFilter", DummyBizoSchema.Z0_Code);
			hasInitialCodeForSearchFilter.Prefix = "Prefix";
			var hasDefaultValueFilter = new ModuleTextFilter("hasDefaultValueFilter", DummyBizoSchema.GenericStringSchemaColumn);

			collection.AddCustomFilter(someFilter);
			collection.AddCustomFilter(anotherSomeFilter);
			collection.AddCustomFilter(nVarCharMaxFilter);

			collection.AddCustomFilter(hasInitialCodeForSearchFilter);
			collection.AddCustomFilter(hasDefaultValueFilter);

			return collection;
		}
	}
}
