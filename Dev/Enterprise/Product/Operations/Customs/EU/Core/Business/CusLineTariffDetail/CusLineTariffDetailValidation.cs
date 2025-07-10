namespace Enterprise.Customs.EU.Business
{
	public class CusLineTariffDetailValidation : Customs.Business.CusLineTariffDetailValidation
	{
		public CusLineTariffDetailValidation(CusLineTariffDetail parent)
			: base(parent)
		{
		}

		public new CusLineTariffDetail Parent => (CusLineTariffDetail)base.Parent;
	}
}

