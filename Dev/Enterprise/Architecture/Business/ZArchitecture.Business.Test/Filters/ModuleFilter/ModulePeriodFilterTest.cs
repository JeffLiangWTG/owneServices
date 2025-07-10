using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(ModulePeriodFilter))]
	sealed class ModulePeriodFilterTest : ModuleTextFilterTest
	{
		protected override ModuleTextFilter GetNewModuleFilter()
		{
			return new ModulePeriodFilter("moo", DummyBizoSchema.Z0_Description);
		}

		protected override FilterCategory ExpectedDefaultCategory
		{
			get { return FilterCategories.NumbersAndReferences; }
		}
	}
}
