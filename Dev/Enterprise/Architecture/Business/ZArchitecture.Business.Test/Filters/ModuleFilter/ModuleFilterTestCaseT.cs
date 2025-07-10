using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business.Testing
{
	public abstract class ModuleFilterTestCase<T> : NonPersistentBusinessObjectTestCase where T : ModuleFilter
	{
		#region TestCodeAndDescriptionAttributes

		public void TestCodeAndDescriptionAttributes()
		{
			AssertEquals(ExpectedDescription, CodePropertyAttribute.CodeFromBusinessObject(Filter));
			AssertEquals(ExpectedDescription, DescriptionPropertyAttribute.DescriptionFromBusinessObject(Filter));
		}

		protected virtual ZString ExpectedDescription
		{
			get { return "moo"; }
		}

		#endregion

		#region TestIsCommon

		public void TestIsCommon()
		{
			AssertEquals(false, Filter.IsCommon);

			if ((bool)Filter.GetType().GetProperty("SupportsCommon", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Filter, null))
			{
				Filter.IsCommon = true;
				AssertEquals(true, Filter.IsCommon);
			}
			else
			{
				var notSupportedExceptionThrown = false;
				try
				{
					Filter.IsCommon = true;
				}
				catch (NotSupportedException)
				{
					notSupportedExceptionThrown = true;
				}

				AssertEquals(true, notSupportedExceptionThrown);
			}
		}

		#endregion

		#region TestIsExpensiveQuery

		public abstract void TestIsExpensiveQuery();

		#endregion

		#region TestDescription

		public void TestDescription()
		{
			AssertEquals(ExpectedDescription, Filter.Description);
		}

		#endregion

		#region TestVisibility

		public void TestVisibility()
		{
			AssertEquals(FilterVisibility.Visible, Filter.Visibility);
		}

		#endregion

		#region TestCategory

		public void TestCategory()
		{
			AssertNotEquals("Precondition", InitialTestCatergory, Filter.Category);

			Filter.Category = FilterCategories.Organisations;
			AssertEquals(FilterCategories.Organisations, Filter.Category);
		}

		protected virtual FilterCategory InitialTestCatergory
		{
			get { return FilterCategories.Organisations; }
		}

		public void TestCategoryDefault()
		{
			AssertEquals(ExpectedDefaultCategory, Filter.Category);
		}

		protected abstract FilterCategory ExpectedDefaultCategory
		{
			get;
		}

		#endregion

		#region TestQuery

		public virtual void TestQueryIsEmptyByDefault()
		{
			var shouldBeEmpty = !(Filter is ModuleGuidForeignCollectionFilter);
			AssertEquals(shouldBeEmpty, Filter.Query.IsEmpty);
		}

		public void TestQueryForBlankAndNotBlank()
		{
			var filter = new DummyModuleTextBaseFilter("i", DummyBizoSchema.Z0_Code);
			filter.Visibility = FilterVisibility.Visible;

			AssertEquals(0, filter.QueryDelegateCounter);
			AssertEquals(0, filter.QueryUsingFilterColumnsCounter);
			var query = filter.Query;
			AssertEquals(0, filter.QueryDelegateCounter);
			AssertEquals(0, filter.QueryUsingFilterColumnsCounter);

			filter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			query = filter.Query;
			AssertEquals(0, filter.QueryDelegateCounter);
			AssertEquals(1, filter.QueryUsingFilterColumnsCounter);

			filter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			query = filter.Query;
			AssertEquals(0, filter.QueryDelegateCounter);
			AssertEquals(2, filter.QueryUsingFilterColumnsCounter);
		}

		#endregion

		#region TestGetTemplateFilter

		public void TestGetTemplateFilter()
		{
			AssertEquals(new ZQuery(), Filter.XQuery);

			Filter.GetXQuery = (filter) => new ZQuery(DummyBizoSchema.Z0_Code, "aa");
			AssertEquals(new ZQuery(DummyBizoSchema.Z0_Code, "aa"), Filter.XQuery);
		}

		#endregion

		#region TestUpdateZTypeProperties_ShouldInvalidateCachedQuery

		public void TestUpdateZTypeProperties_ShouldInvalidateCachedQuery()
		{
			var factory = new BusinessObjectFactory();
			var filter = Filter;
			var properties = filter.GetType().GetProperties().Where(x => (typeof(IZType).IsAssignableFrom(x.PropertyType) || typeof(BusinessObject).IsAssignableFrom(x.PropertyType)) && x.GetSetMethod(false) != null);
			var dummyValues = GetDummyValuesForCacheInvalidationTest(filter);

			AddValidDummyComparisonOperatorIfNoneAlready(filter, dummyValues);

			CombineAssertions("All settable properties should call InvalidateCachedQuery() in their setters so that the query won't be stale when generated. The following properties don't do this yet:", () =>
			{
				foreach (var property in properties)
				{
					if (!GetPropertiesExcludedFromCacheInvalidationTest(filter).Contains(property.Name))
					{
						filter.CachedQuery_ForTest = new ZQuery();
						object dummyValue;

						if (dummyValues.ContainsKey(property.Name))
						{
							dummyValue = dummyValues[property.Name];
						}
						else if (property.PropertyType == typeof(ZBool))
						{
							dummyValue = new ZBool(!(ZBool)property.GetValue(filter));
						}
						else if (typeof(IZType).IsAssignableFrom(property.PropertyType))
						{
							dummyValue = BusinessObjectHelper.GetNonDefaultValueForZType(property.PropertyType);
						}
						else
						{
							dummyValue = factory.New(property.PropertyType);
						}

						if (dummyValue != null)
						{
							property.SetValue(filter, dummyValue);
							AssertNull(property.Name, filter.CachedQuery_ForTest);
						}
					}
				}
			});
		}

		void AddValidDummyComparisonOperatorIfNoneAlready(T filter, Dictionary<string, IZType> dummyValues)
		{
			if (filter is IModuleFilterWithComparisonOperator comparisonFilter && !dummyValues.ContainsKey(nameof(comparisonFilter.ComparisonOperator)))
			{
				var index = comparisonFilter.AllowedComparisonOperators[0].IsNullOrEmpty() ? 1 : 0;

				if (comparisonFilter.AllowedComparisonOperators.Count > index + 1)
				{
					ZString dummyValue = comparisonFilter.AllowedComparisonOperators[index] != comparisonFilter.GetComparisonOperatorDefault()
						? comparisonFilter.AllowedComparisonOperators[index]
						: comparisonFilter.AllowedComparisonOperators[index + 1];

					dummyValues.Add(nameof(comparisonFilter.ComparisonOperator), dummyValue);
				}
			}
		}

		protected virtual string[] GetPropertiesExcludedFromCacheInvalidationTest(T filter)
		{
			return new[] { nameof(filter.OriginalCode), nameof(filter.Visible), "PropertyCode", "SupportsXQuery" };
		}

		protected virtual Dictionary<string, IZType> GetDummyValuesForCacheInvalidationTest(T filter)
		{
			return new Dictionary<string, IZType>();
		}

		#endregion

		#region Implementation

		public T Filter
		{
			get { return fFilter ?? (fFilter = GetNewModuleFilter()); }
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewModuleFilter();
		}

		protected abstract T GetNewModuleFilter();

		T fFilter;

		#endregion
	}
}
