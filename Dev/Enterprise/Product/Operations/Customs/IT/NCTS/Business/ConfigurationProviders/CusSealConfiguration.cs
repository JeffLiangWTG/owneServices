using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IT.NCTS.Business;

public sealed class CusSealConfiguration : EU.NCTS.Business.CusSealConfiguration
{
	protected override ICusSealPhase5ValidationDecider GetCusSealPhase5ValidationDecider() => new CusSealPhase5ValidationDecider();
}
