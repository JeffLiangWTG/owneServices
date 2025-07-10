using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.FR.H7.Business
{
	public class CusGoodsLocation : EU.H7.Business.CusGoodsLocation, Integration.Customs.FRH7.ICusGoodsLocation
	{
		public CusGoodsLocation(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new CusGoodsLocationLookups Lookups => (CusGoodsLocationLookups)base.Lookups;

		protected override Customs.Business.CusGoodsLocationLookups GetNewLookups() => new CusGoodsLocationLookups(this);
	}
}
