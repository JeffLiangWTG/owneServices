namespace Enterprise.Customs.CN.Business
{
	public class CusEntryLineLookups : Customs.Business.CusEntryLineLookups
	{
		public CusEntryLineLookups(CusEntryLine parent)
			: base(parent)
		{
		}

		public CusEntryLine EntryLine => Parent;

		protected new CusEntryLine Parent => (CusEntryLine)base.Parent;
	}
}
