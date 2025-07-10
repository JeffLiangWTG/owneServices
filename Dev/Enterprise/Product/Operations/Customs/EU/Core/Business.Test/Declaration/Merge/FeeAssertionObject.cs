using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.Testing
{
	public struct FeeAssertionObject
	{
		public ZString ChargeType { get; set; }
		public ZDecimal ChargeAmount { get; set; }
		public ZDecimal BaseValue { get; set; }
		public ZDecimal Rate { get; set; }
		public ZString MethodOfCalculation { get; set; }
		public ZString OverrideReason { get; set; }
		public ZString MethodOfPayment { get; set; }
	}
}
