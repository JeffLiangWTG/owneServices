using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using GlowIndexQueryService.Business;
using NUnit.Framework;
using static Enterprise.ZArchitecture.Business.ModuleNumberRangeFilter;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(IndexSearchModuleNumberRangeFilter))]
	sealed class IndexSearchModuleNumberRangeFilterTest : IndexSearchModuleFilterTestCase<IndexSearchModuleNumberRangeFilter>
	{
		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.NumbersAndReferences;

		protected override ModuleFilter GetNewModuleFilter()
		{
			return new IndexSearchModuleNumberRangeFilter(SearchField.Create("moo"));
		}

		protected override string[] GetPropertiesExcludedFromCacheInvalidationTest(ModuleFilter modulefilter)
		{
			var filter = modulefilter as IndexSearchModuleNumberRangeFilter;
			return base.GetPropertiesExcludedFromCacheInvalidationTest(filter).Concat(new[] { nameof(filter.MinValue), nameof(filter.MaxValue), nameof(filter.DefaultPropertySearch), nameof(filter.GreaterThanOrEqualToDefaultProperty), nameof(filter.LessThanOrEqualToDefaultProperty), nameof(filter.EqualToDefaultProperty), nameof(filter.BetweenDefaultProperty1), nameof(filter.BetweenDefaultProperty2) }).ToArray();
		}

		protected override Dictionary<string, IZType> GetDummyValuesForCacheInvalidationTest(ModuleFilter modulefilter)
		{
			var filter = modulefilter as IndexSearchModuleNumberRangeFilter;
			var values = base.GetDummyValuesForCacheInvalidationTest(filter);
			values.Add(nameof(filter.Property1), new ZDecimal(10m));
			values.Add(nameof(filter.Property2), new ZDecimal(20m));

			return values;
		}

		public override void TestGetGlowIndexQuery()
		{
			var filter = new IndexSearchModuleNumberRangeFilter(SearchField.Create("moo"));

			filter.PropertySearch = SearchTexts.EqualTo.GetUnresolvedString();
			filter.Property1 = 5;
			AssertEquals(true, filter.IsEqualToSearch);
			AssertEquals("(moo eq 5)", filter.GetGlowIndexQuery().ToUrlComponent());

			filter.PropertySearch = SearchTexts.Between.GetUnresolvedString();
			filter.Property1 = 5;
			filter.Property2 = 10;
			AssertEquals("((moo ge 5) and (moo le 10))", filter.GetGlowIndexQuery().ToUrlComponent());

			filter.PropertySearch = SearchTexts.LessThanOrEqualTo.GetUnresolvedString();
			filter.Property2 = 10;
			AssertEquals("(moo le 10)", filter.GetGlowIndexQuery().ToUrlComponent());

			filter.PropertySearch = SearchTexts.GreaterThanOrEqualTo.GetUnresolvedString();
			filter.Property1 = 5;
			AssertEquals("(moo ge 5)", filter.GetGlowIndexQuery().ToUrlComponent());
		}

		public void TestDecimals()
		{
			var filter = new IndexSearchModuleNumberRangeFilter(SearchField.Create("moo"));
			AssertEquals("Default Decimals is 0", (byte)0, filter.Decimals);

			var field = new SearchField("NUM", null, typeof(int), uiHidden: false, isUtcTime: false, scale: 2);
			filter = new IndexSearchModuleNumberRangeFilter(field);
			AssertEquals((byte)2, filter.Decimals);
		}
	}

	[TestedType(typeof(IndexSearchModuleNumberRangeFilter))]
	sealed class IndexSearchModuleNumberRangeFilter_ModuleFilterTest : ModuleNumberRangeFilterTest
	{
		protected override ModuleNumberRangeFilter GetNewModuleFilter()
		{
			var field = SearchField.Create("moo");
			return new IndexSearchModuleNumberRangeFilter(field);
		}
	}
}
