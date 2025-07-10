using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;

public class TSCustomsNumberViewStmNumsLookups : CustomsNumberViewStmNumsLookups
{
	public TSCustomsNumberViewStmNumsLookups(CustomsNumberViewStmNums parent) : base(parent) { }

	public override CodeDescriptionPairList TypeList => Factory.GetCachedValue<NumberRangeTypeList>();
}
