using CargoWise.EntityFramework;

namespace Enterprise.Customs.KR.Business
{
	public class RefundSessionalDataValidation : Customs.Business.CusSupportingInfoValidation
	{
		public RefundSessionalDataValidation(RefundSessionalData parent) : base(parent)
		{
		}
		public new RefundSessionalData Parent
		{
			get { return (RefundSessionalData)base.Parent; }
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateRefundCauseCode();
			ValidateRefundReasonCode();
			ValidateRefundType();
			ValidateRefundRequestYN();
			ValidateCustomsDisbursementBill();
		}
		public void ValidateRefundCauseCode() => ValidateCalculatedProperty(Parent.RefundCauseCodeInfo);
		public void ValidateRefundReasonCode() => ValidateCalculatedProperty(Parent.RefundReasonCodeInfo);
		public void ValidateRefundType() => ValidateCalculatedProperty(Parent.RefundTypeInfo);
		public void ValidateRefundRequestYN() => ValidateCalculatedProperty(Parent.RefundRequestYNInfo);
		public void ValidateCustomsDisbursementBill() => ValidateCalculatedProperty(Parent.CustomsDisbursementBillInfo);

		protected void CheckRefundCauseCode() => ListValidation.MessageErrorIfInvalidCode(Parent.RefundCauseCodeInfo);
		protected void CheckRefundReasonCode() => ListValidation.MessageErrorIfInvalidCode(Parent.RefundReasonCodeInfo);
		protected void CheckRefundType() => ListValidation.MessageErrorIfInvalidCode(Parent.RefundTypeInfo);
		protected void CheckRefundRequestYN() => ListValidation.MessageErrorIfInvalidCode(Parent.RefundRequestYNInfo);
		protected void CheckCustomsDisbursementBill() => ListValidation.MessageErrorIfInvalidCode(Parent.CustomsDisbursementBillInfo);
	}
}
