using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.NCTS;

public class CusGoodsLocationAddress(BusinessObjectFactory factory, DataRow row) : EU.NCTS.Business.CusGoodsLocationAddress(factory, row)
{
	public new CusGoodsLocation GoodsLocation => (CusGoodsLocation)base.GoodsLocation;

	public new CusGoodsLocationAddressLookups Lookups => (CusGoodsLocationAddressLookups)base.Lookups;

	protected override JobDocAddressLookups GetNewLookups() => new CusGoodsLocationAddressLookups(this);
}
