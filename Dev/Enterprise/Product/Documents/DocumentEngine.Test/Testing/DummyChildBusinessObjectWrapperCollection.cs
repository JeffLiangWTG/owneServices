using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class DummyChildBusinessObjectWrapperCollection : DocumentWrapperCollection<DummyChildBusinessObjectWrapper>
	{
		public DummyChildBusinessObjectWrapperCollection(ChildDummyBusinessObjectCollection collection, BusinessObjectFactory factory)
			: base(factory)
		{
			foreach (ChildDummyBusinessObject child in collection)
			{
				Add(new DummyChildBusinessObjectWrapper(child, factory));
			}
		}
	}
}
