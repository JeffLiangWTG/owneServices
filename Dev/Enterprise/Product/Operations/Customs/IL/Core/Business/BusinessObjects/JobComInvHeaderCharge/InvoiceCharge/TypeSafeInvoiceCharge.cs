namespace Enterprise.Customs.IL.Business
{
	public partial class InvoiceCharge : AutoInvoiceCharge
	{
		public new InvoiceCharge Clone() => (InvoiceCharge)base.Clone();

		public new InvoiceChargeLookups Lookups => (InvoiceChargeLookups)base.Lookups;

		public new InvoiceChargeValidation Validation => (InvoiceChargeValidation)base.Validation;

		protected override Common.JobComInvHeaderChargeLookups GetNewLookups() => new InvoiceChargeLookups(this);

		protected override Common.JobComInvHeaderChargeValidation GetNewValidation() => new InvoiceChargeValidation(this);
	}
}
