namespace Enterprise.Customs.CA.Business
{
	public class CusEntryLineFeeLookups : Customs.Business.CusEntryLineFeeLookups
	{
		public CusEntryLineFeeLookups(CusEntryLineFee parent)
			: base(parent)
		{
		}

		public CusEntryLineFee EntryLineFee
		{
			get { return Parent; }
		}

		protected new CusEntryLineFee Parent
		{
			get { return (CusEntryLineFee)base.Parent; }
		}
	}
}
