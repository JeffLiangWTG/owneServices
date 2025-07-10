namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public partial class GroupInvoiceCharge : AutoGroupInvoiceCharge
	{
		#region New'd objects for type safety (typeDeciders/concrete classes must take care of instantiation

		public new GroupInvoiceCharge Clone()
		{
			return (GroupInvoiceCharge)base.Clone();
		}

		public new GroupInvoiceChargeLookups Lookups
		{
			get { return (GroupInvoiceChargeLookups)base.Lookups; }
		}

		public new GroupInvoiceChargeValidation Validation
		{
			get { return (GroupInvoiceChargeValidation)base.Validation; }
		}

		#endregion

		#region Implementation

		#region protected override

		protected override Common.JobComInvHeaderChargeLookups GetNewLookups()
		{
			return new GroupInvoiceChargeLookups(this);
		}

		protected override Common.JobComInvHeaderChargeValidation GetNewValidation()
		{
			return new GroupInvoiceChargeValidation(this);
		}

		#endregion

		#endregion
	}
}
