
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class PenaltyRefundRequestMessageSendingObjectValidation : Customs.Business.JobDeclarationMessageSendingObjectValidation
	{
		public PenaltyRefundRequestMessageSendingObjectValidation(JobDeclarationMessageSendingObject sendingObject) : base(sendingObject)
		{
		}
		public new PenaltyRefundRequestMessageSendingObject Parent => (PenaltyRefundRequestMessageSendingObject)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateRefundCause();
			ValidateRefundReason();
			ValidateTaxOffice();
		}
		public void ValidateRefundCause() => ValidateCalculatedProperty(Parent.RefundCauseInfo);
		public void ValidateRefundReason() => ValidateCalculatedProperty(Parent.RefundReasonInfo);
		public void ValidateTaxOffice() => ValidateCalculatedProperty(Parent.TaxOfficeInfo);

		protected void CheckRefundCause() => ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.RefundCauseInfo);
		protected void CheckRefundReason() => ListValidation.MessageErrorIfInvalidCode(Parent.RefundReasonInfo);
		protected void CheckTaxOffice() => ListValidation.MessageErrorIfInvalidCode(Parent.TaxOfficeInfo);

		protected override void CheckShouldSend()
		{
			base.CheckShouldSend();
			if (Parent.ShouldSend)
			{
				if (Parent.Header.EntryNumbers.Cast<CusEntryNumber>().Any(x => x.CE_EntryStatus == CustomsMessageStatusTypeList.Codes.OriginalSent && x.CE_EntryType == ElectronicDocumentTypeList.Codes._5UL && x.CE_EntryLineReference == Parent.CustomsDisbursementBillNumber))
				{
					Parent.ShouldSendInfo.AddError(Res.GetString("A4E5F1B2-0C3D-4F6A-8D7C-9E5B1F2A0D3A", "You cannot send this message. Its status indicates the last electronic document is still waiting for a response."));
				}
			}
		}
	}
}
