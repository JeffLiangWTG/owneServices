using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageProcessors;

namespace Enterprise.Customs.ES.NCTS.Messaging.MessageProcessors
{
	public interface INctsArrivalResponseMessageProvider : ICUSRESMessageProvider
	{
		ZString PreviousSummaryDiscrepancy { get; }
		ZString TransitReferenceNumber { get; }
		ZString SummaryReferenceNumber { get; }
	}
}
