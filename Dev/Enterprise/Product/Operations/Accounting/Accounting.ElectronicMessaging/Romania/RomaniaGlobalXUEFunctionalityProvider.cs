using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.EventMessageProcessor;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Accounting.ElectronicMessaging.Romania
{
	class RomaniaGlobalXUEFunctionalityProvider : GlobalXUEFunctionalityProvider
	{
		public RomaniaGlobalXUEFunctionalityProvider(ICountryEInvoicingObjectFactory countryEInvoicingObjectFactory)
			: base(countryEInvoicingObjectFactory)
		{
		}

		protected override bool IsBatchEventMessageProcessSupported(string eventType, string messageSubType)
		{
			return IsMessageSubTypeSupported() && IsEventTypeSupported();

			bool IsMessageSubTypeSupported()
			{
				return messageSubType == RomaniaEInvoiceAPICommandList.Codes.GenerateInvoiceRequest
					|| messageSubType == RomaniaEInvoiceAPICommandList.Codes.QueryInvoiceRequest;
			}

			bool IsEventTypeSupported()
			{
				return eventType == AutoEvents.InterchangeAcknowledgedCode
					|| eventType == AutoEvents.InterchangeRejectedCode;
			}
		}

		protected override EInvoicingEventMessageProcessor GetEventMessageProcessor(EventMessageProcessorData eventMessageProcessorData)
			=> new RomaniaEInvoicingEventMessageProcessor(eventMessageProcessorData, CountryEInvoicingObjectFactory);
	}
}
