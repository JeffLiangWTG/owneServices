using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business.Declaration;

public sealed class CusGoodsLocationLookups : EU.Business.CusGoodsLocationLookups
{
	public CusGoodsLocationLookups(CusGoodsLocation parent) : base(parent)
	{
	}

	public new CusGoodsLocation Parent => (CusGoodsLocation)base.Parent;

	public override CodeDescriptionPairList QualifierList => CusGoodsLocationLookupsHelper.GetQualifierList(Factory);

	public OrganisationsFindBoxCollection OrganisationList => CusGoodsLocationLookupsHelper.GetOrganisationList(Factory);

	public CodeDescriptionPairList AdditionalIdentifierList
		=> CusGoodsLocationLookupsHelper.GetAdditionalIdentifierList(Factory, new CusGoodsLocationWrapper(Parent));
}
