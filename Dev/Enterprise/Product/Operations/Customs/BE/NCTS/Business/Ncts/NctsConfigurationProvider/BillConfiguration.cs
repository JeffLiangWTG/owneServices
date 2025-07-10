using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.BE.NCTS.Business;

public sealed class BillConfiguration : EU.NCTS.Business.BillConfiguration
{
	protected override INctsBillPhase5ValidationDecider GetBillDeparturePhase5ValidationDecider() => new NctsBillDeparturePhase5ValidationDecider();
}
