using System.Linq;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(ModuleCardinalityFilter))]
	sealed class ModuleCardinalityFilterTest : ModuleFilterTestCase<ModuleCardinalityFilter>
	{
		#region TestIsExpensiveQuery

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		#endregion

		#region Implementation

		protected override ModuleCardinalityFilter GetNewModuleFilter()
		{
			return new ModuleCardinalityFilter("moo", "CARDINALITY", null, null);
		}

		protected override FilterCategory ExpectedDefaultCategory
		{
			get { return FilterCategories.Execution; }
		}

		protected override string[] GetPropertiesExcludedFromCacheInvalidationTest(ModuleCardinalityFilter filter)
		{
			return base.GetPropertiesExcludedFromCacheInvalidationTest(filter).Concat(new[] { "Item" }).ToArray();
		}

		#endregion
	}
}
