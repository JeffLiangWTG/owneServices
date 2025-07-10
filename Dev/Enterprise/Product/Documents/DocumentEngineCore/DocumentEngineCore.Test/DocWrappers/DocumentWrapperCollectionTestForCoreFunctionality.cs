using CargoWise.EntityFramework.Testing;

namespace Enterprise.DocumentEngineCore.DocWrappers.Testing
{
	sealed class DocumentWrapperCollectionTestForCoreFunctionality : TestCaseWithFactory
	{
		public void TestConstructorAddsWrappedObjects()
		{
			var collection = new TestClasses.TestBOCollection(Factory);
			var zero = new TestClasses.TestBO();
			var one = new TestClasses.TestBO();
			var two = new TestClasses.TestBO();
			collection.Add(zero);
			collection.Add(one);
			collection.Add(two);

			var docCollection = new TestClasses.DocTestBOWrapperCollection(collection);
			AssertEquals("Wrapped bizo zero", zero, docCollection[0].WrappedObject);
			AssertEquals("Wrapped bizo one", one, docCollection[1].WrappedObject);
			AssertEquals("Wrapped bizo two", two, docCollection[2].WrappedObject);
			AssertEquals("Count", 3, docCollection.Count);
		}

		public void TestContainsWrappedObject()
		{
			var collection = new TestClasses.TestBOCollection(Factory);
			var zero = new TestClasses.TestBO();
			var one = new TestClasses.TestBO();
			var two = new TestClasses.TestBO();

			collection.Add(zero);
			collection.Add(one);

			var docCollection = new TestClasses.DocTestBOWrapperCollection(collection);
			AssertEquals("Count", 2, docCollection.Count);

			AssertEquals("Wrapped bizo zero", zero, docCollection[0].WrappedObject);
			Assert("ContainsWrappedObject - Zero", docCollection.ContainsWrappedObject(zero));
			Assert("ContainsWrappedObject by PK - Zero", docCollection.ContainsWrappedObject(zero.PK));

			AssertEquals("Wrapped bizo one", one, docCollection[1].WrappedObject);
			Assert("ContainsWrappedObject - One", docCollection.ContainsWrappedObject(one));
			Assert("ContainsWrappedObject by PK- One", docCollection.ContainsWrappedObject(one.PK));

			Assert("ContainsWrappedObject - Two", !docCollection.ContainsWrappedObject(two));
			Assert("ContainsWrappedObject by PK- Two", !docCollection.ContainsWrappedObject(two.PK));
		}
	}
}
