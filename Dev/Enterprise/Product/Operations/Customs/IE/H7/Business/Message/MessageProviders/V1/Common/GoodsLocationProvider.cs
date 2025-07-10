using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.Customs.IE.MessageContracts.Interfaces.AIS.H7V1;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.H7.Business.Messaging.V1
{
	public class GoodsLocationProvider : IGoodsLocation
	{
		public GoodsLocationProvider(CusGoodsLocation goodsLocation)
		{
			this.goodsLocation = Argument.NotNull(goodsLocation, nameof(goodsLocation));
		}

		readonly CusGoodsLocation goodsLocation;

		public string UNLOCODE => goodsLocation.Unlocode;

		public string AdditionalIdentifier => goodsLocation.CGL_AdditionalIdentifier;

		public string TypeOfLocation => goodsLocation.CGL_Type;

		public string QualifierOfIdentification => goodsLocation.CGL_Qualifier;

		public IAddress Address => CachedValueHelper.GetValue(ref addressCached, () => new AddressProvider(goodsLocation.Address));

		CachedValue<IAddress> addressCached;
	}
}
