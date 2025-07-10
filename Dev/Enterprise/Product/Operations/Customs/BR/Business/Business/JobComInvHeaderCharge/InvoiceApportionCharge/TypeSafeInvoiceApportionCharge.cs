namespace Enterprise.Customs.BR.Business
{
	public partial class InvoiceApportionCharge : AutoInvoiceApportionCharge
	{
		#region New'd objects for type safety (typeDeciders/concrete classes must take care of instantiation

		public new InvoiceApportionCharge Clone()
		{
			return (InvoiceApportionCharge)base.Clone();
		}

		public new InvoiceApportionChargeValidation Validation
		{
			get { return (InvoiceApportionChargeValidation)base.Validation; }
		}

		public new InvoiceApportionChargeLookups Lookups
		{
			get { return (InvoiceApportionChargeLookups)base.Lookups; }
		}

		#endregion

		#region Implementation

		#region protected override

		protected override Common.JobComInvHeaderChargeValidation GetNewValidation()
		{
			return new InvoiceApportionChargeValidation(this);
		}

		protected override Common.JobComInvHeaderChargeLookups GetNewLookups()
		{
			return new InvoiceApportionChargeLookups(this);
		}

		#endregion

		#endregion
	}
}
