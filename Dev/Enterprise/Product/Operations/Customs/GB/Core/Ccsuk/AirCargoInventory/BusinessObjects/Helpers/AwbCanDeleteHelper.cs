using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	static class AwbCanDeleteHelper
	{
		internal static bool ProcessCanDeleteRequestAndSetReason(ICcsukCusAwb awb, ref MultilingualString reasonForNotAbleToDelete)
		{
			reasonForNotAbleToDelete = (NoResString)string.Empty;
			var canDelete = true;

			if (((BusinessObject)awb).IsInDatabase)
			{
				var mawb = awb as CusMAWB;

				if (awb.HasSplits)
				{
					canDelete = false;
					reasonForNotAbleToDelete = ResString.GetMultilingualString("7D41C43F-12E6-4F54-B81C-3DC323562EEF", "The AWB has splits. Delete (or request to delete) those first.");
				}
				else if (!awb.ReadOnlyAndPermissionHelper.IsActionAllowed(Actions.Delete))
				{
					canDelete = false;
					reasonForNotAbleToDelete = ResString.GetMultilingualString("4DBCD4CA-9B39-4074-B35B-8693595A599F", "The AWB has a Customs Action Code that means it is locked. It cannot be deleted.");
				}
				else if (awb.IsLodgedAtCcsuk)
				{
					canDelete = false;
					reasonForNotAbleToDelete = ResString.GetMultilingualString("82C569A2-7C70-480E-ACA6-2EA378BE6820", "The AWB is listed as being registered on the national network.  First open the AWB and send the FRX message.");
				}
				else if (awb.PresenceOnNetworkStatus == PresenceOnNetworkList.Codes.AssumedOnCommDbSentWithoutRejection
						|| awb.PresenceOnNetworkStatus == PresenceOnNetworkList.Codes.NoInformationSendAnFsrWithUpdateToCheck
						|| awb.PresenceOnNetworkStatus == PresenceOnNetworkList.Codes.SendPendingCheckYourCukServiceTask
						)
				{
					if (awb.Messages.Count > 0)
					{
						canDelete = false;
						reasonForNotAbleToDelete = ResString.GetMultilingualString("D1DF327C-E6AE-429A-A91D-ABAF10895068", "The presence of this AWB on the national network is not known for certain. Send an FSR to confirm whether it can be deleted locally.");
					}
				}
				else if (mawb != null)
				{
					foreach (var hawb in mawb.ChildBills)
					{
						if (!hawb.CanDelete)
						{
							canDelete = false;
							reasonForNotAbleToDelete = MultilingualString.Join(System.Environment.NewLine, ResString.GetMultilingualString("F022241B-658D-438F-AF44-29F51AA4575E", "The MAWB has at least one HAWB which cannot be deleted.\r\nAt least one HAWB cannot be deleted for the following reason:"), hawb.ReasonForNotAbleToDelete);
							break;
						}
					}
				}
			}
			return canDelete;
		}
	}
}
