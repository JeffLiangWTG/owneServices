using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.Common;
using NUnit.Framework;

namespace CargoWise.Common.Testing
{
	class IDictionaryExtensionsTest : TestCase
	{
		public void TestToSortedDictionary()
		{
			var values = new Dictionary<string, int> { { "a", 1 }, { "b", 2 }, { "c", 3 }, };
			var sortedDictionary = values.ToSortedDictionary(s => s.Key, StringComparer.OrdinalIgnoreCase);
			AssertEquals(1, sortedDictionary["a"].Value);
			AssertEquals(2, sortedDictionary["b"].Value);
			AssertEquals(3, sortedDictionary["c"].Value);
		}

		public void TestMerge_NoDuplicatedKeyHandlingActionSupplied()
		{
			var dictionary1 = new Dictionary<string, int> { { "One", 1 }, { "Two", 2 }, };
			var dictionary2 = new Dictionary<string, int> { { "One", 1 }, { "Two", 2 }, { "Threee!", 3 }, };
			dictionary1.Merge(dictionary2, (kvp) =>
			{
			});
			AssertContainsExactElementsInAnyOrder("Supplying an empty merge action shouldn't explode, and shouldn't alter duplicated entries", new[] { new KeyValuePair<string, int>("One", 1), new KeyValuePair<string, int>("Two", 2), new KeyValuePair<string, int>("Threee!", 3), }, dictionary1);
		}

		public void TestMerge_MergingDuplicatedKeyHandlingActionSupplied()
		{
			var dictionary1 = new Dictionary<string, int> { { "One", 1 }, { "Two", 2 }, };
			var dictionary2 = new Dictionary<string, int> { { "One", 1 }, { "Two", 2 }, { "Threee!", 3 }, };
			dictionary1.Merge(dictionary2, (kvp) => dictionary1[kvp.Key] += kvp.Value);
			AssertContainsExactElementsInAnyOrder("Supplying an empty merge action shouldn't explode, and shouldn't alter duplicated entries", new[] { new KeyValuePair<string, int>("One", 2), new KeyValuePair<string, int>("Two", 4), new KeyValuePair<string, int>("Threee!", 3), }, dictionary1);
			AssertContainsExactElementsInAnyOrder("Second dictionary should not be mutated", new[] { new KeyValuePair<string, int>("One", 1), new KeyValuePair<string, int>("Two", 2), new KeyValuePair<string, int>("Threee!", 3), }, dictionary2);
		}

		public void TestGetOrAdd_WithFunc()
		{
			var dictionary = new Dictionary<int, string>();
			AssertEquals("one", dictionary.GetOrAdd(1, () => "one"));
			AssertEquals(1, dictionary.Count);
			AssertEquals("one", dictionary.GetOrAdd(1, () => "boo"));
			AssertEquals(1, dictionary.Count);
			AssertEquals("two", dictionary.GetOrAdd(2, () => "two"));
			AssertEquals(2, dictionary.Count);
		}

		public void TestGetOrAdd_WithDefaultConstructor()
		{
			var dictionary = new Dictionary<int, int>();
			AssertEquals(0, dictionary.GetOrAdd(1));
			AssertEquals(1, dictionary.Count);
			AssertEquals(0, dictionary.GetOrAdd(1));
			AssertEquals(1, dictionary.Count);
		}

		public void TestGetValueSafe()
		{
			var dictionary1 = new Dictionary<int, int>();
			AssertEquals(0, dictionary1.GetValueSafe(49));
			dictionary1[2] = 1;
			AssertEquals(1, dictionary1.GetValueSafe(2));
			var dictionary2 = new Dictionary<int, object>();
			AssertNull(dictionary2.GetValueSafe(91));
			dictionary2[2] = 1;
			AssertEquals(1, dictionary1.GetValueSafe(2));
		}

		public void TestGetOrAddOnConcurrentDictionaryIsThreadSafe()
		{
			var dictionary = (IDictionary<int, int>)new ConcurrentDictionary<int, int>();
			Parallel.For(0, 100, i =>
			{
				Parallel.For(0, 10, j => dictionary.GetOrAdd(i, () =>
				{
					return j;
				}));
			});
			AssertEquals(100, dictionary.Count);
		}

		public void TestToDictionaryList()
		{
			var dictionary = Enumerable.Range(0, 10000).ToDictionaryList(t => t % 10);

			AssertEquals(10, dictionary.Count);
			foreach (var pair in dictionary)
			{
				AssertEquals(1000, pair.Value.Count);
			}
		}
	}
}
