using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.DocumentVisualizer.Delivery
{
	public interface IDocumentDelivery
	{
		string DocumentType { get; }
		string DeliveryMode { get; }
		IDocument Document { get; }
		IStmALogParent LogParent { get; }
		IPrintInstructions PrintInstructions { get; }
		IEDocsInstructions EDocsInstructions { get; }
	}
}
