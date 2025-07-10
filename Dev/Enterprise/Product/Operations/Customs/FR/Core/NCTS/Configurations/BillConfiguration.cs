using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.FR.NCTS.Configurations;

public sealed class BillConfiguration : EU.NCTS.Business.BillConfiguration
{
	protected override INctsBillPhase5ValidationDecider GetBillDeparturePhase5ValidationDecider() => new Business.NCTS.NctsBillDeparturePhase5ValidationDecider();
}
