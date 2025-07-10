using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.NCTS.Business;

sealed class NctsCargoDescFeePhase5Lookups : EU.NCTS.Business.NctsCargoDescFeeLookups, INctsCargoDescFeeLookups
{
	public NctsCargoDescFeePhase5Lookups(NctsCargoDescFee parent) : base(parent)
	{
	}

	public CodeDescriptionPairList RateOverrideReasonList => new CodeDescriptionPairList();

	public CodeDescriptionPairList MethodOfPaymentList => new CodeDescriptionPairList();

	public CodeDescriptionPairList MethodOfCalculationList => new CodeDescriptionPairList();

	public override CodeDescriptionPairList ChargeTypeList => new CodeDescriptionPairList();
}
