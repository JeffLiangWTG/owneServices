using Enterprise.DocumentVisualizer.Core;

namespace Enterprise.DocumentVisualizer.Integration
{
	public interface IDocumentAwareEvent
	{
		IDocument Document { get; }
	}
}
