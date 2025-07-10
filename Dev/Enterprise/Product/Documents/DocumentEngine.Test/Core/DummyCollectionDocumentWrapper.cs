using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentEngine.Testing
{
	public class DummyCollectionDocumentWrapper : DocumentWrapperCollection<DummyDocumentWrapper>
	{
		public DummyCollectionDocumentWrapper(BusinessObjectFactory factory)
			: base(factory)
		{ }
	}
}
