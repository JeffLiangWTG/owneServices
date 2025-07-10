namespace Enterprise.Customs.IL.Business
{
	public partial class InvoiceApportionCharge : AutoInvoiceApportionCharge
	{
		public new InvoiceApportionCharge Clone() => (InvoiceApportionCharge)base.Clone();

		public new InvoiceApportionChargeValidation Validation => (InvoiceApportionChargeValidation)base.Validation;

		public new InvoiceApportionChargeLookups Lookups => (InvoiceApportionChargeLookups)base.Lookups;

		protected override Common.JobComInvHeaderChargeValidation GetNewValidation() => new InvoiceApportionChargeValidation(this);

		protected override Common.JobComInvHeaderChargeLookups GetNewLookups() => new InvoiceApportionChargeLookups(this);
	}
}
