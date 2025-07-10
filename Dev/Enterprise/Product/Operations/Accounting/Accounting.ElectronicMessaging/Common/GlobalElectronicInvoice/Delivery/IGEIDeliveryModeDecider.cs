using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Accounting.ElectronicMessaging.Common
{
	public interface IGEIDeliveryModeDecider
	{
		IDelivery GetDeliveryMode(IEDICommunicationsMode ediCommunicationMode, bool hasValidationError);
	}
}
