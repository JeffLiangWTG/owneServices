using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.Business
{
	public class AddInfoCusLineTariffDetailValidation : EU.Business.AddInfoCusLineTariffDetailValidation
	{
		public AddInfoCusLineTariffDetailValidation(AddInfoCusLineTariffDetail parent) : base(parent)
		{
		}

		public new AddInfoCusLineTariffDetail Parent => (AddInfoCusLineTariffDetail)base.Parent;

		protected override void CheckZG_MethodOfPayment()
		{
			base.CheckZG_MethodOfPayment();
			var cusLineTariffDetail = Parent.Parent;
			if (cusLineTariffDetail != null)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ZG_MethodOfPaymentInfo);
				ListValidation.MessageErrorIfInvalidCode(Parent.ZG_MethodOfPaymentInfo);
			}
		}
	}
}
