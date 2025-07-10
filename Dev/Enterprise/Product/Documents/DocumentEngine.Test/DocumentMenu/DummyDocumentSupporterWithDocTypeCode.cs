using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.DocumentEngine.DocumentMenu.Testing
{
	sealed class DummyDocumentSupporterWithDocTypeCode : DummyBODocSupportableDocumentSupporter
	{
		public DummyDocumentSupporterWithDocTypeCode(DummyBODocSupportable docDummyBusinessObject)
			: base(docDummyBusinessObject)
		{
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			var dummyDocumentWrapper = new DummyDocumentWrapperWithDocTypeCode(Parent, Parent.Factory);
			dummyDocumentWrapper.JobNumber = Parent.JobNumber;
			dummyDocumentWrapper.Int32Number = Parent.Z0_Number;

			return new DocumentWrapper[] { dummyDocumentWrapper };
		}
	}
}
