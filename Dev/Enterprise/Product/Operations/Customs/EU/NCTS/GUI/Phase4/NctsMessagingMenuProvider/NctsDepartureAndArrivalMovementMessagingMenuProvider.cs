using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.DataTransfer;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public class NctsDepartureAndArrivalMovementMessagingMenuProvider : NctsArrivalMovementMessagingMenuProvider
	{
		public NctsDepartureAndArrivalMovementMessagingMenuProvider(NctsHeader header)
			: base(header)
		{
		}

		public NctsDepartureAndArrivalMovementMessagingMenuProvider(NctsHeader header, NctsHeaderUniversalMessagingHelper ntcsHeaderUniversalMessagingHelper)
			: base(header, ntcsHeaderUniversalMessagingHelper)
		{
		}
	}
}
