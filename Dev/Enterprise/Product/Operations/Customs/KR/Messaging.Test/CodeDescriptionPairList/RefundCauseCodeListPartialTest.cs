namespace Enterprise.Customs.KR.Messaging.Testing
{
	public class RefundCauseCodeListPartialTest : NUnit.Framework.TestCase
	{
		public void TestIsRefundReasonCodeMandatory()
		{
			Assert(RefundCauseCodeList.IsRefundReasonCodeMandatory(RefundCauseCodeList.Codes._03));
			Assert(RefundCauseCodeList.IsRefundReasonCodeMandatory(RefundCauseCodeList.Codes._04));
			Assert(RefundCauseCodeList.IsRefundReasonCodeMandatory(RefundCauseCodeList.Codes._05));
			Assert(RefundCauseCodeList.IsRefundReasonCodeMandatory(RefundCauseCodeList.Codes._06));
			Assert(RefundCauseCodeList.IsRefundReasonCodeMandatory(RefundCauseCodeList.Codes._07));
			Assert(RefundCauseCodeList.IsRefundReasonCodeMandatory(RefundCauseCodeList.Codes._08));
			Assert(RefundCauseCodeList.IsRefundReasonCodeMandatory(RefundCauseCodeList.Codes._09));
			Assert(RefundCauseCodeList.IsRefundReasonCodeMandatory(RefundCauseCodeList.Codes._10));

			Assert(!RefundCauseCodeList.IsRefundReasonCodeMandatory(RefundCauseCodeList.Codes._01));
			Assert(!RefundCauseCodeList.IsRefundReasonCodeMandatory(RefundCauseCodeList.Codes._02));
			Assert(!RefundCauseCodeList.IsRefundReasonCodeMandatory(RefundCauseCodeList.Codes._11));
			Assert(!RefundCauseCodeList.IsRefundReasonCodeMandatory(RefundCauseCodeList.Codes._12));
			Assert(!RefundCauseCodeList.IsRefundReasonCodeMandatory(RefundCauseCodeList.Codes._13));
		}
	}
}
