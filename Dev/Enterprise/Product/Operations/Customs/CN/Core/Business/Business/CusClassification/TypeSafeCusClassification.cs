namespace Enterprise.Customs.CN.Business
{
	public partial class CusClassification : Customs.Business.BaseCusClassification
	{
		#region New'd objects for type safety (typeDeciders/concrete classes must take care of instantiation

		public new CusClassification Clone()
		{
			return (CusClassification)base.Clone();
		}

		public new CusClassificationLookups Lookups => (CusClassificationLookups)base.Lookups;

		public new CusClassificationValidation Validation => (CusClassificationValidation)base.Validation;

		#endregion

		#region Implementation

		#region protected override

		protected override Customs.Business.CusClassificationLookups GetNewLookups()
		{
			return new CusClassificationLookups(this);
		}

		protected override Customs.Business.CusClassificationValidation GetNewValidation()
		{
			return new CusClassificationValidation(this);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CC_ClassificationType = CusClassification.ClassificationType.Both;
		}

		#endregion

		#endregion
	}
}
