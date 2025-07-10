using System;
using System.Linq;
using NUnit.Framework;

namespace CargoWise.Common.Testing
{
	public class LinkedHashSetTest : TestCase
	{
		public void TestCollection()
		{
			var set = new LinkedHashSet<IntWrapper>();
			Action<int, int[]> addAndAssertOrder = (num, expectedNums) =>
			{
				var result = new IntWrapper(num);
				set.Add(result);
				AssertSetElementsInExactOrder(expectedNums, set);
			};
			Action<IntWrapper, int[]> removeAndAssertOrder = (elementToRemove, expectedNums) =>
			{
				set.Remove(elementToRemove);
				AssertSetElementsInExactOrder(expectedNums, set);
			};
			addAndAssertOrder(5, new[] { 5 });
			addAndAssertOrder(10, new[] { 5, 10 });
			addAndAssertOrder(7, new[] { 5, 10, 7 });
			addAndAssertOrder(-4, new[] { 5, 10, 7, -4 });
			addAndAssertOrder(3, new[] { 5, 10, 7, -4, 3 });
			removeAndAssertOrder(set.ElementAt(2), new[] { 5, 10, -4, 3 });
			addAndAssertOrder(12, new[] { 5, 10, -4, 3, 12 });
			removeAndAssertOrder(set.ElementAt(0), new[] { 10, -4, 3, 12 });
		}

		void AssertSetElementsInExactOrder(int[] expectedNums, LinkedHashSet<IntWrapper> set)
		{
			var collectionsEqual = expectedNums.Zip(set.Select(x => x.Num), (x, y) => x == y).All(x => x);
			if (!collectionsEqual)
			{
				var message = $@"Expected:
{string.Join(", ", expectedNums)}
But was:
{string.Join(", ", set.Select(x => x.Num))}";
				Fail(message);
			}
			else
			{
				Assert(true);
			}
		}

		// A reference type to use for testing instead of int, so that we have unpredictable hash code distribution
		class IntWrapper
		{
			public IntWrapper(int num)
			{
				Num = num;
			}

			public int Num { get; }
		}
	}
}