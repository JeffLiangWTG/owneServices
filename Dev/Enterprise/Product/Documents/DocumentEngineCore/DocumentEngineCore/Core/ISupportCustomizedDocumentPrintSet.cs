using Enterprise.DocumentEngineIntegration;
using Enterprise.Integration.DocumentEngine;

namespace Enterprise.DocumentEngineCore
{
	public interface ISupportCustomizedDocumentPrintSet
	{
		IPrintTask GetCustomizedDocumentPrintSet(IDocumentCommand command);
		bool ShouldCustomizedDocumentPrintSet(IDocumentCommand command);
	}
}
