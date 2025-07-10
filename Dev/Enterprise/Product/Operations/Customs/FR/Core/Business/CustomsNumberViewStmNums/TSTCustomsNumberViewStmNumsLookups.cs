using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business
{
	public class TSTCustomsNumberViewStmNumsLookups : CustomsNumberViewStmNumsLookups
	{
		public TSTCustomsNumberViewStmNumsLookups(CustomsNumberViewStmNums parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList TypeList => Factory.GetCachedValue<CusAuthorisationHeaderCustomsNumberRangeTypeList>();
	}
}
