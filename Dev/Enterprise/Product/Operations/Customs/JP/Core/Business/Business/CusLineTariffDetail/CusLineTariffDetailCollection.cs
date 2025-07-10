namespace Enterprise.Customs.JP.Business
{
	public class CusLineTariffDetailCollection : Customs.Business.CusLineTariffDetailCollection<CusLineTariffDetail>
	{
		public CusLineTariffDetailCollection(JobComInvoiceLine parent) : base(parent)
		{
		}

		public static int MaxRowCount => 6;

		protected override bool AllowNewCore => base.AllowNewCore && Count < MaxRowCount;
	}
}
