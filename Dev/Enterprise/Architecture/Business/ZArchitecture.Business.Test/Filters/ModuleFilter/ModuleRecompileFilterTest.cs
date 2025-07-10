using System.Linq;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(ModuleRecompileFilter))]
	sealed class ModuleRecompileFilterTest : ModuleFilterTestCase<ModuleRecompileFilter>
	{
		#region TestIsExpensiveQuery

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		#endregion

		#region Implementation

		protected override ModuleRecompileFilter GetNewModuleFilter()
		{
			return new ModuleRecompileFilter("moo", "RECOMPILE", null, null);
		}

		protected override FilterCategory ExpectedDefaultCategory
		{
			get { return FilterCategories.Execution; }
		}

		protected override string[] GetPropertiesExcludedFromCacheInvalidationTest(ModuleRecompileFilter filter)
		{
			return base.GetPropertiesExcludedFromCacheInvalidationTest(filter).Concat(new[] { "Item" }).ToArray();
		}

		#endregion
	}
}
