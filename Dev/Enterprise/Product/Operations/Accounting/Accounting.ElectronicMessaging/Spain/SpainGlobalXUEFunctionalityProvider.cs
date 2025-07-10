using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.EventMessageProcessor;

namespace Enterprise.Accounting.ElectronicMessaging.Spain
{
	class SpainGlobalXUEFunctionalityProvider : GlobalXUEFunctionalityProvider
	{
		public SpainGlobalXUEFunctionalityProvider(ICountryEInvoicingObjectFactory countryEInvoicingObjectFactory)
			: base(countryEInvoicingObjectFactory)
		{
		}

		protected override EInvoicingEventMessageProcessor GetEventMessageProcessor(EventMessageProcessorData eventMessageProcessorData)
			=> new TransactionBatchEventMessageProcessor(eventMessageProcessorData, CountryEInvoicingObjectFactory);
	}
}
