using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class CusGoodsLocationLookups : EU.NCTS.Business.CusGoodsLocationLookups
	{
		public CusGoodsLocationLookups(EU.NCTS.Business.CusGoodsLocation parent) : base(parent)
		{
		}

		public override ZZRefCusCodeListCombinedCollection CustomsOfficeList => EUCustomsOfficeCodeCollection.CustomsOfficesWithRequiredRoles(Factory, new ZString[] { "IE" }, new ZString[] { "DEP" });
	}
}
