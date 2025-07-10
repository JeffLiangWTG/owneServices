namespace Enterprise.Customs.MX.Business
{
	public partial class CusClassification : Customs.Business.BaseCusClassification
	{
		public new CusClassification Clone() => (CusClassification)base.Clone();

		public new CusClassificationLookups Lookups => (CusClassificationLookups)base.Lookups;

		public new CusClassificationValidation Validation => (CusClassificationValidation)base.Validation;

		protected override Customs.Business.CusClassificationLookups GetNewLookups() => new CusClassificationLookups(this);

		protected override Customs.Business.CusClassificationValidation GetNewValidation() => new CusClassificationValidation(this);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CC_ClassificationType = CusClassification.ClassificationType.Both;
		}
	}
}
