using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(ModuleFilterWithSubDescriptions))]
	sealed class ModuleFilterWithListsTest : ModuleFilterWithSubDescriptionsTest
	{
		protected override ModuleFilterWithSubDescriptions GetNewModuleFilter()
		{
			var list = new StmNoteNonDependentCollection(Factory);
			return new DummyModuleFilterWithLists("moo", DummyBizoSchema.Z0_Description, list, DummyBizoSchema.Z0_Description, list);
		}

		protected override FilterCategory ExpectedDefaultCategory
		{
			get { return FilterCategories.Other; }
		}
	}
}
