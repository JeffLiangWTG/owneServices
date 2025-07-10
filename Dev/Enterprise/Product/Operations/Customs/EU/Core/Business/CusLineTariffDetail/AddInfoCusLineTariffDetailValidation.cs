namespace Enterprise.Customs.EU.Business
{
	public class AddInfoCusLineTariffDetailValidation : EUAddInfoValidation
	{
		public AddInfoCusLineTariffDetailValidation(AddInfoCusLineTariffDetail parent)
			: base(parent)
		{
		}

		public new AddInfoCusLineTariffDetail Parent => (AddInfoCusLineTariffDetail)base.Parent;
	}
}

