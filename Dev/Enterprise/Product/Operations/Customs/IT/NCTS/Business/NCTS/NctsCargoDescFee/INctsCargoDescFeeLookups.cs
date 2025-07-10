using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.NCTS.Business;

public interface INctsCargoDescFeeLookups
{
	CodeDescriptionPairList RateOverrideReasonList { get; }

	CodeDescriptionPairList MethodOfPaymentList { get; }

	CodeDescriptionPairList MethodOfCalculationList { get; }
}
