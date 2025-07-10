using System;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public sealed class CusGoodsLocation : EU.Business.CusGoodsLocation, Integration.Customs.FR.ICusGoodsLocation
	{
		public CusGoodsLocation(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override Customs.Business.CusGoodsLocationValidation GetNewValidation() => new CusGoodsLocationValidation(this);

		protected override Type AddressType => typeof(CusGoodsLocationAddress);
	}
}
