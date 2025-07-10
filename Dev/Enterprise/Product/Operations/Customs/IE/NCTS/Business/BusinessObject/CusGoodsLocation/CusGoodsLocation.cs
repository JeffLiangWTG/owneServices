using System;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class CusGoodsLocation : EU.NCTS.Business.CusGoodsLocation, Integration.Customs.IENCTS.INctsCusGoodsLocation
	{
		public CusGoodsLocation(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override Type AddressType => typeof(CusGoodsLocationAddress);

		public new CusGoodsLocationAddress Address => (CusGoodsLocationAddress)base.Address;

		public new CusGoodsLocationLookups Lookups => (CusGoodsLocationLookups)base.Lookups;

		protected override Customs.Business.CusGoodsLocationLookups GetNewLookups()
		{
			return new CusGoodsLocationLookups(this);
		}
	}
}
