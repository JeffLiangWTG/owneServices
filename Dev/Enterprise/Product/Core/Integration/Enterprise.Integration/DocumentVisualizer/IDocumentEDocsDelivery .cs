using Enterprise.Integration.DocumentEngine;

namespace Enterprise.Integration.DocumentVisualizer
{
	public interface IDocumentEDocsDelivery
	{
		void SaveCopyToEDocs(IDocument document, string name, string title);
	}
}
