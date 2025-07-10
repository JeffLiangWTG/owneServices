using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Accounting.ElectronicMessaging.China
{
	public class EInvoicingEventMessageCNProcessorForEvent : EInvoicingEventMessageProcessor
	{
		public EInvoicingEventMessageCNProcessorForEvent(IXmlSessionTracker logger, IEDIMessage message, UniversalEvent universalEvent, InvoicingBase invoice) : base(logger, message, universalEvent, invoice)
		{
		}

		public override void Process()
		{
			if (universalEvent.EventType.HasValue && universalEvent.EventType.Value == AutoEvents.InterchangeAcknowledgedCode)
			{
				ChinaEInvoiceHelper.UpdateDDIReferenceAndFileName(invoice, universalEvent.AttachedDocumentCollection);
			}
		}

		protected override bool ShouldSendEmail => false;

		protected override GEIEmailNotificationCreator GetEmailNotificationCreator(ZString errorMessage, ILogger logCollector) => null;

		protected override string LogErrorNotFoundKey => "TransactionNotFound";
	}
}
