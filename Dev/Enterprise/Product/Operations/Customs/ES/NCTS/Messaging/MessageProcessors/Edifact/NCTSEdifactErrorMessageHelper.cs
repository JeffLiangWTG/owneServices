using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageProcessors;

namespace Enterprise.Customs.ES.NCTS.Messaging.MessageProcessors
{
	public class NCTSEdifactErrorMessageHelper : EdifactErrorMessageHelper, INctsDepartureAndTIRResponseMessageProvider, INctsArrivalResponseMessageProvider
	{
		public NCTSEdifactErrorMessageHelper(BusinessObjectFactory factory, ZString error, ZString description) : base(factory, error, description)
		{
		}

		ZString INctsDepartureAndTIRResponseMessageProvider.CustomsClearanceCriteria => ZString.Empty;

		ZString INctsArrivalResponseMessageProvider.PreviousSummaryDiscrepancy => ZString.Empty;

		ZString INctsArrivalResponseMessageProvider.TransitReferenceNumber => ZString.Empty;

		ZString INctsArrivalResponseMessageProvider.SummaryReferenceNumber => ZString.Empty;
	}
}
