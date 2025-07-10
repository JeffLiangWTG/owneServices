using Enterprise.Customs.NZ.Business.Documents.MAFCoverSheet;
using Enterprise.DocumentWrappers.Testing;

namespace Enterprise.DocumentWrappers.Customs.NZ.MAFCoverSheet.Testing
{
	sealed class DocMAFCoverSheetContainerTest : DocBaseWrapperTest
	{
		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new DocMAFCoverSheetContainer(new NZDocsMAFCSContainer(Factory), Factory, 0);
		}
	}
}
