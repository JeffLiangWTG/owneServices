using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	static class AwbEcStatusReleaseSetUnsetHelper
	{
		public static void SetEcStatusRelease(bool isSetting, ICcsukCusAwb awb, EnterpriseBusinessObject bizO)
		{
			if (isSetting)
			{
				awb.SetCustomsActionCode(CustomsStatusCodes.Codes._CargoWise_ERTS_EcStatusRelease, ZDateTime.Now);
				awb.LatestCustomsActionText = "EC Status";
				bizO.Logs.AddNew(Events.CustomsEntryStatus, "EC status release set by " + Environment.Env.CurrentUser.FullName, ZDateTimeOffset.Now);
			}
			else
			{
				awb.SetCustomsActionCode("", ZDateTime.Now);
				awb.LatestCustomsActionText = "";
				bizO.Logs.AddNew(Events.CustomsEntryStatus, "EC status release revoked by " + Environment.Env.CurrentUser.FullName, ZDateTimeOffset.Now);
				foreach (CusOutTurn outTurn in awb.OutTurns)
				{
					if (outTurn.SplitReferenceToWhichThisPertains == awb.SplitReference  // should not be anything but wholes AWBs for this EC function
						&& outTurn.IsReleasedAlready
						&& !outTurn.IsDelivered)
					{
						outTurn.IsReleasedAlready = false;
						outTurn.IsBeingReleasedNow = false; // unrelease
					}
				}
				NumberOfPiecesReleasedHelper.ResetAllReleaseCountsToZeroUponCxStatusUpdate(awb);
				new EDocsDeleter(awb).DeleteOldEdocs();
				if (awb.PresenceOnNetworkStatus == PresenceOnNetworkList.Codes.CompletedOnCcsUk)
				{
					awb.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.OnCommDb;
				}
			}
			awb.RefreshBindingIncludingChildren();
		}
	}
}
