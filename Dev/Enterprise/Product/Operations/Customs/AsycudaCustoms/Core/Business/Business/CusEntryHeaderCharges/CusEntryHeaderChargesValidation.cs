namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public class CusEntryHeaderChargesValidation : Customs.Business.CusEntryHeaderChargesValidation
	{
		public CusEntryHeaderChargesValidation(CusEntryHeaderCharges parent)
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
