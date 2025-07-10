using System.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Web.Business.Utilities.Testing
{
	sealed class CustomSorterTest : WebCollectionSorterTest
	{
		public void TestSortingCustomSortera()
		{
			DummyBusinessObjectCollection collection = new DummyBusinessObjectCollection(Factory);
			PopulateCollection(collection);

			AssertEquals("Collection should contain 5 bizobjects", 5, collection.Count);

			collection.Sort(new ActualEstimateCollectionSorter(DummyBizoSchema.Z0_Date.Name, DummyBizoSchema.Z0_AnotherDate.Name, ListSortDirection.Ascending));

			AssertEquals("Collection not sorted by Z0_Date Ascending", new ZDateTime(2005, 2, 12), collection[0].Z0_Date);

			AssertEquals("Collection not sorted by Z0_Date Ascending", new ZDateTime(2005, 2, 14), collection[1].Z0_Date);

			AssertEquals("Collection not sorted by Z0_Date Ascending", ZDateTime.Empty, collection[2].Z0_Date);
			AssertEquals("Collection not sorted by Z0_Date Ascending", new ZDateTime(2005, 2, 18), collection[2].Z0_AnotherDate);

			AssertEquals("Collection not sorted by Z0_Date Ascending", ZDateTime.Empty, collection[3].Z0_Date);
			AssertEquals("Collection not sorted by Z0_Date Ascending", new ZDateTime(2005, 3, 14), collection[3].Z0_AnotherDate);

			AssertEquals("Collection not sorted by Z0_Date Ascending", new ZDateTime(2005, 4, 19), collection[4].Z0_Date);

			collection.Sort(new ActualEstimateCollectionSorter(DummyBizoSchema.Z0_Date.Name, DummyBizoSchema.Z0_AnotherDate.Name, ListSortDirection.Descending));

			AssertEquals("Collection not sorted by Z0_Date Descending", new ZDateTime(2005, 4, 19), collection[0].Z0_Date);

			AssertEquals("Collection not sorted by Z0_Date Descending", new ZDateTime(2005, 3, 14), collection[1].Z0_AnotherDate);
			AssertEquals("Collection not sorted by Z0_Date Descending", ZDateTime.Empty, collection[1].Z0_Date);

			AssertEquals("Collection not sorted by Z0_Date Ascending", new ZDateTime(2005, 2, 18), collection[2].Z0_AnotherDate);
			AssertEquals("Collection not sorted by Z0_Date Descending", ZDateTime.Empty, collection[2].Z0_Date);

			AssertEquals("Collection not sorted by Z0_Date Descending", new ZDateTime(2005, 2, 14), collection[3].Z0_Date);

			AssertEquals("Collection not sorted by Z0_Date Descending", new ZDateTime(2005, 2, 12), collection[4].Z0_Date);
		}

		void PopulateCollection(DummyBusinessObjectCollection collection)
		{
			DummyBusinessObject dummy1 = collection.AddNew();
			dummy1.Z0_Date = new ZDateTime(2005, 2, 12);
			dummy1.Z0_AnotherDate = new ZDateTime(2005, 2, 13);

			DummyBusinessObject dummy2 = collection.AddNew();
			dummy2.Z0_Date = ZDateTime.Empty;
			dummy2.Z0_AnotherDate = new ZDateTime(2005, 2, 18);

			DummyBusinessObject dummy3 = collection.AddNew();
			dummy3.Z0_Date = new ZDateTime(2005, 2, 14);
			dummy3.Z0_AnotherDate = ZDateTime.Empty;

			DummyBusinessObject dummy4 = collection.AddNew();
			dummy4.Z0_Date = ZDateTime.Empty;
			dummy4.Z0_AnotherDate = new ZDateTime(2005, 3, 14);

			DummyBusinessObject dummy5 = collection.AddNew();
			dummy5.Z0_Date = new ZDateTime(2005, 4, 19);
			dummy5.Z0_AnotherDate = new ZDateTime(2005, 4, 23);
		}
	}
}
