using System.Linq;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.DocumentVisualizer.Business
{
	public static class DialogExtensions
	{
		public static bool IsWithdrawal(this IDialog dialog)
		{
			return dialog != null
				&& dialog.TransmissionCode == Events.MessageWithdrawCancelRequestCode;
		}

		public static bool HasReceivedResponse(this IDialog dialog)
		{
			return dialog != null && !string.IsNullOrEmpty(dialog.ResponseCode);
		}

		public static bool HasBeenAccepted(this IDialog dialog)
		{
			return dialog != null
				&& MessageEventCodes.AcceptanceEventCodes.Contains(dialog.ResponseCode);
		}

		public static bool HasBeenRejected(this IDialog dialog)
		{
			return dialog != null
				&& MessageEventCodes.RejectionEventCodes.Contains(dialog.ResponseCode);
		}

		public static bool HasBeenResetToOriginal(this IDialog dialog)
		{
			return dialog != null
				&& dialog.TransmissionCode == Events.StatusUpdatedCode;
		}
	}
}