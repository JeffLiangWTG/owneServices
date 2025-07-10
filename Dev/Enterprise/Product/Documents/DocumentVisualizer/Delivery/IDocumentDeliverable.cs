using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.DeliveryMethods;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentVisualizer.Delivery
{
	public interface IDocumentDeliverable : IDeliverable
	{
		IDocManagerSupport EDocsParent { get; }
		DeliveryInfo GetDeliveryInfo(bool isDraft, FileType fileType);
	}
}
