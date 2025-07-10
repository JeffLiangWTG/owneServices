using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.EventMessageProcessor
{
	public class EventMessageProcessorData
	{
		public EventMessageProcessorData(ZString eventType, ZString messageSubType, IXmlSessionTracker logger, IEDIMessage message, UniversalEvent universalEvent, AccEInvoicingBatch invoiceBatch)
		{
			EventType = eventType;
			MessageSubType = messageSubType;
			Logger = logger;
			Message = message;
			UniversalEvent = universalEvent;
			InvoiceBatch = invoiceBatch;
		}

		public ZString EventType { get; }
		public ZString MessageSubType { get; }
		public IXmlSessionTracker Logger { get; }
		public IEDIMessage Message { get; }
		public UniversalEvent UniversalEvent { get; }
		public AccEInvoicingBatch InvoiceBatch { get; }
	}
}
