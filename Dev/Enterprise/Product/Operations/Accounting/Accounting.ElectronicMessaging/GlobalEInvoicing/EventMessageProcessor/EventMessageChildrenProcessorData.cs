using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.EventMessageProcessor
{
	public class EventMessageChildrenProcessorData
	{
		public EventMessageChildrenProcessorData(IXmlSessionTracker logger, IEDIMessage message, UniversalEvent universalEvent, InvoicingBase invoicingBase)
		{
			Logger = logger;
			Message = message;
			UniversalEvent = universalEvent;
			InvoicingBase = invoicingBase;
		}

		public IXmlSessionTracker Logger { get; }
		public IEDIMessage Message { get; }
		public UniversalEvent UniversalEvent { get; }
		public InvoicingBase InvoicingBase { get; }
	}
}
