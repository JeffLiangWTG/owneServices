using CargoWise.Common;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.EventMessageProcessor;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing
{
	public interface IGlobalXUEFunctionalityProvider
	{
		bool IsInvoiceEventMessageProcessSupported(string messageSubType);
		bool IsBatchEventMessageProcessSupported(string eventType, string messageSubType);
		EInvoicingEventMessageProcessor GetEventMessageProcessor(EventMessageProcessorData eventMessageProcessorData);

		bool IsEventMessageChildrenProcessSupported(string eventType);
		EInvoicingEventMessageProcessor GetEventMessageChildrenProcessor(EventMessageChildrenProcessorData eventMessageChildrenProcessorData);

		void AfterEventMessageProcessed(UniversalEvent universalEvent, AccEInvoicingBatch invoiceBatch, IXmlSessionTracker logger);
	}

	class GlobalXUEFunctionalityProvider : IGlobalXUEFunctionalityProvider
	{
		public ICountryEInvoicingObjectFactory CountryEInvoicingObjectFactory { get; }

		public GlobalXUEFunctionalityProvider(ICountryEInvoicingObjectFactory countryEInvoicingObjectFactory)
		{
			CountryEInvoicingObjectFactory = Argument.NotNull(countryEInvoicingObjectFactory, nameof(countryEInvoicingObjectFactory));
		}

		bool IGlobalXUEFunctionalityProvider.IsInvoiceEventMessageProcessSupported(string messageSubType)
			=> IsInvoiceEventMessageProcessSupported(messageSubType);

		bool IGlobalXUEFunctionalityProvider.IsBatchEventMessageProcessSupported(string eventType, string messageSubType)
			=> IsBatchEventMessageProcessSupported(eventType, messageSubType);
		EInvoicingEventMessageProcessor IGlobalXUEFunctionalityProvider.GetEventMessageProcessor(EventMessageProcessorData eventMessageProcessorData)
			=> GetEventMessageProcessor(eventMessageProcessorData);

		bool IGlobalXUEFunctionalityProvider.IsEventMessageChildrenProcessSupported(string eventType) => IsEventMessageChildrenProcessSupported(eventType);
		EInvoicingEventMessageProcessor IGlobalXUEFunctionalityProvider.GetEventMessageChildrenProcessor(EventMessageChildrenProcessorData eventMessageChildrenProcessorData)
			=> GetEventMessageChildrenProcessor(eventMessageChildrenProcessorData);

		void IGlobalXUEFunctionalityProvider.AfterEventMessageProcessed(UniversalEvent universalEvent, AccEInvoicingBatch invoiceBatch, IXmlSessionTracker logger)
			=> AfterEventMessageProcessed(universalEvent, invoiceBatch, logger);

		#region Overrides

		protected virtual bool IsInvoiceEventMessageProcessSupported(string messageSubType) => false;

		protected virtual bool IsBatchEventMessageProcessSupported(string eventType, string messageSubType)
			=> eventType == AutoEvents.InterchangeAcknowledgedCode || eventType == AutoEvents.InterchangeRejectedCode;

		protected virtual EInvoicingEventMessageProcessor GetEventMessageProcessor(EventMessageProcessorData eventMessageProcessorData)
			=> new GlobalEInvoicingEventMessageProcessor(eventMessageProcessorData, CountryEInvoicingObjectFactory);

		protected virtual bool IsEventMessageChildrenProcessSupported(string eventType) => false;
		protected virtual EInvoicingEventMessageProcessor GetEventMessageChildrenProcessor(EventMessageChildrenProcessorData eventMessageChildrenProcessorData) => null;

		protected virtual void AfterEventMessageProcessed(UniversalEvent universalEvent, AccEInvoicingBatch invoiceBatch, IXmlSessionTracker logger) { }

		#endregion
	}
}
