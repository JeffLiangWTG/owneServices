using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.Business
{
	public class AddInfoCusLineTariffDetailLookups : EU.Business.AddInfoCusLineTariffDetailLookups
	{
		public AddInfoCusLineTariffDetailLookups(EU.Business.AddInfoCusLineTariffDetail parent) : base(parent)
		{
		}

		public CodeDescriptionPairList PaymentMethodList => Factory.GetCachedValue<PaymentMethodList>();
	}
}
