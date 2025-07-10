using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.EventMessageProcessor;

namespace Enterprise.Accounting.ElectronicMessaging.Israel
{
	class IsraelGlobalXUEFunctionalityProvider : GlobalXUEFunctionalityProvider
	{
		public IsraelGlobalXUEFunctionalityProvider(ICountryEInvoicingObjectFactory countryEInvoicingObjectFactory)
			: base(countryEInvoicingObjectFactory)
		{
		}

		protected override bool IsBatchEventMessageProcessSupported(string eventType, string messageSubType)
			=> messageSubType == IsraelEInvoiceAPICommandList.Codes.GenerateInvoiceRequest
				&& base.IsBatchEventMessageProcessSupported(eventType, messageSubType);

		protected override EInvoicingEventMessageProcessor GetEventMessageProcessor(EventMessageProcessorData eventMessageProcessorData)
			=> new EInvoicingEventMessageILProcessor(eventMessageProcessorData, CountryEInvoicingObjectFactory);
	}
}
