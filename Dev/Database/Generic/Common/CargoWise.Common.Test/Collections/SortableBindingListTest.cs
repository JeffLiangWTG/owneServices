using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Moq;
using NUnit.Framework;

namespace CargoWise.Common.Collections.Testing
{
	public class SortableBindingListTests : TestCase
	{
		class SortingItem
		{
			public int Age { get; set; }
			public string Name { get; set; }
			public object NonComparable => new object();
		}

		public void TestIsBindingList()
		{
			AssertEquals(true, new SortableBindingList<string>() is BindingList<string>);
			AssertEquals(true, new SortableBindingList<int>() is BindingList<int>);
			AssertEquals(true, new SortableBindingList<SortingItem>() is BindingList<SortingItem>);
		}

		public void TestSupportsSorting()
		{
			AssertEquals(true, ((IBindingList)new SortableBindingList<string>()).SupportsSorting);
			AssertEquals(true, ((IBindingList)new SortableBindingList<int>()).SupportsSorting);
			AssertEquals(true, ((IBindingList)new SortableBindingList<SortingItem>()).SupportsSorting);
		}

		public void TestInvalidSortPropertyNameThrowsArgumentException()
		{
			var list = new SortableBindingList<SortingItem>();
			list.Add(new SortingItem() { Age = 10, Name = "ABC" });
			list.Add(new SortingItem() { Age = 30, Name = "LMN" });

			var bindingList = list as IBindingList;
			var mockDescriptor = new Mock<PropertyDescriptor>("Title", null);
			mockDescriptor.Setup(d => d.Name).Returns("Title");

			AssertExceptionThrown<ArgumentException>(() => (bindingList).ApplySort(mockDescriptor.Object, ListSortDirection.Ascending));
		}

		public void TestNonComparableSortPropertyThrowsArgumentException()
		{
			var list = GetSortingList();

			var bindingList = list as IBindingList;
			var mockDescriptor = new Mock<PropertyDescriptor>("NonComparable", null);
			mockDescriptor.Setup(d => d.Name).Returns("NonComparable");

			AssertExceptionThrown<ArgumentException>(() => (bindingList).ApplySort(mockDescriptor.Object, ListSortDirection.Ascending));
		}

		public void TestSort()
		{
			var list = GetSortingList();

			var listChangedCalled = false;
			list.ListChanged += (sender, e) => { listChangedCalled = true; };

			void TestSort(string propertyName, Type propertyType, ListSortDirection direction, List<SortingItem> sortedList)
			{
				listChangedCalled = false;
				
				var mockDescriptor = new Mock<PropertyDescriptor>(propertyName, null);
				mockDescriptor.Setup(d => d.Name).Returns(propertyName);
				mockDescriptor.Setup(d => d.PropertyType).Returns(propertyType);
				mockDescriptor.Setup(d => d.ComponentType).Returns(typeof(SortingItem));

				var bindingList = list as IBindingList;
				(bindingList).ApplySort(mockDescriptor.Object, direction);

				for (int i = 0; i < sortedList.Count; i++)
				{
					AssertEquals("List items should be ordered correctly", sortedList[i], list[i]);
				}
				AssertEquals("IsStorted must be true", true, bindingList.IsSorted);
				AssertEquals("Sorting direction must be correct", direction, bindingList.SortDirection);
				AssertEquals("Sort Property must be correct", true, ReferenceEquals(mockDescriptor.Object, bindingList.SortProperty));
				AssertEquals("ListChanged Event Must Be raised", listChangedCalled, true);
			}

			TestSort("Age", typeof(int), ListSortDirection.Ascending, list.OrderBy(i => i.Age).ToList());
			TestSort("Age", typeof(int), ListSortDirection.Descending, list.OrderByDescending(i => i.Age).ToList());
			TestSort("Name", typeof(string), ListSortDirection.Ascending, list.OrderBy(i => i.Name).ToList());
			TestSort("Name", typeof(string), ListSortDirection.Descending, list.OrderByDescending(i => i.Name).ToList());
		}

		public void TestSort_OnlySortedListsChanged()
		{
			var list = GetSortingList();

			void TestSort(string propertyName, Type propertyType, ListSortDirection direction, List<SortingItem> sortedList)
			{
				var mockDescriptor = new Mock<PropertyDescriptor>(propertyName, null);
				mockDescriptor.Setup(d => d.Name).Returns(propertyName);
				mockDescriptor.Setup(d => d.PropertyType).Returns(propertyType);
				mockDescriptor.Setup(d => d.ComponentType).Returns(typeof(SortingItem));

				var bindingList = list as IBindingList;
				(bindingList).ApplySort(mockDescriptor.Object, direction);

				var listChangedCalled = false;
				list.ListChanged += (sender, e) => { listChangedCalled = true; };

				(bindingList).ApplySort(mockDescriptor.Object, direction);
				AssertEquals("Would you sort an already sorted collection?", false, listChangedCalled);
			}

			TestSort("Age", typeof(int), ListSortDirection.Ascending, list.OrderBy(i => i.Age).ToList());
			TestSort("Age", typeof(int), ListSortDirection.Descending, list.OrderByDescending(i => i.Age).ToList());
			TestSort("Name", typeof(string), ListSortDirection.Ascending, list.OrderBy(i => i.Name).ToList());
			TestSort("Name", typeof(string), ListSortDirection.Descending, list.OrderByDescending(i => i.Name).ToList());
		}

		static SortableBindingList<SortingItem> GetSortingList()
		{
			var list = new SortableBindingList<SortingItem>();
			list.Add(new SortingItem() { Age = 10, Name = "ABC" });
			list.Add(new SortingItem() { Age = 5, Name = "XYZ" });
			list.Add(new SortingItem() { Age = 30, Name = "LMN" });
			return list;
		}
	}
}
