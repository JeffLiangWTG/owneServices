using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace CargoWise.Common.Collections.Testing
{
	class IListExtensionsTest : TestCase
	{
		public void TestIsRepeatingPattern()
		{
			AssertEquals(true, new[] { 1, 1, 1, 1, 1 }.IsRepeatingPattern());
			AssertEquals(true, new[] { 1, 2, 1, 2 }.IsRepeatingPattern());
			AssertEquals(true, new[] { 1, 2, 3, 1, 2, 3 }.IsRepeatingPattern());
			AssertEquals(true, new[] { 1, 2, 3, 1, 2, 3, 1 }.IsRepeatingPattern());
			AssertEquals(true, new[] { 1, 2, 3, 1, 2, 3, 1, 2 }.IsRepeatingPattern());
			AssertEquals(true, new[] { 1, 2, 3, 1, 2, 3, 1, 2, 3 }.IsRepeatingPattern());
			AssertEquals(false, new[] { 1, 1, 1, 1, 2 }.IsRepeatingPattern());
			AssertEquals(false, new[] { 2, 1, 1 }.IsRepeatingPattern());
			AssertEquals(false, new[] { 2, 1, 1 }.IsRepeatingPattern());
			AssertEquals(false, new[] { 1, 2, 3, 1, 2, 5 }.IsRepeatingPattern());
			AssertEquals(false, new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 }.IsRepeatingPattern());
			AssertEquals(false, new[] { 1, 2, 1, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 }.IsRepeatingPattern());
			AssertEquals(false, new[] { 1, 2, 1, 2, 1, 2, 1, 2, 1, 3, 7, 9 }.IsRepeatingPattern());
		}

		public void TestEnumerateWithCountChangeRetry()
		{
			var list = new List<int>();
			list.Add(1);
			list.Add(2);
			var itemsEnumerated = new List<int>();
			foreach (int item in list.EnumerateWithCountChangeRetry(3))
			{
				itemsEnumerated.Add(item);
			}

			AssertEquals(2, itemsEnumerated.Count);
			AssertEquals(1, itemsEnumerated[0]);
			AssertEquals(2, itemsEnumerated[1]);
			itemsEnumerated.Clear();
			foreach (int item in list.EnumerateWithCountChangeRetry(3))
			{
				if (item == 1)
				{
					list.Add(3);
				}

				itemsEnumerated.Add(item);
			}

			AssertEquals(9, itemsEnumerated.Count);
			AssertEquals(1, itemsEnumerated[0]);
			AssertEquals(2, itemsEnumerated[1]);
			AssertEquals(1, itemsEnumerated[2]);
			AssertEquals(2, itemsEnumerated[3]);
			AssertEquals(3, itemsEnumerated[4]);
			AssertEquals(1, itemsEnumerated[5]);
			AssertEquals(2, itemsEnumerated[6]);
			AssertEquals(3, itemsEnumerated[7]);
			AssertEquals(3, itemsEnumerated[8]);
		}

		public void TestStableSort()
		{
			var list = new List<Tuple<int, string>>();
			list.Add(new Tuple<int, string>(1, "a"));
			list.Add(new Tuple<int, string>(2, "c"));
			list.Add(new Tuple<int, string>(1, "b"));
			list.Add(new Tuple<int, string>(3, "d"));
			list.Add(new Tuple<int, string>(1, "c"));
			list.StableSort((x, y) => x.Item1.CompareTo(y.Item1));
			AssertEquals(new Tuple<int, string>(1, "a"), list[0]);
			AssertEquals(new Tuple<int, string>(1, "b"), list[1]);
			AssertEquals(new Tuple<int, string>(1, "c"), list[2]);
			AssertEquals(new Tuple<int, string>(2, "c"), list[3]);
			AssertEquals(new Tuple<int, string>(3, "d"), list[4]);
			list = new List<Tuple<int, string>>();
			list.Add(new Tuple<int, string>(1, "d"));
			list.Add(new Tuple<int, string>(2, "c"));
			list.Add(new Tuple<int, string>(1, "b"));
			list.Add(new Tuple<int, string>(3, "a"));
			list.Add(new Tuple<int, string>(1, "c"));
			list.StableSort(new ItemTwoComparer());
			AssertEquals(new Tuple<int, string>(3, "a"), list[0]);
			AssertEquals(new Tuple<int, string>(1, "b"), list[1]);
			AssertEquals(new Tuple<int, string>(2, "c"), list[2]);
			AssertEquals(new Tuple<int, string>(1, "c"), list[3]);
			AssertEquals(new Tuple<int, string>(1, "d"), list[4]);
			list = new List<Tuple<int, string>>();
			list.Add(new Tuple<int, string>(1, "d"));
			list.Add(new Tuple<int, string>(2, "c"));
			list.Add(new Tuple<int, string>(1, "b"));
			list.Add(new Tuple<int, string>(3, "a"));
			list.Add(new Tuple<int, string>(1, "c"));
			list.StableSort(x => x.Item2, x => x.Item1);
			AssertEquals(new Tuple<int, string>(3, "a"), list[0]);
			AssertEquals(new Tuple<int, string>(1, "b"), list[1]);
			AssertEquals(new Tuple<int, string>(1, "c"), list[2]);
			AssertEquals(new Tuple<int, string>(2, "c"), list[3]);
			AssertEquals(new Tuple<int, string>(1, "d"), list[4]);
			list.StableSort(x => x.Item1, x => x.Item2);
			AssertEquals(new Tuple<int, string>(1, "b"), list[0]);
			AssertEquals(new Tuple<int, string>(1, "c"), list[1]);
			AssertEquals(new Tuple<int, string>(1, "d"), list[2]);
			AssertEquals(new Tuple<int, string>(2, "c"), list[3]);
			AssertEquals(new Tuple<int, string>(3, "a"), list[4]);
			var array = new Tuple<int, string>[5];
			array[0] = new Tuple<int, string>(1, "a");
			array[1] = new Tuple<int, string>(2, "c");
			array[2] = new Tuple<int, string>(1, "b");
			array[3] = new Tuple<int, string>(3, "d");
			array[4] = new Tuple<int, string>(1, "c");
			array.StableSort((x, y) => x.Item1.CompareTo(y.Item1));
			AssertEquals(new Tuple<int, string>(1, "a"), array[0]);
			AssertEquals(new Tuple<int, string>(1, "b"), array[1]);
			AssertEquals(new Tuple<int, string>(1, "c"), array[2]);
			AssertEquals(new Tuple<int, string>(2, "c"), array[3]);
			AssertEquals(new Tuple<int, string>(3, "d"), array[4]);
			array = new Tuple<int, string>[5];
			array[0] = new Tuple<int, string>(1, "a");
			array[1] = new Tuple<int, string>(2, "c");
			array[2] = new Tuple<int, string>(1, "b");
			array[3] = new Tuple<int, string>(3, "d");
			array[4] = new Tuple<int, string>(1, "c");
			array.StableSort(x => x.Item2);
			AssertEquals(new Tuple<int, string>(1, "a"), array[0]);
			AssertEquals(new Tuple<int, string>(1, "b"), array[1]);
			AssertEquals(new Tuple<int, string>(2, "c"), array[2]);
			AssertEquals(new Tuple<int, string>(1, "c"), array[3]);
			AssertEquals(new Tuple<int, string>(3, "d"), array[4]);
			Assert(list.StableSort(new ItemTwoComparer()).Equals(list));
			Assert(list.StableSort((x, y) => x.Item1.CompareTo(y.Item1)).Equals(list));
			Assert(list.StableSort(x => x.Item1).Equals(list));
			Assert(array.StableSort(new ItemTwoComparer()).Equals(array));
			Assert(array.StableSort((x, y) => x.Item1.CompareTo(y.Item1)).Equals(array));
			Assert(array.StableSort(x => x.Item1).Equals(array));
		}

		public void TestStableSortReorderCheck() => AssertStableSortReorderCheck(Comparer<Tuple<int>>.Default);
		public void TestStableSortReorderCheck_NullComparer() => AssertStableSortReorderCheck(null);
		void AssertStableSortReorderCheck(Comparer<Tuple<int>> comparer)
		{
			var listToSort = new List<Tuple<int>>();
			listToSort.Add(Tuple.Create(3));
			listToSort.Add(Tuple.Create(2));
			listToSort.Add(Tuple.Create(1));
			AssertEquals(true, listToSort.StableSortWithReorderCheck(comparer));
			AssertEquals(false, listToSort.StableSortWithReorderCheck(comparer));
		}

		public void TestStableSortWithReorderCheck_Performance()
		{
			var list = new List<string> { "D", "C", "B", "A" };
			var comparer = new Comparer();
			list.StableSortWithReorderCheck(comparer);
			AssertLessThanOrEqualTo("QuickSort used by IEnumerable<>.OrderBy() performs an exponential number of comparisons to sort in the worst case.", comparer.numberOfComparisons, 4);
		}

		class Comparer : IComparer<string>
		{
			public int numberOfComparisons;
			public int Compare(string x, string y)
			{
				numberOfComparisons++;
				return x.CompareTo(y);
			}
		}

		public void TestInsertInSortOrder()
		{
			var list1 = new List<int>();
			list1.InsertInSortOrder(2);
			list1.InsertInSortOrder(3);
			list1.InsertInSortOrder(2);
			list1.InsertInSortOrder(1);
			AssertEquals(1, list1[0]);
			AssertEquals(2, list1[1]);
			AssertEquals(2, list1[2]);
			AssertEquals(3, list1[3]);
			var list2 = new List<Tuple<int, string>>();
			var itemOneComparer = new ItemOneComparer();
			list2.InsertInSortOrder(Tuple.Create(1, "a"), itemOneComparer);
			list2.InsertInSortOrder(Tuple.Create(2, "c"), itemOneComparer);
			list2.InsertInSortOrder(Tuple.Create(1, "b"), itemOneComparer);
			list2.InsertInSortOrder(Tuple.Create(3, "d"), itemOneComparer);
			list2.InsertInSortOrder(Tuple.Create(1, "c"), itemOneComparer);
			AssertEquals(Tuple.Create(1, "b"), list2[0]);
			AssertEquals(Tuple.Create(1, "c"), list2[1]);
			AssertEquals(Tuple.Create(1, "a"), list2[2]);
			AssertEquals(Tuple.Create(2, "c"), list2[3]);
			AssertEquals(Tuple.Create(3, "d"), list2[4]);
			var list3 = new List<Tuple<int, string>>();
			var itemTwoComparer = new ItemTwoComparer();
			list3.InsertInSortOrder(Tuple.Create(1, "d"), itemTwoComparer);
			list3.InsertInSortOrder(Tuple.Create(2, "c"), itemTwoComparer);
			list3.InsertInSortOrder(Tuple.Create(1, "b"), itemTwoComparer);
			list3.InsertInSortOrder(Tuple.Create(3, "a"), itemTwoComparer);
			list3.InsertInSortOrder(Tuple.Create(1, "c"), itemTwoComparer);
			AssertEquals(Tuple.Create(3, "a"), list3[0]);
			AssertEquals(Tuple.Create(1, "b"), list3[1]);
			AssertEquals(Tuple.Create(1, "c"), list3[2]);
			AssertEquals(Tuple.Create(2, "c"), list3[3]);
			AssertEquals(Tuple.Create(1, "d"), list3[4]);
		}

		public void TestBinarySearch()
		{
			var empty = new List<int>();
			var one = new List<int> { 5 };
			var three = new List<int> { 3, 5, 7 };
			AssertEquals(empty.BinarySearch(5), ((IList<int>)empty).BinarySearch(5));
			AssertEquals(one.BinarySearch(5), ((IList<int>)one).BinarySearch(5));
			AssertEquals(one.BinarySearch(3), ((IList<int>)one).BinarySearch(3));
			AssertEquals(one.BinarySearch(6), ((IList<int>)one).BinarySearch(6));
			AssertEquals(three.BinarySearch(4), ((IList<int>)three).BinarySearch(4));
			AssertEquals(three.BinarySearch(2), ((IList<int>)three).BinarySearch(2));
			AssertEquals(three.BinarySearch(8), ((IList<int>)three).BinarySearch(8));
			AssertEquals(three.BinarySearch(7), ((IList<int>)three).BinarySearch(7));
		}

		public void TestRemoveAll()
		{
			var things = new List<string> { "a", "b", "a", "a", "b" };
			((IList<string>)things).RemoveAll(x => x.Equals("a"));
			AssertEquals(2, things.Count);
			AssertEquals(0, things.Where(x => x.Equals("a")).ToList().Count);
		}

		class ItemOneComparer : IComparer<Tuple<int, string>>
		{
			public int Compare(Tuple<int, string> x, Tuple<int, string> y)
			{
				return x.Item1.CompareTo(y.Item1);
			}
		}

		class ItemTwoComparer : IComparer<Tuple<int, string>>
		{
			public int Compare(Tuple<int, string> x, Tuple<int, string> y)
			{
				return x.Item2.CompareTo(y.Item2);
			}
		}
	}
}
