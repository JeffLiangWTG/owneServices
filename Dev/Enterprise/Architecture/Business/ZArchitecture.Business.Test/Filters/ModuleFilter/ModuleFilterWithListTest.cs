using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(ModuleFilterWithList))]
	sealed class ModuleFilterWithListTest : ModuleFilterTestCase<ModuleFilterWithList>
	{
		protected override ModuleFilterWithList GetNewModuleFilter()
		{
			var list = new StmNoteNonDependentCollection(Factory);
			return new DummyModuleFilterWithList("moo", DummyBizoSchema.Z0_Description, list);
		}

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		protected override FilterCategory ExpectedDefaultCategory
		{
			get { return FilterCategories.Other; }
		}
	}
}
