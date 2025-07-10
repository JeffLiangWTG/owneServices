using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.Interfaces.AIS.H7V1;
using CargoWise.EntityFramework;
namespace Enterprise.Customs.IE.H7.Business.Messaging.V1
{
	public class LocationOfGoodsGNSSProvider : ILocationOfGoodsGNSS
	{
		public LocationOfGoodsGNSSProvider(CusGoodsLocation goodsLocation)
		{
			this.goodsLocation = Argument.NotNull(goodsLocation, nameof(goodsLocation));
		}

		readonly CusGoodsLocation goodsLocation;

		public IGoodsLocation GoodsLocation => CachedValueHelper.GetValue(ref goodsLocationCached, () => new GoodsLocationProvider(goodsLocation));
		CachedValue<IGoodsLocation> goodsLocationCached;
	}
}
