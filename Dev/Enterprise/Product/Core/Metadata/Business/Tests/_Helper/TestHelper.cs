using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;

namespace Enterprise.Metadata.Business.Tests
{
	public class TestHelper : TestCase
	{
		public void AssertIListEqualsByElements(IList expected, IList actual)
		{
			var expectedArray = new Object[expected.Count];
			expected.CopyTo(expectedArray, 0);
			var actualArray = new Object[actual.Count];
			actual.CopyTo(actualArray, 0);
			AssertArrayEqualsByElements(expectedArray, actualArray);
		}

		public void AssertDictionaryEqualsByElements<K, V>(IDictionary<K,V> expected, IDictionary<K,V> actual)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Dictionaries Lengths should be equal.", expected.Count, actual.Count);
				AssertSubDictionary(expected, actual, "Expected");
				AssertSubDictionary(actual, expected, "Actual");
			});
		}

		/// <summary>
		/// Assert dic1 is a sub-dictionary of dic2
		/// </summary>
		void AssertSubDictionary<K,V>(IDictionary<K,V> dic1, IDictionary<K,V> dic2, string message)
		{
			foreach (var pair in dic1)
			{
				Assert(string.Format("{0} '{1}'->'{2}' is not found.", message, pair.Key, pair.Value), dic2.Contains(pair));
			}
		}
	}
}
