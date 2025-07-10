using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.UPE.Business.Testing
{
	public static class TestHelperForRefund
	{
		public static ClientRefund PopulateRefund(ClientRefund refund)
		{
			GlbStaff staff = ClientRefundTest.TestHelper.NewStaff("Mr Bob", "email@fortest.com", refund.Factory);
			refund.T10_ControlNumber = "0223573";
			refund.T10_RefundReason = refund.ReasonTypesPairList[0].Code;
			refund.T10_GS_NKAtFaultUser = staff.GS_Code;
			refund.T10_EnquiryContact = staff.GS_FullName;
			refund.T10_EnquiryPhoneNumber = "123123123";
			refund.T10_EnquiryDetails = "T10_EnquiryDetails";
			refund.T10_EnquiryRaisedBy = refund.RaisedByList[0].Code;
			refund.Remarks = "Remarks";
			refund.Factory.Save();
			return refund;
		}
	}
}
