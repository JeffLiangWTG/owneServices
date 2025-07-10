using Enterprise.Customs.IT.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.NCTS.Business;

public sealed class CusGoodsLocationLookups : EU.NCTS.Business.CusGoodsLocationLookups
{
	public CusGoodsLocationLookups(CusGoodsLocation parent) : base(parent)
	{
	}

	public new CusGoodsLocation Parent => (CusGoodsLocation)base.Parent;

	public override CodeDescriptionPairList QualifierList => CusGoodsLocationLookupsHelper.GetQualifierList(Factory);

	public OrganisationsFindBoxCollection OrganisationList => CusGoodsLocationLookupsHelper.GetOrganisationList(Factory);

	public CodeDescriptionPairList AdditionalIdentifierList
		=> IsPhase5
			? CusGoodsLocationLookupsHelper.GetAdditionalIdentifierList(Factory, new NctsPhase5CusGoodsLocationWrapper(Parent))
			: new CodeDescriptionPairList();

	#region Implementation

	bool IsPhase5 => Parent.Header?.IsPhase5 ?? false;

	#endregion
}
