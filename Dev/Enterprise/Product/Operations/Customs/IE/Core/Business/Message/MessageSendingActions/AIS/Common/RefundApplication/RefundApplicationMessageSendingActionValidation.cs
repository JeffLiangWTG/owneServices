using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.Business
{
	public class RefundApplicationMessageSendingActionValidation : CusEntryHeaderMessageSendingActionValidation
	{
		public RefundApplicationMessageSendingActionValidation(RefundApplicationMessageSendingAction parent) : base(parent) { }

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateRefundType();
			ValidateOfficeOfDebt();
			ValidateOfficeOfResponsibility();
			ValidateLegalBasis();
			ValidateDescriptionOfGrounds();
			ValidateAmount();
		}

		public void ValidateRefundType()
		{
			ValidateCalculatedProperty(Parent.RefundTypeInfo);
		}

		protected void CheckRefundType()
		{
			if (Parent.ShouldSend)
			{
				var targetInfo = Parent.RefundTypeInfo;
				MandatoryValidation.CheckEntered(targetInfo);
				ListValidation.ErrorIfInvalidCode(targetInfo);
			}
		}

		public void ValidateOfficeOfDebt()
		{
			ValidateCalculatedProperty(Parent.OfficeOfDebtInfo);
		}

		protected void CheckOfficeOfDebt()
		{
			if (Parent.ShouldSend)
			{
				var targetInfo = Parent.OfficeOfDebtInfo;
				MandatoryValidation.CheckEntered(targetInfo);
				ListValidation.ErrorIfInvalidCode(targetInfo);
			}
		}

		public void ValidateOfficeOfResponsibility()
		{
			ValidateCalculatedProperty(Parent.OfficeOfResponsibilityInfo);
		}

		protected void CheckOfficeOfResponsibility()
		{
			if (Parent.ShouldSend)
			{
				ListValidation.ErrorIfInvalidCode(Parent.OfficeOfResponsibilityInfo);
			}
		}

		public void ValidateLegalBasis()
		{
			ValidateCalculatedProperty(Parent.LegalBasisInfo);
		}

		protected void CheckLegalBasis()
		{
			if (Parent.ShouldSend)
			{
				var targetInfo = Parent.LegalBasisInfo;
				MandatoryValidation.CheckEntered(targetInfo);
				ListValidation.ErrorIfInvalidCode(targetInfo);
			}
		}

		public void ValidateDescriptionOfGrounds()
		{
			ValidateCalculatedProperty(Parent.DescriptionOfGroundsInfo);
		}

		protected void CheckDescriptionOfGrounds()
		{
			if (Parent.ShouldSend)
			{
				MandatoryValidation.CheckEntered(Parent.DescriptionOfGroundsInfo);
			}
		}

		public void ValidateAmount()
		{
			ValidateCalculatedProperty(Parent.AmountInfo);
		}

		protected void CheckAmount()
		{
			if (Parent.ShouldSend)
			{
				var targetInfo = Parent.AmountInfo;
				MandatoryValidation.CheckNotNegative(targetInfo);
				MandatoryValidation.CheckNotZero(targetInfo);
			}
		}

		protected override void CheckMessageType()
		{
			// do not call base as MessageType is not visible and is has been set 'F15' as default
		}

		new RefundApplicationMessageSendingAction Parent => (RefundApplicationMessageSendingAction)base.Parent;
	}
}
