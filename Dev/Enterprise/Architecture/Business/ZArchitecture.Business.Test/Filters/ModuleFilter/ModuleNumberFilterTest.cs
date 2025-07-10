using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business.Testing
{
	public abstract class ModuleNumberFilterTest : ModuleTextFilterTest
	{
		protected override ModuleTextFilter GetNewModuleFilter()
		{
			return new ModuleNumberFilter("moo", DummyBizoSchema.Z0_Description);
		}

		protected override FilterCategory ExpectedDefaultCategory
		{
			get { return FilterCategories.NumbersAndReferences; }
		}
	}
}
