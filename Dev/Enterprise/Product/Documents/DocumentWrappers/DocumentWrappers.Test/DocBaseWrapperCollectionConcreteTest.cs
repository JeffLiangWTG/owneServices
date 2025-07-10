using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(TestWrapperCollection))]
	sealed class DocBaseWrapperCollectionConcreteTest : DocBaseWrapperCollectionTest<TestWrapperCollection>
	{
		protected override TestWrapperCollection GetNewDocumentWrapperCollection()
		{
			return new TestWrapperCollection(Factory);
		}

		protected override DocumentEngineCore.DocWrappers.DocumentWrapper AddNewDocumentWrapperToCollection(DocumentEngineCore.DocWrappers.DocumentWrapperCollection collection)
		{
			TestWrapper wrapper = new TestWrapper(Factory);
			collection.Add(wrapper);
			return wrapper;
		}

		protected override object GetNewObjectToWrap()
		{
			return null;
		}
	}
}
