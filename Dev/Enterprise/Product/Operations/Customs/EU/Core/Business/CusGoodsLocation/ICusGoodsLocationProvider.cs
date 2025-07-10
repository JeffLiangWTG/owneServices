using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business
{
	public interface ICusGoodsLocationProvider
	{
		CusGoodsLocation GoodsLocation { get; }

		ZString GoodsLocationDescription { get; }

		ZPropertyInfo GoodsLocationDescriptionInfo { get; }

		void ValidateGoodsLocationDescription();

		ZString ProviderKey { get; }
	}
}
