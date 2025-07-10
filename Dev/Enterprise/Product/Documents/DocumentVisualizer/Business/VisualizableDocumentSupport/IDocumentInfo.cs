using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;

namespace Enterprise.DocumentVisualizer.Business
{
	public interface IDocumentInfo
	{
		IDocument Document { get; }
		IDocumentDescriptor Descriptor { get; }
		IServiceContainer Services { get; }
		IVisualizerDocumentData DocumentData { get; }
		ITemplate Template { get; }
	}
}