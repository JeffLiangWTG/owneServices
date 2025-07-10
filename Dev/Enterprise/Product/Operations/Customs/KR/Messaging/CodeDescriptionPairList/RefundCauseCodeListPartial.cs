namespace Enterprise.Customs.KR.Messaging
{
	public partial class RefundCauseCodeList
	{
		public static bool IsRefundReasonCodeMandatory(string refundCauseCode)
		{
			switch (refundCauseCode)
			{
				case RefundCauseCodeList.Codes._03:
				case RefundCauseCodeList.Codes._04:
				case RefundCauseCodeList.Codes._05:
				case RefundCauseCodeList.Codes._06:
				case RefundCauseCodeList.Codes._07:
				case RefundCauseCodeList.Codes._08:
				case RefundCauseCodeList.Codes._09:
				case RefundCauseCodeList.Codes._10:
					return true;
				default:
					return false;
			}
		}
	}
}
