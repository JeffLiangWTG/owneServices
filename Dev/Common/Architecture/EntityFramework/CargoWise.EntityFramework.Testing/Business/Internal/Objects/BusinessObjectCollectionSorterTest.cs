using System;
using System.ComponentModel;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.EntityFramework.Testing
{
	sealed class BusinessObjectCollectionSorterTest : TestCaseWithDummy
	{
		public void TestNonComparableSortProperty()
		{
			DummyBusinessObjectCollection collection = new DummyBusinessObjectCollection(Factory);
			BusinessObjectCollectionSorter sorter = collection.Sorter;

			DummyBusinessObject b1 = collection.AddNew();
			DummyBusinessObject b2 = collection.AddNew();
			DummyBusinessObject b3 = collection.AddNew();
			PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(b1);
			PropertyDescriptor unComparableProperty = properties.Find("Collection", false);

			sorter.ApplySort(unComparableProperty, ListSortDirection.Ascending);
			AssertEquals("UnComparableSortProperty", ErrorReporter.LastKeyReported);
			AssertEquals(sorter.SortProperty, null);
			ErrorReporter.Clear();

			sorter.Sort("Collection", ListSortDirection.Ascending);
			AssertEquals("UnComparableSortProperty", ErrorReporter.LastKeyReported);
			AssertEquals(sorter.SortProperty, null);
			ErrorReporter.Clear();
		}

		public void TestSort()
		{
			DummyBusinessObjectCollection collection = new DummyBusinessObjectCollection(Factory);

			Assert(collection.Count == 0);
			DummyBusinessObject b1 = collection.AddNew();
			DummyBusinessObject b2 = collection.AddNew();
			DummyBusinessObject b3 = collection.AddNew();

			b1.Z0_Description = "A";
			b3.Z0_Description = "B";
			b2.Z0_Description = "C";

			AssertEquals(collection[0], b1);
			AssertEquals(collection[1], b2);
			AssertEquals(collection[2], b3);

			collection.Sort("Z0_Description", ListSortDirection.Ascending);
			AssertEquals(collection[0], b1);
			AssertEquals(collection[1], b3);
			AssertEquals(collection[2], b2);

			collection.Sort("Z0_Description", ListSortDirection.Descending);
			AssertEquals(collection[0], b2);
			AssertEquals(collection[1], b3);
			AssertEquals(collection[2], b1);
		}

		public void TestResort()
		{
			DummyBusinessObjectCollection collection = new DummyBusinessObjectCollection(Factory);

			Assert(collection.Count == 0);
			DummyBusinessObject b1 = collection.AddNew();
			DummyBusinessObject b2 = collection.AddNew();
			DummyBusinessObject b3 = collection.AddNew();

			b1.Z0_Description = "A";
			b3.Z0_Description = "B";
			b2.Z0_Description = "D";

			AssertEquals(collection[0], b1);
			AssertEquals(collection[1], b2);
			AssertEquals(collection[2], b3);

			collection.Sort("Z0_Description", ListSortDirection.Ascending);
			AssertEquals(collection[0], b1);
			AssertEquals(collection[1], b3);
			AssertEquals(collection[2], b2);

			DummyBusinessObject b4 = collection.AddNew();
			b4.Z0_Description = "C";

			collection.Sorter.Resort();
			AssertEquals(collection[0], b1);
			AssertEquals(collection[1], b3);
			AssertEquals(collection[2], b4);
			AssertEquals(collection[3], b2);
		}

		public void TestResort_WithDateTimeOffset()
		{
			DummyBusinessObjectCollection collection = new DummyBusinessObjectCollection(Factory);

			Assert(collection.Count == 0);
			DummyBusinessObject b1 = collection.AddNew();
			DummyBusinessObject b2 = collection.AddNew();
			DummyBusinessObject b3 = collection.AddNew();

			b1.Z0_DateTimeOffset = new ZDateTimeOffset(2000, 1, 1, 1, 30, 40, TimeSpan.Zero);
			b3.Z0_DateTimeOffset = new ZDateTimeOffset(2000, 1, 1, 1, 30, 40, TimeSpan.FromHours(-1));
			b2.Z0_DateTimeOffset = new ZDateTimeOffset(2000, 1, 1, 1, 30, 40, TimeSpan.FromHours(-3));

			AssertEquals(collection[0], b1);
			AssertEquals(collection[1], b2);
			AssertEquals(collection[2], b3);

			collection.Sort("Z0_DateTimeOffset", ListSortDirection.Ascending);
			AssertEquals(collection[0], b1);
			AssertEquals(collection[1], b3);
			AssertEquals(collection[2], b2);

			DummyBusinessObject b4 = collection.AddNew();
			b4.Z0_DateTimeOffset = new ZDateTimeOffset(2000, 1, 1, 1, 30, 40, TimeSpan.FromHours(-2));

			collection.Sorter.Resort();
			AssertEquals(collection[0], b1);
			AssertEquals(collection[1], b3);
			AssertEquals(collection[2], b4);
			AssertEquals(collection[3], b2);

			collection.Sort("Z0_DateTimeOffset", ListSortDirection.Descending);

			AssertEquals(collection[0], b2);
			AssertEquals(collection[1], b4);
			AssertEquals(collection[2], b3);
			AssertEquals(collection[3], b1);
		}

		public void TestResort_WithCustomPropertyDescriptor()
		{
			DummyBusinessObjectCollection collection = new DummyBusinessObjectCollection(Factory);

			Assert(collection.Count == 0);
			DummyBusinessObject b1 = collection.AddNew();
			DummyBusinessObject b2 = collection.AddNew();
			DummyBusinessObject b3 = collection.AddNew();

			b1.Z0_Description = "AZ";
			b3.Z0_Description = "BX";
			b2.Z0_Description = "DW";

			AssertEquals(b1.Z0_Description, collection[0], b1);
			AssertEquals(b2.Z0_Description, collection[1], b2);
			AssertEquals(b3.Z0_Description, collection[2], b3);

			ReverseStringPropertyDescriptor reverseDescriptionPropertyDescriptor = new ReverseStringPropertyDescriptor("Z0_Description_Reversed", DummyBizoSchema.Z0_Description);
			collection.Sorter.ApplySort(reverseDescriptionPropertyDescriptor, ListSortDirection.Ascending);
			AssertArrayEqualsByElements(collection.ToArray(), new[] { b2, b3, b1 });

			DummyBusinessObject b4 = collection.AddNew();
			b4.Z0_Description = "CY";

			collection.Sorter.Resort();
			AssertArrayEqualsByElements(collection.ToArray(), new[] { b2, b3, b4, b1 });
		}

		public void TestApplySort()
		{
			DummyBusinessObjectCollection collection = new DummyBusinessObjectCollection(Factory);

			Assert(collection.Count == 0);
			DummyBusinessObject b1 = collection.AddNew();
			DummyBusinessObject b2 = collection.AddNew();
			DummyBusinessObject b3 = collection.AddNew();

			b1.Z0_Description = "A";
			b3.Z0_Description = "B";
			b2.Z0_Description = "C";

			AssertEquals(collection[0], b1);
			AssertEquals(collection[1], b2);
			AssertEquals(collection[2], b3);

			collection.Sorter.ApplySort(b1.Z0_DescriptionInfo.PropertyDescriptor, ListSortDirection.Ascending);
			AssertEquals(collection[0], b1);
			AssertEquals(collection[1], b3);
			AssertEquals(collection[2], b2);

			Assert(collection.Sorter.IsSorted);
			AssertEquals(ListSortDirection.Ascending, collection.Sorter.SortDirection);
			AssertEquals(b1.Z0_DescriptionInfo.PropertyDescriptor, collection.Sorter.SortProperty);

			collection.Sorter.RemoveSort();
			Assert(!collection.Sorter.IsSorted);
			AssertNull(collection.Sorter.SortProperty);
		}

		public void TestApplySort_WithDateTimeOffset()
		{
			DummyBusinessObjectCollection collection = new DummyBusinessObjectCollection(Factory);

			Assert(collection.Count == 0);
			DummyBusinessObject b1 = collection.AddNew();
			DummyBusinessObject b2 = collection.AddNew();
			DummyBusinessObject b3 = collection.AddNew();

			b1.Z0_DateTimeOffset = new ZDateTimeOffset(2000, 1, 1, 1, 30, 40, TimeSpan.Zero);
			b3.Z0_DateTimeOffset = new ZDateTimeOffset(2000, 1, 1, 1, 30, 40, TimeSpan.FromHours(-1));
			b2.Z0_DateTimeOffset = new ZDateTimeOffset(2000, 1, 1, 1, 30, 40, TimeSpan.FromHours(-3));

			AssertEquals(collection[0], b1);
			AssertEquals(collection[1], b2);
			AssertEquals(collection[2], b3);

			collection.Sorter.ApplySort(b1.Z0_DateTimeOffsetInfo.PropertyDescriptor, ListSortDirection.Ascending);
			AssertEquals(collection[0], b1);
			AssertEquals(collection[1], b3);
			AssertEquals(collection[2], b2);

			Assert(collection.Sorter.IsSorted);
			AssertEquals(ListSortDirection.Ascending, collection.Sorter.SortDirection);
			AssertEquals(b1.Z0_DateTimeOffsetInfo.PropertyDescriptor, collection.Sorter.SortProperty);

			collection.Sorter.RemoveSort();
			Assert(!collection.Sorter.IsSorted);
			AssertNull(collection.Sorter.SortProperty);
		}
	}
}
