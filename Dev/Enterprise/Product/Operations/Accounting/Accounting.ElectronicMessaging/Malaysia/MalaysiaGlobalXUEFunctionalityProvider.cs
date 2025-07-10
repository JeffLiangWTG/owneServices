using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.EventMessageProcessor;

namespace Enterprise.Accounting.ElectronicMessaging.Malaysia
{
	class MalaysiaGlobalXUEFunctionalityProvider : GlobalXUEFunctionalityProvider
	{
		public MalaysiaGlobalXUEFunctionalityProvider(ICountryEInvoicingObjectFactory countryEInvoicingObjectFactory)
			: base(countryEInvoicingObjectFactory)
		{
		}

		protected override bool IsBatchEventMessageProcessSupported(string eventType, string messageSubType) => true;

		protected override EInvoicingEventMessageProcessor GetEventMessageProcessor(EventMessageProcessorData eventMessageProcessorData)
			=> new EInvoicingEventMessageMYProcessor(eventMessageProcessorData, base.CountryEInvoicingObjectFactory);
	}
}
