using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(ModuleCurrentDateOffsetFilter))]
	sealed class ModuleCurrentDateOffsetFilterTest : ModuleFilterTestCase<ModuleCurrentDateOffsetFilter>
	{
		public void TestCurrentDateOffsetFilter()
		{
			var dummy1 = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy1.Z0_Date = ZDateTime.UtcToday.AddDays(1);
			Factory.Save();

			var expectedResult = new BusinessObject[] { dummy1 };

			var filter = new ModuleCurrentDateOffsetFilter("moo", DummyBizoSchema.Z0_Date);
			filter.Offset = 2;

			var actualResult = Factory.Load<DummyBusinessObject>(DBQuery(filter.Query));
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, expectedResult, actualResult);

			dummy1.Z0_Date = ZDateTime.UtcToday.AddDays(-1);
			Factory.Save();

			actualResult = Factory.Load<DummyBusinessObject>(DBQuery(filter.Query));
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, expectedResult, actualResult);

			dummy1.Z0_Date = ZDateTime.UtcToday.AddDays(3);
			Factory.Save();

			expectedResult = System.Array.Empty<BusinessObject>();
			actualResult = Factory.Load<DummyBusinessObject>(DBQuery(filter.Query));
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, expectedResult, actualResult);
		}

		ZQuery DBQuery(ZQuery innerQuery)
		{
			var query = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			query.AddToFilter(innerQuery);
			return query;
		}

		protected override ModuleCurrentDateOffsetFilter GetNewModuleFilter()
		{
			return new ModuleCurrentDateOffsetFilter("moo", DummyBizoSchema.Z0_Date);
		}

		protected override FilterCategory ExpectedDefaultCategory
		{
			get { return FilterCategories.Dates; }
		}

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		#region TestQueryIsEmptyByDefault
		public override void TestQueryIsEmptyByDefault()
		{
			AssertEquals("ModuleCurrentDateOffsetFilter.Query is not empty by default", false, Filter.IsEmpty);
		}

		#endregion
	}
}
