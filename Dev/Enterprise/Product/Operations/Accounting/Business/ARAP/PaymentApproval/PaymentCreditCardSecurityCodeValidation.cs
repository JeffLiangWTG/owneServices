using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval
{
	public class PaymentCreditCardSecurityCodeValidation : AutoPaymentCreditCardSecurityCodeValidation
	{
		public PaymentCreditCardSecurityCodeValidation(AutoPaymentCreditCardSecurityCode parent)
			: base(parent) { }

		#region Implementation

		public new PaymentCreditCardSecurityCode Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (PaymentCreditCardSecurityCode)base.Parent; }
		}

		#endregion

		protected override void CheckCardSecurityCode()
		{
			base.CheckCardSecurityCode();
			MandatoryValidation.CheckEntered(Parent.CardSecurityCodeInfo);
			if ((Parent.CardSecurityCode.Length < 3) || (Parent.CardSecurityCode.Length > 4))
			{
				Parent.CardSecurityCodeInfo.AddError(Res.GetString("71c44717-19f1-49f9-88f0-48c23387abf5", "Card Security Code must be 3 or 4 digits in length."));
			}
		}
	}
}