using Enterprise.ZArchitecture.Core;
using CustomsChargeTypeList = Enterprise.Customs.Business.CustomsChargeTypeList;

namespace Enterprise.Customs.IL.Business
{
	public class CusEntryLineFeeLookups : Customs.Business.CusEntryLineFeeLookups
	{
		public CusEntryLineFeeLookups(CusEntryLineFee parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList ChargeTypeList
		{
			get
			{
				return Factory.GetCachedValue("Enterprise.Customs.IL.Business.CusEntryLineFeeLookups.ChargeTypeList", () =>
				{
					var result = new CustomsChargeTypeList();
					result.Add(new CodeDescriptionPair(Constants.EntryLineFee.VATFeeTypeCode, Constants.EntryLineFee.VATFeeType));
					return result;
				});
			}
		}

		public override CodeDescriptionPairList RateOverrideReasonList => Factory.GetCachedValue<ILRateOverrideReasonList>();
	}
}
