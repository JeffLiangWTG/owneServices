namespace Enterprise.Customs.EU.Business.MasterFiles
{
	partial class CusClassification : AutoCusClassification
	{
		#region New'd objects for type safety (typeDeciders/concrete classes must take care of instantiation

		public new CusClassificationLookups Lookups
		{
			get { return (CusClassificationLookups)base.Lookups; }
		}

		public new CusClassificationValidation Validation
		{
			get { return (CusClassificationValidation)base.Validation; }
		}

		public new TariffFormatter CurrentTariffFormatter
		{
			get { return (TariffFormatter)base.CurrentTariffFormatter; }
		}

		#endregion

		#region Implementation

		#region Overridden 'CreateNew' methods

		protected override Customs.Business.CusClassificationLookups GetNewLookups()
		{
			return new CusClassificationLookups(this);
		}

		protected override Customs.Business.CusClassificationValidation GetNewValidation()
		{
			return new CusClassificationValidation(this);
		}

		#endregion

		protected override Customs.Business.TariffFormatter GetTariffFormatter()
		{
			return TariffFormatter.New(Country?.Code);
		}

		#endregion
	}
}
