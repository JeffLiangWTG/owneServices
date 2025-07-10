using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.H7.Business
{
	public class RF415MessageSendingObjectValidation : AutoRF415MessageSendingObjectValidation
	{
		public RF415MessageSendingObjectValidation(AutoRF415MessageSendingObject parent)
			: base(parent)
		{
		}

		new RF415MessageSendingObject Parent => (RF415MessageSendingObject)base.Parent;

		protected override void CheckRefundType()
		{
			base.CheckRefundType();
			if (Parent.ShouldSend)
			{
				MandatoryValidation.CheckEntered(Parent.RefundTypeInfo);
				ListValidation.ErrorIfInvalidCode(Parent.RefundTypeInfo);
			}
		}

		protected override void CheckOfficeOfDebt()
		{
			base.CheckOfficeOfDebt();
			if (Parent.ShouldSend)
			{
				MandatoryValidation.CheckEntered(Parent.OfficeOfDebtInfo);
				ListValidation.ErrorIfInvalidCode(Parent.OfficeOfDebtInfo);
			}
		}

		protected override void CheckOfficeOfResponsibility()
		{
			base.CheckOfficeOfResponsibility();
			if (Parent.ShouldSend)
			{
				ListValidation.ErrorIfInvalidCode(Parent.OfficeOfResponsibilityInfo);
			}
		}

		protected override void CheckLegalBasis()
		{
			base.CheckLegalBasis();
			if (Parent.ShouldSend)
			{
				MandatoryValidation.CheckEntered(Parent.LegalBasisInfo);
				ListValidation.ErrorIfInvalidCode(Parent.LegalBasisInfo);
			}
		}

		protected override void CheckDescriptionOfGrounds()
		{
			base.CheckDescriptionOfGrounds();
			if (Parent.ShouldSend)
			{
				MandatoryValidation.CheckEntered(Parent.DescriptionOfGroundsInfo);
			}
		}

		protected override void CheckAmount()
		{
			base.CheckAmount();
			if (Parent.ShouldSend)
			{
				MandatoryValidation.CheckNotNegative(Parent.AmountInfo);
				MandatoryValidation.CheckNotZero(Parent.AmountInfo);
			}
		}
	}
}
