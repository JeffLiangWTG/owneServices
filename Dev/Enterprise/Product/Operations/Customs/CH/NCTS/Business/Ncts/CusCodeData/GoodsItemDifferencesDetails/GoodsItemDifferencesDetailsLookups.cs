using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.NCTS.Business;

public sealed class GoodsItemDifferencesDetailsLookups : CusCodeDataLookups
{
	public GoodsItemDifferencesDetailsLookups(GoodsItemDifferencesDetails parent) : base(parent)
	{
	}

	public new GoodsItemDifferencesDetails Parent => (GoodsItemDifferencesDetails)base.Parent;

	public override CodeDescriptionPairList CY_CodeList
	{
		get
		{
			var unloadedState = Parent.Parent?.BY_UnloadedState ?? ZString.Empty;
			var isNew = unloadedState == EU.NCTS.Business.NctsUnloadedStateList.Codes.NEW;
			return Parent.Factory.GetCachedValue("CH.GoodsItemDifferencesDetails.CodeList|" + isNew,
				() => GetCodeList(isNew));
		}
	}

	CodeDescriptionPairList GetCodeList(bool isNew)
	{
		var unloadingRemarkCodeList = new UnloadingRemarkCodeList();
		if (isNew)
		{
			unloadingRemarkCodeList.RemoveCode(UnloadingRemarkCodeList.Codes.NotShipped);
			unloadingRemarkCodeList.RemoveCode(UnloadingRemarkCodeList.Codes.Stolen);
		}
		return unloadingRemarkCodeList;
	}
}
