using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Messaging.Integration;

namespace Enterprise.Accounting.ElectronicMessaging.Common
{
	public interface IGlobalElectronicInvoicingDelivery
	{
		IXmlEDIInterchange[] Deliver(GlobalElectronicInvoicing eInvoice, bool hasValidationError = false);
	}
}
