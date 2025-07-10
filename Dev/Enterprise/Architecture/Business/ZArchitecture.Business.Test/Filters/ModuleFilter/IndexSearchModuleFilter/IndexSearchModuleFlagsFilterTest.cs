using System.Linq;
using GlowIndexQueryService.Business;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(IndexSearchModuleFlagsFilter))]
	sealed class IndexSearchModuleFlagsFilterTest : IndexSearchModuleFilterTestCase<IndexSearchModuleFlagsFilter>
	{
		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.StatusAndFlags;

		protected override ModuleFilter GetNewModuleFilter()
		{
			var field = SearchField.Create("moo", "Flag1");
			return new IndexSearchModuleFlagsFilter(field);
		}

		public override void TestGetGlowIndexQuery()
		{
			var filter = new IndexSearchModuleFlagsFilter(SearchField.Create("moo", "Flag1"));

			filter.Property0 = true;
			AssertEquals("((moo eq true) or (moo eq null))", filter.GetGlowIndexQuery().ToUrlComponent());

			filter.Property0 = false;
			AssertEquals("(moo eq false)", filter.GetGlowIndexQuery().ToUrlComponent());
		}

		protected override string[] GetPropertiesExcludedFromCacheInvalidationTest(ModuleFilter modulefilter)
		{
			var filter = modulefilter as IndexSearchModuleFlagsFilter;
			return base.GetPropertiesExcludedFromCacheInvalidationTest(filter).Concat(new[] { "Item" }).ToArray(); // This will cause the test to skip the indexer, which is problematic and sets values for properties that are tested independently anyway.
		}
	}

	[TestedType(typeof(IndexSearchModuleFlagsFilter))]
	sealed class IndexSearchModuleFlagsFilter_ModuleFilterTest : ModuleFlagsFilterTest
	{
		protected override ModuleFlagsFilter GetNewModuleFilter()
		{
			return new IndexSearchModuleFlagsFilter(SearchField.Create("moo", "Flag"));
		}
	}
}
