using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.DocumentEngine.DocumentMenu.Testing
{
	sealed class DummyDocumentSupporterWithInvalidWrapper : DummyBODocSupportableDocumentSupporter
	{
		public DummyDocumentSupporterWithInvalidWrapper(DummyBODocSupportable docDummyBusinessObject)
			: base(docDummyBusinessObject)
		{
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return new DocumentWrapper[] { null };
		}
	}
}
