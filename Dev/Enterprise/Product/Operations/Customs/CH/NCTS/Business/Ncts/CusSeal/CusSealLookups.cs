using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.NCTS.Business;

public class CusSealLookups : EU.NCTS.Business.CusSealLookups
{
	public CusSealLookups(CusSeal parent) : base(parent)
	{
	}

	new CusSeal Parent => (CusSeal)base.Parent;

	protected override CodeDescriptionPairList GetUnloadedStatesCore()
	{
		if (!Parent.IsInDatabase || (ZString)Parent.BK_UnloadingStateInfo.OriginalValue == NctsUnloadedStateList.Codes.NEW)
		{
			return Factory.GetCachedValue("CH.CusSealsLookups.UnloadedStates.NEW", () =>
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(NctsUnloadedStateList.Codes.NEW, NctsUnloadedStateList.Descriptions.NEW);
				return result;
			});
		}

		return Factory.GetCachedValue("CH.CusSealsLookups.UnloadedStates", () =>
		{
			var result = new NctsUnloadedStateList();
			result.RemoveCode(NctsUnloadedStateList.Codes.NEW);
			return result;
		});
	}
}
