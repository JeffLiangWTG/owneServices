using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class DummyBusinessObjectWrapper : DocumentWrapper
	{
		public DummyBusinessObjectWrapper(DummyDocumentSupportable parent, BusinessObjectFactory factory)
			: base(parent, factory)
		{
			this.collection = new DummyChildBusinessObjectWrapperCollection(parent.Collection, factory);
		}

		readonly DummyChildBusinessObjectWrapperCollection collection;

		public DummyChildBusinessObjectWrapperCollection Collection { get { return collection; } }
	}
}
