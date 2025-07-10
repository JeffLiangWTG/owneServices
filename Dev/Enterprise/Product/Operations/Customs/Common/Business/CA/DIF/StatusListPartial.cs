
namespace Enterprise.Customs.Common.CA.DIF
{
	partial class StatusList
	{
		public static bool HasBeenLodgedAtCustoms(string code)
		{
			return code != Codes.AwaitingOriginal
				&& code != Codes.AcknowledgedWithdrawal
				&& code != Codes.ErrorAcknowledgedOriginal
				&& !string.IsNullOrEmpty(code);
		}

		public static bool HasResponseFromCBSA(string code)
		{
			return code == Codes.AcceptedOriginal
				|| code == Codes.AcceptedAmendment
				|| code == Codes.AcceptedChange
				|| code == Codes.AcceptedWithdrawal
				|| code == Codes.RejectedOriginal
				|| code == Codes.RejectedAmendment
				|| code == Codes.RejectedChange
				|| code == Codes.RejectedWithdrawal;
		}

		public static bool IsWithdrawn(string code)
		{
			return code == Codes.AcknowledgedWithdrawal;
		}

		public static bool IsDocumentIDSentToCustoms(string code)
		{
			return IsWaitingForResponse(code)
				|| HasBeenLodgedAtCustoms(code)
				|| IsWithdrawn(code)
				|| HasResponseFromCBSA(code);
		}

		public static bool IsWaitingForResponse(string code)
		{
			return code == Codes.AwaitingOriginal
				|| code == Codes.AwaitingAmendment
				|| code == Codes.AwaitingChange
				|| code == Codes.AwaitingWithdrawal;
		}
	}
}
