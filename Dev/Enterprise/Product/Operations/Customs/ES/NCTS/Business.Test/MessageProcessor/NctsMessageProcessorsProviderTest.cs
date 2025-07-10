using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	sealed class NctsMessageProcessorsProviderTest : TestCaseWithFactory
	{
		public void TestDefaultMessageProcessors()
		{
			var logger = new LoggingInformation();
			var result = new NctsMessageProcessorsProvider().GetProcessors(logger)
				.Select(processor => processor.GetType());

			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					typeof(NctsDepartureDeclarationResponseMessageProcessor),
					typeof(NctsTIRResponseMessageProcessor),
					typeof(NctsArrivalDeclarationResponseMessageProcessor),
					typeof(CancelNCTSResponseMessageProcessor),
					typeof(NotifGoodsNCTSResponseMessageProcessor),
					typeof(DepartureNCTSResponseMessageProcessor),
					typeof(NctsClearanceEmailResponseMessageProcessor),
					typeof(ArrivalNCTSResponseMessageProcessor),
					typeof(AmendmentNCTSResponseMessageProcessor),
					typeof(InboxNotificationNctsDepartureClearanceResponseMessageProcessor),
					typeof(QueryNCTSResponseMessageProcessor),
					typeof(InboxNotificationNctsControlCommunicationMessageProcessor),
					typeof(InboxNotificationNctsInvalidationTransitMessageProcessor),
					typeof(AnnexNCTSResponseMessageProcessor),
					typeof(InboxNotifNonConformityResponseMessageProcessor),
					typeof(TNNNCTSResponseMessageProcessor),
					typeof(NotificationUnloadingNCTSResponseMessageProcessor),
				},
				result);
		}
	}
}
