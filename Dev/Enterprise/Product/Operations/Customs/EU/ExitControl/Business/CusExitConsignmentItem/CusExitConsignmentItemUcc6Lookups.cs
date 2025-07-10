using Enterprise.Customs.ExitControlBase.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.ExitControl.Business;

public class CusExitConsignmentItemUcc6Lookups : CusExitConsignmentItemLookups
{
	public CusExitConsignmentItemUcc6Lookups(AutoCusExitConsignmentItem parent) : base(parent)
	{
	}

	protected override CodeDescriptionPairList StatusListCore => Factory.GetCachedValue<DiscrepanciesStatusCodeList>();

	protected override CodeDescriptionPairList UCRStatusListCore => Factory.GetCachedValue("EUCusExitConsignmentItemUcc6Lookups.UCRStatusList", () =>
	{
		var result = new DiscrepanciesStatusCodeList();
		result.RemoveCode(DiscrepanciesStatusCodeList.Codes.Missing);
		return result;
	});
}
