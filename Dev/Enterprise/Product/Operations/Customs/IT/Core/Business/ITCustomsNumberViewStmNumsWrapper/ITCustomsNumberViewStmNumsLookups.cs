using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business;

public class ITCustomsNumberViewStmNumsLookups : CustomsNumberViewStmNumsLookups
{
	public ITCustomsNumberViewStmNumsLookups(CustomsNumberViewStmNums parent)
		: base(parent)
	{ }

	public override CodeDescriptionPairList TypeList => Factory.GetCachedValue<NumberRangeTypeList>();
}
