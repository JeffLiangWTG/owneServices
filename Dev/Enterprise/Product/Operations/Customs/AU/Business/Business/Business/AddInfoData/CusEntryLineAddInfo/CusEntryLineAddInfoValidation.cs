using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusEntryLineAddInfoValidation : AUAddInfoValidation
	{
		public CusEntryLineAddInfoValidation(CusEntryLineAddInfo parent)
			: base(parent)
		{
		}

		protected new CusEntryLineAddInfo Parent
		{
			get { return (CusEntryLineAddInfo)base.Parent; }
		}

		protected override void CheckZA_RRC_Hidden()
		{
			base.CheckZA_RRC_Hidden();
			ListValidation.MessageErrorIfInvalidCode(Parent.ZA_RRC_HiddenInfo, Parent.Lookups.ZA_RRC_List);
			if (Parent.Parent.Header != null)
			{
				ZString refundReasonCode = ((ICusEntryLine)Parent.EntryLine).RefundReasonCode;
				if (refundReasonCode.IsEmpty)
				{
					if (Parent.EntryLine.Header.IsCustomsChargePaid && Parent.EntryLine.IsActive && Parent.EntryLine.IsLessDutyAndTax)
					{
						Parent.ZA_RRC_HiddenInfo.AddWarning("The entry is paid and the current total duty and tax amount calculated is less than total duty and tax advised in the last lodgement message for this line. Therefore you might need a Refund reason Code for this amendment.");
					}
				}
				else
				{
					if (!Parent.EntryLine.Header.IsCustomsChargePaid)
					{
						Parent.ZA_RRC_HiddenInfo.AddMessageError("This entry is not paid yet and so a refund cannot occur.");
					}
					else if (Parent.EntryLine.Header.Questions.IsGoodsDeliveredQuestionAnsweredNo &&
						refundReasonCode != DeletedLineAmendment.RefundReasonForDeletedLines)
					{
						Parent.ZA_RRC_HiddenInfo.AddMessageError("According to declaration questions, goods are not delivered and the refund reason code should be '126A' prior to delivery.");
					}
					else if (Parent.EntryLine.IsActive && !Parent.EntryLine.IsLessDutyAndTax && !Parent.ZA_RRC_Hidden.IsEmpty && Parent.EntryLine.Header.TotalDeferredDutyFromCustoms.IsEmpty)
					{
						Parent.ZA_RRC_HiddenInfo.AddWarning("The current total duty and tax amount calculated is not less than what you paid last time and so a refund reason is not required.");
					}
				}
			}
		}
	}
}
