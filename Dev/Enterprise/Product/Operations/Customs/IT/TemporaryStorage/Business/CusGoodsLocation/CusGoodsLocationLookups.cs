using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.TemporaryStorage.Business;

public sealed class CusGoodsLocationLookups(CusGoodsLocation parent) : EU.Business.CusGoodsLocationLookups(parent)
{
	public CodeDescriptionPairList AdditionalIdentifierList
		=> new CusGoodsLocationAdditionalIdentifierListProvider(Factory, new CusGoodsLocationWrapper(parent))
		.GetCachedList(new ZString[]
		{
			CusAuthorizationHeaderTypeList.Codes.TemporaryStorage
		});

	public override CodeDescriptionPairList QualifierList
	{
		get
		{
			return Factory.GetCachedValue("IT.TemporaryStorage.CusGoodsLocationLookups.QualifierList", () =>
			{
				var qualifierList = new CodeDescriptionPairList();
				qualifierList.AddPair(CusGoodsLocationQualifierList.Codes.AuthorizationNumber, CusGoodsLocationQualifierList.Descriptions.AuthorizationNumber);
				return qualifierList;
			});
		}
	}

	public override CodeDescriptionPairList TypeList
	{
		get
		{
			return Factory.GetCachedValue("IT.TemporaryStorage.CusGoodsLocationLookups.TypeList", () =>
			{
				var typeList = new CodeDescriptionPairList();
				typeList.AddPair(CusGoodsLocationTypeList.Codes.ApprovedPlace, CusGoodsLocationTypeList.Descriptions.ApprovedPlace);
				return typeList;
			});
		}
	}
}
