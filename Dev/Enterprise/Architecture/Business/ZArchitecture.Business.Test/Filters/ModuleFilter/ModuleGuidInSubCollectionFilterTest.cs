using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(ModuleGuidInSubCollectionFilter))]
	sealed class ModuleGuidInSubCollectionFilterTest : ModuleFilterTestCase<ModuleGuidInSubCollectionFilter>
	{
		#region Implementation

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		protected override FilterCategory ExpectedDefaultCategory
		{
			get { return FilterCategories.Other; }
		}

		protected override ModuleGuidInSubCollectionFilter GetNewModuleFilter()
		{
			var list = new StmNoteNonDependentCollection(Factory);
			return new ModuleGuidInSubCollectionFilter("moo", FilterCategories.Other, ModuleIDs.JobShipment, new GetGuidQueryWithNotIn((_, x_) => new ZQuery()), list);
		}

		#endregion
	}
}
