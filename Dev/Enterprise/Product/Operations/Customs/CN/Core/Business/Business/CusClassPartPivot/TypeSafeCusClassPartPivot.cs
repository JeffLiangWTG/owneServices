namespace Enterprise.Customs.CN.Business
{
	public partial class CusClassPartPivot
	{
		public new CusClassPartPivot Clone() => (CusClassPartPivot)base.Clone();

		public new CusClassPartPivotLookups Lookups => (CusClassPartPivotLookups)base.Lookups;

		public new CusClassPartPivotValidation Validation => (CusClassPartPivotValidation)base.Validation;

		public new OrgSupplierPart Part => (OrgSupplierPart)base.Part;

		public new CusClassification Classification => (CusClassification)base.Classification;

		protected override Customs.Business.CusClassPartPivotLookups GetNewLookups() => new CusClassPartPivotLookups(this);

		protected override Customs.Business.CusClassPartPivotValidation GetNewValidation() => new CusClassPartPivotValidation(this);
	}
}
