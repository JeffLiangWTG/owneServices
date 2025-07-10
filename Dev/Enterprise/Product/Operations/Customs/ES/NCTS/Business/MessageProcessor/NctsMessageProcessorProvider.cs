using System.Collections.Generic;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public sealed class NctsMessageProcessorsProvider : IESNctsMessageProcessorsProvider
	{
		public IEnumerable<BranchCustomsApplicationTypeMessageProcessor> GetProcessors(LoggingInformation logger)
		{
			return new List<BranchCustomsApplicationTypeMessageProcessor>
			{
				new NctsDepartureDeclarationResponseMessageProcessor(logger),
				new NctsTIRResponseMessageProcessor(logger),
				new NctsArrivalDeclarationResponseMessageProcessor(logger),
				new CancelNCTSResponseMessageProcessor(logger),
				new NotifGoodsNCTSResponseMessageProcessor(logger),
				new DepartureNCTSResponseMessageProcessor(logger),
				new NctsClearanceEmailResponseMessageProcessor(logger),
				new ArrivalNCTSResponseMessageProcessor(logger),
				new AmendmentNCTSResponseMessageProcessor(logger),
				new InboxNotificationNctsDepartureClearanceResponseMessageProcessor(logger),
				new InboxNotificationNctsInvalidationTransitMessageProcessor(logger),
				new QueryNCTSResponseMessageProcessor(logger),
				new InboxNotificationNctsControlCommunicationMessageProcessor(logger),
				new AnnexNCTSResponseMessageProcessor(logger),
				new InboxNotifNonConformityResponseMessageProcessor(logger),
				new TNNNCTSResponseMessageProcessor(logger),
				new NotificationUnloadingNCTSResponseMessageProcessor(logger),
			};
		}
	}
}
