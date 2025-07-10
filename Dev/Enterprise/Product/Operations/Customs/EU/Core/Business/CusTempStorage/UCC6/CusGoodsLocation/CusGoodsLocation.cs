using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class CusGoodsLocation : Business.CusGoodsLocation, Integration.Customs.EU.ITemporaryStorageCusGoodsLocation
	{
		public CusGoodsLocation(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public static new readonly CusGoodsLocationTypeDecider TypeDecider = new CusGoodsLocationTypeDecider();
	}
}
