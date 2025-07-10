namespace Enterprise.Customs.EU.Business
{
	public class CusLineTariffDetailLookups : Customs.Business.CusLineTariffDetailLookups
	{
		public CusLineTariffDetailLookups(CusLineTariffDetail parent)
			: base(parent)
		{
		}

		public new CusLineTariffDetail Parent => (CusLineTariffDetail)base.Parent;
	}
}

