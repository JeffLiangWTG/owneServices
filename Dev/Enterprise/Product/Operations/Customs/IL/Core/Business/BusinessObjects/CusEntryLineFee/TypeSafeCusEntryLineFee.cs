namespace Enterprise.Customs.IL.Business
{
	public partial class CusEntryLineFee : AutoCusEntryLineFee
	{
		public new CusEntryLineFee Clone() => (CusEntryLineFee)base.Clone();

		public new CusEntryLineFeeLookups Lookups => (CusEntryLineFeeLookups)base.Lookups;

		protected override Customs.Business.CusEntryLineFeeLookups GetNewLookups() => new CusEntryLineFeeLookups(this);

		public new CusEntryLineFeeValidation Validation => (CusEntryLineFeeValidation)base.Validation;

		protected override Customs.Business.CusEntryLineFeeValidation GetNewValidation() => new CusEntryLineFeeValidation(this);
	}
}
