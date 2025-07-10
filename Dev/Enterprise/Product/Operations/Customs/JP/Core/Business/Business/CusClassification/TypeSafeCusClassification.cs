using Enterprise.Customs.Business;

namespace Enterprise.Customs.JP.Business
{
	public partial class CusClassification : BaseCusClassification, Integration.Customs.JP.ICusClassPartPivot
	{
		#region New'd objects for type safety (typeDeciders/concrete classes must take care of instantiation

		public new CusClassification Clone()
		{
			return (CusClassification)base.Clone();
		}

		public new CusClassificationLookups Lookups
		{
			get { return (CusClassificationLookups)base.Lookups; }
		}

		public new CusClassificationValidation Validation
		{
			get { return (CusClassificationValidation)base.Validation; }
		}

		#endregion

		#region Implementation

		#region protected override

		protected override TariffFormatter GetTariffFormatter() => new Common.TariffFormatter();

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
