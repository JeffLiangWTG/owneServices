using Enterprise.DocumentVisualizer.Core;

namespace Enterprise.DocumentVisualizer.Integration
{
	public interface IPrintEventsProcessorProvider
	{
		IPrintEventsProcessor GetPrintEventsProcessor(IDocument document);
	}
}
