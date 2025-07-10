using System.Linq;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(ModuleUnionOrOrFilter))]
	sealed class ModuleUnionOrOrFilterTest : ModuleFilterTestCase<ModuleUnionOrOrFilter>
	{
		#region TestIsExpensiveQuery

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		#endregion

		#region Implementation

		protected override ModuleUnionOrOrFilter GetNewModuleFilter()
		{
			return new ModuleUnionOrOrFilter("moo", "UNION?", null, null);
		}

		protected override FilterCategory ExpectedDefaultCategory
		{
			get { return FilterCategories.Other; }
		}

		protected override string[] GetPropertiesExcludedFromCacheInvalidationTest(ModuleUnionOrOrFilter filter)
		{
			return base.GetPropertiesExcludedFromCacheInvalidationTest(filter).Concat(new[] { "Item" }).ToArray(); // This will cause the test to skip the indexer, which is problematic and sets values for properties that are tested independently anyway.
		}

		#endregion
	}
}
