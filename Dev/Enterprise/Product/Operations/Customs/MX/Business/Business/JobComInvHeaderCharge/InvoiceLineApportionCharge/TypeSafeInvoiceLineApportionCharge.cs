namespace Enterprise.Customs.MX.Business
{
	public partial class InvoiceLineApportionCharge : AutoInvoiceLineApportionCharge
	{
		public new InvoiceLineApportionCharge Clone() => (InvoiceLineApportionCharge)base.Clone();

		public new InvoiceLineApportionChargeValidation Validation => (InvoiceLineApportionChargeValidation)base.Validation;

		public new InvoiceLineApportionChargeLookups Lookups => (InvoiceLineApportionChargeLookups)base.Lookups;

		protected override Common.JobComInvHeaderChargeValidation GetNewValidation() => new InvoiceLineApportionChargeValidation(this);

		protected override Common.JobComInvHeaderChargeLookups GetNewLookups() => new InvoiceLineApportionChargeLookups(this);
	}
}
