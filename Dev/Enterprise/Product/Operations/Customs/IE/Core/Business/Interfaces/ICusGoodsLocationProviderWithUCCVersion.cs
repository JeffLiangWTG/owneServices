using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.Business
{
	public interface ICusGoodsLocationProviderWithUCCVersion : EU.Business.ICusGoodsLocationProvider
	{
		bool IsUCC5 { get; }
		bool IsUCC6 { get; }
		ZPropertyInfo IsUCC5Info { get; }
	}
}
