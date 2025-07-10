using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace CargoWise.Common.Enumeration.Testing
{
	class ConcatCollectionTest : TestCase
	{
		public void TestHitsAllElements()
		{
			var mock = new ConcatCollection<int>(new[] { new[] { 1, 2, 3, 4, 5 }, new[] { 6, 7, 8, 9, 10 } });
			AssertEquals(10, mock.Count);
			AssertEquals(10, mock.Count);
			AssertEquals(10, mock.Count);
			AssertArrayEqualsByElements(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, mock.ToArray());
		}

		public void TestDoesNotStackOverflow()
		{
			// Note that "collection = collection.ConcatCollection( new[] {i} )" will stack overflow
			var collectionOfCollections = new List<List<int>>();
			for (int i = 0; i < 1000000; ++i)
			{
				collectionOfCollections.Add(new List<int> { i, i, i, i });
			}

			AssertEquals(4000000, new ConcatCollection<int>(collectionOfCollections).Count);
		}
	}
}