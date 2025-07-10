namespace Enterprise.Customs.CN.Business
{
	public class CusEntryHeaderChargesValidation : Customs.Business.CusEntryHeaderChargesValidation
	{
		public CusEntryHeaderChargesValidation(CusEntryHeaderCharges parent)
			: base(parent)
		{
		}

		public CusEntryHeaderCharges EntryHeaderCharges => Parent;

		protected new CusEntryHeaderCharges Parent => (CusEntryHeaderCharges)base.Parent;
	}
}
