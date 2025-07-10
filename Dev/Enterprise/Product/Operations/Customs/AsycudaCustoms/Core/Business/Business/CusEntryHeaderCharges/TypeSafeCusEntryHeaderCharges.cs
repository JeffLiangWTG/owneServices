namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public partial class CusEntryHeaderCharges : AutoCusEntryHeaderCharges
	{
		#region New'd objects for type safety (typeDeciders/concrete classes must take care of instantiation

		public new CusEntryHeaderCharges Clone()
		{
			return (CusEntryHeaderCharges)base.Clone();
		}

		public new CusEntryHeaderChargesValidation Validation
		{
			get { return (CusEntryHeaderChargesValidation)base.Validation; }
		}

		public new CusEntryHeaderChargesLookups Lookups
		{
			get { return (CusEntryHeaderChargesLookups)base.Lookups; }
		}

		#endregion

		#region Implementation

		#region protected override

		protected override Customs.Business.CusEntryHeaderChargesValidation GetNewValidation()
		{
			return new CusEntryHeaderChargesValidation(this);
		}
		protected override Customs.Business.CusEntryHeaderChargesLookups GetNewLookups()
		{
			return new CusEntryHeaderChargesLookups(this);
		}

		#endregion

		#endregion
	}
}
