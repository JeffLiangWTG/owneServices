using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	sealed class DummyDependentFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();

			result.AddGuidFilter("Dummy", DummyModuleIDs.Dummy, DummyDependentBizoSchema.ZD1_Z0, new DummyBusinessObjectCollection(Factory));
			result.AddCustomFilter(new ModuleGuidModuleSpecifiedFilter("Dummy with Specified Module", DummyDependentBizoSchema.ZD1_Z0, new[] { DummyModuleIDs.Dummy }));
			result.AddNumberRangeFilter("ZD1_Number", DummyDependentBizoSchema.ZD1_Number);
			result.AddNkFilter("DummyNk", DummyDependentBizoSchema.ZD1_Code, DummyModuleIDs.Dummy, new DummyBusinessObjectCollection(Factory));
			result.AddTextAndNkFilter("DummyTextAndNk", DummyDependentBizoSchema.ZD1_Code, DummyModuleIDs.Dummy, DummyDependentBizoSchema.ZD1_Code, new DummyBusinessObjectCollection(Factory));

			var invisibleCodeFilter = result.AddTextFilter("Always Applied Code", DummyDependentBizoSchema.ZD1_Code);
			invisibleCodeFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
			invisibleCodeFilter.Visibility = FilterVisibility.AlwaysApplied;
			invisibleCodeFilter.DefaultProperty = "ZZZ";

			return result;
		}

		public DummyDependentFilterBusinessObject()
		{
			QueryObjectType = typeof(DummyDependantBusinessObject);
		}
	}
}
