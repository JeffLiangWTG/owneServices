namespace Enterprise.Customs.JP.Business
{
	public partial class InvoiceLineApportionCharge : AutoInvoiceLineApportionCharge
	{
		#region New'd objects for type safety (typeDeciders/concrete classes must take care of instantiation

		public new InvoiceLineApportionCharge Clone()
		{
			return (InvoiceLineApportionCharge)base.Clone();
		}

		public new InvoiceLineApportionChargeValidation Validation
		{
			get { return (InvoiceLineApportionChargeValidation)base.Validation; }
		}

		public new InvoiceLineApportionChargeLookups Lookups
		{
			get { return (InvoiceLineApportionChargeLookups)base.Lookups; }
		}

		#endregion

		#region Implementation

		#region protected override

		protected override Customs.Common.JobComInvHeaderChargeValidation GetNewValidation()
		{
			return new InvoiceLineApportionChargeValidation(this);
		}

		protected override Customs.Common.JobComInvHeaderChargeLookups GetNewLookups()
		{
			return new InvoiceLineApportionChargeLookups(this);
		}

		#endregion

		#endregion
	}
}
