using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.GB.H7.Business
{
	public sealed class CusGoodsLocation : EU.H7.Business.CusGoodsLocation, Integration.Customs.GB.GBH7.ICusGoodsLocation
	{
		public CusGoodsLocation(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new CusGoodsLocationLookups Lookups => (CusGoodsLocationLookups)base.Lookups;

		protected override Customs.Business.CusGoodsLocationLookups GetNewLookups() => new CusGoodsLocationLookups(this);

		protected override Customs.Business.CusGoodsLocationValidation GetNewValidation() => new CusGoodsLocationValidation(this);
	}
}
