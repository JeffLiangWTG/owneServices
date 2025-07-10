namespace Enterprise.Customs.MX.Business
{
	public partial class GroupInvoiceCharge : AutoGroupInvoiceCharge
	{
		public new GroupInvoiceCharge Clone() => (GroupInvoiceCharge)base.Clone();

		public new GroupInvoiceChargeLookups Lookups => (GroupInvoiceChargeLookups)base.Lookups;

		public new GroupInvoiceChargeValidation Validation => (GroupInvoiceChargeValidation)base.Validation;

		protected override Common.JobComInvHeaderChargeLookups GetNewLookups() => new GroupInvoiceChargeLookups(this);

		protected override Common.JobComInvHeaderChargeValidation GetNewValidation() => new GroupInvoiceChargeValidation(this);
	}
}
