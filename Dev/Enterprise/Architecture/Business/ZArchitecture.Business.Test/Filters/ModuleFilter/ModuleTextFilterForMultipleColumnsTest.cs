using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(ModuleTextFilterForMultipleColumns))]
	sealed class ModuleTextFilterForMultipleColumnsTest : ModuleFilterTestCase<ModuleTextFilterForMultipleColumns>
	{
		#region TestFilter

		public void TestFilter()
		{
			var dummy1 = Factory.New<DummyBusinessObject>();
			dummy1.Z0_Code = "a1";
			dummy1.Z0_Description = "bbb bbb";

			var dummy2 = Factory.New<DummyBusinessObject>();
			dummy2.Z0_Code = "b2";
			dummy2.Z0_Description = "ccc ccc";

			var dummy3 = Factory.New<DummyBusinessObject>();
			dummy3.Z0_Code = "c3";
			dummy3.Z0_Description = "ddd ddd";

			Filter.ComparisonOperator = "starts with";
			Filter.Property = "b";
			var result = Factory.Load<DummyBusinessObject>(Filter.Query);

			AssertEquals(2, result.Length);
			AssertCollectionContains(dummy1, result);
			AssertCollectionContains(dummy2, result);
			AssertCollectionNotContains(dummy3, result);

			Filter.Property = "b2";
			result = Factory.Load<DummyBusinessObject>(Filter.Query);

			AssertEquals(1, result.Length);
			AssertCollectionNotContains(dummy1, result);
			AssertCollectionContains(dummy2, result);
			AssertCollectionNotContains(dummy3, result);

			Filter.Property = "bb";
			result = Factory.Load<DummyBusinessObject>(Filter.Query);

			AssertEquals(1, result.Length);
			AssertCollectionContains(dummy1, result);
			AssertCollectionNotContains(dummy2, result);
			AssertCollectionNotContains(dummy3, result);

			Filter.Property = "too long to fit in Z0_Code";
			result = Factory.Load<DummyBusinessObject>(Filter.Query);
			AssertEquals(0, result.Length);
		}

		public void TestMultipleColumnsFilter()
		{
			var dummy1 = Factory.New<DummyBusinessObject>();
			dummy1.Z0_Code = "CODE1";
			dummy1.Z0_Description = "Apple is tasty.";

			var dummy2 = Factory.New<DummyBusinessObject>();
			dummy2.Z0_Code = "CODE2";
			dummy2.Z0_Description = "Apple is welllooking.";

			Filter.ComparisonOperator = "starts with";
			Filter.Property = "Apple";
			var result = Factory.Load<DummyBusinessObject>(Filter.Query);

			AssertEquals(2, result.Length);
			AssertEquals(100, Filter.MaxLength);
		}

		#endregion

		#region TestSetValueFromInitialCode
		public void TestSetValueFromInitialCode_FilterColumnName()
		{
			Filter.Property = "ABC";
			AssertEquals("Initial code should not be set", false, Filter.SetValueFromInitialCode("Z0_VarCharMax", "XYZ"));
			AssertEquals("ABC", Filter.Property);

			AssertEquals("Initial code should be set", true, Filter.SetValueFromInitialCode("Z0_Code", "XYZ"));
			AssertEquals("XYZ", Filter.Property);
		}

		public void TestSetValueFromInitialCode_Prefix()
		{
			Filter.Property = "ABC";
			AssertEquals("Initial code should be set", true, Filter.SetValueFromInitialCode("Z0_Code", "A:XYZ"));
			AssertEquals("Initial code set from property", "A:XYZ", Filter.Property);

			Filter.Prefix = "A";
			Filter.Property = "ABC";
			AssertEquals("Initial code should be set", true, Filter.SetValueFromInitialCode("Z0_Code", "A:XYZ"));
			AssertEquals("Initial code set from initial code suffix", "XYZ", Filter.Property);

			Filter.Property = "ABC";
			AssertEquals("Initial code should be set", true, Filter.SetValueFromInitialCode("Whatever", "A:XYZ"));
			AssertEquals("Initial code set from initial code suffix regardless of filter column name", "XYZ", Filter.Property);

			Filter.Property = "ABC";
			AssertEquals("Initial code should not be set", false, Filter.SetValueFromInitialCode("Whatever", "B:XYZ"));
			AssertEquals("ABC", Filter.Property);
		}

		public void TestSetValueFromInitialCode_AdditionalColumns()
		{
			Filter.Property = "AAA";

			AssertEquals("Initial code should be set", true, Filter.SetValueFromInitialCode("Z0_Description", "BBB"));
			AssertEquals("Initial code now BBB", "BBB", Filter.Property);

			AssertEquals("Initial code should not be set", false, Filter.SetValueFromInitialCode("Z0_Int", "CCC"));
			AssertEquals("Initial code still BBB", "BBB", Filter.Property);
		}

		public void TestShouldSetValueFromInitialCode_FilterColumnName()
		{
			var filter = new ModuleTextFilter("Filter", DummyBizoSchema.Z0_Code);

			Assert("Initial code should not be set", !filter.ShouldSetValueFromInitialCode("Z0_VarCharMax", "XYZ"));

			Assert("Initial code should be set", filter.ShouldSetValueFromInitialCode("Z0_Code", "XYZ"));
		}

		public void TestShouldSetValueFromInitialCode_Prefix()
		{
			var filter = new ModuleTextFilter("Filter", DummyBizoSchema.Z0_Code);

			Assert("Initial code should be set", filter.ShouldSetValueFromInitialCode("Z0_Code", "A:XYZ"));

			filter.Prefix = "A";
			Assert("Initial code should be set", filter.ShouldSetValueFromInitialCode("Z0_Code", "A:XYZ"));

			Assert("Initial code should be set", filter.ShouldSetValueFromInitialCode("Whatever", "A:XYZ"));

			AssertEquals("Initial code should not be set", false, filter.ShouldSetValueFromInitialCode("Whatever", "B:XYZ"));
		}

		public void TestShouldSetValueFromInitialCode_AdditionalColumns()
		{
			AssertEquals("Initial code should be set", true, Filter.ShouldSetValueFromInitialCode("Z0_Description", "BBB"));

			AssertEquals("Initial code should not be set", false, Filter.ShouldSetValueFromInitialCode("Z0_Int", "CCC"));
		}
		#endregion

		#region TestIsExpensiveQuery

		public override void TestIsExpensiveQuery()
		{
			// without filter data
			Filter.Property = "";

			Filter.ComparisonOperator = "starts with";
			AssertEquals(false, Filter.IsExpensiveQuery);

			Filter.ComparisonOperator = "contains";
			AssertEquals(false, Filter.IsExpensiveQuery);

			Filter.ComparisonOperator = "exact";
			AssertEquals(false, Filter.IsExpensiveQuery);

			// with filter data
			Filter.Property = "cell";

			Filter.ComparisonOperator = "starts with";
			AssertEquals(false, Filter.IsExpensiveQuery);

			Filter.ComparisonOperator = "contains";
			AssertEquals(true, Filter.IsExpensiveQuery);

			Filter.ComparisonOperator = "exact";
			AssertEquals(false, Filter.IsExpensiveQuery);

			Filter.ComparisonOperator = "not starting";
			AssertEquals(false, Filter.IsExpensiveQuery);

			Filter.ComparisonOperator = "not contain";
			AssertEquals(true, Filter.IsExpensiveQuery);

			Filter.ComparisonOperator = "not equal";
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		#endregion

		#region TestMaxMaximumLength

		public void TestMaxMaximumLength()
		{
			var collection = new ModuleFilterCollection();
			var nVarCharMaxFilter = collection.AddTextFilterForMultipleColumns("nVarCharMaxFilter", DummyBizoSchema.Z0_NVarCharMax);

			AssertEquals(ModuleFilter.MaxMaximumLength, nVarCharMaxFilter.MaxLength);
		}

		public void TestReturnsCorrectMaximumLengthWhenMultipleColumns()
		{
			var collection = new ModuleFilterCollection();
			var codeOrFullNameFilter = collection.AddTextFilterForMultipleColumns("Code or Full Name", GlbStaffSchema.GS_Code, GlbStaffSchema.GS_FullName); // Filter name

			AssertEquals(GlbStaffSchema.GS_FullName.MaxLength, codeOrFullNameFilter.PropertyInfo.MaxLength);
		}

		public void TestReturnsCorrectCollectionWhenMultipleColumns()
		{
			var moduleFilterCombiner = new ModuleFilterCombiner();

			var moduleFilters = new[] {
				new ModuleTextFilterForMultipleColumns("Filter", DummyBizoSchema.Z0_Code, DummyBizoSchema.Z0_Description) { Property = "AAAAA", ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Contains, OrCategory = FilterOrCategory.Red, GroupName = "Group1" },
				new ModuleTextFilterForMultipleColumns("Filter", DummyBizoSchema.Z0_Code, DummyBizoSchema.Z0_Description) { Property = "CCCCC", ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Contains, OrCategory = FilterOrCategory.Red, GroupName = "Group1" }
			};

			var resultQuery = moduleFilterCombiner.GetCombinedFilter(moduleFilters);

			var dummy1 = Factory.New<DummyBusinessObject>();
			dummy1.Z0_Code = "AAAAA";
			dummy1.Z0_Description = "bbbbbb bbb";

			var dummy2 = Factory.New<DummyBusinessObject>();
			dummy2.Z0_Code = "BBBBB";
			dummy2.Z0_Description = "cccccc ccc";

			var dummy3 = Factory.New<DummyBusinessObject>();
			dummy3.Z0_Code = "CCCCC";
			dummy3.Z0_Description = "dddddd ddd";

			Assert("Filter query should contains Z0_Code", resultQuery.LiteralTextSqlFormatted.Contains("Z0_Code"));
			Assert("Filter query should contains Z0_Description", resultQuery.LiteralTextSqlFormatted.Contains("Z0_Description"));

			var result = Factory.Load<DummyBusinessObject>(resultQuery);

			AssertEquals(3, result.Length);
			AssertCollectionContains(dummy1, result);
			AssertCollectionContains(dummy2, result);
			AssertCollectionContains(dummy3, result);
		}

		#endregion

		#region Implementation

		protected override FilterCategory ExpectedDefaultCategory
		{
			get { return FilterCategories.TextSearch; }
		}

		protected override ZString ExpectedDescription
		{
			get { return "Dummy"; }
		}

		protected override ModuleTextFilterForMultipleColumns GetNewModuleFilter()
		{
			return new ModuleTextFilterForMultipleColumns("Dummy", DummyBizoSchema.Z0_Code, DummyBizoSchema.Z0_Description);
		}

		#endregion
	}
}
