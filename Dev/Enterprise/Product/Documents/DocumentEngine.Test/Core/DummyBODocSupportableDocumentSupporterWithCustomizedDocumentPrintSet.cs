using System.Collections.Generic;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineIntegration;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class DummyBODocSupportableDocumentSupporterWithCustomizedDocumentPrintSet : DummyBODocSupportableDocumentSupporter, ISupportCustomizedDocumentPrintSet
	{
		public DummyBODocSupportableDocumentSupporterWithCustomizedDocumentPrintSet(DummyBODocSupportable docDummyBusinessObject)
			: base(docDummyBusinessObject)
		{
		}

		public IPrintTask GetCustomizedDocumentPrintSet(Enterprise.Integration.DocumentEngine.IDocumentCommand command)
		{
			documentPrintSetWithStreamingWasCalled = true;
			return new DocumentPrintSetWithStreaming((DocumentCommand)command, 50, GetDocumentPacks());
		}

		public bool ShouldCustomizedDocumentPrintSet(Enterprise.Integration.DocumentEngine.IDocumentCommand command)
		{
			return true;
		}

		IEnumerable<DocumentPack> GetDocumentPacks()
		{
			for (int i = 0; i < 10; i++)
			{
				yield return new DocumentPack();
			}
		}

		public bool documentPrintSetWithStreamingWasCalled;
	}
}
