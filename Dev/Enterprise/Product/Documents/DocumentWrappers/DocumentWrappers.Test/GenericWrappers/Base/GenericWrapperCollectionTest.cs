using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Base.Testing
{
	[TestsSubclassesOf(typeof(GenericWrapperCollection))]
	public abstract class GenericWrapperCollectionTest<T> : DocBaseWrapperCollectionTest<T> where T : GenericWrapperCollection
	{
		protected abstract GenericWrapper GetNewWrapperToAddToTheCollection();

		protected sealed override DocumentWrapper AddNewDocumentWrapperToCollection(DocumentWrapperCollection collection)
		{
			GenericWrapper wrapper = GetNewWrapperToAddToTheCollection();
			collection.Add(wrapper);
			return wrapper;
		}

		protected sealed override object GetNewObjectToWrap()
		{
			return null;
		}
	}
}
