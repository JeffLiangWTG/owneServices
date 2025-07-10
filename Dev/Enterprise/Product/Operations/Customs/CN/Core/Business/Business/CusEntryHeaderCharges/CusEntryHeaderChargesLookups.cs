namespace Enterprise.Customs.CN.Business
{
	public class CusEntryHeaderChargesLookups : Customs.Business.CusEntryHeaderChargesLookups
	{
		public CusEntryHeaderChargesLookups(CusEntryHeaderCharges parent)
			: base(parent)
		{
		}

		public CusEntryHeaderCharges EntryHeaderCharges => Parent;

		protected new CusEntryHeaderCharges Parent => (CusEntryHeaderCharges)base.Parent;
	}
}
