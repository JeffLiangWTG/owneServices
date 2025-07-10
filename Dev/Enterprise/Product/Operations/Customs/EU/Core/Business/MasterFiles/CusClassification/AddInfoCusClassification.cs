using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business.MasterFiles
{
	public class AddInfoCusClassification : AddInfo
	{
		public AddInfoCusClassification(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty)
		{
		}

		public new AddInfoCusClassificationLookups Lookups
		{
			get { return (AddInfoCusClassificationLookups)base.Lookups; }
		}

		public new AddInfoCusClassificationValidation Validation
		{
			get { return (AddInfoCusClassificationValidation)base.Validation; }
		}

		protected override EUAddInfoLookups GetNewLookups()
		{
			return new AddInfoCusClassificationLookups(this);
		}

		protected override EUAddInfoValidation GetNewValidation()
		{
			return new AddInfoCusClassificationValidation(this);
		}
	}
}
