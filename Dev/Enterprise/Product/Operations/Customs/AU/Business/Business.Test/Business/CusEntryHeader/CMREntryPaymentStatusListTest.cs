using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class CMREntryPaymentStatusListTest : TestCase
	{
		public void TestIsClearedStatus()
		{
			AssertEquals(false, CMREntryPaymentStatusList.IsClearedStatus("blah"));
			AssertEquals(true, CMREntryPaymentStatusList.IsClearedStatus(CMREntryPaymentStatusList.Codes.Paid));
			AssertEquals(true, CMREntryPaymentStatusList.IsClearedStatus(CMREntryPaymentStatusList.Codes.Refunded));
			AssertEquals(false, CMREntryPaymentStatusList.IsClearedStatus(CMREntryPaymentStatusList.Codes.NoAmountDue));
			AssertEquals(false, CMREntryPaymentStatusList.IsClearedStatus(CMREntryPaymentStatusList.Codes.NotPaid));
			AssertEquals(false, CMREntryPaymentStatusList.IsClearedStatus(CMREntryPaymentStatusList.Codes.PayPending));
			AssertEquals(false, CMREntryPaymentStatusList.IsClearedStatus(CMREntryPaymentStatusList.Codes.PayRejected));
			AssertEquals(false, CMREntryPaymentStatusList.IsClearedStatus(CMREntryPaymentStatusList.Codes.RefundPending));
			AssertEquals(false, CMREntryPaymentStatusList.IsClearedStatus(CMREntryPaymentStatusList.Codes.RefundRejected));
		}

		public void TestArePaymentDetailsRequiredForAmendmentOrWithdrawal()
		{
			AssertEquals(false, CMREntryPaymentStatusList.ArePaymentDetailsRequiredForAmendmentOrWithdrawal("blah"));
			AssertEquals(true, CMREntryPaymentStatusList.ArePaymentDetailsRequiredForAmendmentOrWithdrawal(CMREntryPaymentStatusList.Codes.Paid));
			AssertEquals(true, CMREntryPaymentStatusList.ArePaymentDetailsRequiredForAmendmentOrWithdrawal(CMREntryPaymentStatusList.Codes.Refunded));
			AssertEquals(false, CMREntryPaymentStatusList.ArePaymentDetailsRequiredForAmendmentOrWithdrawal(CMREntryPaymentStatusList.Codes.NoAmountDue));
			AssertEquals(false, CMREntryPaymentStatusList.ArePaymentDetailsRequiredForAmendmentOrWithdrawal(CMREntryPaymentStatusList.Codes.NotPaid));
			AssertEquals(false, CMREntryPaymentStatusList.ArePaymentDetailsRequiredForAmendmentOrWithdrawal(CMREntryPaymentStatusList.Codes.PayPending));
			AssertEquals(false, CMREntryPaymentStatusList.ArePaymentDetailsRequiredForAmendmentOrWithdrawal(CMREntryPaymentStatusList.Codes.PayRejected));
			AssertEquals(true, CMREntryPaymentStatusList.ArePaymentDetailsRequiredForAmendmentOrWithdrawal(CMREntryPaymentStatusList.Codes.PayAckPending));
			AssertEquals(true, CMREntryPaymentStatusList.ArePaymentDetailsRequiredForAmendmentOrWithdrawal(CMREntryPaymentStatusList.Codes.RefundPending));
			AssertEquals(true, CMREntryPaymentStatusList.ArePaymentDetailsRequiredForAmendmentOrWithdrawal(CMREntryPaymentStatusList.Codes.RefundRejected));
		}
	}
}
