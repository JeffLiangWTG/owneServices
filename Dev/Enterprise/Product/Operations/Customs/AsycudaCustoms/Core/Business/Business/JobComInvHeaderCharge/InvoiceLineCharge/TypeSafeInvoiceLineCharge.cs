namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public partial class InvoiceLineCharge : AutoInvoiceLineCharge
	{
		#region New'd objects for type safety (typeDeciders/concrete classes must take care of instantiation

		public new InvoiceLineCharge Clone()
		{
			return (InvoiceLineCharge)base.Clone();
		}

		public new InvoiceLineChargeValidation Validation
		{
			get { return (InvoiceLineChargeValidation)base.Validation; }
		}

		public new InvoiceLineChargeLookups Lookups
		{
			get { return (InvoiceLineChargeLookups)base.Lookups; }
		}

		#endregion

		#region Implementation

		#region protected override

		protected override Common.JobComInvHeaderChargeValidation GetNewValidation()
		{
			return new InvoiceLineChargeValidation(this);
		}

		protected override Common.JobComInvHeaderChargeLookups GetNewLookups()
		{
			return new InvoiceLineChargeLookups(this);
		}

		#endregion

		#endregion
	}
}
