using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.DataTransfer;
using Enterprise.Customs.FR.Business;

namespace Enterprise.Customs.FR.GUI.NCTS
{
	public class FRNctsArrivalMovementMessagingMenuProvider : EU.NCTS.GUI.NctsArrivalMovementMessagingMenuProvider
	{
		public FRNctsArrivalMovementMessagingMenuProvider(EU.NCTS.Business.NctsHeader header, NctsMovementForm nctsMovementForm)
		: base(header)
		{
			ParentForm = nctsMovementForm;
		}

		public FRNctsArrivalMovementMessagingMenuProvider(EU.NCTS.Business.NctsHeader header, NctsHeaderUniversalMessagingHelper ntcsHeaderUniversalMessagingHelper)
			: base(header, ntcsHeaderUniversalMessagingHelper)
		{
		}

		protected override ZBool CanSendArrivalMessage
		{
			get
			{
				var status = Header.ArrivalMovementHeader?.BM_CustomsStatus ?? ZString.Empty;
				return status == NctsTransitStatusList.Codes.Unknown || status == NctsTransitStatusList.Codes.ArrivalRejected;
			}
		}
	}
}
