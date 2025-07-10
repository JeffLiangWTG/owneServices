using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(ModuleNumberRangeSubFilter))]
	sealed class ModuleNumberRangeSubFilterTest : ModuleNumberRangeFilterTest
	{
		public void TestPropertiesIfSetToGreaterThan()
		{
			var filter = new ModuleNumberRangeSubFilter("Filter", DummyBizoSchema.Z0_Number);
			var filterStripBizO = new DummyFilterStripBusinessObject();
			filterStripBizO.AddModuleFilterForTest(filter);

			var savedFilter = filterStripBizO.SaveLayout("savedFilter");

			filterStripBizO.LoadLayout(savedFilter);

			var loadedFilter = (ModuleNumberRangeFilter)filterStripBizO[filter.Description];
			AssertNotEquals("PreCondition", new ZString(ModuleNumberRangeSubFilter.SearchTexts.GreaterThan.GetUnresolvedString()), loadedFilter.PropertySearch);
			AssertNotEquals("PreCondition", 3m, loadedFilter.Property1);
			AssertNotEquals("PreCondition", (ZDecimal)int.MaxValue, loadedFilter.Property2);

			filter.PropertySearch = new ZString(ModuleNumberRangeSubFilter.SearchTexts.GreaterThan.GetUnresolvedString());
			filter.GreaterThanOrEqualToDefaultProperty = 3;

			AssertEquals(new ZString(ModuleNumberRangeSubFilter.SearchTexts.GreaterThan.GetUnresolvedString()), loadedFilter.PropertySearch);
			AssertEquals(3m, loadedFilter.Property1);
			AssertEquals((ZDecimal)int.MaxValue, loadedFilter.Property2);
		}

		public new void TestClearResetsToDefaults()
		{
			base.TestClearResetsToDefaults();

			var filter = new ModuleNumberRangeSubFilter("Filter", DummyBizoSchema.Z0_Number);
			filter.GreaterThanDefaultProperty = 3;

			var filterStripBizO = new DummyFilterStripBusinessObject();
			filterStripBizO.AddModuleFilterForTest(filter);

			var savedFilter = filterStripBizO.SaveLayout("savedFilter");

			filterStripBizO.LoadLayout(savedFilter);

			var loadedFilter = (ModuleNumberRangeSubFilter)filterStripBizO[filter.Description];
			loadedFilter.DefaultPropertySearch = new ZString(ModuleNumberRangeSubFilter.SearchTexts.GreaterThan.GetUnresolvedString());
			loadedFilter.Clear();
			AssertEquals(loadedFilter.PropertySearch, new ZString(ModuleNumberRangeSubFilter.SearchTexts.GreaterThan.GetUnresolvedString()));
			AssertEquals(3m, loadedFilter.Property1);
			AssertEquals((ZDecimal)int.MaxValue, loadedFilter.Property2);
		}

		public new void TestPropertySearch_ListContainsSpecifiedDateRange()
		{
			base.TestPropertySearch_ListContainsSpecifiedDateRange();
			AssertEquals(true, Filter.PropertySearch_List.ContainsCode(ModuleNumberRangeSubFilter.SearchTexts.GreaterThan));
		}

		protected override int CountOfPropertySearch_List => 5;

		protected override string[] GetPropertiesExcludedFromCacheInvalidationTest(ModuleNumberRangeFilter filter)
		{
			return base.GetPropertiesExcludedFromCacheInvalidationTest(filter).Concat(new[] { "GreaterThanDefaultProperty" }).ToArray();
		}

		protected override ModuleNumberRangeFilter GetNewModuleFilter()
		{
			return new ModuleNumberRangeSubFilter("moo", DummyBizoSchema.Z0_Number);
		}
	}
}
