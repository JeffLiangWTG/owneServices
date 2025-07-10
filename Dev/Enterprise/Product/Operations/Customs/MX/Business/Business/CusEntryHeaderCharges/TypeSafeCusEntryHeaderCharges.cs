namespace Enterprise.Customs.MX.Business
{
	public partial class CusEntryHeaderCharges : AutoCusEntryHeaderCharges
	{
		public new CusEntryHeaderCharges Clone() => (CusEntryHeaderCharges)base.Clone();

		public new CusEntryHeaderChargesValidation Validation => (CusEntryHeaderChargesValidation)base.Validation;

		public new CusEntryHeaderChargesLookups Lookups => (CusEntryHeaderChargesLookups)base.Lookups;

		protected override Customs.Business.CusEntryHeaderChargesValidation GetNewValidation() => new CusEntryHeaderChargesValidation(this);

		protected override Customs.Business.CusEntryHeaderChargesLookups GetNewLookups() => new CusEntryHeaderChargesLookups(this);
	}
}
