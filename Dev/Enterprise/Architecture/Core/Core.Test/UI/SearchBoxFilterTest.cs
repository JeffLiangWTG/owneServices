using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class SearchBoxFilterTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			var dependents = new DummyDependentBusinessObjectCollection(Factory);
			var dependent = dependents.AddNew();
			dependent.ZD1_Code = "ONE";

			dependent = dependents.AddNew();
			dependent.ZD1_Code = "TWO";

			dependent = dependents.AddNew();
			dependent.ZD1_Code = "THREE";

			dependent = dependents.AddNew();
			dependent.ZD1_Code = "FOUR";

			dependent = dependents.AddNew();
			dependent.ZD1_Code = "FIVE";

			dependent = dependents.AddNew();
			dependent.ZD1_Code = "SIX";

			dependent = dependents.AddNew();
			dependent.ZD1_Code = "SEVEN";

			dependent = dependents.AddNew();
			dependent.ZD1_Code = "EIGHT";

			dependent = dependents.AddNew();
			dependent.ZD1_Code = "NINE";

			dependent = dependents.AddNew();
			dependent.ZD1_Code = "TEN";

			Factory.Save();

			base.SetUp();
		}

		public void TestFiltering()
		{
			var collection = new DummyDependentBusinessObjectCollection(Factory);
			var searchBoxFilter = new SearchBoxFilter(DummyDependentBizoSchema.ZD1_Code);

			searchBoxFilter.ApplySearch(collection, "T");

			AssertEquals("all codes that have T", 4, collection.Count);

			searchBoxFilter.MaximumRows = 3;

			searchBoxFilter.ApplySearch(collection, "T");

			AssertEquals("all codes that have T and a maximum of three rows", 3, collection.Count);

			searchBoxFilter.ApplySearch(collection, "TE");

			AssertEquals("all codes that have TE", 1, collection.Count);
		}

		public void TestFiltering_ActiveCollection()
		{
			var collection = new ActiveBusinessObjectCollection<DummyDependentBusinessObject>(Factory);
			var searchBoxFilter = new SearchBoxFilter(DummyDependentBizoSchema.ZD1_Code);

			searchBoxFilter.ApplySearch(collection, "T");

			AssertEquals("all codes that have T", 4, collection.Count);

			searchBoxFilter.MaximumRows = 3;

			searchBoxFilter.ApplySearch(collection, "T");

			AssertEquals("all codes that have T and a maximum of three rows", 3, collection.Count);

			searchBoxFilter.ApplySearch(collection, "TE");

			AssertEquals("all codes that have TE", 1, collection.Count);
		}
	}
}
