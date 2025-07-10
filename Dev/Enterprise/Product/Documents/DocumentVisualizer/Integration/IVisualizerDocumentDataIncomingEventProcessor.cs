using Enterprise.Integration;

namespace Enterprise.DocumentVisualizer.Integration
{
	public interface IVisualizerDocumentDataIncomingEventProcessor
	{
		void Process(IVisualizerDocumentData visualizerDocumentData, IStmALog log);
	}
}