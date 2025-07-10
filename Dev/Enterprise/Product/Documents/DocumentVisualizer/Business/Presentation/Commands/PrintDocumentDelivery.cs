using CargoWise.Common;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Delivery;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.DocumentVisualizer.Presentation
{
	public sealed class PrintDocumentDelivery : IDocumentDelivery
	{
		public PrintDocumentDelivery(IDocumentInfo info, string deliveryMode)
		{
			Argument.NotNull(info, nameof(info));

			DocumentType = info.Descriptor.DocumentType;
			DeliveryMode = deliveryMode;
			Document = info.Document;
			LogParent = info.DocumentData as IStmALogParent;
			PrintInstructions = info.Descriptor.PrintInstructions;
			EDocsInstructions = info.Descriptor.EDocsInstructions;
		}

		public string DocumentType { get; }
		public string DeliveryMode { get; }
		public IDocument Document { get; }
		public IStmALogParent LogParent { get; }
		public IPrintInstructions PrintInstructions { get; }
		public IEDocsInstructions EDocsInstructions { get; }
	}
}
