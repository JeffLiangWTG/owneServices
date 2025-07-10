using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.EventMessageProcessor;

namespace Enterprise.Accounting.ElectronicMessaging.Israel
{
	internal class EInvoicingEventMessageILProcessor : GlobalEInvoicingEventMessageProcessor
	{
		public EInvoicingEventMessageILProcessor(EventMessageProcessorData eventMessageProcessorData, ICountryEInvoicingObjectFactory countryEInvoicingObjectFactory)
			: base(eventMessageProcessorData, countryEInvoicingObjectFactory)
		{
		}
	}
}
