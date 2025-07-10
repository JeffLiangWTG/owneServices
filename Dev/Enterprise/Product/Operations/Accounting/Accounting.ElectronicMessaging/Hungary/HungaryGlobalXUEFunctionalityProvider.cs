using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.EventMessageProcessor;

namespace Enterprise.Accounting.ElectronicMessaging.Hungary
{
	class HungaryGlobalXUEFunctionalityProvider : GlobalXUEFunctionalityProvider
	{
		public HungaryGlobalXUEFunctionalityProvider(ICountryEInvoicingObjectFactory countryEInvoicingObjectFactory)
			: base(countryEInvoicingObjectFactory)
		{
		}

		protected override EInvoicingEventMessageProcessor GetEventMessageProcessor(EventMessageProcessorData eventMessageProcessorData)
			=> new EInvoicingEventMessageHUProcessor(eventMessageProcessorData.Logger, eventMessageProcessorData.Message, eventMessageProcessorData.UniversalEvent, eventMessageProcessorData.InvoiceBatch);
	}
}
