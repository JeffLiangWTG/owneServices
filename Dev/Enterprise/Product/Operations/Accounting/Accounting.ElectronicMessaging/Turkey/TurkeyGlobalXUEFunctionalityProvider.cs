using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.EventMessageProcessor;

namespace Enterprise.Accounting.ElectronicMessaging.Turkey
{
	class TurkeyGlobalXUEFunctionalityProvider : GlobalXUEFunctionalityProvider
	{
		public TurkeyGlobalXUEFunctionalityProvider(ICountryEInvoicingObjectFactory countryEInvoicingObjectFactory)
			: base(countryEInvoicingObjectFactory)
		{
		}

		protected override EInvoicingEventMessageProcessor GetEventMessageProcessor(EventMessageProcessorData eventMessageProcessorData)
			=> new EInvoicingEventMessageTRProcessor(eventMessageProcessorData.Logger, eventMessageProcessorData.Message, eventMessageProcessorData.UniversalEvent, eventMessageProcessorData.InvoiceBatch);
	}
}
