using CargoWise.Types;

namespace Enterprise.Customs.CL.Manifest.Business
{
	internal static class BLSendChileHelper
	{
		internal static ZString GetLocationCode(AsycudaBill bill, ZString locationName)
		{
			var port = ZString.Empty;
			var masterBill = bill.Header?.MasterBill;

			switch (locationName)
			{
				case WrappersConstants.LocationName.Le:
				case WrappersConstants.LocationName.Lrm:
					port = bill.ABL_RL_NKOrigin;
					break;
				case WrappersConstants.LocationName.Pe:
					port = masterBill?.ABL_RL_NKPortOfLoading ?? ZString.Empty;
					break;
				case WrappersConstants.LocationName.Pd:
				case WrappersConstants.LocationName.Lem:
					port = masterBill?.ABL_RL_NKPortOfDischarge ?? ZString.Empty;
					break;
				case WrappersConstants.LocationName.Ld:
					port = bill.ABL_RL_NKFinalDestination;
					break;
			}

			return port;
		}

		internal static string ContainerStatusCodeCalculator(ZString status)
		{
			var res = BLSendChileConstants.StatusContainer.Empty;
			switch (status)
			{
				case Core.Constants.ContainerModes.FCL:
					res = BLSendChileConstants.StatusContainer.FclFcl;
					break;
				case Core.Constants.ContainerModes.LCL:
					res = BLSendChileConstants.StatusContainer.LclLcl;
					break;
			}
			return res;
		}
	}
}
