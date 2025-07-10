using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public sealed class CusSealConfiguration : EU.NCTS.Business.CusSealConfiguration
{
	protected override ICusSealValidationDecider GetCusSealValidationDecider() => new CusSealValidationDecider();

	protected override ICusSealPhase5ValidationDecider GetCusSealPhase5ValidationDecider() => new CusSealPhase5ValidationDecider();
}
