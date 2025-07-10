using Enterprise.DocumentEngine.DeliveryMethods;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine
{
	public interface ICreateDeliveryInfoStrategy
	{
		DeliveryInfo CreateDeliveryInfo(IDeliverable deliverable, DocDeliveryContact deliveryContact, DeliveryInstructions deliveryInstructions);
	}
}
