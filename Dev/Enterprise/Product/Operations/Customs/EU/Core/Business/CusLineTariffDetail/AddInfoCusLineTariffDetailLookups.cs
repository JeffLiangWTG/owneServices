namespace Enterprise.Customs.EU.Business
{
	public class AddInfoCusLineTariffDetailLookups : EUAddInfoLookups
	{
		public AddInfoCusLineTariffDetailLookups(AddInfoCusLineTariffDetail parent)
			: base(parent)
		{
		}

		public new AddInfoCusLineTariffDetail Parent => (AddInfoCusLineTariffDetail)base.Parent;
	}
}

