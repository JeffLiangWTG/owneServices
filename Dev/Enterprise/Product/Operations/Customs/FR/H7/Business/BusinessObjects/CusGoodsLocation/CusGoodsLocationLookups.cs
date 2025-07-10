using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.H7.Business
{
	public class CusGoodsLocationLookups : EU.Business.CusGoodsLocationLookups
	{
		public CusGoodsLocationLookups(EU.Business.CusGoodsLocation parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList QualifierList => Factory.GetCachedValue("FRH7.CusGoodsLocationLookups.QualifierList", () =>
		{
			var qualifierList = new CodeDescriptionPairList();
			qualifierList.AddPair(CusGoodsLocationQualifierList.Codes.Address, CusGoodsLocationQualifierList.Descriptions.Address);
			return qualifierList;
		});
	}
}
