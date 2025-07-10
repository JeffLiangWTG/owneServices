using System.Linq;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace CargoWise.EntityFramework.Testing
{
	sealed class FilterBusinessObjectDefaultTest : TestCaseWithDummy
	{
		public void TestFilterBusinessObjectDefault()
		{
			FilterBusinessObjectDefault @default = new FilterBusinessObjectDefault("FilterName", "PropertyName", new ZString("Value"));
			AssertEquals("FilterName", @default.FilterName);
			AssertEquals("PropertyName", @default.PropertyName);
			AssertEquals("Value", @default.Value);
			AssertEquals(FilterOrCategory.None, @default.Category);
			AssertEquals(0, @default.Instance);
		}

		public void TestFilterBusinessObjectDefault_WithDelegate()
		{
			FilterBusinessObjectDefault @default = new FilterBusinessObjectDefault("FilterName", "PropertyName", delegate
			{ return (ZString)"Value"; });
			AssertEquals("FilterName", @default.FilterName);
			AssertEquals("PropertyName", @default.PropertyName);
			AssertEquals("Value", @default.Value);
		}

		public void TestFilterBusinessObjectDefault_IsRemovableForMultipleProperties()
		{
			var filterDefaults = new FilterBusinessObjectDefaults();
			var property1 = new FilterBusinessObjectDefault("FilterName", "Property", new ZString("Value1"), FilterOrCategory.Blue, instance: 0, false);
			var property2 = new FilterBusinessObjectDefault("FilterName", "Property", new ZString("Value2"), FilterOrCategory.Blue, instance: 1);

			filterDefaults.Add(property1);
			filterDefaults.Add(property2);

			AssertEquals("Property instance 0 is not removable", false, filterDefaults["FilterName:Property"].IsRemovable);
			AssertEquals("Property instance 1 is removable", true, filterDefaults["FilterName:Property:1"].IsRemovable);
		}

		public void TestCollectionConstructorAdds()
		{
			FilterBusinessObjectDefaults defaults = new FilterBusinessObjectDefaults();
			defaults.Add(new FilterBusinessObjectDefault(new ZString("Field"), new ZString("Value")));
			AssertNotNull(defaults["Field"]);
		}

		public void TestAdd()
		{
			FilterBusinessObjectDefault @default = new FilterBusinessObjectDefault(new ZString("Field"), new ZString("Value"));
			FilterBusinessObjectDefaults defaults = new FilterBusinessObjectDefaults();
			FilterBusinessObjectDefault default2 = new FilterBusinessObjectDefault(new ZString("Field2"), new ZString("Value2"));

			defaults.Add(@default);
			defaults.Add(default2);

			@default = defaults["Field"];
			AssertEquals(@default.PropertyName, new ZString("Field"));
			AssertEquals(@default.Value, new ZString("Value"));

			@default = defaults["Field2"];
			AssertEquals(@default.PropertyName, new ZString("Field2"));
			AssertEquals(@default.Value, new ZString("Value2"));
		}

		public void TestContainsDefaultFor()
		{
			FilterBusinessObjectDefaults defaults = new FilterBusinessObjectDefaults();
			defaults.Add(new FilterBusinessObjectDefault(new ZString("Field"), new ZString("Value")));
			Assert("Should be in coll'n", defaults.ContainsDefaultFor("Field"));
		}

		public void TestRemove()
		{
			FilterBusinessObjectDefault @default = new FilterBusinessObjectDefault(new ZString("Field"), new ZString("Value"));
			FilterBusinessObjectDefaults defaults = new FilterBusinessObjectDefaults();
			FilterBusinessObjectDefault default2 = new FilterBusinessObjectDefault(new ZString("Field2"), new ZString("Value2"));

			defaults.Add(@default);
			defaults.Add(default2);

			AssertEquals("Contains default1", true, defaults.ContainsDefaultFor(@default.PropertyName));
			AssertEquals("Contains default2", true, defaults.ContainsDefaultFor(default2.PropertyName));

			defaults.Remove(@default.PropertyName);
			AssertEquals("Doesn't contain default1", false, defaults.ContainsDefaultFor(@default.PropertyName));
			AssertEquals("Contains default2", true, defaults.ContainsDefaultFor(default2.PropertyName));

			defaults.Remove(default2.PropertyName);
			AssertEquals("Doesn't contain default1", false, defaults.ContainsDefaultFor(@default.PropertyName));
			AssertEquals("Doesn't contain default2", false, defaults.ContainsDefaultFor(default2.PropertyName));
		}

		public void TestRemoveAll()
		{
			FilterBusinessObjectDefault default1 = new FilterBusinessObjectDefault(new ZString("Field"), new ZString("Value"));
			FilterBusinessObjectDefault default2 = new FilterBusinessObjectDefault(new ZString("Field2"), new ZString("Value2"));
			FilterBusinessObjectDefaults defaults = new FilterBusinessObjectDefaults();

			defaults.Add(default1);
			defaults.Add(default2);

			AssertEquals("Contains default1", true, defaults.ContainsDefaultFor(default1.PropertyName));
			AssertEquals("Contains default2", true, defaults.ContainsDefaultFor(default2.PropertyName));

			defaults.RemoveAll();

			AssertEquals("Doesn't contain default1", false, defaults.ContainsDefaultFor(default1.PropertyName));
			AssertEquals("Doesn't contain default2", false, defaults.ContainsDefaultFor(default2.PropertyName));
		}

		public void TestEnumerator()
		{
			FilterBusinessObjectDefault @default = new FilterBusinessObjectDefault(new ZString("Field"), new ZString("Value"));
			FilterBusinessObjectDefaults defaults = new FilterBusinessObjectDefaults();
			FilterBusinessObjectDefault default2 = new FilterBusinessObjectDefault(new ZString("Field2"), new ZString("Value2"));

			defaults.Add(@default);
			defaults.Add(default2);

			int count = 0;
			foreach (FilterBusinessObjectDefault d in defaults)
			{
				count++;
				Assert("Should be default1 or 2", d.Equals(@default) || d.Equals(default2));
			}

			AssertEquals("Should have foreached each element", 2, count);
		}

		public void TestSetDynamicDefaultFilters()
		{
			var filterDefaults = new FilterBusinessObjectDefaults();
			var refreshTimes = 0;
			var defaultFiltersCount = 0;
			filterDefaults.SetDynamicDefaultFilters(() =>
			{
				refreshTimes++;
				filterDefaults.Add(new FilterBusinessObjectDefault(new ZString("Field"), new ZString("Value" + refreshTimes)));
			});

			AssertEquals(0, refreshTimes);
			AssertEquals(false, filterDefaults.ContainsDefaultFor("Field"));

			foreach (FilterBusinessObjectDefault filter in filterDefaults)
			{
				defaultFiltersCount++;
				AssertEquals("Field", filter.PropertyName);
				AssertEquals("Value1", filter.Value);
			}

			AssertEquals(1, refreshTimes);
			AssertEquals(1, defaultFiltersCount);

			defaultFiltersCount = 0;
			foreach (FilterBusinessObjectDefault filter in filterDefaults)
			{
				defaultFiltersCount++;
				AssertEquals("Field", filter.PropertyName);
				AssertEquals("Value2", filter.Value);
			}

			AssertEquals(2, refreshTimes);
			AssertEquals(1, defaultFiltersCount);
		}

		public void TestSetMultiplesOfSameFilter_SingleProperty()
		{
			var filterDefaults = new FilterBusinessObjectDefaults();
			var filter1 = new FilterBusinessObjectDefault("FilterName", "PropertyName", new ZString("Value1"), FilterOrCategory.Blue, instance: 1);
			var filter2 = new FilterBusinessObjectDefault("FilterName", "PropertyName", new ZString("Value2"), FilterOrCategory.Blue, instance: 2);

			filterDefaults.Add(filter1);
			filterDefaults.Add(filter2);

			AssertEquals("Two Filters should have been added", 2, filterDefaults.Count);
			AssertEquals("Filter 1 value incorrect", "Value1", filterDefaults["FilterName:PropertyName:1"].Value);
			AssertEquals("Filter 2 value incorrect", "Value2", filterDefaults["FilterName:PropertyName:2"].Value);
		}

		public void TestSetMultiplesOfSameFilter_MultipleProperties()
		{
			var filterDefaults = new FilterBusinessObjectDefaults();
			var filter1prop1 = new FilterBusinessObjectDefault("FilterName", "Property1", new ZString("Value1"), FilterOrCategory.Blue, instance: 1);
			var filter1prop2 = new FilterBusinessObjectDefault("FilterName", "Property2", new ZString("Value2"), FilterOrCategory.Blue, instance: 1);
			var filter2prop1 = new FilterBusinessObjectDefault("FilterName", "Property1", new ZString("Value3"), FilterOrCategory.Blue, instance: 2);
			var filter2prop2 = new FilterBusinessObjectDefault("FilterName", "Property2", new ZString("Value4"), FilterOrCategory.Blue, instance: 2);

			filterDefaults.Add(filter1prop1);
			filterDefaults.Add(filter1prop2);
			filterDefaults.Add(filter2prop1);
			filterDefaults.Add(filter2prop2);

			AssertEquals("4 Filter-Property pairs should have been added", 4, filterDefaults.Count);
			AssertEquals("Filter 1 Property 1 value incorrect", "Value1", filterDefaults["FilterName:Property1:1"].Value);
			AssertEquals("Filter 1 Property 2 value incorrect", "Value2", filterDefaults["FilterName:Property2:1"].Value);
			AssertEquals("Filter 2 Property 1 value incorrect", "Value3", filterDefaults["FilterName:Property1:2"].Value);
			AssertEquals("Filter 2 Property 2 value incorrect", "Value4", filterDefaults["FilterName:Property2:2"].Value);
		}

		public void TestSetFilterDefaultCategoryColor()
		{
			var filterDefaults = new FilterBusinessObjectDefaults();
			var filter1 = new FilterBusinessObjectDefault("Filter1", "Property1", new ZString("Value1"), FilterOrCategory.Blue);
			var filter2 = new FilterBusinessObjectDefault("Filter2", "Property1", new ZString("Value2"), FilterOrCategory.Green);
			var filter3 = new FilterBusinessObjectDefault("Filter3", "Property1", new ZString("Value3"), FilterOrCategory.Red);
			var filter4 = new FilterBusinessObjectDefault("Filter3", "Property2", new ZString("Value4"), FilterOrCategory.Red);
			var filter5 = new FilterBusinessObjectDefault("Filter5", "Property1", new ZString("Value5"));

			filterDefaults.Add(filter1);
			filterDefaults.Add(filter2);
			filterDefaults.Add(filter3);
			filterDefaults.Add(filter4);
			filterDefaults.Add(filter5);

			AssertEquals("Count", 5, filterDefaults.Count);
			AssertEquals("Filter 1 Category", Enterprise.ZArchitecture.Business.FilterOrCategory.Blue, filterDefaults["Filter1:Property1"].Category);
			AssertEquals("Filter 2 Category", Enterprise.ZArchitecture.Business.FilterOrCategory.Green, filterDefaults["Filter2:Property1"].Category);
			AssertEquals("Filter 3.1 Category", Enterprise.ZArchitecture.Business.FilterOrCategory.Red, filterDefaults["Filter3:Property1"].Category);
			AssertEquals("Filter 3.2 Category", Enterprise.ZArchitecture.Business.FilterOrCategory.Red, filterDefaults["Filter3:Property2"].Category);
			AssertEquals("Filter 5 Category", Enterprise.ZArchitecture.Business.FilterOrCategory.None, filterDefaults["Filter5:Property1"].Category);
		}

		public void TestSameNameWithDifferentSearchType()
		{
			var filterDefaults = new FilterBusinessObjectDefaults();
			var filterIndex = new FilterBusinessObjectDefault("Filter1", "Property1", new ZString("Value1"), searchType: SearchType.Index);
			var filterSql = new FilterBusinessObjectDefault("Filter1", "Property1", new ZString("Value2"), searchType: SearchType.Sql);

			filterDefaults.Add(filterIndex);
			filterDefaults.Add(filterSql);

			AssertEquals("Count", 2, filterDefaults.Count);
			AssertEquals("Filter 1 Index", SearchType.Index, filterDefaults["Filter1:Property1", SearchType.Index].SearchType);
			AssertEquals("Filter 2 Sql", SearchType.Sql, filterDefaults["Filter1:Property1"].SearchType);

			var indexDefaults = filterDefaults.GetDefaultsToUse(SearchType.Index);
			AssertEquals("Count", 1, indexDefaults.Count);
			AssertEquals("Filter 1 Index", true, indexDefaults.Cast<FilterBusinessObjectDefault>().All(d => d.SearchType == SearchType.Index));

			var sqlDefaults = filterDefaults.GetDefaultsToUse(SearchType.Sql);
			AssertEquals("Count", 1, sqlDefaults.Count);
			AssertEquals("Filter 2 Sql", true, sqlDefaults.Cast<FilterBusinessObjectDefault>().All(d => d.SearchType == SearchType.Sql));
		}

		public void TestRemoveBySearchType()
		{
			var filterDefaults = new FilterBusinessObjectDefaults();
			var filterIndex = new FilterBusinessObjectDefault("Filter1", "Property1", new ZString("Value1"), searchType: SearchType.Index);
			var filterSql = new FilterBusinessObjectDefault("Filter1", "Property1", new ZString("Value2"), searchType: SearchType.Sql);

			filterDefaults.Add(filterIndex);
			filterDefaults.Add(filterSql);

			AssertEquals("Count", 2, filterDefaults.Count);

			filterDefaults.Remove("Filter1:Property1", SearchType.Index);
			AssertEquals("Count", 1, filterDefaults.Count);
			AssertEquals("Filter 2 Sql", SearchType.Sql, filterDefaults["Filter1:Property1"].SearchType);

			var indexDefaults = filterDefaults.GetDefaultsToUse(SearchType.Index);
			AssertEquals("Count", 0, indexDefaults.Count);
		}

		public void TestHasDefaultsFor()
		{
			var filterDefaults = new FilterBusinessObjectDefaults();
			var filterIndex = new FilterBusinessObjectDefault("Filter1", "Property1", new ZString("Value1"), searchType: SearchType.Index);
			var filterSql = new FilterBusinessObjectDefault("Filter1", "Property1", new ZString("Value2"), searchType: SearchType.Sql);
			filterDefaults.Add(filterIndex);
			filterDefaults.Add(filterSql);

			Assert(filterDefaults.HasDefaultsFor(SearchType.Sql));
			Assert(filterDefaults.HasDefaultsFor(SearchType.Index));
		}
	}
}
