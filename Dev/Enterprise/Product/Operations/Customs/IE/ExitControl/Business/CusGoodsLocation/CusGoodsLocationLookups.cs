using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.ExitControl.Business
{
	public class CusGoodsLocationLookups : Customs.Business.CusGoodsLocationLookups
	{
		public CusGoodsLocationLookups(CusGoodsLocation parent)
			: base(parent)
		{
		}

		public RefUNLOCOCollection UNLOCOs => new RefUNLOCOCollection(Factory);

		public CusGoodsLocationTypeList TypeOfLocationList => Factory.GetCachedValue<CusGoodsLocationTypeList>();

		protected new CusGoodsLocation Parent => (CusGoodsLocation)base.Parent;
	}
}
