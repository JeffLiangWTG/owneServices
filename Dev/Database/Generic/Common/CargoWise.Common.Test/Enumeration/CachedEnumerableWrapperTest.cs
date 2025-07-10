using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;

namespace CargoWise.Common
{
	class CachedEnumerableWrapperTest : TestCase
	{
		public void TestResultsMatchEachEnumeration()
		{
			var counterValue = 0;
			IEnumerable<int> Counter()
			{
				for (var j = 0; j < 3; j++)
				{
					yield return counterValue;
				}
			}

			var cache = new CachedEnumerableWrapper<int>(Counter());
			var en1 = cache.GetEnumerator();
			var en2 = cache.GetEnumerator();
			AssertEquals(en1.MoveNext(), en2.MoveNext());
			AssertEquals(en1.Current, en2.Current);
			AssertEquals(en1.MoveNext(), en2.MoveNext());
			AssertEquals(en1.Current, en2.Current);
			AssertEquals(en1.MoveNext(), en2.MoveNext());
			AssertEquals(en1.Current, en2.Current);
			Assert("End of enumerator - en1", !en1.MoveNext());
			Assert("End of enumerator - en2", !en2.MoveNext());
		}

		public void TestFlush()
		{
			var i = 0;
			IEnumerable<int> Counter()
			{
				while (i < 3)
				{
					yield return i++;
				}
			}

			var cache = new CachedEnumerableWrapper<int>(Counter());
			AssertEquals("We haven't enumerated yet, so i should still be 0", 0, i);
			cache.Flush();
			Assert("Should have fully loaded the cache", cache.IsFullyEnumerated);
			AssertEquals("Should be fully enumerated", 3, i);
		}

		public void TestEnumerate()
		{
			List<object> list = new List<object>();
			list.Add(1);
			list.Add(2);
			list.Add(3);
			CachedEnumerableWrapper<object> enumerable = new CachedEnumerableWrapper<object>(list);
			int expectedNext = 1;
			foreach (int actualNext in enumerable)
			{
				AssertEquals(actualNext, expectedNext);
				expectedNext++;
			}

			expectedNext = 1;
			list[0] = "splaty";
			list[1] = "blah";
			list[2] = "aha";
			// should be cached now
			foreach (int actual_next in enumerable)
			{
				AssertEquals(actual_next, expectedNext);
				expectedNext++;
			}
		}

		public void TestPartialEnumerateCache()
		{
			object[] list = new object[5];
			list[0] = 0;
			list[1] = 1;
			list[2] = 2;
			list[3] = 3;
			list[4] = 4;
			CachedEnumerableWrapper<object> enumerable = new CachedEnumerableWrapper<object>(list);
			int expected_next = 0;
			foreach (int actual_next in enumerable)
			{
				// should allow nesting of foreach's
				int expected_next2 = 0;
				foreach (int actual_next2 in enumerable)
				{
					AssertEquals(actual_next2, expected_next2);
					if (expected_next2 == 3)
					{
						break;
					}

					expected_next2++;
				}

				AssertEquals(actual_next, expected_next);
				if (expected_next == 2)
				{
					break;
				}

				expected_next++;
			}

			list[0] = "zero";
			list[1] = "one";
			list[2] = "two";
			list[3] = "three";
			list[4] = "four";
			for (int i = 0; i < 2; i++)
			{
				ArrayList resultant_items = new ArrayList();
				foreach (object actual_next in enumerable)
				{
					resultant_items.Add(actual_next);
				}

				// should cache only the ones that have been enumerated so far
				AssertEquals(0, resultant_items[0]);
				AssertEquals(1, resultant_items[1]);
				AssertEquals(2, resultant_items[2]);
				AssertEquals(3, resultant_items[3]); // this one MUST return the cached value (4, not 'four') because the cached enumerator should be the *most enumerated*
				AssertEquals("four", resultant_items[4]);
			}
		}
	}
}