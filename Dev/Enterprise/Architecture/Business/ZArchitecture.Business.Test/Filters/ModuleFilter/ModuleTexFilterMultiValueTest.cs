using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
//using Modules;
//using static ModuleFilterWithListAndComparisonOperators<ZString>;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class ModuleTexFilterMultiValueTest : TestCaseWithFactory
	{
		public void TestCanGroup()
		{
			var filter = new ModuleTextFilter("Code", DummyBizoSchema.Z0_Code);
			var multiValueFilter = (ISupportMultiValuesFilter)filter;

			filter.Property = "ABC";
			filter.OrCategory = FilterOrCategory.None;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			Assert("Cannot group filter with operator = and sql operator AND", !multiValueFilter.CanGroup);

			filter.OrCategory = FilterOrCategory.Red;
			Assert("Can group filter with operator = and sql operator OR -> will use IN", multiValueFilter.CanGroup);

			filter.Property = "";
			Assert("Cannot group empty filter", !multiValueFilter.CanGroup);
			filter.Property = "ABC";

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			Assert("Cannot group filter with operator <> and sql operator OR", !multiValueFilter.CanGroup);

			filter.OrCategory = FilterOrCategory.None;
			Assert("Can group filter with operator <> and sql operator ARD -> will use NOT IN", multiValueFilter.CanGroup);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			Assert("Cannot group filter with operator LIKE", !multiValueFilter.CanGroup);
		}

		public void TestCanGroupWithQueryDelegate()
		{
			var filter = new ModuleTextFilter("Code", s => new ZQuery(), new CodeDescriptionPairList());
			var multiValueFilter = (ISupportMultiValuesFilter)filter;

			filter.Property = "ABC";
			filter.OrCategory = FilterOrCategory.Red;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			Assert("Cannot group filter with QueryDelegate but without MultiValueQueryDelegate", !multiValueFilter.CanGroup);

			filter.MultiValueQueryDelegate = (o, c) => new ZQuery();
			Assert("Can group filter with QueryDelegate and MultiValueQueryDelegate", multiValueFilter.CanGroup);
		}

		public void TestCanGroupWithCheckingTableName()
		{
			var filterStrip = new DummyFilterStripBusinessObject();
			filterStrip.SetActiveStatusFilter(JobShipmentSchema.JS_ShipmentStatus, false);
			var filter = new ModuleTextFilter("Code", DummyBizoSchema.Z0_Code);
			var multiValueFilter = (ISupportMultiValuesFilter)filter;

			filter.Property = "ABC";
			filter.OrCategory = FilterOrCategory.Red;
			filter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact;
			Assert("Can group filter with operator = and sql operator OR -> will use IN", multiValueFilter.CanGroup);

			filter.ActiveModuleFiltersProviderHelper = filterStrip;
			Assert("Cannot group filter with different table name", !multiValueFilter.CanGroup);

			filterStrip.SetActiveStatusFilter(DummyBizoSchema.Z0_NVarChar, false);
			Assert("Can group filter with same table name", multiValueFilter.CanGroup);
		}

		public void TestGroupKey()
		{
			var filter = new ModuleTextFilter("Code", DummyBizoSchema.Z0_Code);
			var multiValueFilter = (ISupportMultiValuesFilter)filter;

			filter.Property = "ABC";
			filter.OrCategory = FilterOrCategory.None;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			AssertEquals("exact", multiValueFilter.GroupKey);

			filter.OrCategory = FilterOrCategory.Red;
			AssertEquals("exact", multiValueFilter.GroupKey);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			AssertEquals("not equal", multiValueFilter.GroupKey);
		}

		public void TestGetCombinedValue()
		{
			var filter1 = new ModuleTextFilter("Code", DummyBizoSchema.Z0_Code);
			filter1.Property = "AAA";
			filter1.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;

			var filter2 = new ModuleTextFilter("Code", DummyBizoSchema.Z0_Code);
			filter2.Property = "BBB";

			var filter3 = new ModuleTextFilter("Code", DummyBizoSchema.Z0_Code);
			filter3.Property = "CCC";

			var multiValueFilter = (ISupportMultiValuesFilter)filter1;

			var combinedValue = multiValueFilter.GetCombinedValue(new ModuleFilter[] { filter1, filter2, filter3 });

			Assert(combinedValue is List<ZString>);
			var combinedValueList = (List<ZString>)combinedValue;

			AssertEquals(3, combinedValueList.Count);
			AssertEquals("AAA", combinedValueList[0]);
			AssertEquals("BBB", combinedValueList[1]);
			AssertEquals("CCC", combinedValueList[2]);
		}

		public void TestGetCombinedQuery()
		{
			var filter = new ModuleTextFilter("Code", DummyBizoSchema.Z0_Code);
			filter.Property = "ABC";
			filter.OrCategory = FilterOrCategory.Red;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;

			var multiValueFilter = (ISupportMultiValuesFilter)filter;
			var combinedValue = new ZString[] { "AAA", "BBB", "CCC" };

			AssertEquals("(Z0_Code in ('AAA', 'BBB', 'CCC'))", multiValueFilter.GetCombinedQuery(combinedValue).LiteralTextADO);

			filter.OrCategory = FilterOrCategory.None;

			AssertEquals("Current Or Category should not matter", "(Z0_Code in ('AAA', 'BBB', 'CCC'))", multiValueFilter.GetCombinedQuery(combinedValue).LiteralTextADO);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;

			AssertEquals("(Z0_Code not in ('AAA', 'BBB', 'CCC'))", multiValueFilter.GetCombinedQuery(combinedValue).LiteralTextADO);
		}

		public void TestGetCombinedQueryWithMultiValueQueryDelegate()
		{
			var filter = new ModuleTextFilter("Code", s => new ZQuery(), new CodeDescriptionPairList());
			filter.Property = "ABC";
			filter.OrCategory = FilterOrCategory.Red;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter.MultiValueQueryDelegate = (value, comparisonOperator) => new ZQuery(DummyBizoSchema.Z0_Description, comparisonOperator, value);

			var filterStrip = new DummyFilterStripBusinessObject();
			filterStrip.SetActiveStatusFilter(JobShipmentSchema.JS_ShipmentStatus, false);
			filter.ActiveModuleFiltersProviderHelper = filterStrip;
			var multiValueFilter = (ISupportMultiValuesFilter)filter;
			CombineAssertions(() =>
			{
				Assert("Force group filters with MultiValueQueryDelegate", multiValueFilter.CanGroup);

				var combinedValue = new ZString[] { "AAA", "BBB", "CCC" };
				AssertEquals("(Z0_Description in ('AAA', 'BBB', 'CCC'))", multiValueFilter.GetCombinedQuery(combinedValue).LiteralTextADO);

				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
				AssertEquals("(Z0_Description not in ('AAA', 'BBB', 'CCC'))", multiValueFilter.GetCombinedQuery(combinedValue).LiteralTextADO);
			});
		}

		public void TestActiveStatusPairList()
		{
			var codePairList = new CodeDescriptionPairList();
			codePairList.AddPair("Active", "Show Active Only");
			codePairList.AddPair("Inactive", "Show Inactive Only");
			codePairList.AddPair("All", "Show all records");
			var filter = new DummyFilterBusinessObject();
			AssertContainsExactElementsInAnyOrder(codePairList, filter.CancelledStatusList);
		}
	}
}
