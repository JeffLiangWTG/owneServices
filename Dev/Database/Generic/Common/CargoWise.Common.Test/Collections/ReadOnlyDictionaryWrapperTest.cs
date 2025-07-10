using System.Collections.Generic;
using NUnit.Framework;

namespace CargoWise.Common.Collections.Testing
{
	public class ReadOnlyDictionaryWrapperTest : TestCase
	{
		public void TestAllOfTheFunctions()
		{
			var dictionary = new Dictionary<int, int>();
			dictionary[0] = 1;
			dictionary[1] = 2;
			dictionary[2] = 3;
			dictionary[3] = 4;
			var read = new ReadOnlyDictionaryWrapper<int, int>(() => dictionary);
			AssertEquals(dictionary[0], read[0]);
			AssertEquals(dictionary.ContainsKey(0), read.ContainsKey(0));
			AssertContainsExactElementsInAnyOrder(dictionary.Values, read.Values);
			AssertContainsExactElementsInAnyOrder(dictionary.Keys, read.Keys);
			AssertEquals(dictionary.Count, read.Count);
		}

		public void TestNotImmutable()
		{
			var dictionary = new Dictionary<int, int>();
			dictionary[0] = 1;
			var read = new ReadOnlyDictionaryWrapper<int, int>(() => dictionary);
			dictionary[3] = 4;
			AssertEquals(2, read.Count);
		}

		public void TestReplacable()
		{
			var dictionary = new Dictionary<int, int>();
			dictionary[0] = 1;
			var read = new ReadOnlyDictionaryWrapper<int, int>(() => dictionary);
			dictionary = new Dictionary<int, int> { { 1, 1 }, { 2, 2 }, { 3, 3 } };
			AssertEquals(3, read.Count);
		}
	}
}