namespace Enterprise.Customs.AU.Declaration.Business
{
	public enum CMRMessageTypes { PreLodge, LodgeWithPay, LodgeWithoutPay, Payment, Amendment, Withdrawal, OriginalForAmendmentDetection }

	public class CMRAmendmentWithdrawalReason : Customs.Business.AmendmentWithdrawalReason
	{
		#region CheckReasonText

		protected override void CheckReasonText()
		{
			base.CheckReasonText();
			if (ReasonText.Length > 250)
			{
				ReasonTextInfo.AddMessageError("The maximum length that can be entered for the reason is 250 characters.");
			}
		}

		#endregion
	}
}
