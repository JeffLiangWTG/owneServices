using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.EventMessageProcessor;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Accounting.ElectronicMessaging.Common.Universal.AccountingInvoiceDataContextManager;

namespace Enterprise.Accounting.ElectronicMessaging.China
{
	class ChinaGlobalXUEFunctionalityProvider : GlobalXUEFunctionalityProvider
	{
		public ChinaGlobalXUEFunctionalityProvider(ICountryEInvoicingObjectFactory countryEInvoicingObjectFactory)
			: base(countryEInvoicingObjectFactory)
		{
		}

		protected override bool IsBatchEventMessageProcessSupported(string eventType, string messageSubType) => true;

		protected override EInvoicingEventMessageProcessor GetEventMessageProcessor(EventMessageProcessorData eventMessageProcessorData)
			=> new EInvoicingEventMessageCNProcessor(eventMessageProcessorData, base.CountryEInvoicingObjectFactory);

		protected override bool IsEventMessageChildrenProcessSupported(string eventType) => eventType == AutoEvents.InterchangeAcknowledgedCode;

		protected override EInvoicingEventMessageProcessor GetEventMessageChildrenProcessor(EventMessageChildrenProcessorData eventMessageChildrenProcessorData)
			=> new EInvoicingEventMessageCNProcessorForEvent(eventMessageChildrenProcessorData.Logger, eventMessageChildrenProcessorData.Message, eventMessageChildrenProcessorData.UniversalEvent, eventMessageChildrenProcessorData.InvoicingBase);

		protected override bool IsInvoiceEventMessageProcessSupported(string messageSubType) => messageSubType == EventDataConstants.ElectronicInvoicingEventSubType_SIU;
	}
}
