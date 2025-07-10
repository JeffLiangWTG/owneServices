using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public interface ICusGoodsLocationValidationDecider
	{
		bool IsRuleC0382Active { get; }

		bool IsRuleTR0061Active { get; }
	}

	public interface ICusGoodsLocationProviderWithValidationDecider : ICusGoodsLocationProvider
	{
		ICusGoodsLocationValidationDecider GoodsLocationValidationDecider { get; }
	}
}
