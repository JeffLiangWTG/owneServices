namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public class CusEntryHeaderChargesLookups : Customs.Business.CusEntryHeaderChargesLookups
	{
		public CusEntryHeaderChargesLookups(CusEntryHeaderCharges parent)
			: base(parent)
		{
		}

		public CusEntryHeaderCharges EntryHeaderCharges
		{
			get { return Parent; }
		}

		protected new CusEntryHeaderCharges Parent
		{
			get { return (CusEntryHeaderCharges)base.Parent; }
		}
	}
}
