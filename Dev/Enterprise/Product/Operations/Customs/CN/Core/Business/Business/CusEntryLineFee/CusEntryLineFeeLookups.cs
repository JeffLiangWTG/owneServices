namespace Enterprise.Customs.CN.Business
{
	public class CusEntryLineFeeLookups : Customs.Business.CusEntryLineFeeLookups
	{
		public CusEntryLineFeeLookups(CusEntryLineFee parent)
			: base(parent)
		{
		}

		public CusEntryLineFee EntryLineFee => Parent;

		protected new CusEntryLineFee Parent => (CusEntryLineFee)base.Parent;
	}
}
