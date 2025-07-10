using Enterprise.Accounting.Business.ARAP.HotCheque;
using NUnit.Framework;

namespace Enterprise.Accounting.Utility.Testing
{
	public static class AccHotChequeTestHelper
	{
		public static void AssertReadOnly(string message, AccHotCheque cheque, bool expectedReadOnly)
		{
			Assertion.AssertEquals(message, expectedReadOnly, cheque.AQ_AHInfo.ReadOnly);
			Assertion.AssertEquals(message, expectedReadOnly, cheque.AQ_AmountInfo.ReadOnly);
			Assertion.AssertEquals(message, expectedReadOnly, cheque.AQ_ActualOrMaxIndicatorInfo.ReadOnly);
			Assertion.AssertEquals(message, expectedReadOnly, cheque.AQ_AKInfo.ReadOnly);
			Assertion.AssertEquals(message, expectedReadOnly, cheque.AQ_Calc_ActualAmountIndicatorInfo.ReadOnly);
			Assertion.AssertEquals(message, expectedReadOnly, cheque.AQ_Calc_MaximumAmountIndicatorInfo.ReadOnly);
			Assertion.AssertEquals(message, expectedReadOnly, cheque.AQ_CancelledInfo.ReadOnly);
			Assertion.AssertEquals(message, expectedReadOnly, cheque.AQ_ChequeDateInfo.ReadOnly);
			Assertion.AssertEquals(message, expectedReadOnly, cheque.AQ_ChequeNumberInfo.ReadOnly);
			Assertion.AssertEquals(message, expectedReadOnly, cheque.AQ_ChequePayeeInfo.ReadOnly);
			Assertion.AssertEquals(message, expectedReadOnly, cheque.AQ_DescriptionInfo.ReadOnly);
			Assertion.AssertEquals(message, expectedReadOnly, cheque.AQ_GS_NKResponsibleStaffInfo.ReadOnly);
			Assertion.AssertEquals(message, expectedReadOnly, cheque.AQ_HouseBillInfo.ReadOnly);
			Assertion.AssertEquals(message, expectedReadOnly, cheque.AQ_JHInfo.ReadOnly);
			Assertion.AssertEquals(message, expectedReadOnly, cheque.AQ_MasterBillInfo.ReadOnly);
			Assertion.AssertEquals(message, expectedReadOnly, cheque.AQ_OHInfo.ReadOnly);

			Assertion.Assert("Status should always be read-only, because its value is determined by conditions outside the cheque.", cheque.AQ_Calc_ChequeStatusInfo.ReadOnly);
			Assertion.Assert("Currency should always be read-only, because its value is derived from the country of the chequing account.", cheque.AQ_Calc_RX_NKInfo.ReadOnly);
		}
	}
}
