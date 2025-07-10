using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Delivery;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class DummyDocumentDelivery : IDocumentDelivery
	{
		public string DocumentType { get; set; }
		public string DeliveryMode { get; set; }
		public IDocument Document { get; set; }
		public IStmALogParent LogParent { get; set; }
		public IPrintInstructions PrintInstructions { get; set; }
		public IEDocsInstructions EDocsInstructions { get; set; }
	}
}
