namespace Enterprise.Customs.ES.NCTS.Business
{
	public sealed class BillConfiguration : EU.NCTS.Business.BillConfiguration
	{
		protected override EU.NCTS.Business.INctsBillPhase5ValidationDecider GetBillDeparturePhase5ValidationDecider() => new NctsBillDeparturePhase5ValidationDecider();

		protected override EU.NCTS.Business.INctsBillArrivalPhase5ValidationDecider GetBillArrivalPhase5ValidationDecider() => new NctsBillArrivalPhase5ValidationDecider();
	}
}
