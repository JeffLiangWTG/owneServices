using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business
{
	public class CusGoodsLocationAddress : EU.Business.CusGoodsLocationAddress
	{
		public CusGoodsLocationAddress(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override JobDocAddressValidation GetNewValidation() => new CusGoodsLocationAddressValidation(this);
	}
}
