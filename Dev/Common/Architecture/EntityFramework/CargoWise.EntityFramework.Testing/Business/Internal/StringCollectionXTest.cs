using System.Collections.Specialized;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class StringCollectionXTest : TestCase
	{
		public void TestStringCollectionConstructor()
		{
			StringCollection collection = new StringCollection();
			collection.Add("teapot");
			collection.Add("noodle");

			StringCollectionX collectionX = new StringCollectionX(collection);

			AssertEquals("ordered correct results", "teapot", collectionX.ToArray()[0]);
			AssertEquals("ordered correct results", "noodle", collectionX.ToArray()[1]);
		}

		public void TestAddRangeConstructor()
		{
			StringCollectionX collection = new StringCollectionX("String1", "String2");
			AssertEquals("ordered correct results", "String1", collection.ToArray()[0]);
			AssertEquals("ordered correct results", "String2", collection.ToArray()[1]);
		}

		public void TestToArray()
		{
			StringCollectionX collection = new StringCollectionX();
			collection.Add("teapot");
			collection.Add("noodle");

			AssertEquals("ordered correct results", "teapot", collection.ToArray()[0]);
			AssertEquals("ordered correct results", "noodle", collection.ToArray()[1]);
		}

		public void TestAddRange()
		{
			StringCollectionX collection = new StringCollectionX();

			StringCollectionX rangeCollection = new StringCollectionX();
			rangeCollection.Add("teapot");
			rangeCollection.Add("noodle");

			collection.AddRange(rangeCollection);

			AssertEquals("ordered correct results", "teapot", collection.ToArray()[0]);
			AssertEquals("ordered correct results", "noodle", collection.ToArray()[1]);
		}

		public void TestToString()
		{
			StringCollectionX collection = new StringCollectionX();
			collection.Add("teapot");
			collection.Add("noodle");

			AssertEquals("teapot\r\nnoodle", collection.ToString());
		}
	}
}
