using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class NctsHeaderGenerator : EU.NCTS.Business.NctsHeaderGenerator
	{
		protected override string NewMessageStatus => NctsMessageStatusList.Codes.ArrivalNotificationNotSent;

		protected override bool FillUnloadingDate => false;
		protected override bool FillDischargeType => false;
	}
}
