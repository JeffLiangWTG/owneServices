using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class DummyFilterBusinessObjectWithDateFilter : DummyFilterBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = base.GetModuleFiltersCore();
			filters.AddDateFilter("Dat Date", DummyBizoSchema.Z0_Date);

			return filters;
		}
	}
}
