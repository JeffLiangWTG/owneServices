using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Business
{
	public class CusGoodsLocationLookups : EU.Business.CusGoodsLocationLookups
	{
		public CusGoodsLocationLookups(CusGoodsLocation parent) : base(parent)
		{
		}

		CusGoodsLocation GoodsLocation => (CusGoodsLocation)Parent;

		public override CodeDescriptionPairList QualifierList
		{
			get
			{
				var isImpAndH2 = GoodsLocation.IsImportAndH2;
				return Factory.GetCachedValue("QualifierList_" + isImpAndH2, () =>
				{
					var qualifierList = new CodeDescriptionPairList();
					if (isImpAndH2)
					{
						qualifierList.AddPair(CusGoodsLocationQualifierList.Codes.AuthorizationNumber, CusGoodsLocationQualifierList.Descriptions.AuthorizationNumber);
						qualifierList.AddPair(CusGoodsLocationQualifierList.Codes.Address, CusGoodsLocationQualifierList.Descriptions.Address);
					}
					else
					{
						qualifierList = base.QualifierList;
					}

					return qualifierList;
				});
			}
		}
	}
}
