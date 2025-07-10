//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoClientRefundValidation
//
//    This class should be used for overriding validation in AutoClientRefundValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

namespace Enterprise.Client.UPE.Business
{
	public class ClientRefundValidation : AutoClientRefundValidation
	{
		public ClientRefundValidation(AutoClientRefund parent)
			: base(parent)
		{
		}

		protected override void CheckT10_EnquiryPhoneNumber()
		{
			base.CheckT10_EnquiryPhoneNumber();
			if (Parent.T10_EnquiryPhoneNumber.ContainsAnyLetters)
			{
				Parent.T10_EnquiryPhoneNumberInfo.AddError("The Phone number must be Digits only.");
			}
		}

		protected override void CheckT10_EnquiryRaisedBy()
		{
			base.CheckT10_EnquiryRaisedBy();
			ListValidation.ErrorIfInvalidCode(Parent.T10_EnquiryRaisedByInfo, ((ClientRefund)Parent).RaisedByList);
		}

		protected override void CheckT10_GS_NKAtFaultUser()
		{
			base.CheckT10_GS_NKAtFaultUser();
			MandatoryValidation.CheckEntered(Parent.T10_GS_NKAtFaultUserInfo);
			ListValidation.ErrorIfInvalidCode(Parent.T10_GS_NKAtFaultUserInfo, ((ClientRefund)Parent).AtFaultList);
		}

		protected override void CheckT10_RefundReason()
		{
			base.CheckT10_RefundReason();
			MandatoryValidation.CheckEntered(Parent.T10_RefundReasonInfo);
			ListValidation.ErrorIfInvalidCode(Parent.T10_RefundReasonInfo, ((ClientRefund)Parent).ReasonTypesPairList);
		}

		protected override void CheckT10_RefundRejectedDetails()
		{
			base.CheckT10_RefundRejectedDetails();
			if (Parent.T10_IsRefundRejected)
			{
				MandatoryValidation.CheckEntered(Parent.T10_RefundRejectedDetailsInfo);
			}
		}
	}
}
