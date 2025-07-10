using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IL.Business
{
	public class CusEntryHeaderChargesLookups : Customs.Business.CusEntryHeaderChargesLookups
	{
		public CusEntryHeaderChargesLookups(CusEntryHeaderCharges parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList RateOverrideReasonCodeList => Factory.GetCachedValue<ILRateOverrideReasonList>();
	}
}
