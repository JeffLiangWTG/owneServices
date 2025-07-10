using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class GoodsLocationProvider : IGoodsLocation
	{
		public GoodsLocationProvider(CusGoodsLocation goodsLocation)
		{
			this.goodsLocation = Argument.NotNull(goodsLocation, nameof(goodsLocation));
		}
		CusGoodsLocation goodsLocation { get; }

		public string Qualifier => goodsLocation.CGL_Qualifier;

		public string Type => goodsLocation.CGL_Type;

		public string AdditionalIdentifier => goodsLocation.CGL_AdditionalIdentifier;

		public IAddress Address => CachedValueHelper.GetValue(ref addressCached, () => AddressProvider.New(goodsLocation.Address));
		CachedValue<IAddress> addressCached;

		public string ID => goodsLocation.CGL_CustomsOffice;
	}
}
