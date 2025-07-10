using System.Collections.Generic;
using NUnit.Framework;

namespace CargoWise.Common.Collections.Testing
{
	class MergeSortTest : TestCase
	{
		public void TestSort_SingleElement()
		{
			AssertNoExceptionThrown(() =>
			{
				MergeSort.Sort(new List<string> { "A" }, Comparer<string>.Default);
			});
		}

		public void TestSort_IsStable()
		{
			var list = new List<(string key, string value)> { ("1", "A"), ("1", "B"), ("1", "C"), ("0", "B"), ("0", "A"), ("2", "A"), };
			MergeSort.Sort(list, new Comparer());
			AssertContainsExactElementsInExactOrder(new List<(string, string)> { ("0", "B"), ("0", "A"), ("1", "A"), ("1", "B"), ("1", "C"), ("2", "A"), }, list);
		}

		public void TestSort()
		{
			var list = new List<int> { 3, 2, 1 };
			MergeSort.Sort(list, Comparer<int>.Default);
			AssertContainsExactElementsInExactOrder(new List<int> { 1, 2, 3 }, list);
		}

		public void TestSort_OrderChanged_SingleElement()
		{
			AssertEquals("Sorting a list with a single element cannot change the order", false, MergeSort.Sort(new List<(string, string)> { ("A", "B") }, new Comparer()));
		}

		public void TestSort_OrderChanged_AlreadyOrdered()
		{
			var list = new List<(string key, string value)> { ("1", "A"), ("2", "B"), ("3", "C"), };
			AssertEquals("Sorting a sorted list doesn't change the order", false, MergeSort.Sort(list, new Comparer()));
		}

		public void TestSort_OrderChanged_Unsorted()
		{
			var list = new List<(string key, string value)> { ("2", "B"), ("1", "A"), ("3", "C"), };
			AssertEquals("Sorting an unsorted list should change the order", true, MergeSort.Sort(list, new Comparer()));
		}

		public void TestSort_OrderChanged_Unsorted2()
		{
			var list = new List<(string key, string value)> { ("3", "C"), ("1", "A"), ("2", "B"), };
			AssertEquals("Sorting an unsorted list should change the order", true, MergeSort.Sort(list, new Comparer()));
		}

		class Comparer : IComparer<(string key, string value)>
		{
			public int Compare((string key, string value) x, (string key, string value) y)
			{
				return x.key.CompareTo(y.key);
			}
		}
	}
}