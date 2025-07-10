using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.EventMessageProcessor;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Accounting.ElectronicMessaging.KoreaSouth
{
	class KoreaSouthGlobalXUEFunctionalityProvider : GlobalXUEFunctionalityProvider
	{
		public KoreaSouthGlobalXUEFunctionalityProvider(ICountryEInvoicingObjectFactory countryEInvoicingObjectFactory)
			: base(countryEInvoicingObjectFactory)
		{
		}

		protected override EInvoicingEventMessageProcessor GetEventMessageProcessor(EventMessageProcessorData eventMessageProcessorData)
			=> new KoreaSouthEInvoicingEventMessageProcessor(eventMessageProcessorData.Logger, eventMessageProcessorData.Message, eventMessageProcessorData.UniversalEvent, eventMessageProcessorData.InvoiceBatch);

		protected override bool IsBatchEventMessageProcessSupported(string eventType, string messageSubType)
		{
			return IsMessageSubTypeSupported() && IsEventTypeSupported();

			bool IsMessageSubTypeSupported()
			{
				return messageSubType == KoreaSouthEInvoiceAPICommandList.Codes.GenerateInvoiceRequest
					|| messageSubType == KoreaSouthEInvoiceAPICommandList.Codes.QueryInvoiceRequest
					|| messageSubType == KoreaSouthEInvoiceAPICommandList.Codes.CallbackInvoceRequest;
			}

			bool IsEventTypeSupported()
			{
				return eventType == AutoEvents.InterchangeAcknowledgedCode
					|| eventType == AutoEvents.InterchangeRejectedCode;
			}
		}
	}
}
