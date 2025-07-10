namespace Enterprise.Customs.CA.Business
{
	public partial class CusClassPartPivot
	{
		#region New'd objects for type safety (typeDeciders/concrete classes must take care of instantiation

		public new CusClassPartPivot Clone()
		{
			return (CusClassPartPivot)base.Clone();
		}

		public new CusClassPartPivotLookups Lookups
		{
			get { return (CusClassPartPivotLookups)base.Lookups; }
		}

		public new CusClassPartPivotValidation Validation
		{
			get { return (CusClassPartPivotValidation)base.Validation; }
		}

		public new OrgSupplierPart Part
		{
			get { return (OrgSupplierPart)base.Part; }
		}

		public new CusClassification Classification
		{
			get { return (CusClassification)base.Classification; }
		}

		#endregion

		#region Implementation

		#region protected override

		protected override Customs.Business.CusClassPartPivotLookups GetNewLookups()
		{
			return new CusClassPartPivotLookups(this);
		}

		protected override Customs.Business.CusClassPartPivotValidation GetNewValidation()
		{
			return new CusClassPartPivotValidation(this);
		}

		protected override Customs.Business.TariffFormatter GetTariffFormatter()
		{
			return new TariffFormatter();
		}

		#endregion

		#endregion
	}
}
