using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.Customs.IE.MessageContracts.NCTS.Interfaces;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class IE007IncidentLocationProvider : IIE007IncidentLocation
	{
		public IE007IncidentLocationProvider(EU.Business.CusGoodsLocation goodsLocation)
		{
			this.goodsLocation = goodsLocation;
		}
		readonly EU.Business.CusGoodsLocation goodsLocation;

		public string LocationCodeType => null;

		public string UNLocode => goodsLocation.Unlocode;

		public string Country => goodsLocation.Unlocode.Left(2);

		public IAddress Address => null;

		public string QualifierOfIdentification => goodsLocation.CGL_Qualifier;

		public IGNSS GNSS => null;
	}
}
