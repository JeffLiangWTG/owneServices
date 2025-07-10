using System.Linq;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.EventMessageProcessor;

namespace Enterprise.Accounting.ElectronicMessaging.India
{
	class IndiaGlobalXUEFunctionalityProvider : GlobalXUEFunctionalityProvider
	{
		public IndiaGlobalXUEFunctionalityProvider(ICountryEInvoicingObjectFactory countryEInvoicingObjectFactory)
			: base(countryEInvoicingObjectFactory)
		{
		}

		protected override EInvoicingEventMessageProcessor GetEventMessageProcessor(EventMessageProcessorData eventMessageProcessorData)
			=> IsNewStyleXUE(eventMessageProcessorData)
				? new GlobalEInvoicingEventMessageProcessor(eventMessageProcessorData, base.CountryEInvoicingObjectFactory)
				: new EInvoicingEventMessageINProcessor(eventMessageProcessorData.Logger, eventMessageProcessorData.Message, eventMessageProcessorData.UniversalEvent, eventMessageProcessorData.InvoiceBatch);

		bool IsNewStyleXUE(EventMessageProcessorData eventMessageProcessorData)
			=> eventMessageProcessorData.UniversalEvent.ContextCollection.Any(x => (x.Type?.Type ?? "") == EventContextTypeCode.AHF_DateTime);
	}
}
