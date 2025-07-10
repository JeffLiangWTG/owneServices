using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using WTG.StaticAnalysis.Annotation;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Accounting.ElectronicMessaging.Argentina
{
	[CodeAlive("Will be used in future eInvoice for Argentina.")]
	public class EInvoicingEventMessageARProcessor : EInvoicingEventMessageProcessor
	{
		protected override string LogErrorNotFoundKey => "TransactionNotFound"; // Error Message Key for Developers Only

		internal new static class ContextTypeCode
		{
			public const string EHubAllocatedNumber = "eHubAllocatedNumber";
			public const string GovernmentAllocatedNumber = "GovernmentAllocatedNumber";
			public const string ResponseMessage = "ResponseMessage";
		}

		public EInvoicingEventMessageARProcessor(IXmlSessionTracker logger, IEDIMessage message, UniversalEvent universalEvent, AccEInvoicingBatch invoiceBatch)
		: base(logger, message, universalEvent, invoiceBatch)
		{
		}

		public override void Process()
		{
		}

		#region Helpers

		protected override bool ShouldSendEmail => false;

		protected override GEIEmailNotificationCreator GetEmailNotificationCreator(ZString errorMessage, ILogger logCollector)
		{
			return null;
		}

		#endregion
	}
}