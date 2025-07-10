using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business
{
	public class AddInfoCusLineTariffDetail : AddInfo
	{
		public AddInfoCusLineTariffDetail(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty)
		{
		}

		public new AddInfoCusLineTariffDetailLookups Lookups => (AddInfoCusLineTariffDetailLookups)base.Lookups;
		protected override EUAddInfoLookups GetNewLookups() => new AddInfoCusLineTariffDetailLookups(this);

		public new AddInfoCusLineTariffDetailValidation Validation => (AddInfoCusLineTariffDetailValidation)base.Validation;
		protected override EUAddInfoValidation GetNewValidation() => new AddInfoCusLineTariffDetailValidation(this);
	}
}

