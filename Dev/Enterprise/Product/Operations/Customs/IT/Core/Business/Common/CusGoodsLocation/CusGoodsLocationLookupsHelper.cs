using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business;

public static class CusGoodsLocationLookupsHelper
{
	public static CodeDescriptionPairList GetAdditionalIdentifierList(BusinessObjectFactory factory, ICusGoodsLocationWrapper goodsLocationWrapper)
	{
		return new CusGoodsLocationAdditionalIdentifierListProvider(factory, goodsLocationWrapper).GetCachedList();
	}

	public static OrganisationsFindBoxCollection GetOrganisationList(BusinessObjectFactory factory) => new OrganisationsFindBoxCollection(factory);

	public static CodeDescriptionPairList GetQualifierList(BusinessObjectFactory factory)
	{
		const string qualifierListCachedKey = "IT.CusGoodsLocationLookups.QualifierList";

		return factory.GetCachedValue(qualifierListCachedKey, () =>
		{
			var qualifierList = new CodeDescriptionPairList();
			qualifierList.AddPair(CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier, CusGoodsLocationQualifierList.Descriptions.CustomsOfficeIdentifier);
			qualifierList.AddPair(CusGoodsLocationQualifierList.Codes.AuthorizationNumber, CusGoodsLocationQualifierList.Descriptions.AuthorizationNumber);
			qualifierList.AddPair(CusGoodsLocationQualifierList.Codes.Address, CusGoodsLocationQualifierList.Descriptions.Address);
			return qualifierList;
		});
	}
}
