namespace Enterprise.Customs.MX.Business
{
	public partial class CusEntryLineFee : AutoCusEntryLineFee
	{
		public new CusEntryLineFee Clone() => (CusEntryLineFee)base.Clone();

		public new CusEntryLineFeeLookups Lookups => (CusEntryLineFeeLookups)base.Lookups;

		protected override Customs.Business.CusEntryLineFeeLookups GetNewLookups() => new CusEntryLineFeeLookups(this);
	}
}
