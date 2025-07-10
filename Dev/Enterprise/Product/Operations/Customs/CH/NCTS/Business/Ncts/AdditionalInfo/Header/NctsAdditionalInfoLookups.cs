using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NctsAdditionalInfoLookups : NctsAdditionalInfoPhase5Lookups
{
	public NctsAdditionalInfoLookups(NctsAdditionalInfo parent) : base(parent)
	{
	}
	protected new NctsAdditionalInfo Parent => (NctsAdditionalInfo)base.Parent;

	public override CodeDescriptionPairList SubTypeList => Parent?.AdditionalInfoParent?.IsNationalTransitSwitzerland ?? false ? NationalTransitSubTypeList : base.SubTypeList;

	CodeDescriptionPairList NationalTransitSubTypeList => GetNationalTransitSubTypeList();

	CodeDescriptionPairList GetNationalTransitSubTypeList()
	{
		bool parentIsGoodsItem = Parent.ParentAsGoodsItem != null;
		return Factory.GetCachedValue("CH.NctsAdditionalInfoLookups.NationalTransitSubTypeList.ParentIsGoodsItem_" + parentIsGoodsItem, () =>
		{
			var list = new CodeDescriptionPairList(base.SubTypeList);
			list.RemoveCode(AdditionalInfoSubTypeList.Codes.AdditionalReference);
			return list;
		});
	}
}
