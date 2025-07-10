using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public class CusGoodsLocationLookups : EU.Business.CusGoodsLocationLookups
	{
		public CusGoodsLocationLookups(EU.Business.CusGoodsLocation parent)
			: base(parent)
		{
		}

		protected new CusGoodsLocation Parent => (CusGoodsLocation)base.Parent;

		public override CodeDescriptionPairList QualifierList => Factory.GetCachedValue("ESH7.CusGoodsLocationLookups.QualifierList", () =>
			new CodeDescriptionPairList
			{
				new CodeDescriptionPair(CusGoodsLocationQualifierList.Codes.AuthorizationNumber, CusGoodsLocationQualifierList.Descriptions.AuthorizationNumber)
			});

		public override CodeDescriptionPairList TypeList => Factory.GetCachedValue("ESH7.CusGoodsLocationLookups.TypeList", () =>
			new CodeDescriptionPairList
			{
				new CodeDescriptionPair(CusGoodsLocationTypeList.Codes.AuthorizedPlace, CusGoodsLocationTypeList.Descriptions.AuthorizedPlace)
			});
	}
}
