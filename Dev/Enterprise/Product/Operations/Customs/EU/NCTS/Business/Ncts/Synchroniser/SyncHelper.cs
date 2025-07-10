using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	static class SyncHelper
	{
		public static ZBool ShouldSyncTransportDetails(ZString sourceTransportMode, bool takeTransportModeInAccount) => (takeTransportModeInAccount && sourceTransportMode == TransportTypeList.Codes.Road) || Registry.EUCustomsDataRegistry.Instance.SyncTransportDetailsFromForwardingToNCTS.Value;

		public static ZString GetCalculatedTransportMode(ZString sourceTransportMode)
		{
			return !ShouldSyncTransportDetails(sourceTransportMode, true) ? (ZString)TransportTypeList.Codes.Road : sourceTransportMode;
		}
	}
}
