using CargoWise.EntityFramework.Testing;

namespace Enterprise.DocumentScanning.Business.Test
{
	public class DocumentResultCollectionTest : TestCaseWithFactory
	{
		public void TestAdd()
		{
			AssertEquals("No elements in array", 0, Collection.Length);
			Collection.Add(DocResult);
			AssertEquals("One element in array", 1, Collection.Length);
			AssertEquals("Same element", DocResult, Collection[0]);

			Collection.Add(DocResult2);
			AssertEquals("Two elements in array", 2, Collection.Length);
			AssertEquals("Same element in correct order", DocResult2, Collection[1]);
		}

		public void TestRemove()
		{
			AssertEquals("No elements in array", 0, Collection.Length);
			Collection.Add(DocResult);
			AssertEquals("One element in array", 1, Collection.Length);
			Collection.Remove(DocResult);
			AssertEquals("No elements in array", 0, Collection.Length);
		}

		public void TestLength()
		{
			AssertEquals("No elements in array", 0, Collection.Length);
			Collection.Add(DocResult);
			AssertEquals("One element in array", 1, Collection.Length);
			Collection.Add(DocResult2);
			AssertEquals("Two elements in array", 2, Collection.Length);
		}

		public void TestContains()
		{
			Assert("Element not in collection", !Collection.Contains(DocResult));
			Assert("Element not in collection", !Collection.Contains(DocResult2));
			Collection.Add(DocResult);
			Assert("Element is now in collection", Collection.Contains(DocResult));
			Assert("Element not in collection", !Collection.Contains(DocResult2));
		}

		public void TestIndexOf()
		{
			AssertEquals("Index of element not in there yet: ", -1, Collection.IndexOf(DocResult));
			Collection.Add(DocResult);
			Collection.Add(DocResult2);
			AssertEquals("Index of element DocResult should be 0", 0, Collection.IndexOf(DocResult));
			AssertEquals("Index of element DocResult2 should be 1", 1, Collection.IndexOf(DocResult2));
		}

		public void TestIndexer()
		{
			Collection.Add(DocResult);
			Collection.Add(DocResult2);

			AssertEquals("Indexer should return DocResult element in 0 position", DocResult, Collection[0]);
			AssertEquals("Indexer should return DocResult2 element in 1 position", DocResult2, Collection[1]);
		}

		protected override void SetUp()
		{
			base.SetUp();
			MasterFactory = new DocumentFactoryProvider().GetFactory(Factory);
			Collection = new DocumentResultCollection();
			DocResult = new DocumentResult(MasterFactory);
			DocResult2 = new DocumentResult(MasterFactory);
		}

		DocumentResultCollection Collection;
		DocumentFactory MasterFactory;
		DocumentResult DocResult;
		DocumentResult DocResult2;
	}
}
