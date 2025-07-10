namespace Enterprise.Customs.IL.Business
{
	public partial class InvoiceLineCharge : AutoInvoiceLineCharge
	{
		public new InvoiceLineCharge Clone() => (InvoiceLineCharge)base.Clone();

		public new InvoiceLineChargeValidation Validation => (InvoiceLineChargeValidation)base.Validation;

		public new InvoiceLineChargeLookups Lookups => (InvoiceLineChargeLookups)base.Lookups;

		protected override Common.JobComInvHeaderChargeValidation GetNewValidation() => new InvoiceLineChargeValidation(this);

		protected override Common.JobComInvHeaderChargeLookups GetNewLookups() => new InvoiceLineChargeLookups(this);
	}
}
