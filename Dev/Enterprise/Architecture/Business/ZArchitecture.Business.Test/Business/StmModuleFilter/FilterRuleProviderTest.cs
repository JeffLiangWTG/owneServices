using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class FilterRuleProviderTest : TestCaseWithFactory
	{
		public void TestCopyFilterStrips_WithEmptyFilterField_ShouldNotAccessNewFilter()
		{
			var dummy = Factory.New<DummyWithRelatedFilters>();
			var clone = (DummyWithRelatedFilters)dummy.Clone();

			AssertEquals(false, clone.FilterProvider.IsFilterCached);

			var filter = dummy.Filter;
			clone = (DummyWithRelatedFilters)dummy.Clone();

			AssertEquals(true, clone.FilterProvider.IsFilterCached);

			dummy.FilterProvider.DeleteFilter();
			AssertEquals(true, filter.IsDeleted);
		}
	}
}
