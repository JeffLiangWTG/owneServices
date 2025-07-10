using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.EventMessageProcessor;

namespace Enterprise.Accounting.ElectronicMessaging.D365
{
	class D365GlobalXUEFunctionalityProvider : GlobalXUEFunctionalityProvider
	{
		public D365GlobalXUEFunctionalityProvider(ICountryEInvoicingObjectFactory countryEInvoicingObjectFactory)
			: base(countryEInvoicingObjectFactory)
		{
		}

		protected override EInvoicingEventMessageProcessor GetEventMessageProcessor(EventMessageProcessorData eventMessageProcessorData)
			=> new TransactionBatchEventMessageProcessor(eventMessageProcessorData, CountryEInvoicingObjectFactory);
	}
}
