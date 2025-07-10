using CargoWise.Types;

namespace Enterprise.Customs.EU.Business
{
	public interface ICusGoodsLocationProviderWhichAllowsMixedCase : ICusGoodsLocationProvider
	{
		ZBool AllowMixedCaseAuthorisationNumbers { get; }
	}
}
