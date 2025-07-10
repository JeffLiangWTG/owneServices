using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(FormFilterBusinessObjectForTest))]
	public class FormFilterBuisnessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestShouldNotAddCustomSqlFilter()
		{
			AssertEquals(false, FilterBO.HasCustomSqlFilter);
		}

		public void TestEveryModuleFilterMustHaveAFunc()
		{
			FilterBO.ModuleFilters.ForEach(x =>
			{
				Assert(FilterBO.FuncDictionary.ContainsKey(x.Description));
			});

			FilterBO.FuncDictionary.Keys.ForEach(x =>
			{
				Assert(FilterBO.ModuleFilters.Any(y => y.Description == x));
			});
		}

		public void TestTextFilter_Description()
		{
			var dummy1 = Factory.New<DummyBusinessObject>();
			dummy1.Z0_Description = "111";
			var dummy2 = Factory.New<DummyBusinessObject>();
			dummy2.Z0_Description = "111";
			var dummy3 = Factory.New<DummyBusinessObject>();
			dummy3.Z0_Description = "222";
			var dummy4 = Factory.New<DummyBusinessObject>();
			dummy4.Z0_Description = "222";
			DummyBusinessObjects.AddRange(new List<DummyBusinessObject>
			{
				dummy1, dummy2, dummy3, dummy4
			});

			var filter = (ModuleTextFilter)FilterBO[FormFilterBusinessObjectConstants.Description];
			filter.IsActive = true;

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = string.Empty;
			AssertEquals(4, DummyBusinessObjects.Count(x => FilterBO.Predicate.Invoke(x)));
			filter.Property = "111";
			var resultOfSearch = DummyBusinessObjects.Where(x => FilterBO.Predicate.Invoke(x)).ToList();
			AssertCollectionContains(FormFilterBusinessObjectConstants.Description, dummy1, resultOfSearch);
			AssertCollectionContains(FormFilterBusinessObjectConstants.Description, dummy2, resultOfSearch);
			AssertCollectionNotContains(FormFilterBusinessObjectConstants.Description, dummy3, resultOfSearch);
			AssertCollectionNotContains(FormFilterBusinessObjectConstants.Description, dummy4, resultOfSearch);

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = string.Empty;
			AssertEquals(4, DummyBusinessObjects.Count(x => FilterBO.Predicate.Invoke(x)));
			filter.Property = "111";
			resultOfSearch = DummyBusinessObjects.Where(x => FilterBO.Predicate.Invoke(x)).ToList();
			AssertCollectionContains(FormFilterBusinessObjectConstants.Description, dummy1, resultOfSearch);
			AssertCollectionContains(FormFilterBusinessObjectConstants.Description, dummy2, resultOfSearch);
			AssertCollectionNotContains(FormFilterBusinessObjectConstants.Description, dummy3, resultOfSearch);
			AssertCollectionNotContains(FormFilterBusinessObjectConstants.Description, dummy4, resultOfSearch);

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = string.Empty;
			AssertEquals(4, DummyBusinessObjects.Count(x => FilterBO.Predicate.Invoke(x)));
			filter.Property = "111";
			resultOfSearch = DummyBusinessObjects.Where(x => FilterBO.Predicate.Invoke(x)).ToList();
			AssertCollectionContains(FormFilterBusinessObjectConstants.Description, dummy1, resultOfSearch);
			AssertCollectionContains(FormFilterBusinessObjectConstants.Description, dummy2, resultOfSearch);
			AssertCollectionNotContains(FormFilterBusinessObjectConstants.Description, dummy3, resultOfSearch);
			AssertCollectionNotContains(FormFilterBusinessObjectConstants.Description, dummy4, resultOfSearch);

			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.Property = string.Empty;
			AssertEquals(4, DummyBusinessObjects.Count(x => FilterBO.Predicate.Invoke(x)));
			filter.Property = "111";
			resultOfSearch = DummyBusinessObjects.Where(x => FilterBO.Predicate.Invoke(x)).ToList();
			AssertCollectionNotContains(FormFilterBusinessObjectConstants.Description, dummy1, resultOfSearch);
			AssertCollectionNotContains(FormFilterBusinessObjectConstants.Description, dummy2, resultOfSearch);
			AssertCollectionContains(FormFilterBusinessObjectConstants.Description, dummy3, resultOfSearch);
			AssertCollectionContains(FormFilterBusinessObjectConstants.Description, dummy4, resultOfSearch);

			filter.SqlComparisonOperator = SQLComparisonOperator.DoesNotStartWith;
			filter.Property = string.Empty;
			AssertEquals(4, DummyBusinessObjects.Count(x => FilterBO.Predicate.Invoke(x)));
			filter.Property = "111";
			resultOfSearch = DummyBusinessObjects.Where(x => FilterBO.Predicate.Invoke(x)).ToList();
			AssertCollectionNotContains(FormFilterBusinessObjectConstants.Description, dummy1, resultOfSearch);
			AssertCollectionNotContains(FormFilterBusinessObjectConstants.Description, dummy2, resultOfSearch);
			AssertCollectionContains(FormFilterBusinessObjectConstants.Description, dummy3, resultOfSearch);
			AssertCollectionContains(FormFilterBusinessObjectConstants.Description, dummy4, resultOfSearch);

			filter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			filter.Property = string.Empty;
			AssertEquals(4, DummyBusinessObjects.Count(x => FilterBO.Predicate.Invoke(x)));
			filter.Property = "111";
			resultOfSearch = DummyBusinessObjects.Where(x => FilterBO.Predicate.Invoke(x)).ToList();
			AssertCollectionNotContains(FormFilterBusinessObjectConstants.Description, dummy1, resultOfSearch);
			AssertCollectionNotContains(FormFilterBusinessObjectConstants.Description, dummy2, resultOfSearch);
			AssertCollectionContains(FormFilterBusinessObjectConstants.Description, dummy3, resultOfSearch);
			AssertCollectionContains(FormFilterBusinessObjectConstants.Description, dummy4, resultOfSearch);

			filter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			resultOfSearch = DummyBusinessObjects.Where(x => FilterBO.Predicate.Invoke(x)).ToList();
			AssertCollectionNotContains(FormFilterBusinessObjectConstants.Description, dummy1, resultOfSearch);
			AssertCollectionNotContains(FormFilterBusinessObjectConstants.Description, dummy2, resultOfSearch);
			AssertCollectionNotContains(FormFilterBusinessObjectConstants.Description, dummy3, resultOfSearch);
			AssertCollectionNotContains(FormFilterBusinessObjectConstants.Description, dummy4, resultOfSearch);

			filter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			resultOfSearch = DummyBusinessObjects.Where(x => FilterBO.Predicate.Invoke(x)).ToList();
			AssertCollectionContains(FormFilterBusinessObjectConstants.Description, dummy1, resultOfSearch);
			AssertCollectionContains(FormFilterBusinessObjectConstants.Description, dummy2, resultOfSearch);
			AssertCollectionContains(FormFilterBusinessObjectConstants.Description, dummy3, resultOfSearch);
			AssertCollectionContains(FormFilterBusinessObjectConstants.Description, dummy4, resultOfSearch);

			// filter AND filter2
			var filterStrip2 = FilterBO.FilterStrips.AddNew();
			filterStrip2.FilterDescription = filter.Description;
			var filter2 = (ModuleTextFilter)FilterBO[FormFilterBusinessObjectConstants.Description + " (1)"];
			filter2.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			resultOfSearch = DummyBusinessObjects.Where(x => FilterBO.Predicate.Invoke(x)).ToList();
			AssertEquals(2, FilterBO.ActiveModuleFilters.Count);
			AssertCollectionNotContains(FormFilterBusinessObjectConstants.Description, dummy1, resultOfSearch);
			AssertCollectionNotContains(FormFilterBusinessObjectConstants.Description, dummy2, resultOfSearch);
			AssertCollectionNotContains(FormFilterBusinessObjectConstants.Description, dummy3, resultOfSearch);
			AssertCollectionNotContains(FormFilterBusinessObjectConstants.Description, dummy4, resultOfSearch);

			// filter OR filter2
			filter.OrCategory = FilterOrCategory.Red;
			filter2.OrCategory = FilterOrCategory.Red;
			resultOfSearch = DummyBusinessObjects.Where(x => FilterBO.Predicate.Invoke(x)).ToList();
			AssertEquals(2, FilterBO.ActiveModuleFilters.Count);
			AssertCollectionContains(FormFilterBusinessObjectConstants.Description, dummy1, resultOfSearch);
			AssertCollectionContains(FormFilterBusinessObjectConstants.Description, dummy2, resultOfSearch);
			AssertCollectionContains(FormFilterBusinessObjectConstants.Description, dummy3, resultOfSearch);
			AssertCollectionContains(FormFilterBusinessObjectConstants.Description, dummy4, resultOfSearch);

			// (filter OR filter2) AND filter3
			var filterStrip3 = FilterBO.FilterStrips.AddNew();
			filterStrip3.FilterDescription = filter.Description;
			var filter3 = (ModuleTextFilter)FilterBO[FormFilterBusinessObjectConstants.Description + " (2)"];
			filter3.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			filter3.OrCategory = FilterOrCategory.None;
			resultOfSearch = DummyBusinessObjects.Where(x => FilterBO.Predicate.Invoke(x)).ToList();
			AssertEquals(3, FilterBO.ActiveModuleFilters.Count);
			AssertCollectionNotContains(FormFilterBusinessObjectConstants.Description, dummy1, resultOfSearch);
			AssertCollectionNotContains(FormFilterBusinessObjectConstants.Description, dummy2, resultOfSearch);
			AssertCollectionNotContains(FormFilterBusinessObjectConstants.Description, dummy3, resultOfSearch);
			AssertCollectionNotContains(FormFilterBusinessObjectConstants.Description, dummy4, resultOfSearch);
		}

		public void TestNumberRangeFilter_Decimal()
		{
			var dummy1 = Factory.New<DummyBusinessObject>();
			dummy1.Z0_Decimal = 111M;
			var dummy2 = Factory.New<DummyBusinessObject>();
			dummy2.Z0_Decimal = 111M;
			var dummy3 = Factory.New<DummyBusinessObject>();
			dummy3.Z0_Decimal = 222M;
			var dummy4 = Factory.New<DummyBusinessObject>();
			dummy4.Z0_Decimal = 222M;
			DummyBusinessObjects.AddRange(new List<DummyBusinessObject>
			{
				dummy1, dummy2, dummy3, dummy4
			});

			var filter = (ModuleNumberRangeFilter)FilterBO[FormFilterBusinessObjectConstants.Decimal];
			filter.IsActive = true;
			filter.PropertySearch = ModuleNumberRangeFilter.SearchTexts.EqualTo;
			filter.Property1 = 0M;
			filter.Property2 = 0M;
			AssertEquals(4, DummyBusinessObjects.Count(x => FilterBO.Predicate.Invoke(x)));
			filter.Property1 = 111M;
			filter.Property2 = 111M;
			var resultOfSearch = DummyBusinessObjects.Where(x => FilterBO.Predicate.Invoke(x)).ToList();
			AssertCollectionContains(FormFilterBusinessObjectConstants.Decimal, dummy1, resultOfSearch);
			AssertCollectionContains(FormFilterBusinessObjectConstants.Decimal, dummy2, resultOfSearch);
			AssertCollectionNotContains(FormFilterBusinessObjectConstants.Decimal, dummy3, resultOfSearch);
			AssertCollectionNotContains(FormFilterBusinessObjectConstants.Decimal, dummy4, resultOfSearch);

			filter.PropertySearch = ModuleNumberRangeFilter.SearchTexts.GreaterThanOrEqualTo;
			filter.Property1 = 0M;
			AssertEquals(4, DummyBusinessObjects.Count(x => FilterBO.Predicate.Invoke(x)));
			filter.Property1 = 222M;
			resultOfSearch = DummyBusinessObjects.Where(x => FilterBO.Predicate.Invoke(x)).ToList();
			AssertCollectionNotContains(FormFilterBusinessObjectConstants.Decimal, dummy1, resultOfSearch);
			AssertCollectionNotContains(FormFilterBusinessObjectConstants.Decimal, dummy2, resultOfSearch);
			AssertCollectionContains(FormFilterBusinessObjectConstants.Decimal, dummy3, resultOfSearch);
			AssertCollectionContains(FormFilterBusinessObjectConstants.Decimal, dummy4, resultOfSearch);

			filter.PropertySearch = ModuleNumberRangeFilter.SearchTexts.LessThanOrEqualTo;
			filter.Property2 = 0M;
			AssertEquals(4, DummyBusinessObjects.Count(x => FilterBO.Predicate.Invoke(x)));
			filter.Property2 = 111M;
			resultOfSearch = DummyBusinessObjects.Where(x => FilterBO.Predicate.Invoke(x)).ToList();
			AssertCollectionContains(FormFilterBusinessObjectConstants.Decimal, dummy1, resultOfSearch);
			AssertCollectionContains(FormFilterBusinessObjectConstants.Decimal, dummy2, resultOfSearch);
			AssertCollectionNotContains(FormFilterBusinessObjectConstants.Decimal, dummy3, resultOfSearch);
			AssertCollectionNotContains(FormFilterBusinessObjectConstants.Decimal, dummy4, resultOfSearch);

			filter.PropertySearch = ModuleNumberRangeFilter.SearchTexts.Between;
			filter.Property1 = 0M;
			filter.Property2 = 0M;
			AssertEquals(4, DummyBusinessObjects.Count(x => FilterBO.Predicate.Invoke(x)));
			filter.Property1 = 111M;
			filter.Property2 = 111M;
			resultOfSearch = DummyBusinessObjects.Where(x => FilterBO.Predicate.Invoke(x)).ToList();
			AssertCollectionContains(FormFilterBusinessObjectConstants.Decimal, dummy1, resultOfSearch);
			AssertCollectionContains(FormFilterBusinessObjectConstants.Decimal, dummy2, resultOfSearch);
			AssertCollectionNotContains(FormFilterBusinessObjectConstants.Decimal, dummy3, resultOfSearch);
			AssertCollectionNotContains(FormFilterBusinessObjectConstants.Decimal, dummy4, resultOfSearch);

			// filter AND filter2
			var filterStrip2 = FilterBO.FilterStrips.AddNew();
			filterStrip2.FilterDescription = filter.Description;
			var filter2 = (ModuleNumberRangeFilter)FilterBO[FormFilterBusinessObjectConstants.Decimal + " (1)"];
			filter2.PropertySearch = ModuleNumberRangeFilter.SearchTexts.EqualTo;
			filter2.Property1 = 222M;
			resultOfSearch = DummyBusinessObjects.Where(x => FilterBO.Predicate.Invoke(x)).ToList();
			AssertEquals(2, FilterBO.ActiveModuleFilters.Count);
			AssertCollectionNotContains(FormFilterBusinessObjectConstants.Decimal, dummy1, resultOfSearch);
			AssertCollectionNotContains(FormFilterBusinessObjectConstants.Decimal, dummy2, resultOfSearch);
			AssertCollectionNotContains(FormFilterBusinessObjectConstants.Decimal, dummy3, resultOfSearch);
			AssertCollectionNotContains(FormFilterBusinessObjectConstants.Decimal, dummy4, resultOfSearch);

			// filter OR filter2
			filter.OrCategory = FilterOrCategory.Red;
			filter2.OrCategory = FilterOrCategory.Red;
			resultOfSearch = DummyBusinessObjects.Where(x => FilterBO.Predicate.Invoke(x)).ToList();
			AssertEquals(2, FilterBO.ActiveModuleFilters.Count);
			AssertCollectionContains(FormFilterBusinessObjectConstants.Decimal, dummy1, resultOfSearch);
			AssertCollectionContains(FormFilterBusinessObjectConstants.Decimal, dummy2, resultOfSearch);
			AssertCollectionContains(FormFilterBusinessObjectConstants.Decimal, dummy3, resultOfSearch);
			AssertCollectionContains(FormFilterBusinessObjectConstants.Decimal, dummy4, resultOfSearch);

			// (filter OR filter2) AND filter3
			var filterStrip3 = FilterBO.FilterStrips.AddNew();
			filterStrip3.FilterDescription = filter.Description;
			var filter3 = (ModuleNumberRangeFilter)FilterBO[FormFilterBusinessObjectConstants.Decimal + " (2)"];
			filter3.PropertySearch = ModuleNumberRangeFilter.SearchTexts.EqualTo;
			filter3.Property1 = 222M;
			filter3.OrCategory = FilterOrCategory.None;
			resultOfSearch = DummyBusinessObjects.Where(x => FilterBO.Predicate.Invoke(x)).ToList();
			AssertEquals(3, FilterBO.ActiveModuleFilters.Count);
			AssertCollectionNotContains(FormFilterBusinessObjectConstants.Decimal, dummy1, resultOfSearch);
			AssertCollectionNotContains(FormFilterBusinessObjectConstants.Decimal, dummy2, resultOfSearch);
			AssertCollectionContains(FormFilterBusinessObjectConstants.Decimal, dummy3, resultOfSearch);
			AssertCollectionContains(FormFilterBusinessObjectConstants.Decimal, dummy4, resultOfSearch);
		}

		public void TestTextFunc_ChildDescription()
		{
			var dummy1 = Factory.New<DummyBusinessObject>();
			dummy1.Collection.AddNew().Z0_Description = "001";
			dummy1.Collection.AddNew().Z0_Description = "100";
			var dummy2 = Factory.New<DummyBusinessObject>();
			dummy2.Collection.AddNew().Z0_Description = "002";
			dummy2.Collection.AddNew().Z0_Description = "100";
			dummy2.Collection.AddNew().Z0_Description = "";
			var dummy3 = Factory.New<DummyBusinessObject>();
			dummy3.Collection.AddNew().Z0_Description = "";
			var dummy4 = Factory.New<DummyBusinessObject>();

			DummyBusinessObjects.AddRange(new List<DummyBusinessObject>
			{
				dummy1, dummy2, dummy3, dummy4
			});

			CombineAssertions(() =>
			{
				AssertResultComparison(SQLComparisonOperator.Equal, "001", dummy1);
				AssertResultComparison(SQLComparisonOperator.Equal, "100", dummy1, dummy2);
				AssertResultComparison(SQLComparisonOperator.StartsWith, "0", dummy1, dummy2);
				AssertResultComparison(SQLComparisonOperator.StartsWith, "2", System.Array.Empty<DummyBusinessObject>());
				AssertResultComparison(SQLComparisonOperator.Contains, "0", dummy1, dummy2);
				AssertResultComparison(SQLComparisonOperator.Contains, "2", dummy2);
				AssertResultComparison(SQLComparisonOperator.NotEqual, "001", dummy1, dummy2, dummy3);
				AssertResultComparison(SQLComparisonOperator.DoesNotStartWith, "0", dummy1, dummy2, dummy3);
				AssertResultComparison(SQLComparisonOperator.NotContains, "2", dummy1, dummy2, dummy3);
				AssertResultSpecialComparison(SQLComparisonOperator.IsBlank, dummy2, dummy3, dummy4);
				AssertResultSpecialComparison(SQLComparisonOperator.IsNotBlank, dummy1, dummy2);
			});
		}

		void AssertResultSpecialComparison(SQLComparisonOperator sQLComparison, params DummyBusinessObject[] expectedDummyObjs)
		{
			var filter = (ModuleTextFilter)FilterBO[FormFilterBusinessObjectConstants.DescriptionChild];
			filter.IsActive = true;
			filter.SqlComparisonOperator = sQLComparison;

			var resultOfSearch = DummyBusinessObjects.Where(x => FilterBO.Predicate.Invoke(x)).ToList();
			AssertContainsExactElementsInAnyOrder($"{FormFilterBusinessObjectConstants.DescriptionChild} {filter.SqlComparisonOperator}", expectedDummyObjs, resultOfSearch);
		}

		void AssertResultComparison(SQLComparisonOperator sQLComparison, ZString value, params DummyBusinessObject[] expectedDummyObjs)
		{
			var filter = (ModuleTextFilter)FilterBO[FormFilterBusinessObjectConstants.DescriptionChild];
			filter.IsActive = true;
			filter.SqlComparisonOperator = sQLComparison;

			filter.Property = ZString.Empty;
			var resultOfSearch = DummyBusinessObjects.Where(x => FilterBO.Predicate.Invoke(x)).ToList();
			AssertEquals(4, resultOfSearch.Count);

			filter.Property = value;
			resultOfSearch = DummyBusinessObjects.Where(x => FilterBO.Predicate.Invoke(x)).ToList();
			AssertContainsExactElementsInAnyOrder($"{FormFilterBusinessObjectConstants.DescriptionChild} {filter.SqlComparisonOperator} {filter.Property}", expectedDummyObjs, resultOfSearch);
		}

		protected override bool ShouldBeLocalizable => false;

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new FormFilterBusinessObjectForTest();
		}

		FormFilterBusinessObjectForTest FilterBO;
		List<DummyBusinessObject> DummyBusinessObjects;

		protected override void SetUp()
		{
			base.SetUp();
			DummyBusinessObjects = new List<DummyBusinessObject>();
			FilterBO = (FormFilterBusinessObjectForTest)GetNewFilterStripBusinessObject();
		}
	}
}
