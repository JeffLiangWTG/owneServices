using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.DE.Business
{
	public class AddInfoCusLineTariffDetail : EU.Business.AddInfoCusLineTariffDetail
	{
		public AddInfoCusLineTariffDetail(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty)
		{
		}

		public new AddInfoCusLineTariffDetailValidation Validation => (AddInfoCusLineTariffDetailValidation)base.Validation;
		protected override EUAddInfoValidation GetNewValidation() => new AddInfoCusLineTariffDetailValidation(this);
		public new CusLineTariffDetail Parent => (CusLineTariffDetail)base.Parent;
	}
}
